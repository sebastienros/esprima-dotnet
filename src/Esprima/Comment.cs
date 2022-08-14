using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Esprima;

public enum CommentType
{
    Block,
    Line
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct Comment
{
    internal Comment(
        CommentType type,
        in Range slice,
        int start,
        int end,
        in Position startPosition,
        in Position endPosition)
    {
        Type = type;
        Slice = slice;
        Start = start;
        End = end;
        StartPosition = startPosition;
        EndPosition = endPosition;
    }

    public readonly CommentType Type;

    public readonly Range Slice;

    public readonly int Start;
    public readonly int End;

    public readonly Position StartPosition;
    public readonly Position EndPosition;
}

public record class ParsedComment
{
    public ParsedComment(CommentType type, string value, in Range range, in Location location)
    {
        Type = type;

        Value = value;

        _range = range;
        _location = location;
    }

    public CommentType Type { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    public string Value { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    private readonly Range _range;
    public ref readonly Range Range { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _range; }

    private readonly Location _location;
    public ref readonly Location Location { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _location; }
}
