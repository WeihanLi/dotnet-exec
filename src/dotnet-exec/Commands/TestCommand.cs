using System.CommandLine;
using System.CommandLine.Invocation;
using Xunit.ConsoleClient;

namespace Exec.Commands;

internal sealed class TestCommand : Command
{
    public TestCommand() : base("test", "Execute xunit test cases")
    {
        var testFileArgument = new Argument<string>("testFile", "The xunit test file to execute");
        AddArgument(testFileArgument);

        this.Handler = CommandHandler.Create<string>(async (testFile) =>
        {
            var consoleRunner = new ConsoleRunner();
            var result = await consoleRunner.RunAsync(new[] { testFile });
            return result;
        });
    }
}
