using Esprima.Utils;

namespace Esprima.Ast;

public sealed class AccessorProperty : Node
{
    /// <summary>
    /// <see cref="Expression" /> | <see cref="PrivateIdentifier" />
    /// </summary>
    public readonly Expression Key;
    public readonly Expression? Value;
    public readonly bool Computed;
    public readonly bool Static;
    public readonly NodeList<Decorator> Decorators;

    public AccessorProperty(Expression key, Expression? value, bool computed, bool isStatic, in NodeList<Decorator> decorators) : base(Nodes.AccessorProperty)
    {
        Key = key;
        Value = value;
        Computed = computed;
        Static = isStatic;
        Decorators = decorators;
    }

    public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(NodeList.Create(CreateChildNodes()));

    protected internal override object? Accept(AstVisitor visitor)
    {
        return visitor.VisitAccessorProperty(this);
    }

    public AccessorProperty UpdateWith(Expression key, Expression? value, in NodeList<Decorator> decorators)
    {
        if (key == Key && value == Value && NodeList.AreSame(decorators, Decorators))
        {
            return this;
        }

        return new AccessorProperty(key, value, Computed, Static, decorators).SetAdditionalInfo(this);
    }

    private IEnumerable<Node> CreateChildNodes()
    {
        yield return Key;

        if (Value is not null)
        {
            yield return Value;
        }

        foreach (var node in Decorators)
        {
            yield return node;
        }
    }
}
