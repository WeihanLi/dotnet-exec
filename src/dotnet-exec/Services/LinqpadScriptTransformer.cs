// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using ReferenceResolver;
using System.Text;
using System.Xml.Linq;

namespace Exec.Services;
internal sealed class LinqpadScriptTransformer : IScriptTransformer
{
    public HashSet<string> SupportedExtensions { get; } =
    [
        ".linq",
        ".linqpad"
    ];

    public Task InvokeAsync(ExecOptions context, string[] scriptLines)
    {
        var queryXml = string.Empty;
        var queryBuilder = new StringBuilder();
        foreach (var line in scriptLines)
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
        switch (kind)
        {
            case null:
            case "Statements":
            case "Program":
                context.CompilerType = Helper.Default;
                break;

            case "Expression":
                context.CompilerType = Helper.Script;
                context.ExecutorType = Helper.Script;
                break;

            default:
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

        return Task.CompletedTask;
    }
}
