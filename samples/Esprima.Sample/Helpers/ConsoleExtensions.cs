using System;
using McMaster.Extensions.CommandLineUtils;

namespace Esprima.Sample.Helpers;

internal static class ConsoleExtensions
{
    public static string ReadString(this IConsole console)
    {
        if (console.IsInputRedirected)
        {
            return console.In.ReadToEnd();
        }

        console.WriteLine("Press CTRL+C (in an empty line) to complete input.");
        console.WriteLine();

        void Console_CancelKeyPress(object? sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            console.CancelKeyPress -= Console_CancelKeyPress;
        }
        console.CancelKeyPress += Console_CancelKeyPress;

        var result = console.In.ReadToEnd();

        console.WriteLine();

        return result;
    }
}
