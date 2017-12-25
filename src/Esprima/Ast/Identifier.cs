namespace Esprima.Ast
{
    public class Identifier : Node,
        BindingIdentifier,
        PropertyKey,
        Expression
    {
        public string Name { get; }

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