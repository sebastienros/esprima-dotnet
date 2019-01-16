namespace Esprima.Ast
{
    public class Identifier : Node,
        BindingIdentifier,
        Expression
    {
        public readonly string Name;

        public Identifier(string name) :
            base(Nodes.Identifier)
        {
            Name = name;
        }
    }
}