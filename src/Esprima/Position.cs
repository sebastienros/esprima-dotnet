using System.Globalization;
using System.Runtime.CompilerServices;

namespace Esprima;

/// <summary>
/// Represents a source position as line number and column offset, where
/// the first line is 1 and first column is 0.
/// </summary>
/// <remarks>
/// A position where <see cref="Line"/> and <see cref="Column"/> are zero
/// is an allowed (and the default) value but considered an invalid
/// position.
/// </remarks>
public readonly struct Position : IEquatable<Position>, IComparable<Position>
{
    public readonly int Line;
    public readonly int Column;

    private static bool Validate(int line, int column, bool throwOnError = true)
    {
        if (line < 0 || line == 0 && column != 0)
        {
            if (throwOnError)
            {
                EsprimaExceptionHelper.ThrowArgumentOutOfRangeException(nameof(line), line, Exception<ArgumentOutOfRangeException>.DefaultMessage);
            }
            return false;
        }

        if (column < 0)
        {
            if (throwOnError)
            {
                EsprimaExceptionHelper.ThrowArgumentOutOfRangeException(nameof(column), column, Exception<ArgumentOutOfRangeException>.DefaultMessage);
            }
            return false;
        }

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Position From(int line, int column)
    {
        Validate(line, column);
        return new Position(line, column);
    }

    internal Position(int line, int column)
    {
#if LOCATION_ASSERTS
        Validate(line, column);
#endif

        Line = line;
        Column = column;
    }

    public override bool Equals(object obj)
    {
        return obj is Position other && Equals(other);
    }

    public bool Equals(Position other)
    {
        return Line == other.Line && Column == other.Column;
    }

    public int CompareTo(Position other)
    {
        return
            Line < other.Line ? -1 :
            Line > other.Line ? 1 :
            Column < other.Column ? -1 :
            Column > other.Column ? 1 :
            0;
    }

    public static bool operator ==(Position left, Position right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Position left, Position right)
    {
        return !left.Equals(right);
    }

    public static bool operator <(Position left, Position right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator <=(Position left, Position right)
    {
        return left.CompareTo(right) <= 0;
    }

    public static bool operator >(Position left, Position right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator >=(Position left, Position right)
    {
        return left.CompareTo(right) >= 0;
    }

    public override int GetHashCode()
    {
        return unchecked((Line * 397) ^ Column);
    }

    public override string ToString()
    {
        return Line.ToString(CultureInfo.InvariantCulture)
               + ","
               + Column.ToString(CultureInfo.InvariantCulture);
    }

    private static bool TryParseCore(ReadOnlySpan<char> s, bool throwIfInvalid, out Position result)
    {
        if (s.Length < 3)
        {
            goto InvalidFormat;
        }

        if (!ParserExtensions.TryConsumeInt(ref s, out var line))
        {
            goto InvalidFormat;
        }

        if (s.Length < 2 || s[0] != ',')
        {
            goto InvalidFormat;
        }
        s = s.Slice(1);

        if (!ParserExtensions.TryConsumeInt(ref s, out var column) || s.Length > 0)
        {
            goto InvalidFormat;
        }

        if (Validate(line, column, throwIfInvalid))
        {
            result = new Position(line, column);
            return true;
        }

InvalidFormat:
        result = default;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(ReadOnlySpan<char> s, out Position result) => TryParseCore(s, throwIfInvalid: false, out result);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(string s, out Position result) => TryParse(s.AsSpan(), out result);

    public static Position Parse(ReadOnlySpan<char> s)
    {
        return TryParseCore(s, throwIfInvalid: true, out var result) ? result : throw new FormatException("Input string was not in a correct format.");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Position Parse(string s) => Parse(s.AsSpan());

    public void Deconstruct(out int line, out int column)
    {
        line = Line;
        column = Column;
    }
}
