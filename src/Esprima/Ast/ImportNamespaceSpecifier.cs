using Esprima.Utils;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Local) })]
public sealed class ImportNamespaceSpecifier : ImportDeclarationSpecifier
{
    public ImportNamespaceSpecifier(Identifier local) : base(local, Nodes.ImportNamespaceSpecifier)
    {
    }

    internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Local);

    protected internal override object? Accept(AstVisitor visitor) => visitor.VisitImportNamespaceSpecifier(this);

    public ImportNamespaceSpecifier UpdateWith(Identifier local)
    {
        if (local == Local)
        {
            return this;
        }

        return new ImportNamespaceSpecifier(local);
    }
}
