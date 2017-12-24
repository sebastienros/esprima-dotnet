namespace Esprima.Ast
{
    public class VariableDeclarator : Node
    {
        public readonly ArrayPatternElement Id; // BindingIdentifier | BindingPattern;
        public readonly Expression Init;

        public VariableDeclarator(ArrayPatternElement id, Expression init)
        {
            Type = Nodes.VariableDeclarator;
            Id = id;
            Init = init;
        }
    }
}