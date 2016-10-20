namespace Esprima.Ast
{
    public class LabeledStatement : Statement
    {
        public Identifier Label;
        public Statement Body;

        public LabeledStatement(Identifier label, Statement body)
        {
            Type = Nodes.LabeledStatement;
            Label = label;
            Body = body;
            body.LabelSet = label;
        }

    }
}