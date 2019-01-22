namespace Esprima.Ast
{
    public class CatchClause : Statement
    {
        public readonly IArrayPatternElement Param; // BindingIdentifier | BindingPattern;
        public readonly BlockStatement Body;

        public CatchClause(IArrayPatternElement param, BlockStatement body) :
            base(Nodes.CatchClause)
        {
            Param = param;
            Body = body;
        }
    }
}