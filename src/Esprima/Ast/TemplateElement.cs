namespace Esprima.Ast
{
    public class TemplateElement : Node
    {
        public TemplateElementValue Value { get; }
        public bool Tail { get; }

        public TemplateElement(TemplateElementValue value, bool tail)
        {
            Type = Nodes.TemplateElement;
            Value = value;
            Tail = tail;
        }

        public class TemplateElementValue
        {
            public string Cooked;
            public string Raw;
        }
    }
}