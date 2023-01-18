using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast;

public sealed class ClassDeclaration : Declaration, IClass
{
    private readonly NodeList<Decorator> _decorators;

    public ClassDeclaration(Identifier? id, Expression? superClass, ClassBody body, in NodeList<Decorator> decorators) :
        base(Nodes.ClassDeclaration)
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

    internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => enumerator.MoveNextNullableAt1_2(Decorators, Id, SuperClass, Body);

    protected internal override T Accept<T>(AstVisitor<T> visitor) => visitor.VisitClassDeclaration(this);

    public ClassDeclaration UpdateWith(Identifier? id, Expression? superClass, ClassBody body, in NodeList<Decorator> decorators)
    {
        if (id == Id && superClass == SuperClass && body == Body && NodeList.AreSame(decorators, Decorators))
        {
            return this;
        }

        return new ClassDeclaration(id, superClass, body, decorators);
    }
}
