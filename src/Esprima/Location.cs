using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using static Esprima.EsprimaExceptionHelper;

namespace Esprima;

public readonly struct Location : IEquatable<Location>
{
    public readonly Position Start;
    public readonly Position End;
    public readonly string? Source;

    private static bool Validate(Position start, Position end, bool throwOnError)
    {
        if (start == default && end != default)
        {
            if (throwOnError)
            {
                throw new ArgumentOutOfRangeException(nameof(start), start, null);
            }
            return false;
        }

        if (end == default ? start != default : end < start)
        {
            if (throwOnError)
            {
                throw new ArgumentOutOfRangeException(nameof(end), end, null);
            }
            return false;
        }

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Location From(Position start, Position end, string? source = null)
    {
        Validate(start, end, throwOnError: true);
        return new Location(start, end, source);
    }

    internal Location(Position start, Position end) : this(start, end, null)
    {
    }

    internal Location(Position start, Position end, string? source)
    {
        Debug.Assert(Validate(start, end, throwOnError: false));

        Start = start;
        End = end;
        Source = source;
    }

    public Location WithPosition(Position start, Position end)
    {
        return From(start, end, Source);
    }

    public Location WithSource(string source)
    {
        return new Location(Start, End, source);
    }

    public override bool Equals(object obj)
    {
        return obj is Location other && Equals(other);
    }

    public bool Equals(in Location other)
    {
        return Start.Equals(other.Start)
               && End.Equals(other.End)
               && string.Equals(Source, other.Source, StringComparison.Ordinal);
    }

    bool IEquatable<Location>.Equals(Location other) => Equals(in other);

    public static bool operator ==(in Location left, in Location right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(in Location left, in Location right)
    {
        return !left.Equals(right);
    }

    public override int GetHashCode()
    {
        return unchecked((Start.GetHashCode() * 397)
                         ^ (End.GetHashCode() * 397)
                         ^ (Source?.GetHashCode() ?? 0));
    }

    public override string ToString()
    {
        string interval;
        if (Start.Line != End.Line)
        {
            interval = Start.Column != End.Column
                ? string.Format(CultureInfo.InvariantCulture, "[{0},{1}..{2},{3})", Start.Line, Start.Column, End.Line, End.Column)
                : string.Format(CultureInfo.InvariantCulture, "[{0}..{1},{2})", Start.Line, End.Line, Start.Column);
        }
        else
        {
            interval = Start.Column != End.Column
                ? string.Format(CultureInfo.InvariantCulture, "[{0},{1}..{2})", Start.Line, Start.Column, End.Column)
                : string.Format(CultureInfo.InvariantCulture, "[{0},{1})", Start.Line, Start.Column);
        }

        return Source is not null ? interval + ": " + Source : interval;
    }

    private static bool TryParseCore(ReadOnlySpan<char> s, bool throwIfInvalid, out Location result)
    {
        int i;
        if (s[0] != '[' || (i = s.IndexOf(')')) < 0 || ++i < 5)
        {
            goto InvalidFormat;
        }

        var source = s.Slice(i);
        if (source.Length > 0 && source[0] != ':')
        {
            goto InvalidFormat;
        }

        s = s.Slice(1, i - 2);

        int startLine, startColumn, endLine, endColumn;
        if (!ParserExtensions.TryConsumeInt(ref s, out startLine))
        {
            goto InvalidFormat;
        }

        if (s.Length >= 5 && s[0] == '.' && s[1] == '.')
        {
            startColumn = -1;
            goto EndPart;
        }
        else if (s.Length < 2 || s[0] != ',')
        {
            goto InvalidFormat;
        }
        s = s.Slice(1);

        if (!ParserExtensions.TryConsumeInt(ref s, out startColumn))
        {
            goto InvalidFormat;
        }

        if (s.Length == 0)
        {
            endLine = startLine;
            endColumn = startColumn;
            goto SourcePart;
        }
        else if (s.Length < 3 || s[0] != '.' || s[1] != '.')
        {
            goto InvalidFormat;
        }

EndPart:
        s = s.Slice(2);

        if (!ParserExtensions.TryConsumeInt(ref s, out var number))
        {
            goto InvalidFormat;
        }

        if (s.Length == 0)
        {
            endLine = startLine;
            endColumn = number;
            goto SourcePart;
        }
        else if (s.Length < 2 || s[0] != ',')
        {
            goto InvalidFormat;
        }
        endLine = number;
        s = s.Slice(1);

        if (!ParserExtensions.TryConsumeInt(ref s, out endColumn) || s.Length > 0)
        {
            goto InvalidFormat;
        }

        if (startColumn < 0)
        {
            startColumn = endColumn;
        }

SourcePart:
        if (source.Length > 0)
        {
            source = source.Slice(1).Trim();
            if (source.Length == 0)
            {
                goto InvalidFormat;
            }
        }

        var start = new Position(startLine, startColumn);
        var end = new Position(endLine, endColumn);

        if (Validate(start, end, throwIfInvalid))
        {
            result = new Location(start, end, source.Length > 0 ? source.ToString() : null);
            return true;
        }

InvalidFormat:
        result = default;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(ReadOnlySpan<char> s, out Location result) => TryParseCore(s, throwIfInvalid: false, out result);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(string s, out Location result) => TryParse(s.AsSpan(), out result);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Location Parse(ReadOnlySpan<char> s)
    {
        return TryParseCore(s, throwIfInvalid: true, out var result) ? result : ThrowFormatException<Location>("Input string was not in a correct format.");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Location Parse(string s) => Parse(s.AsSpan());

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void Deconstruct(out Position start, out Position end)
    {
        start = Start;
        end = End;
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void Deconstruct(out Position start, out Position end, out string? source)
    {
        start = Start;
        end = End;
        source = Source;
    }
}
