using System.Collections.Generic;

namespace Esprima.Ast
{
    public sealed class CatchClause : Statement
    {
        public readonly ArrayPatternElement Param; // BindingIdentifier | BindingPattern;
        public readonly BlockStatement Body;

        public CatchClause(ArrayPatternElement param, BlockStatement body) :
            base(Nodes.CatchClause)
        {
            Param = param;
            Body = body;
        }

        public override IEnumerable<Node> ChildNodes => ChildNodeYielder.Yield(Param, Body);
    }
}