namespace Esprima.Ast
{
    public abstract class Node
    {
        protected Node(Nodes type)
        {
            Type = type;
        }

        public readonly Nodes Type;
        public Range Range;    
        public Location Location;

        public abstract NodeCollection ChildNodes { get; }
    }
}
