namespace Esprima.Ast
{
    public class Node : INode
    {
        private Location _location;

        public Nodes Type { get; set; }
        public Range Range { get; set; }

        public Location Location
        {
            get => _location  = _location ?? new Location();
            set => _location = value;
        }
    }
}
