namespace Esprima.Ast
{
    public sealed class TemplateElement : Node
    {
        public readonly TemplateElementValue Value;
        public readonly bool Tail;

        public TemplateElement(TemplateElementValue value, bool tail) : base(Nodes.TemplateElement)
        {
            Value = value;
            Tail = tail;
        }

        public sealed class TemplateElementValue
        {
            public string Cooked = "";
            public string Raw = "";
        }

        public override NodeCollection ChildNodes => NodeCollection.Empty;
    }
}