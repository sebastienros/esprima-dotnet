namespace Esprima.Ast
{
    public class CatchClause : Statement
    {
        public readonly ArrayPatternElement Param; // BindingIdentifier | BindingPattern;
        public readonly BlockStatement Body;

        public CatchClause(ArrayPatternElement param, BlockStatement body) :
            base(Nodes.CatchClause)
        {
            Param = param;
            Body = body;
        }
    }
}