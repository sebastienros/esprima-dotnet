using System.Runtime.CompilerServices;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Body) })]
public sealed partial class ClassBody : Node
{
    private readonly NodeList<ClassElement> _body;

    public ClassBody(in NodeList<ClassElement> body) : base(Nodes.ClassBody)
    {
        _body = body;
    }

    /// <remarks>
    /// <see cref="MethodDefinition"/> | <see cref="PropertyDefinition"/> | <see cref="StaticBlock"/>
    /// </remarks>
    public ref readonly NodeList<ClassElement> Body { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _body; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ClassBody Rewrite(in NodeList<ClassElement> body)
    {
        return new ClassBody(body);
    }
}
