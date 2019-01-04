namespace Esprima.Ast
{
    public struct Range
    {
        public readonly int Start;
        public readonly int End;

        public Range(int start, int end)
        {
            Start = start;
            End = end;
        }
    }
}