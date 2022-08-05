using System.Runtime.CompilerServices;

namespace Esprima;

public enum CommentType
{
    Block,
    Line
}

public class Comment
{
    public CommentType Type;

    public string? Value;
    public Range Slice;

    public int Start;
    public int End;

    public Range Range
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new Range(Start, End);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => value.Deconstruct(out Start, out End);
    }
    public Location Location;
}
