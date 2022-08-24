using static Esprima.EsprimaExceptionHelper;

namespace Esprima;

public sealed class ParseError
{
    public string Description { get; }
    public string? Source { get; }

    public bool IsIndexDefined => Index >= 0;
    public int Index { get; }

    public bool IsPositionDefined => Position.Line > 0;
    public Position Position { get; }
    public int LineNumber => Position.Line;
    public int Column => Position.Column;

    public ParseError(string description, string? source = null, int index = -1, in Position position = default)
    {
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Source = source;
        Index = index;
        Position = position;
    }

    public override string ToString()
    {
        return LineNumber > 0 ? $"Line {LineNumber}: {Description}" : Description;
    }
}
