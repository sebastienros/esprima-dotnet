using System;
using Esprima.Ast;
using Esprima.Sample.Helpers;
using Esprima.Utils;
using Esprima.Utils.Jsx;
using McMaster.Extensions.CommandLineUtils;

namespace Esprima.Sample.Commands;

internal enum JavaScriptCodeType
{
    Script,
    Module,
    Expression,
}

[Command(CommandName, Description = "Parse JS code and print resulting AST in JSON format.")]
internal sealed class ParseCommand
{
    public const string CommandName = "parse";

    private readonly IConsole _console;

    public ParseCommand(IConsole console)
    {
        _console = console;
    }

    [Option("--type", Description = "Type of the JS code to parse.")]
    public JavaScriptCodeType CodeType { get; set; }

    [Option("--jsx", Description = "Allow JSX expressions.")]
    public bool AllowJsx { get; set; }

    [Option("--comments", Description = "Also include comments.")]
    public bool Comments { get; set; }

    [Option("--tokens", Description = "Also include tokens.")]
    public bool Tokens { get; set; }

    [Option("--skip-regexp", Description = "Skip parsing of regular expressions.")]
    public bool SkipRegExp { get; set; }

    [Option("-t|--tolerant", Description = "Tolerate noncritical syntax errors.")]
    public bool Tolerant { get; set; }

    [Option("-l|--linecol", Description = "Include line and column location information.")]
    public bool IncludeLineColumn { get; set; }

    [Option("-r|--range", Description = "Include range location information.")]
    public bool IncludeRange { get; set; }

    [Argument(0, Description = "The JS code to parse. If omitted, the code will be read from the standard input.")]
    public string? Code { get; }

    private T CreateParserOptions<T>() where T : ParserOptions, new() => new T
    {
        RegExpParseMode = !SkipRegExp ? RegExpParseMode.Validate : RegExpParseMode.Skip,
        Comments = Comments,
        Tokens = Tokens,
        Tolerant = Tolerant,
    };

    private T CreateAstToJsonOptions<T>() where T : AstToJsonOptions, new() => new T
    {
        IncludeLineColumn = IncludeLineColumn,
        IncludeRange = IncludeRange,
        IncludeTokens = Tokens,
        IncludeComments = Comments
    };

    public int OnExecute()
    {
        var code = Code ?? _console.ReadString();

        var parser = AllowJsx
            ? new JsxParser(CreateParserOptions<JsxParserOptions>())
            : new JavaScriptParser(CreateParserOptions<ParserOptions>());

        Node rootNode = CodeType switch
        {
            JavaScriptCodeType.Script => parser.ParseScript(code),
            JavaScriptCodeType.Module => parser.ParseModule(code),
            JavaScriptCodeType.Expression => parser.ParseExpression(code),
            _ => throw new InvalidOperationException()
        };

        var astToJsonOptions = AllowJsx
            ? CreateAstToJsonOptions<JsxAstToJsonOptions>()
            : CreateAstToJsonOptions<AstToJsonOptions>();

        _console.WriteLine(rootNode.ToJsonString(astToJsonOptions, indent: "  "));

        return 0;
    }
}
