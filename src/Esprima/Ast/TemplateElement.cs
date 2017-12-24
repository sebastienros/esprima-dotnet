namespace Esprima.Ast
{
    public class TemplateElement : Node
    {
        public readonly TemplateElementValue Value;
        public readonly bool Tail;

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