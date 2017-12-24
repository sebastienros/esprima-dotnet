namespace Esprima.Ast
{
    public class BreakStatement : Statement
    {
        public readonly Identifier Label;

        public BreakStatement(Identifier label)
        {
            Type = Nodes.BreakStatement;
            Label = label;
        }
    }
}