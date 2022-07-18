﻿using System.Runtime.CompilerServices;
using Esprima.Ast;

namespace Esprima.Utils;

partial class JavascriptTextWriter
{
    public struct WriteContext
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WriteContext From(Node? parentNode, Node node) =>
            new WriteContext(parentNode, node ?? throw new ArgumentNullException(nameof(node)));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal WriteContext(Node? parentNode, Node node)
        {
            ParentNode = parentNode;
            Node = node;
            _nodePropertyName = null;
            _nodePropertyValueAccessor = null;
        }

        public Node? ParentNode { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        public Node Node { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

        private string? _nodePropertyName;
        public string? NodePropertyName { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _nodePropertyName; }

        private Delegate? _nodePropertyValueAccessor;
        private Delegate NodePropertyAccessor
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _nodePropertyValueAccessor ?? throw new InvalidOperationException("The context has no associated node property.");
        }

        public bool NodePropertyHasListValue
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => NodePropertyAccessor.GetType().IsGenericType;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Type GetNodePropertyListItemType()
        {
            var type = NodePropertyAccessor.GetType();
            return type.IsGenericType
                ? type.GetGenericArguments()[0]
                : throw new InvalidOperationException("The context has an associated node property but its value is not a node list.");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetNodePropertyValue<T>() =>
            (T) ((NodePropertyValueAccessor) NodePropertyAccessor)(Node)!;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly NodeList<T> GetNodePropertyListValue<T>() where T : Node? =>
            ref ((NodePropertyListValueAccessor<T>) NodePropertyAccessor)(Node);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearNodeProperty()
        {
            _nodePropertyName = null;
            _nodePropertyValueAccessor = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SetNodeProperty(string name, NodePropertyValueAccessor valueAccessor)
        {
            _nodePropertyName = name;
            _nodePropertyValueAccessor = valueAccessor;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ChangeNodeProperty(string name, NodePropertyValueAccessor valueAccessor) =>
            SetNodeProperty(name ?? throw new ArgumentNullException(nameof(name)), valueAccessor ?? throw new ArgumentNullException(nameof(valueAccessor)));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SetNodeProperty<T>(string name, NodePropertyListValueAccessor<T> listValueAccessor) where T : Node?
        {
            _nodePropertyName = name;
            _nodePropertyValueAccessor = listValueAccessor;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ChangeNodeProperty<T>(string name, NodePropertyListValueAccessor<T> listValueAccessor) where T : Node? =>
            SetNodeProperty(name ?? throw new ArgumentNullException(nameof(name)), listValueAccessor ?? throw new ArgumentNullException(nameof(listValueAccessor)));
    }
}