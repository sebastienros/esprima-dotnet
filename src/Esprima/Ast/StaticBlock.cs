namespace Esprima.Ast
{
    public sealed class StaticBlock : BlockStatement
    {
        public StaticBlock(in NodeList<Statement> body) : base(body, Nodes.StaticBlock)
        {
        }
    }
}
