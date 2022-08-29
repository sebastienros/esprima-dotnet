using Esprima.Ast;

namespace Esprima;

/// <summary>
/// Parser options.
/// </summary>
public record class ParserOptions : IScannerOptions
{
    public static readonly ParserOptions Default = new();

    internal readonly ScannerOptions _scannerOptions = new();

    /// <summary>
    /// Gets or sets whether the tokens are included in the parsed tree, defaults to <see langword="false"/>.
    /// </summary>
    public bool Tokens { get; init; }

    /// <summary>
    /// Gets or sets whether the comments are included in the parsed tree, defaults to <see langword="false"/>.
    /// </summary>
    public bool Comments { get; init; }

    /// <summary>
    /// Gets or sets whether the parser is tolerant to errors, defaults to <see langword="true"/>.
    /// </summary>
    public bool Tolerant { get; init; } = true;

    /// <summary>
    /// Gets or sets the <see cref="ErrorHandler"/> to use, defaults to <see cref="ErrorHandler.Default"/>.
    /// </summary>
    public ErrorHandler ErrorHandler { get; init; } = ErrorHandler.Default;

    /// <summary>
    /// Gets or sets whether the Regular Expression syntax should be converted to a .NET compatible one, defaults to <see langword="true"/>.
    /// </summary>
    public bool AdaptRegexp { get; init; } = true;

    /// <summary>
    /// Default timeout for created regexes, defaults to 10 seconds.
    /// </summary>
    public TimeSpan RegexTimeout { get; init; } = TimeSpan.FromSeconds(10);

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
    ///         child.SetAdditionalData("Parent", node);
    ///     }
    /// };
    /// </code>
    /// </remarks>
    public Action<Node>? OnNodeCreated { get; init; }
}
