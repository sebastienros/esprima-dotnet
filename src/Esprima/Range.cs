using System.Globalization;
using System.Runtime.CompilerServices;

namespace Esprima;

public readonly struct Range : IEquatable<Range>
{
    public readonly int Start;
    public readonly int End;

    private static bool Validate(int start, int end, bool @throw = true)
    {
        if (start < 0)
        {
            if (@throw)
            {
                EsprimaExceptionHelper.ThrowArgumentOutOfRangeException(nameof(start), start, Exception<ArgumentOutOfRangeException>.DefaultMessage);
            }
            return false;
        }

        if (end < start)
        {
            if (@throw)
            {
                EsprimaExceptionHelper.ThrowArgumentOutOfRangeException(nameof(end), end, Exception<ArgumentOutOfRangeException>.DefaultMessage);
            }
            return false;
        }

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Range From(int start, int end)
    {
        Validate(start, end);
        return new Range(start, end);
    }

    internal Range(int start, int end)
    {
#if LOCATION_ASSERTS
        Validate(start, end);
#endif

        Start = start;
        End = end;
    }

    public bool Equals(Range other)
    {
        return Start == other.Start && End == other.End;
    }

    public override bool Equals(object obj)
    {
        return obj is Range other && Equals(other);
    }

    public static bool operator ==(Range left, Range right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Range left, Range right)
    {
        return !left.Equals(right);
    }

    public override int GetHashCode()
    {
        return unchecked((Start * 397) ^ End);
    }

    public override string ToString()
    {
        return Start != End
            ? string.Format(CultureInfo.InvariantCulture, "[{0}..{1})", Start, End)
            : string.Format(CultureInfo.InvariantCulture, "[{0})", Start);
    }

    private static bool TryParse(ReadOnlySpan<char> s, bool throwIfInvalid, out Range result)
    {
        if (s.Length < 3 || s[0] != '[' || s[s.Length - 1] != ')')
        {
            goto InvalidFormat;
        }
        s = s.Slice(1, s.Length - 2);

        int start, end;
        if (!ParserExtensions.TryConsumeInt(ref s, out start))
        {
            goto InvalidFormat;
        }

        if (s.Length == 0)
        {
            end = start;
            goto Success;
        }
        else if (s.Length < 3 || s[0] != '.' || s[1] != '.')
        {
            goto InvalidFormat;
        }
        s = s.Slice(2);

        if (!ParserExtensions.TryConsumeInt(ref s, out end) || s.Length > 0)
        {
            goto InvalidFormat;
        }

Success:
        if (Validate(start, end, throwIfInvalid))
        {
            result = From(start, end);
            return true;
        }

InvalidFormat:
        result = default;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(ReadOnlySpan<char> s, out Range result) => TryParse(s, throwIfInvalid: false, out result);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(string s, out Range result) => TryParse(s.AsSpan(), out result);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Range Parse(ReadOnlySpan<char> s)
    {
        return TryParse(s, throwIfInvalid: true, out var result) ? result : throw new FormatException("Input string was not in a correct format.");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Range Parse(string s) => Parse(s.AsSpan());

    public void Deconstruct(out int start, out int end)
    {
        start = Start;
        end = End;
    }
}
