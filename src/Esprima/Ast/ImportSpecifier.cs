using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast;

public sealed class ImportSpecifier : ImportDeclarationSpecifier
{
    public ImportSpecifier(Identifier local, Expression imported) : base(local, Nodes.ImportSpecifier)
    {
        Imported = imported;
    }

    /// <remarks>
    /// <see cref="Identifier"/> | <see cref="Literal"/> (string)
    /// </remarks>
    public Expression Imported { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => enumerator.MoveNextImportSpecifier(Imported, Local);

    protected internal override T Accept<T>(AstVisitor<T> visitor) => visitor.VisitImportSpecifier(this);

    public ImportSpecifier UpdateWith(Expression imported, Identifier local)
    {
        if (imported == Imported && local == Local)
        {
            return this;
        }

        return new ImportSpecifier(local, imported);
    }
}
