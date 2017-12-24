namespace Esprima.Ast
{
    public class Identifier : Node,
        BindingIdentifier,
        PropertyKey,
        Expression
    {
        public readonly string Name;

        public Identifier(string name)
        {
            Type = Nodes.Identifier;
            Name = name;
        }

        string PropertyKey.GetKey()
        {
            return Name;
        }
    }
}