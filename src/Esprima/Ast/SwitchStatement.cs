namespace Esprima.Ast
{
    public class SwitchStatement : Statement
    {
        public readonly Expression Discriminant;
        public readonly List<SwitchCase> Cases;

        public SwitchStatement(Expression discriminant, List<SwitchCase> cases) :
            base(Nodes.SwitchStatement)
        {
            Discriminant = discriminant;
            Cases = cases;
        }
    }
}