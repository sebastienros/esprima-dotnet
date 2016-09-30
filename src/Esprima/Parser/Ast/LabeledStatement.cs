namespace Esprima.Ast
{
    public class LabeledStatement : Node, Statement
    {
        public Identifier Label;
        public Statement Body;

        public LabeledStatement(Identifier label, Statement body)
        {
            Type = Nodes.LabeledStatement;
            Label = label;
            Body = body;
        }

    }
}