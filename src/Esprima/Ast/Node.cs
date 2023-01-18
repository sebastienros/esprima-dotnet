using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast;

public abstract class Node : SyntaxElement
{
    private protected const int TokensPropertyIndex = 1;
    private protected const int CommentsPropertyIndex = 2;

    protected Node(Nodes type)
    {
        Type = type;
    }

    public Nodes Type { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    public ChildNodes ChildNodes => new ChildNodes(this);

    /// <remarks>
    /// Inheritors who extend the AST with custom node types should override this method and provide an actual implementation.
    /// </remarks>
    protected internal virtual IEnumerator<Node>? GetChildNodes() => null;

    internal virtual Node? NextChildNode(ref ChildNodes.Enumerator enumerator) =>
        throw new NotImplementedException($"User-defined node types should override the {nameof(GetChildNodes)} method and provide an actual implementation.");

    protected internal abstract T Accept<T>(AstVisitor<T> visitor);

    /// <summary>
    /// Dispatches the visitation of the current node to <see cref="AstVisitor.VisitExtension(Node)"/>.
    /// </summary>
    /// <remarks>
    /// When defining custom node types, inheritors can use this method to implement the abstract <see cref="Accept{T}"/> method.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected T AcceptAsExtension<T>(AstVisitor<T> visitor)
    {
        return visitor.VisitExtension(this);
    }

    private static readonly AstToJavaScriptOptions s_toStringOptions = AstToJavaScriptOptions.Default with { IgnoreExtensions = true };
    public override string ToString() => this.ToJavaScriptString(KnRJavaScriptTextFormatterOptions.Default, s_toStringOptions);

    private protected override string GetDebuggerDisplay()
    {
        return $"/*{Type}*/  {this}";
    }
}
