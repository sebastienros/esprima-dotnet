namespace Esprima.Ast
{
    public class DebuggerStatement: Node,
        Statement
    {
        public DebuggerStatement()
        {
            Type = Nodes.DebuggerStatement;
        }
    }
}