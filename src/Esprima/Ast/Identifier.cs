namespace Esprima.Ast
{
    public sealed class Identifier : Expression
    {
        public readonly string? Name;

        public Identifier(string? name) : base(Nodes.Identifier)
        {
            Name = name;
        }

        public override NodeCollection ChildNodes => NodeCollection.Empty;
    }
}