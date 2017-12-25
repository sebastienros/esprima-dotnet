namespace Esprima.Ast
{
    public class CatchClause : Statement
    {
        public ArrayPatternElement Param { get; } // BindingIdentifier | BindingPattern;
        public BlockStatement Body { get; }

        public CatchClause(ArrayPatternElement param, BlockStatement body)
        {
            Type = Nodes.CatchClause;
            Param = param;
            Body = body;
        }
    }
}