namespace Esprima.Ast
{
    public class Node : INode
    {
        public Nodes Type { get; set; }
        public Range Range { get; set; }

        public Location Location { get; set; }
    }
}
