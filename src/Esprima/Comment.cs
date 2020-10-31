using System;

namespace Esprima
{
    public enum CommentType
    {
        Block,
        Line
    }

    public class Comment
    {
        public CommentType Type;
        public string? Value;

        public bool MultiLine;
        public int[] Slice = Array.Empty<int>();
        public int Start;
        public int End;
        public SourceLocation? Loc;
    }
}
