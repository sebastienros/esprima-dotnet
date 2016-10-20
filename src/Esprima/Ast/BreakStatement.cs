namespace Esprima.Ast
{
    public class BreakStatement : Statement
    {
        public Identifier Label;

        public BreakStatement(Identifier label)
        {
            Type = Nodes.BreakStatement;
            Label = label;
        }
    }
}