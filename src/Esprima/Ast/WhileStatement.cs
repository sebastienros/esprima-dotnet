using System.Collections.Generic;

namespace Esprima.Ast
{
    public class WhileStatement : Statement
    {
        public readonly Expression Test;
        public readonly Statement Body;

        public WhileStatement(Expression test, Statement body)
        {
            Type = Nodes.WhileStatement;
            Test = test;
            Body = body;
        }

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(Test, Body);
    }
}