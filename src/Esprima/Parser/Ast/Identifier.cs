namespace Esprima.Ast
{
    public class Identifier : Node,
        BindingIdentifier,
        PropertyKey,
        Expression
    {
        public string Name;

        public Identifier(string name)
        {
            Type = Nodes.Identifier;
            Name = name;
        }
    }
}