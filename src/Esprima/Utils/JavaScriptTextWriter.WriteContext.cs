using System.Diagnostics;
using System.Runtime.CompilerServices;
using Esprima.Ast;
using static Esprima.EsprimaExceptionHelper;

namespace Esprima.Utils;

public delegate object? NodePropertyValueAccessor(Node node);

public delegate ref readonly NodeList<T> NodePropertyListValueAccessor<T>(Node node) where T : Node?;

internal abstract class NodePropertyListValueHelper
{
    public abstract Type ItemType { get; }

    public abstract NodeList<Node?> GetNodeList(ref JavaScriptTextWriter.WriteContext context);
}

internal sealed class NodePropertyListValueHelper<T> : NodePropertyListValueHelper where T : Node?
{
    public static readonly NodePropertyListValueHelper<T> Instance = new();

    private NodePropertyListValueHelper() { }

    public override Type ItemType => typeof(T);

    public override NodeList<Node?> GetNodeList(ref JavaScriptTextWriter.WriteContext context) => context.GetNodePropertyListValue<T>().As<Node?>();
}

partial class JavaScriptTextWriter
{
    public struct WriteContext
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WriteContext From(Node? parentNode, Node node) =>
            new WriteContext(parentNode, node ?? ThrowArgumentNullException<Node>(nameof(node)));

        private string? _nodePropertyName;
        private Delegate? _nodePropertyAccessor;
        private NodePropertyListValueHelper? _nodePropertyListValueHelper;
        private int _nodePropertyItemIndex;
        internal AdditionalDataSlot _additionalDataSlot;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal WriteContext(Node? parentNode, Node node)
        {
            ParentNode = parentNode;
            Node = node;
            _nodePropertyName = null;
            _nodePropertyAccessor = null;
            _nodePropertyListValueHelper = null;
            _nodePropertyItemIndex = -1;
            _additionalDataSlot = default;
        }

        public Node? ParentNode { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        public Node Node { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

        public string? NodePropertyName { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _nodePropertyName; }

        public bool NodePropertyHasListValue
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _nodePropertyListValueHelper is not null;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Delegate EnsureNodePropertyAccessor()
        {
            return _nodePropertyAccessor ?? ThrowInvalidOperationException<Delegate>("The context has no associated node property.");
        }

        private NodePropertyValueAccessor EnsureNodePropertyValueAccessor()
        {
            if (_nodePropertyAccessor is NodePropertyValueAccessor accessor)
            {
                return accessor;
            }

            EnsureNodePropertyAccessor();
            return ThrowInvalidOperationException<NodePropertyValueAccessor>("The context has an associated node property but its value is a node list.");
        }

        private NodePropertyListValueHelper EnsureNodePropertyListValueAccessor()
        {
            if (_nodePropertyListValueHelper is not null)
            {
                Debug.Assert(_nodePropertyAccessor?.GetType() is { IsGenericType: true });
                return _nodePropertyListValueHelper;
            }

            EnsureNodePropertyAccessor();
            return ThrowInvalidOperationException<NodePropertyListValueHelper>("The context has an associated node property but its value is not a node list.");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetNodePropertyValue<T>()
        {
            return (T) EnsureNodePropertyValueAccessor()(Node)!;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly NodeList<T> GetNodePropertyListValue<T>() where T : Node?
        {
            EnsureNodePropertyListValueAccessor();
            return ref ((NodePropertyListValueAccessor<T>) _nodePropertyAccessor!)(Node);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NodeList<Node?> GetNodePropertyListValue()
        {
            return EnsureNodePropertyListValueAccessor().GetNodeList(ref this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetNodePropertyListItem<T>() where T : Node?
        {
            return (T) GetNodePropertyListValue()[_nodePropertyItemIndex]!;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearNodeProperty()
        {
            _nodePropertyName = null;
            _nodePropertyAccessor = null;
            _nodePropertyListValueHelper = null;
            _nodePropertyItemIndex = -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SetNodeProperty(string name, NodePropertyValueAccessor valueAccessor)
        {
            _nodePropertyName = name;
            _nodePropertyAccessor = valueAccessor;
            _nodePropertyListValueHelper = null;
            _nodePropertyItemIndex = -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ChangeNodeProperty(string name, NodePropertyValueAccessor valueAccessor) =>
            SetNodeProperty(name ?? ThrowArgumentNullException<string>(nameof(name)), valueAccessor ?? ThrowArgumentNullException<NodePropertyValueAccessor>(nameof(valueAccessor)));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SetNodeProperty<T>(string name, NodePropertyListValueAccessor<T> listValueAccessor) where T : Node?
        {
            _nodePropertyName = name;
            _nodePropertyAccessor = listValueAccessor;
            _nodePropertyListValueHelper = NodePropertyListValueHelper<T>.Instance;
            _nodePropertyItemIndex = -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ChangeNodeProperty<T>(string name, NodePropertyListValueAccessor<T> listValueAccessor) where T : Node? =>
            SetNodeProperty(name ?? ThrowArgumentNullException<string>(nameof(name)), listValueAccessor ?? ThrowArgumentNullException<NodePropertyListValueAccessor<T>>(nameof(listValueAccessor)));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SetNodePropertyItemIndex(int index)
        {
            Debug.Assert(_nodePropertyAccessor is not null && index >= 0);
            _nodePropertyItemIndex = index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ChangeNodePropertyItemIndex(int index)
        {
            EnsureNodePropertyAccessor();
            if (index < 0)
            {
                ThrowArgumentOutOfRangeException(nameof(index), index);
            }
            SetNodePropertyItemIndex(index);
        }

        /// <summary>
        /// Gets or sets the arbitrary, user-defined data object associated with the current <see cref="WriteContext"/>.
        /// </summary>
        /// <remarks>
        /// The operation is not guaranteed to be thread-safe. In case concurrent access or update is possible, the necessary synchronization is caller's responsibility.
        /// </remarks>
        public object? AssociatedData
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _additionalDataSlot[1];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _additionalDataSlot[1] = value;
        }
    }
}
