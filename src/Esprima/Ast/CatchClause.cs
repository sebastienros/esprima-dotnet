using System.Collections.Generic;

namespace Esprima.Ast
{
    public class CatchClause : Statement
    {
        public readonly ArrayPatternElement Param; // BindingIdentifier | BindingPattern;
        public readonly BlockStatement Body;

        public CatchClause(ArrayPatternElement param, BlockStatement body)
        {
            Type = Nodes.CatchClause;
            Param = param;
            Body = body;
        }

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(Param, Body);
    }
}