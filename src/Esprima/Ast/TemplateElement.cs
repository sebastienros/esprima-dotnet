using System.Collections.Generic;

namespace Esprima.Ast
{
    public class TemplateElement : Node
    {
        public readonly TemplateElementValue Value;
        public readonly bool Tail;

        public TemplateElement(TemplateElementValue value, bool tail) :
            base(Nodes.TemplateElement)
        {
            Value = value;
            Tail = tail;
        }

        public class TemplateElementValue
        {
            public string Cooked;
            public string Raw;
        }

        public override IEnumerable<Node> ChildNodes => ZeroChildNodes;
    }
}