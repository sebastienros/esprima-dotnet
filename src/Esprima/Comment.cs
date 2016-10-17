namespace Esprima
{
    public enum CommentType
    {
        Block,
        Line
    }

    public struct Comment
    {
        public CommentType Type;
        public string Value;

        public bool MultiLine;
        public int[] Slice;
        public int Start;
        public int End;
        public Loc Loc;
    }
}
