using System;
using System.Linq;
using Esprima.Sample.Commands;
using McMaster.Extensions.CommandLineUtils;

namespace Esprima.Sample;

[Command("esprimatest", Description = "A command line tool for testing Esprima.NET features.",
    UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.StopParsingAndCollect)]
[HelpOption(Inherited = true)]
[Subcommand(typeof(ParseCommand), typeof(TokenizeCommand))]
public class Program
{
    public static int Main(string[] args)
    {
        var console = PhysicalConsole.Singleton;

        using var app = new CommandLineApplication<Program>(console, Environment.CurrentDirectory);
        app.Conventions.UseDefaultConventions();
        try
        {
            return app.Execute(args);
        }
        catch (ParserException ex)
        {
            console.Error.WriteLine(ex.LineNumber > 0 ? $"{ex.LineNumber},{ex.Column}: {ex.Description}" : ex.Description);
            return -1;
        }
        catch (CommandParsingException ex)
        {
            console.Error.WriteLine(ex.Message);
            return 1;
        }
    }

    public int OnExecute(CommandLineApplication app)
    {
        var args = app.RemainingArguments.Prepend(ParseCommand.CommandName).ToArray();
        return app.Execute(args);
    }
}
