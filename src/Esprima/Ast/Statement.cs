using System.Runtime.CompilerServices;

namespace Esprima.Ast;

public abstract class Statement : StatementListItem
{
    internal Identifier? _labelSet;

    protected Statement(Nodes type) : base(type)
    {
    }

    public Identifier? LabelSet { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _labelSet; }
}
