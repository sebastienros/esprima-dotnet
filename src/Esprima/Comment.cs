namespace Esprima
{
    public struct Comment
    {
        public string Type;
        public string Value;

        public bool MultiLine;
        public int[,] Slice;
        public int Start;
        public int End;
        public Loc Loc;
    }
}
