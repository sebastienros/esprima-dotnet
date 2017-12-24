namespace Esprima.Ast
{
    public class ContinueStatement : Statement
    {
        public readonly Identifier Label;

        public ContinueStatement(Identifier label)
        {
            Type = Nodes.ContinueStatement;
            Label = label;
        }
    }
}