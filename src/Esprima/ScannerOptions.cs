namespace Esprima;

/// <summary>
/// Scanner options.
/// </summary>
public record class ScannerOptions
{
    public static readonly ScannerOptions Default = new();

    internal bool _comments;
    internal bool _tolerant = true;
    internal ErrorHandler _errorHandler = ErrorHandler.Default;
    internal bool _adaptRegexp = true;
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
    public bool AdaptRegexp { get => _adaptRegexp; init => _adaptRegexp = value; }

    /// <summary>
    /// Default timeout for created regexes, defaults to 10 seconds.
    /// </summary>
    public TimeSpan RegexTimeout { get => _regexTimeout; init => _regexTimeout = value; }
}
