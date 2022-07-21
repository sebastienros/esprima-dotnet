using Esprima.Utils;

namespace Esprima.Ast;

public sealed class ImportDefaultSpecifier : ImportDeclarationSpecifier
{
    public ImportDefaultSpecifier(Identifier local) : base(local, Nodes.ImportDefaultSpecifier)
    {
    }

    internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Local);

    protected internal override object? Accept(AstVisitor visitor) => visitor.VisitImportDefaultSpecifier(this);

    public ImportDefaultSpecifier UpdateWith(Identifier local)
    {
        if (local == Local)
        {
            return this;
        }

        return new ImportDefaultSpecifier(local);
    }
}
