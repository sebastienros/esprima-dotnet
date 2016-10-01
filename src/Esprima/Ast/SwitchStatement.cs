using System.Collections.Generic;

namespace Esprima.Ast
{
    public class SwitchStatement : Node,
        Statement
    {
        public Expression Discriminant;
        public IEnumerable<SwitchCase> Cases;

        public SwitchStatement(Expression discriminant, IEnumerable<SwitchCase> cases)
        {
            Type = Nodes.SwitchStatement;
            Discriminant = discriminant;
            Cases = cases;
        }
    }
}