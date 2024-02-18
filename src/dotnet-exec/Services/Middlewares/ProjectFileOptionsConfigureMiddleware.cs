// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using ReferenceResolver;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Exec.Services.Middlewares;

internal sealed class ProjectFileOptionsConfigureMiddleware
    (IUriTransformer uriTransformer, ILogger logger) : IOptionsConfigureMiddleware
{
    public async Task InvokeAsync(ExecOptions options, Func<ExecOptions, Task> next)
    {
        // exact reference and usings from project file
        if (options.ProjectPath.IsNotNullOrEmpty())
        {
            var startTime = Stopwatch.GetTimestamp();
            // https://learn.microsoft.com/en-us/dotnet/standard/linq/linq-xml-overview
            var projectPath = uriTransformer.Transform(options.ProjectPath);
            var element = XElement.Load(projectPath);
            var itemGroups = element.Descendants("ItemGroup").ToArray();
            if (itemGroups.HasValue())
            {
                var propertyRegex = new Regex(@"\$\((?<propertyName>\w+)\)", RegexOptions.Compiled);
                var usingElements = itemGroups.SelectMany(x => x.Descendants("Using"));
                foreach (var usingElement in usingElements)
                {
                    var usingText = usingElement.Attribute("Include")?.Value;
                    if (usingText.IsNotNullOrEmpty())
                    {
                        if (usingText.Contains("$("))
                        {
                            var propertyMatch = false;
                            var match = propertyRegex.Match(usingText);
                            if (match.Success)
                            {
                                var propertyValue = element.Descendants("PropertyGroup")
                                    .Descendants(match.Groups["propertyName"].Value)
                                    .FirstOrDefault()?.Value;
                                if (propertyValue != null)
                                {
                                    usingText = usingText.Replace(match.Value, propertyValue);
                                    propertyMatch = !usingText.Contains("$(");
                                }
                                else
                                {
                                    propertyMatch = false;
                                }
                            }

                            if (!propertyMatch) continue;
                        }
                        if (usingElement.Attribute("Static")?.Value == "true")
                        {
                            usingText = $"static {usingText}";
                        }

                        var alias = usingElement.Attribute("Alias")?.Value;
                        if (alias.IsNotNullOrEmpty())
                        {
                            usingText = $"{alias} = {usingText}";
                        }
                    }
                    else
                    {
                        usingText = usingElement.Attribute("Remove")?.Value;
                        if (usingText.IsNotNullOrEmpty())
                        {
                            usingText = $"- {usingText}";
                        }
                    }

                    if (usingText.IsNotNullOrEmpty())
                    {
                        options.Usings.Add(usingText);
                    }
                }

                var packageReferenceElements = itemGroups.SelectMany(x => x.Descendants("PackageReference"));
                foreach (var packageReferenceElement in packageReferenceElements)
                {
                    var packageIdAttribute = packageReferenceElement.Attribute("Include") ?? packageReferenceElement.Attribute("Update");
                    if (packageIdAttribute is null) continue;
                    var packageId = packageIdAttribute.Value;
                    var packageVersion = packageReferenceElement.Attribute("Version")?.Value ?? string.Empty;
                    if (packageVersion.Contains("$("))
                    {
                        var newPackageVersion = string.Empty;
                        var match = propertyRegex.Match(packageVersion);
                        if (match.Success)
                        {
                            var propertyValue = element.Descendants("PropertyGroup")
                                .Descendants(match.Groups["propertyName"].Value)
                                .FirstOrDefault()?.Value;
                            if (propertyValue != null)
                            {
                                var packageVersionUpdated = packageVersion.Replace(match.Value, propertyValue);
                                if (!packageVersionUpdated.Contains("$("))
                                {
                                    newPackageVersion = packageVersionUpdated;
                                }
                            }
                        }
                        if (newPackageVersion.IsNullOrWhiteSpace())
                        {
                            packageVersion = newPackageVersion;
                        }
                    }
#pragma warning disable CA1859 // Use concrete types when possible for improved performance
                    IReference reference = new NuGetReference(packageId, packageVersion);                    
                    options.References.Add(reference.ReferenceWithSchema);
#pragma warning restore CA1859 // Use concrete types when possible for improved performance                    
                }

                if (File.Exists(projectPath))
                {
                    var projectDirectory = Path.GetFullPath(Guard.NotNullOrEmpty(Path.GetDirectoryName(projectPath)));
                    var projectReferenceElements = itemGroups.SelectMany(x => x.Descendants("ProjectReference"));
                    foreach (var projectReferenceElement in projectReferenceElements)
                    {
                        var includeAttribute = projectReferenceElement.Attribute("Include");
                        if (includeAttribute?.Value is null) continue;

                        var referenceProjectPath = includeAttribute.Value;
                        var referenceProjectFullPath = Path.GetFullPath(referenceProjectPath, projectDirectory);
                        if (!File.Exists(referenceProjectPath))
                            continue;

#pragma warning disable CA1859 // Use concrete types when possible for improved performance
                        IReference projectReference = new ProjectReference(referenceProjectFullPath);
                        options.References.Add(projectReference.ReferenceWithSchema);
#pragma warning restore CA1859 // Use concrete types when possible for improved performance
                    }

                }
            }
            var duration = ProfilerHelper.GetElapsedTime(startTime);
            logger.LogDebug("Exact info from project file elapsed time: {duration}", duration);
        }
        await next(options);
    }
}
