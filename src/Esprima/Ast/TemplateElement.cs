﻿using System.Runtime.CompilerServices;
using Esprima.Utils;
using Microsoft.Extensions.Primitives;

namespace Esprima.Ast;

public sealed class TemplateElement : Node
{
    public TemplateElement(TemplateElementValue value, bool tail) : base(Nodes.TemplateElement)
    {
        Value = value;
        Tail = tail;
    }

    public sealed record TemplateElementValue(StringSegment? Cooked, StringSegment Raw);

    public TemplateElementValue Value { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public bool Tail { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => null;

    protected internal override object? Accept(AstVisitor visitor) => visitor.VisitTemplateElement(this);
}
