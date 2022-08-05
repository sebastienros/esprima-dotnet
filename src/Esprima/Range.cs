using System.Globalization;

namespace Esprima;

public readonly struct Range : IEquatable<Range>
{
    public readonly int Start;
    public readonly int End;

    public Range(int start, int end)
    {
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

    public override int GetHashCode()
    {
        return unchecked((Start * 397) ^ End);
    }

    public override string ToString()
    {
        return string.Format(CultureInfo.InvariantCulture, "[{0}..{1})", Start, End);
    }

    public static bool operator ==(Range left, Range right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Range left, Range right)
    {
        return !left.Equals(right);
    }

    public void Deconstruct(out int start, out int end)
    {
        start = Start;
        end = End;
    }
}
