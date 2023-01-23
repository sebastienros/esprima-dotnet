using System.Runtime.CompilerServices;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Decorators), nameof(Id), nameof(SuperClass), nameof(Body) })]
public sealed partial class ClassExpression : Expression, IClass
{
    private readonly NodeList<Decorator> _decorators;

    public ClassExpression(
        Identifier? id,
        Expression? superClass,
        ClassBody body,
        in NodeList<Decorator> decorators) : base(Nodes.ClassExpression)
    {
        Id = id;
        SuperClass = superClass;
        Body = body;
        _decorators = decorators;
    }

    public Identifier? Id { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public Expression? SuperClass { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public ClassBody Body { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public ref readonly NodeList<Decorator> Decorators { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _decorators; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ClassExpression Rewrite(in NodeList<Decorator> decorators, Identifier? id, Expression? superClass, ClassBody body)
    {
        return new ClassExpression(id, superClass, body, decorators);
    }
}
