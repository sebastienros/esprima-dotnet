using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Esprima.Sample.Helpers;
using McMaster.Extensions.CommandLineUtils;

namespace Esprima.Sample.Commands;

[Command(CommandName, Description = "Tokenize JS code and print collected tokens in JSON format.")]
internal sealed class TokenizeCommand
{
    public const string CommandName = "tokenize";

    private readonly IConsole _console;

    public TokenizeCommand(IConsole console)
    {
        _console = console;
    }

    [Option("--comments", Description = "Also include comments.")]
    public bool Comments { get; set; }

    [Option("-t|--tolerant", Description = "Tolerate noncritical syntax errors.")]
    public bool Tolerant { get; set; }

    [Argument(0, Description = "The JS code to tokenize. If omitted, the code will be read from the standard input.")]
    public string? Code { get; }

    public int OnExecute()
    {
        var code = Code ?? _console.ReadString();

        var scanner = new Scanner(code, new ScannerOptions
        {
            AdaptRegexp = false,
            Comments = Comments,
            Tolerant = Tolerant,
        });

        var tokensAndComments = new List<object>();
        Token token;
        do
        {
            var comments = scanner.ScanComments();
            if (Comments)
            {
                foreach (var comment in comments)
                {
                    tokensAndComments.Add(comment);
                }
            }

            token = scanner.Lex();
            tokensAndComments.Add(token);
        }
        while (token.Type != TokenType.EOF);

        var serializerOptions = new JsonSerializerOptions
        {
            IncludeFields = true,
            WriteIndented = true,
            Converters =
            {
                new JsonStringEnumConverter()
            }
        };

        _console.WriteLine(JsonSerializer.Serialize(tokensAndComments, serializerOptions));

        return 0;
    }
}
