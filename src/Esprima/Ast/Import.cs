namespace Esprima.Ast
{
    public sealed class Import : Expression
    {
        public Import() : base(Nodes.Import)
        {
        }

        public override NodeCollection ChildNodes => NodeCollection.Empty;
    }
}
