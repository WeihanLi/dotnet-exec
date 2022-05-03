using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using WeihanLi.Common.Models;

var command = new Command("dotnet-exec");

foreach (var item in ExecOptions.GetArguments())
{
    command.AddArgument(item);
}
foreach (var option in ExecOptions.GetOptions())
{
    command.AddOption(option);
}

command.SetHandler(async (ParseResult parseResult, IConsole console) =>
{
    // 1. options binding
    var options = new ExecOptions();
    options.BindCommandLineArguments(parseResult);
    // 2. construct project
    if (!File.Exists(options.ScriptFile))
    {
        console.Error.Write($"The file {options.ScriptFile} does not exists");
        return;
    }
    var sourceText = await File.ReadAllTextAsync(options.ScriptFile).ConfigureAwait(false);
    // 3. compile and run
    var syntaxTree = CSharpSyntaxTree.ParseText(sourceText, new CSharpParseOptions(LanguageVersion.Latest));
    var references = new[]
        {
            typeof(object).Assembly,
            typeof(Action).Assembly,
            typeof(LambdaExpression).Assembly,
            typeof(TableAttribute).Assembly,
            typeof(DescriptionAttribute).Assembly,
            typeof(Result).Assembly,
            Assembly.Load("System.Runtime"),
        }
        .Select(assembly => assembly.Location)
        .Distinct()
        .Select(l => MetadataReference.CreateFromFile(l))
        .Cast<MetadataReference>()
        .ToArray();

    var assemblyName = $"dotnet-exec.dynamic.{GuidIdGenerator.Instance.NewId()}";
    var compilation = CSharpCompilation.Create(assemblyName)
        .WithOptions(new CSharpCompilationOptions(OutputKind.ConsoleApplication, usings: options.GlobalUsing))
        .AddReferences(references)
        .AddSyntaxTrees(syntaxTree);
    await using var ms = new MemoryStream();
    var compilationResult = compilation.Emit(ms);
    if (compilationResult.Success)
    {
        var assemblyBytes = ms.ToArray();
        var assembly = Assembly.Load(assemblyBytes);
        var entryMethod = assembly.EntryPoint;
        if (entryMethod is null && options.EntryPoint.IsNotNullOrEmpty())
        {
            entryMethod = assembly.GetTypes()
                .Select(x => x.GetMethods(BindingFlags.Static))
                .SelectMany(x => x)
                .FirstOrDefault(x => x.Name.Equals(options.EntryPoint))
                ;
        }
        if (entryMethod is not null)
        {
            var parameters = entryMethod.GetParameters();
            entryMethod.Invoke(null, parameters.IsNullOrEmpty() ? Array.Empty<object>() : args);
        }
    }

    var error = new StringBuilder(compilationResult.Diagnostics.Length * 1024);
    foreach (var t in compilationResult.Diagnostics)
    {
        error.AppendLine($"{t.GetMessage()}");
    }
    throw new ArgumentException($"Compile error:{Environment.NewLine}{error}");
});

await command.InvokeAsync(args);
