namespace Esprima.Ast
{
    public class LabeledStatement : Statement
    {
        public readonly Identifier Label;
        public readonly Statement Body;

        public LabeledStatement(Identifier label, Statement body)
        {
            Type = Nodes.LabeledStatement;
            Label = label;
            Body = body;
            body.LabelSet = label;
        }
    }
}