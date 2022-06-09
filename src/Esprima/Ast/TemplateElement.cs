using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class TemplateElement : Node
    {
        public TemplateElement(TemplateElementValue value, bool tail) : base(Nodes.TemplateElement)
        {
            Value = value;
            Tail = tail;
        }

        public sealed record TemplateElementValue(string? Cooked, string Raw);

        public TemplateElementValue Value { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        public bool Tail { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

        public override NodeCollection ChildNodes => NodeCollection.Empty;

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitTemplateElement(this);
        }
    }
}
