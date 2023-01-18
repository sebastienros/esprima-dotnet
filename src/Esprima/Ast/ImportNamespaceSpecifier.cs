using Esprima.Utils;

namespace Esprima.Ast;

public sealed class ImportNamespaceSpecifier : ImportDeclarationSpecifier
{
    public ImportNamespaceSpecifier(Identifier local) : base(local, Nodes.ImportNamespaceSpecifier)
    {
    }

    internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Local);

    protected internal override T Accept<T>(AstVisitor<T> visitor) => visitor.VisitImportNamespaceSpecifier(this);

    public ImportNamespaceSpecifier UpdateWith(Identifier local)
    {
        if (local == Local)
        {
            return this;
        }

        return new ImportNamespaceSpecifier(local);
    }
}
