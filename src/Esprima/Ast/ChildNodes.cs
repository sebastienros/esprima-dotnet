using System.Collections;

namespace Esprima.Ast;

public readonly partial struct ChildNodes : IEnumerable<Node>
{
    private readonly Node? _parentNode;

    internal ChildNodes(Node parentNode)
    {
        _parentNode = parentNode;
    }

    public bool IsEmpty()
    {
        using var enumerator = GetEnumerator();
        return !enumerator.MoveNext();
    }

    public Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }

    IEnumerator<Node> IEnumerable<Node>.GetEnumerator()
    {
        return new Enumerator(this);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public partial struct Enumerator : IEnumerator<Node>
    {
        private readonly object? _source; // Node | IEnumerator<Node> | null
        private int _propertyIndex;
        private int _listIndex;
        private Node? _current;

        public Enumerator(in ChildNodes childNodes)
        {
            _source = childNodes._parentNode is not null
                ? childNodes._parentNode.GetChildNodes() ?? (object) childNodes._parentNode
                : null;
            _propertyIndex = 0;
            _listIndex = 0;
            _current = null;
        }

        public void Dispose()
        {
            if (_source is IEnumerator<Node> enumerator)
            {
                enumerator.Dispose();
            }
        }

        public bool MoveNext()
        {
            if (_source is Node parentNode)
            {
                _current = parentNode.NextChildNode(ref this);
                return _current is not null;
            }
            else if (_source is IEnumerator<Node> enumerator)
            {
                if (enumerator.MoveNext())
                {
                    _current = enumerator.Current;
                    return true;
                }

                _current = null;
            }

            return false;
        }

        public Node Current => _current!;

        object? IEnumerator.Current => Current;

        void IEnumerator.Reset()
        {
            if (_source is Node)
            {
                _propertyIndex = 0;
                _listIndex = 0;
            }
            else if (_source is IEnumerator<Node> enumerator)
            {
                enumerator.Reset();
            }

            _current = null;
        }
    }
}
