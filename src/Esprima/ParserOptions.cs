using System.Text.RegularExpressions;
using Esprima.Ast;

namespace Esprima;

/// <summary>
/// Parser options.
/// </summary>
public record class ParserOptions
{
    public static readonly ParserOptions Default = new();

    private readonly ScannerOptions _scannerOptions = new();

    public ScannerOptions GetScannerOptions() => _scannerOptions;

    /// <summary>
    /// Gets or sets whether the tokens are included in the parsed tree, defaults to <see langword="false"/>.
    /// </summary>
    public bool Tokens { get; init; }

    /// <summary>
    /// Gets or sets whether the comments are included in the parsed tree, defaults to <see langword="false"/>.
    /// </summary>
    public bool Comments { get => _scannerOptions._comments; init => _scannerOptions._comments = value; }

    /// <summary>
    /// Gets or sets whether the parser is tolerant to errors, defaults to <see langword="true"/>.
    /// </summary>
    public bool Tolerant { get => _scannerOptions._tolerant; init => _scannerOptions._tolerant = value; }

    /// <summary>
    /// Gets or sets whether the parser allows return statement to be used outside of functions, defaults to <see langword="false"/>.
    /// </summary>
    public bool AllowReturnOutsideFunction { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="ErrorHandler"/> to use, defaults to <see cref="ErrorHandler.Default"/>.
    /// </summary>
    public ErrorHandler ErrorHandler { get => _scannerOptions._errorHandler; init => _scannerOptions._errorHandler = value; }

    /// <summary>
    /// Gets or sets whether the Regular Expression syntax should be converted to a .NET compatible one, defaults to <see langword="true"/>.
    /// </summary>
    [Obsolete($"This property is planned to be removed from the next stable version. Please use the {nameof(RegExpParseMode)} property instead.")]
    public bool AdaptRegexp
    {
        get => _scannerOptions.AdaptRegexp;
        init => _scannerOptions._regExpParseMode = value ? RegExpParseMode.AdaptToInterpreted : RegExpParseMode.Skip;
    }

    /// <summary>
    /// Gets or sets how regular expressions should be parsed, defaults to <see cref="RegExpParseMode.AdaptToInterpreted"/>.
    /// </summary>
    public RegExpParseMode RegExpParseMode { get => _scannerOptions._regExpParseMode; init => _scannerOptions._regExpParseMode = value; }

    /// <summary>
    /// Default timeout for created <see cref="Regex"/> instances, defaults to 10 seconds.
    /// </summary>
    public TimeSpan RegexTimeout { get => _scannerOptions._regexTimeout; init => _scannerOptions._regexTimeout = value; }

    /// <summary>
    /// The maximum depth of assignments allowed, defaults to 200.
    /// </summary>
    public int MaxAssignmentDepth { get; init; } = 200;

    /// <summary>
    /// Action to execute on each parsed node.
    /// </summary>
    /// <remarks>
    /// This callback allows you to make changes to the nodes created by the parser.
    /// E.g. you can use it to store a reference to the parent node for later use:
    /// <code>
    /// options.OnNodeCreated = node =>
    /// {
    ///     foreach (var child in node.ChildNodes)
    ///     {
    ///         child.AssociatedData = node;
    ///     }
    /// };
    /// </code>
    /// </remarks>
    public Action<Node>? OnNodeCreated { get; init; }
}
