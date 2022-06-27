namespace Esprima.Ast
{
    public readonly partial struct ChildNodes : IEnumerable<Node>
    {
        public partial struct Enumerator : IEnumerator<Node>
        {
            // Helper methods for common Node.MoveNextChild implementations

            internal Node? MoveNext(Node node1)
            {
                switch (_propertyIndex)
                {
                    case 0:
                        _propertyIndex++;
                        return node1;
                    default:
                        return null;
                }
            }

            internal Node? MoveNextOptional(Node? node1)
            {
                switch (_propertyIndex)
                {
                    case 0:
                        _propertyIndex++;

                        if (node1 is null)
                        {
                            goto default;
                        }

                        return node1;
                    default:
                        return null;
                }
            }
        }
    }
}
