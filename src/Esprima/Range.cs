using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Esprima;

public readonly struct Range : IEquatable<Range>
{
    public readonly int Start;
    public readonly int End;

    public int Length { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => End - Start; }

    private static bool Validate(int start, int end, bool throwOnError)
    {
        if (start < 0)
        {
            if (throwOnError)
            {
                EsprimaExceptionHelper.ThrowArgumentOutOfRangeException(nameof(start), start, Exception<ArgumentOutOfRangeException>.DefaultMessage);
            }
            return false;
        }

        if (end < start)
        {
            if (throwOnError)
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
        Validate(start, end, throwOnError: true);
        return new Range(start, end);
    }

    internal Range(int start, int end)
    {
        Debug.Assert(Validate(start, end, throwOnError: false));

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

    private static bool TryParseCore(ReadOnlySpan<char> s, bool throwIfInvalid, out Range result)
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
    public static bool TryParse(ReadOnlySpan<char> s, out Range result) => TryParseCore(s, throwIfInvalid: false, out result);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(string s, out Range result) => TryParse(s.AsSpan(), out result);

    public static Range Parse(ReadOnlySpan<char> s)
    {
        return TryParseCore(s, throwIfInvalid: true, out var result) ? result : throw new FormatException("Input string was not in a correct format.");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Range Parse(string s) => Parse(s.AsSpan());

    public void Deconstruct(out int start, out int end)
    {
        start = Start;
        end = End;
    }
}
