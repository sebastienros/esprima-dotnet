using System.Text.RegularExpressions;

namespace Esprima;

/// <summary>
/// Specifies how the scanner should parse regular expressions.
/// </summary>
public enum RegExpParseMode
{
    /// <summary>
    /// Scan regular expressions without checking that they are syntactically correct.
    /// </summary>
    Skip,
    /// <summary>
    /// Scan regular expressions and check that they are syntactically correct (throw <see cref="ParserException"/> if an invalid regular expression is encountered)
    /// but don't attempt to convert them to an equivalent <see cref="Regex"/>.
    /// </summary>
    Validate,
    /// <summary>
    /// Scan regular expressions, check that they are syntactically correct (throw <see cref="ParserException"/> if an invalid regular expression is encountered)
    /// and attempt to convert them to an equivalent <see cref="Regex"/> without the <see cref="RegexOptions.Compiled"/> option.
    /// </summary>
    /// <remarks>
    /// In the case of a valid regular expression for which an equivalent <see cref="Regex"/> cannot be constructed, either <see cref="ParserException"/> is thrown
    /// or a <see cref="Token"/> is created with the <see cref="Token.Value"/> property set to <see langword="null"/>, depending on the <see cref="ScannerOptions.Tolerant"/> option.
    /// </remarks>
    AdaptToInterpreted,
    /// <summary>
    /// Scan regular expressions, check that they are syntactically correct (throw <see cref="ParserException"/> if an invalid regular expression is encountered)
    /// and attempt to convert them to an equivalent <see cref="Regex"/> with the <see cref="RegexOptions.Compiled"/> option.
    /// </summary>
    /// <remarks>
    /// In the case of a valid regular expression for which an equivalent <see cref="Regex"/> cannot be constructed, either <see cref="ParserException"/> is thrown
    /// or a <see cref="Token"/> is created with the <see cref="Token.Value"/> property set to <see langword="null"/>, depending on the <see cref="ScannerOptions.Tolerant"/> option.
    /// </remarks>
    AdaptToCompiled,
}

/// <summary>
/// Scanner options.
/// </summary>
public record class ScannerOptions
{
    public static readonly ScannerOptions Default = new();

    internal bool _comments;
    internal bool _tolerant = true;
    internal ErrorHandler _errorHandler = ErrorHandler.Default;
    internal RegExpParseMode _regExpParseMode = RegExpParseMode.AdaptToInterpreted;
    internal TimeSpan _regexTimeout = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Gets or sets whether the comments are collected, defaults to <see langword="false"/>.
    /// </summary>
    public bool Comments { get => _comments; init => _comments = value; }

    /// <summary>
    /// Gets or sets whether the scanner is tolerant to errors, defaults to <see langword="true"/>.
    /// </summary>
    public bool Tolerant { get => _tolerant; init => _tolerant = value; }

    /// <summary>
    /// Gets or sets the <see cref="ErrorHandler"/> to use, defaults to <see cref="ErrorHandler.Default"/>.
    /// </summary>
    public ErrorHandler ErrorHandler { get => _errorHandler; init => _errorHandler = value; }

    /// <summary>
    /// Gets or sets whether the Regular Expression syntax should be converted to a .NET compatible one, defaults to <see langword="true"/>.
    /// </summary>
    [Obsolete($"This property is planned to be removed from the next stable version. Please use the {nameof(RegExpParseMode)} property instead.")]
    public bool AdaptRegexp
    {
        get => _regExpParseMode is RegExpParseMode.AdaptToInterpreted or RegExpParseMode.AdaptToCompiled;
        init => _regExpParseMode = value ? RegExpParseMode.AdaptToInterpreted : RegExpParseMode.Skip;
    }

    /// <summary>
    /// Gets or sets how regular expressions should be parsed, defaults to <see cref="RegExpParseMode.AdaptToInterpreted"/>.
    /// </summary>
    public RegExpParseMode RegExpParseMode { get => _regExpParseMode; init => _regExpParseMode = value; }

    /// <summary>
    /// Default timeout for created <see cref="Regex"/> instances, defaults to 10 seconds.
    /// </summary>
    public TimeSpan RegexTimeout { get => _regexTimeout; init => _regexTimeout = value; }
}
