// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0


using ReferenceResolver;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Xml.Linq;

namespace Exec.Services.Middlewares;

[ExcludeFromCodeCoverage]
internal sealed class LinqpadOptionsPreConfigureMiddleware : IOptionsPreConfigureMiddleware
{
    private const string LinqpadExtension = ".linq";
    public async Task InvokeAsync(ExecOptions context, Func<ExecOptions, Task> next)
    {
        var script = context.Script;
        if (
            LinqpadExtension.EqualsIgnoreCase(Path.GetExtension(script))
            && File.Exists(script)
            )
        {
            var queryXml = string.Empty;
            var queryBuilder = new StringBuilder();
            var lines = File.ReadAllLines(script);
            foreach (var line in lines)
            {
                if (!string.IsNullOrEmpty(line))
                {
                    queryBuilder.AppendLine(line);
                }
                else
                {
                    if (string.IsNullOrEmpty(queryXml))
                    {
                        queryXml = queryBuilder.ToString();
                        queryBuilder.Clear();
                    }
                }
            }

            var doc = XDocument.Parse(queryXml);
            var rootElement = doc.Root;
            ArgumentNullException.ThrowIfNull(rootElement);
            var kind = rootElement.Attribute("Kind")?.Value;
            if (kind is not "Statements" or null)
            {
                throw new NotSupportedException($"Kind `{kind}` is not supported");
            }
            var nugetReferences = rootElement.Elements("NuGetReference")
                .Select(e => e.Value)
                .ToArray()
                ;
            foreach (var reference in nugetReferences)
            {
                context.References.Add(NuGetReference.Parse(reference).ReferenceWithSchema());
            }
            var namespaceUsings = rootElement.Elements("Namespace")
                .Select(e => e.Value)
                .ToArray()
                ;
            // use custom Dump extension to support Dump() extension
            context.Usings.Add("WeihanLi.Extensions.Dump");
            context.References.Add(new NuGetReference("WeihanLi.Common").ReferenceWithSchema());
            foreach (var item in namespaceUsings)
            {
                context.Usings.Add(item);
            }
            context.Script = queryBuilder.ToString();
        }
        await next(context);
    }
}
