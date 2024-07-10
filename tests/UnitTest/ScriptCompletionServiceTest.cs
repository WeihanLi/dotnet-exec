// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using Exec.Services;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Scripting;
using ReferenceResolver;
using System.Reflection;

namespace UnitTest;

public class ScriptCompletionServiceTest
{
    [Theory]
    [InlineData("Guid.", "NewGuid")]
    public async Task CompletionTest(string code, string expectedCompletion)
    {
        var usings = FrameworkReferenceResolver
            .GetImplicitUsingsWithoutGlobalSpecified(FrameworkReferenceResolver.FrameworkNames.Default);
        var defaultReferences = await FrameworkReferenceResolver.ResolveDefaultReferences(ExecOptions.DefaultTargetFramework);
        var metadataReferences = defaultReferences.Select(r =>
        {
            try
            {
                _ = AssemblyName.GetAssemblyName(r);
                return MetadataReference.CreateFromFile(r);
            }
            catch
            {
                return null;
            }
        }).WhereNotNull().ToArray();
        var scriptOptions = ScriptOptions.Default
            .WithImports(usings)
            .WithReferences(metadataReferences)
            ;
        var completionService = new ScriptCompletionService();
        var completions = await completionService.GetCompletions(scriptOptions, code);
        Assert.NotEmpty(completions);
        Assert.Contains(completions, s => s.DisplayText.Contains(expectedCompletion));
    }

    [Theory]
    [InlineData("Guid.New", "NewGuid")]
    public async Task CompletionTest2(string code, string expectedCompletion)
    {
        var usings = FrameworkReferenceResolver
            .GetImplicitUsingsWithoutGlobalSpecified(FrameworkReferenceResolver.FrameworkNames.Default);
        var defaultReferences = await FrameworkReferenceResolver.ResolveDefaultReferences(ExecOptions.DefaultTargetFramework);
        var metadataReferences = defaultReferences.Select(r =>
        {
            try
            {
                _ = AssemblyName.GetAssemblyName(r);
                return MetadataReference.CreateFromFile(r);
            }
            catch
            {
                return null;
            }
        }).WhereNotNull().ToArray();
        var scriptOptions = ScriptOptions.Default
            .WithImports(usings)
            .WithReferences(metadataReferences)
            ;
        var completionService = new ScriptCompletionService();
        var completions = await completionService.GetCompletions(scriptOptions, code);
        Assert.NotEmpty(completions);
        Assert.Contains(completions, s => s.DisplayText.Contains(expectedCompletion));
        Assert.DoesNotContain(completions, s => s.DisplayText.Contains("ToString"));
    }

    [Fact]
    public async Task ExpectedTest()
    {
        var usings = FrameworkReferenceResolver.GetImplicitUsingsWithoutGlobalSpecified(FrameworkReferenceResolver.FrameworkNames.Default);
        var defaultReferences = await FrameworkReferenceResolver.ResolveDefaultReferences("net8.0");
        var metadataReferences = defaultReferences.Select(r =>
        {
            try
            {
                _ = AssemblyName.GetAssemblyName(r);
                return MetadataReference.CreateFromFile(r);
            }
            catch
            {
                return null;
            }
        }).WhereNotNull().ToArray();
        using var workspace = new AdhocWorkspace(MefHostServices.DefaultHost);
        var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Annotations)
            .WithUsings(usings)
            ;

        var projectInfo = ProjectInfo.Create(
            ProjectId.CreateNewId(),
            VersionStamp.Create(),
            "test",
            "test",
            LanguageNames.CSharp,
            isSubmission: true)
            .WithMetadataReferences(
               metadataReferences
            )
            .WithCompilationOptions(compilationOptions)
            ;
        var project = workspace.AddProject(projectInfo);

        var code = "Guid.";

        var documentInfo = DocumentInfo.Create(
            DocumentId.CreateNewId(project.Id), "__Script.cs",
            sourceCodeKind: SourceCodeKind.Script,
            loader: new PlainTextLoader(code));
        var document = workspace.AddDocument(documentInfo);

        var completionService = CompletionService.GetService(document);
        Assert.NotNull(completionService);

        var completions = await completionService.GetCompletionsAsync(document, code.Length);
        Assert.NotEmpty(completions.ItemsList);
        Assert.Contains(completions.ItemsList, i => i.DisplayText.Contains("NewGuid"));
    }
}
