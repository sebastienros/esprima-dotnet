namespace Esprima.Ast
{
    public class BreakStatement : Statement
    {
        public readonly Identifier Label;

        public BreakStatement(Identifier label) :
            base(Nodes.BreakStatement)
        {
            Label = label;
        }
    }
}