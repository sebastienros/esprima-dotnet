namespace Esprima.Ast
{
    public class VariableDeclarator : Node
    {
        public ArrayPatternElement Id; // BindingIdentifier | BindingPattern;
        public Expression Init;

        public VariableDeclarator(ArrayPatternElement id, Expression init)
        {
            Type = Nodes.VariableDeclarator;
            Id = id;
            Init = init;
        }
    }
}