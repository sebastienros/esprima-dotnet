namespace Esprima.Ast
{
    public sealed class Import : Expression
    {
        public readonly Expression Source;

        public Import(Expression source) : base(Nodes.Import)
        {
            Source = source;
        }

        public override NodeCollection ChildNodes => new NodeCollection(Source);
    }
}
