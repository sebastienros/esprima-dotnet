//HintName: ChildNodes.Helpers.g.cs
#nullable enable

namespace Esprima.Ast;

public readonly partial struct ChildNodes : IEnumerable<Node>
{
    public partial struct Enumerator : IEnumerator<Node>
    {
        internal partial Node? MoveNext(Node arg0)
        {
            switch (_propertyIndex)
            {
                case 0:
                    _propertyIndex++;

                    return arg0;
                default:
                    return null;
            }
        }

        internal partial Node? MoveNextNullable(Node? arg0)
        {
            switch (_propertyIndex)
            {
                case 0:
                    _propertyIndex++;

                    if (arg0 is null)
                    {
                        goto default;
                    }

                    return arg0;
                default:
                    return null;
            }
        }

        internal partial Node? MoveNext(Node arg0, Node arg1)
        {
            switch (_propertyIndex)
            {
                case 0:
                    _propertyIndex++;

                    return arg0;
                case 1:
                    _propertyIndex++;

                    return arg1;
                default:
                    return null;
            }
        }

        internal partial Node? MoveNextNullableAt0(Node? arg0, Node arg1)
        {
            switch (_propertyIndex)
            {
                case 0:
                    _propertyIndex++;

                    if (arg0 is null)
                    {
                        goto case 1;
                    }

                    return arg0;
                case 1:
                    _propertyIndex++;

                    return arg1;
                default:
                    return null;
            }
        }

        internal partial Node? MoveNextNullableAt1(Node arg0, Node? arg1)
        {
            switch (_propertyIndex)
            {
                case 0:
                    _propertyIndex++;

                    return arg0;
                case 1:
                    _propertyIndex++;

                    if (arg1 is null)
                    {
                        goto default;
                    }

                    return arg1;
                default:
                    return null;
            }
        }

        internal partial Node? MoveNext(Node arg0, Node arg1, Node arg2)
        {
            switch (_propertyIndex)
            {
                case 0:
                    _propertyIndex++;

                    return arg0;
                case 1:
                    _propertyIndex++;

                    return arg1;
                case 2:
                    _propertyIndex++;

                    return arg2;
                default:
                    return null;
            }
        }

        internal partial Node? MoveNextNullableAt1_2(Node arg0, Node? arg1, Node? arg2)
        {
            switch (_propertyIndex)
            {
                case 0:
                    _propertyIndex++;

                    return arg0;
                case 1:
                    _propertyIndex++;

                    if (arg1 is null)
                    {
                        goto case 2;
                    }

                    return arg1;
                case 2:
                    _propertyIndex++;

                    if (arg2 is null)
                    {
                        goto default;
                    }

                    return arg2;
                default:
                    return null;
            }
        }

        internal partial Node? MoveNextNullableAt2(Node arg0, Node arg1, Node? arg2)
        {
            switch (_propertyIndex)
            {
                case 0:
                    _propertyIndex++;

                    return arg0;
                case 1:
                    _propertyIndex++;

                    return arg1;
                case 2:
                    _propertyIndex++;

                    if (arg2 is null)
                    {
                        goto default;
                    }

                    return arg2;
                default:
                    return null;
            }
        }

        internal partial Node? MoveNextNullableAt0_1_2(Node? arg0, Node? arg1, Node? arg2, Node arg3)
        {
            switch (_propertyIndex)
            {
                case 0:
                    _propertyIndex++;

                    if (arg0 is null)
                    {
                        goto case 1;
                    }

                    return arg0;
                case 1:
                    _propertyIndex++;

                    if (arg1 is null)
                    {
                        goto case 2;
                    }

                    return arg1;
                case 2:
                    _propertyIndex++;

                    if (arg2 is null)
                    {
                        goto case 3;
                    }

                    return arg2;
                case 3:
                    _propertyIndex++;

                    return arg3;
                default:
                    return null;
            }
        }
    }
}
