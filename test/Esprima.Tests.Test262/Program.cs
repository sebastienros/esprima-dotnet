using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Spectre.Console;
using Test262Harness;

namespace Esprima.Tests.Test262;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var rootDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? string.Empty;
        var projectRoot = Path.Combine(rootDirectory, "../../..");

        var allowListFile = Path.Combine(projectRoot, "allow-list.txt");
        var lines = File.Exists(allowListFile) ? await File.ReadAllLinesAsync(allowListFile) : Array.Empty<string>();
        var knownFailing = new HashSet<string>(lines
            .Where(x => !string.IsNullOrWhiteSpace(x) && !x.StartsWith("#"))
        );

        // this should be same in both Test262Harness.settings.json and here
        const string Sha = "53d6cd6d463df461e1c506e0d2be4e36de0ef6fa";
        var stream = await Test262StreamExtensions.FromGitHub(Sha);

        // we materialize to give better feedback on progress
        var test262Files = new ConcurrentBag<Test262File>();

        TestExecutionSummary? summary = null;

        AnsiConsole.Progress()
            .Columns(
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new SpinnerColumn(),
                new ElapsedTimeColumn()
            )
            .Start(ctx =>
            {
                var readTask = ctx.AddTask("Loading tests", maxValue: 90_000);
                readTask.StartTask();

                test262Files = new ConcurrentBag<Test262File>(stream.GetTestFiles());
                readTask.Value = 100;
                readTask.StopTask();

                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("Found [green]{0}[/] test cases to test against", test262Files.Count);

                var testTask = ctx.AddTask("Running tests", maxValue: test262Files.Count);

                var options = new Test262RunnerOptions
                {
                    Execute = static file =>
                    {
                        var parserOptions = new ParserOptions
                        {
                            Tolerant = false
                        };
                        var parser = new JavaScriptParser(file.Program, parserOptions);
                        if (file.Type == ProgramType.Script)
                        {
                            parser.ParseScript(file.Strict);
                        }
                        else
                        {
                            parser.ParseModule();
                        }
                    },
                    IsIgnored = file => knownFailing.Contains(file.ToString()),
                    IsParseError = exception => exception is ParserException,
                    ShouldThrow = file => file.NegativeTestCase?.Type == ExpectedErrorType.SyntaxError || file.NegativeTestCase?.Phase == TestingPhase.Parse,
                    OnTestExecuted = _ => testTask.Increment(1)
                };

                var executor = new Test262Runner(options);
                summary = executor.Run(test262Files);
                testTask.StopTask();
            });

        AnsiConsole.WriteLine("Testing complete.");

        var knownTestFileNames = new HashSet<string>(test262Files.Select(x => x.ToString()));

        summary!.Unrecognized.AddRange(knownFailing
            .Where(x => !knownTestFileNames.Contains(x)));

        Report(summary);

        if (args.Any(x => x == "--update-allow-list"))
        {
            UpdateAllowList(allowListFile, summary, knownFailing.Where(x => knownTestFileNames.Contains(x)).ToList());
        }

        return summary.HasProblems ? 1 : 0;
    }

    private static void Report(TestExecutionSummary testExecutionSummary)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[green]Summary:[/]") { Alignment = Justify.Left });

        AnsiConsole.MarkupLine(" [green]:check_mark: {0,5}[/] valid programs parsed without error", testExecutionSummary.Allowed.Success.Count);
        AnsiConsole.MarkupLine(" [green]:check_mark: {0,5}[/] invalid programs produced a parsing error", testExecutionSummary.Allowed.Failure.Count);
        AnsiConsole.MarkupLine(" [yellow]:check_mark: {0,5}[/] invalid programs did not produce a parsing error (and in allow file)", testExecutionSummary.Allowed.FalsePositive.Count);
        AnsiConsole.MarkupLine(" [yellow]:check_mark: {0,5}[/] valid programs produced a parsing error (and in allow file)", testExecutionSummary.Allowed.FalseNegative.Count);

        var items = new (ConcurrentBag<Test262File> Tests, string Title, string Description)[]
        {
            (Tests: testExecutionSummary.Disallowed.FalsePositive, "Missing parsing error", "invalid programs did not produce a parsing error (without a corresponding entry in the allow list file)"),
            (Tests: testExecutionSummary.Disallowed.FalseNegative, "Invalid parsing error", "valid programs produced a parsing error (without a corresponding entry in the allow list file)"),
            (Tests: testExecutionSummary.Disallowed.Failure, "Unhandled error", "parsing failed with unknown error (without a corresponding entry in the allow list file)")
        };

        if (testExecutionSummary.HasProblems)
        {
            AnsiConsole.WriteLine();

            foreach (var (tests, title, label) in items)
            {
                if (tests.Count == 0)
                {
                    continue;
                }

                WriteProblemHeading(title);
                AnsiConsole.MarkupLine(" [red]:cross_mark: {0,5}[/] {1}", tests.Count, label);
                PrintDetails(tests);
            }
        }

        if (testExecutionSummary.Unrecognized.Count > 0)
        {
            WriteProblemHeading("Non-existent programs");
            AnsiConsole.MarkupLine(" :cross_mark: {0,5} non-existent programs specified in the allow list file", testExecutionSummary.Unrecognized.Count);
            PrintDetails(testExecutionSummary.Unrecognized);
        }
    }

    private static void WriteProblemHeading(string title)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule($"[red]{title}[/]") { Alignment = Justify.Left });
    }

    private static void PrintDetails<T>(IReadOnlyCollection<T> items, int maxCount = 5)
    {
        AnsiConsole.MarkupLine("  Details:");
        foreach (var item in items.Take(maxCount))
        {
            AnsiConsole.WriteLine($"\t{item}");
        }

        if (items.Count > maxCount)
        {
            AnsiConsole.WriteLine($"\t... and {items.Count - maxCount} more");
        }
    }

    private static void UpdateAllowList(
        string targetFile,
        TestExecutionSummary testExecutionSummary,
        List<string> knownFailing)
    {
        // make sure we don't keep new successful ones in list
        var success = new HashSet<string>(
            testExecutionSummary.Allowed.Failure.Concat(testExecutionSummary.Allowed.Success).Select(x => x.ToString())
        );

        var failing = testExecutionSummary.Disallowed.FalseNegative
            .Concat(testExecutionSummary.Disallowed.FalsePositive)
            .Concat(testExecutionSummary.Disallowed.Failure)
            .Select(x => x.ToString())
            .Where(x => !success.Contains(x))
            .Concat(knownFailing)
            .Distinct()
            .OrderBy(x => x)
            .ToList();

        var fileContents = new[]
            {
                "# to generate this file run:",
                "# dotnet run -c Release --project test/Esprima.Tests.Test262/Esprima.Tests.Test262.csproj -- --update-allow-list",
                ""
            }
            .Concat(failing)
            .ToList();

        File.WriteAllLines(targetFile, fileContents);

        AnsiConsole.WriteLine();
        AnsiConsole.WriteLine("Wrote {0} test cases to {1}", failing.Count, targetFile);
        AnsiConsole.WriteLine();
    }
}
