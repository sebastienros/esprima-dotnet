using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast;

public sealed class CatchClause : Node
{
    public CatchClause(Node? param, BlockStatement body) :
        base(Nodes.CatchClause)
    {
        Param = param;
        Body = body;
    }

    /// <remarks>
    /// <see cref="Identifier"/> | <see cref="BindingPattern"/>
    /// </remarks>
    public Node? Param { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public BlockStatement Body { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => enumerator.MoveNextNullableAt0(Param, Body);

    protected internal override T Accept<T>(AstVisitor<T> visitor) => visitor.VisitCatchClause(this);

    public CatchClause UpdateWith(Node? param, BlockStatement body)
    {
        if (param == Param && body == Body)
        {
            return this;
        }

        return new CatchClause(param, body);
    }
}
