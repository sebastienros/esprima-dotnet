namespace Esprima.Ast
{
    public sealed class Super : Expression
    {
        public Super() : base(Nodes.Super)
        {
        }

        public override NodeCollection ChildNodes => NodeCollection.Empty;
    }
}
