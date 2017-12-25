namespace Esprima.Ast
{
    public class VariableDeclarator : Node
    {
        public ArrayPatternElement Id { get; } // BindingIdentifier | BindingPattern;
        public Expression Init { get; }

        public VariableDeclarator(ArrayPatternElement id, Expression init)
        {
            Type = Nodes.VariableDeclarator;
            Id = id;
            Init = init;
        }
    }
}