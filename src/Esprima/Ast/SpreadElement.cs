namespace Esprima.Ast
{
    public sealed class SpreadElement : Expression
    {
        public readonly Expression Argument;

        public SpreadElement(Expression argument) : base(Nodes.SpreadElement)
        {
            Argument = argument;
        }

        public override NodeCollection ChildNodes => new NodeCollection(Argument);
    }
}