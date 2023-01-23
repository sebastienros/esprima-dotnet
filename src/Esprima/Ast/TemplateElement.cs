using System.Runtime.CompilerServices;

namespace Esprima.Ast;

[VisitableNode]
public sealed partial class TemplateElement : Node
{
    public TemplateElement(TemplateElementValue value, bool tail) : base(Nodes.TemplateElement)
    {
        Value = value;
        Tail = tail;
    }

    public sealed record TemplateElementValue(string? Cooked, string Raw);

    public TemplateElementValue Value { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public bool Tail { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
}
