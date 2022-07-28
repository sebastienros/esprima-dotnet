using System.Diagnostics;
using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast;

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(), nq}}")]
public abstract class Node
{
    internal AdditionalDataContainer _additionalDataContainer;

    protected Node(Nodes type)
    {
        Type = type;
    }

    public Nodes Type { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    public Range Range;
    public Location Location;

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

    /// <summary>
    /// Gets additional, user-defined data associated with the specified key.
    /// </summary>
    /// <remarks>
    /// The operation is not guaranteed to be thread-safe. In case concurrent access or update is possible, the necessary synchronization is caller's responsibility.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public object? GetAdditionalData(object key) => _additionalDataContainer.GetData(key);

    /// <summary>
    /// Sets additional, user-defined data associated with the specified key.
    /// </summary>
    /// <remarks>
    /// The operation is not guaranteed to be thread-safe. In case concurrent access or update is possible, the necessary synchronization is caller's responsibility.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetAdditionalData(object key, object? value) => _additionalDataContainer.SetData(key, value);

    public override string ToString() => this.ToJavascriptString(beautify: true);

    private string GetDebuggerDisplay()
    {
        return $"/*{Type}*/  {this}";
    }
}
