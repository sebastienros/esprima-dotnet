namespace Esprima.Ast
{
    public class Node : INode
    {
        public Nodes Type { get; }
        public Range Range { get; set; }

        public Location Location { get; set; }

        protected Node(Nodes type) =>
            Type = type;
    }
}
