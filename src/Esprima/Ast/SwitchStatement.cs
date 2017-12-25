using System.Collections.Generic;

namespace Esprima.Ast
{
    public class SwitchStatement : Statement
    {
        public Expression Discriminant { get; }
        public List<SwitchCase> Cases { get; }

        public SwitchStatement(Expression discriminant, List<SwitchCase> cases)
        {
            Type = Nodes.SwitchStatement;
            Discriminant = discriminant;
            Cases = cases;
        }
    }
}