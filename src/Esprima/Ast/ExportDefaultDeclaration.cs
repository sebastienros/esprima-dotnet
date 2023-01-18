using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast;

public sealed class ExportDefaultDeclaration : ExportDeclaration
{
    public ExportDefaultDeclaration(StatementListItem declaration) : base(Nodes.ExportDefaultDeclaration)
    {
        Declaration = declaration;
    }

    /// <remarks>
    /// <see cref="Expression"/> | <see cref="ClassDeclaration"/> | <see cref="FunctionDeclaration"/>
    /// </remarks>
    public StatementListItem Declaration { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Declaration);

    protected internal override T Accept<T>(AstVisitor<T> visitor) => visitor.VisitExportDefaultDeclaration(this);

    public ExportDefaultDeclaration UpdateWith(StatementListItem declaration)
    {
        if (declaration == Declaration)
        {
            return this;
        }

        return new ExportDefaultDeclaration(declaration);
    }
}
