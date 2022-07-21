using System.Diagnostics;
using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast;

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(), nq}}")]
public abstract class Node
{
    protected Node(Nodes type)
    {
        Type = type;
    }

    public Nodes Type { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    public Range Range;
    public Location Location;

    /// <summary>
    /// A general purpose field for associating additional, user-defined data or context with <see cref="Node"/>.
    /// </summary>
    public object? Data;

    public ChildNodes ChildNodes => new ChildNodes(this);

    /// <remarks>
    /// Inheritors who extend the AST with custom node types should override this method and provide an actual implementation.
    /// </remarks>
    protected internal virtual IEnumerator<Node>? GetChildNodes() => null;

    internal virtual Node? NextChildNode(ref ChildNodes.Enumerator enumerator) =>
        throw new NotImplementedException($"User-defined node types should override the {nameof(GetChildNodes)} method and provide an actual implementation.");

    protected internal abstract object? Accept(AstVisitor visitor);

    /// <summary>
    /// Dispatches the visitation of the current node to <see cref="AstVisitor.VisitExtension(Node)"/>.
    /// </summary>
    /// <remarks>
    /// When defining custom node types, inheritors can use this method to implement the abstract <see cref="Accept(AstVisitor)"/> method.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected object? AcceptAsExtension(AstVisitor visitor)
    {
        return visitor.VisitExtension(this);
    }

    public override string ToString() => this.ToJavascriptString(beautify: true);

    private string GetDebuggerDisplay()
    {
        return $"/*{Type}*/  {this}";
    }
}
