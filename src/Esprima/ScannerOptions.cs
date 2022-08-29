namespace Esprima;

internal interface IScannerOptions
{
    bool Comments { get; }
    bool Tolerant { get; }
    ErrorHandler ErrorHandler { get; }
    bool AdaptRegexp { get; }
    TimeSpan RegexTimeout { get; }
}

/// <summary>
/// Scanner options.
/// </summary>
public record class ScannerOptions : IScannerOptions
{
    public static readonly ScannerOptions Default = new();

    /// <summary>
    /// Gets or sets whether the comments are collected, defaults to <see langword="false"/>.
    /// </summary>
    public bool Comments { get; init; }

    /// <summary>
    /// Gets or sets whether the scanner is tolerant to errors, defaults to <see langword="true"/>.
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
}
