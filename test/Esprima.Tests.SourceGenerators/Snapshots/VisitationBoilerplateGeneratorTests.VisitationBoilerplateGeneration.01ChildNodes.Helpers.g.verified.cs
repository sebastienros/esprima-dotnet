//HintName: ChildNodes.Helpers.g.cs
#nullable enable

namespace Esprima.Ast;

partial struct ChildNodes
{
    partial struct Enumerator
    {
        internal Esprima.Ast.Node? MoveNext(Esprima.Ast.Node arg0)
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

        internal Esprima.Ast.Node? MoveNext<T0>(in Esprima.Ast.NodeList<T0> arg0)
            where T0 : Esprima.Ast.Node
        {
            switch (_propertyIndex)
            {
                case 0:
                    if (_listIndex >= arg0.Count)
                    {
                        _listIndex = 0;
                        _propertyIndex++;
                        goto default;
                    }
                    
                    Esprima.Ast.Node? item = arg0[_listIndex++];
                    
                    return item;
                default:
                    return null;
            }
        }

        internal Esprima.Ast.Node? MoveNextNullable(Esprima.Ast.Node? arg0)
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

        internal Esprima.Ast.Node? MoveNextNullable<T0>(in Esprima.Ast.NodeList<T0?> arg0)
            where T0 : Esprima.Ast.Node
        {
            switch (_propertyIndex)
            {
                case 0:
                    if (_listIndex >= arg0.Count)
                    {
                        _listIndex = 0;
                        _propertyIndex++;
                        goto default;
                    }
                    
                    Esprima.Ast.Node? item = arg0[_listIndex++];
                    
                    if (item is null)
                    {
                        goto case 0;
                    }
                    
                    return item;
                default:
                    return null;
            }
        }

        internal Esprima.Ast.Node? MoveNext(Esprima.Ast.Node arg0, Esprima.Ast.Node arg1)
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

        internal Esprima.Ast.Node? MoveNext<T0>(in Esprima.Ast.NodeList<T0> arg0, Esprima.Ast.Node arg1)
            where T0 : Esprima.Ast.Node
        {
            switch (_propertyIndex)
            {
                case 0:
                    if (_listIndex >= arg0.Count)
                    {
                        _listIndex = 0;
                        _propertyIndex++;
                        goto case 1;
                    }
                    
                    Esprima.Ast.Node? item = arg0[_listIndex++];
                    
                    return item;
                case 1:
                    _propertyIndex++;
                    
                    return arg1;
                default:
                    return null;
            }
        }

        internal Esprima.Ast.Node? MoveNext<T1>(Esprima.Ast.Node arg0, in Esprima.Ast.NodeList<T1> arg1)
            where T1 : Esprima.Ast.Node
        {
            switch (_propertyIndex)
            {
                case 0:
                    _propertyIndex++;
                    
                    return arg0;
                case 1:
                    if (_listIndex >= arg1.Count)
                    {
                        _listIndex = 0;
                        _propertyIndex++;
                        goto default;
                    }
                    
                    Esprima.Ast.Node? item = arg1[_listIndex++];
                    
                    return item;
                default:
                    return null;
            }
        }

        internal Esprima.Ast.Node? MoveNextNullableAt0(Esprima.Ast.Node? arg0, Esprima.Ast.Node arg1)
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

        internal Esprima.Ast.Node? MoveNextNullableAt0<T1>(Esprima.Ast.Node? arg0, in Esprima.Ast.NodeList<T1> arg1)
            where T1 : Esprima.Ast.Node
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
                    if (_listIndex >= arg1.Count)
                    {
                        _listIndex = 0;
                        _propertyIndex++;
                        goto default;
                    }
                    
                    Esprima.Ast.Node? item = arg1[_listIndex++];
                    
                    return item;
                default:
                    return null;
            }
        }

        internal Esprima.Ast.Node? MoveNextNullableAt1(Esprima.Ast.Node arg0, Esprima.Ast.Node? arg1)
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

        internal Esprima.Ast.Node? MoveNext(Esprima.Ast.Node arg0, Esprima.Ast.Node arg1, Esprima.Ast.Node arg2)
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

        internal Esprima.Ast.Node? MoveNext<T0>(in Esprima.Ast.NodeList<T0> arg0, Esprima.Ast.Node arg1, Esprima.Ast.Node arg2)
            where T0 : Esprima.Ast.Node
        {
            switch (_propertyIndex)
            {
                case 0:
                    if (_listIndex >= arg0.Count)
                    {
                        _listIndex = 0;
                        _propertyIndex++;
                        goto case 1;
                    }
                    
                    Esprima.Ast.Node? item = arg0[_listIndex++];
                    
                    return item;
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

        internal Esprima.Ast.Node? MoveNextNullableAt0<T1>(Esprima.Ast.Node? arg0, in Esprima.Ast.NodeList<T1> arg1, Esprima.Ast.Node arg2)
            where T1 : Esprima.Ast.Node
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
                    if (_listIndex >= arg1.Count)
                    {
                        _listIndex = 0;
                        _propertyIndex++;
                        goto case 2;
                    }
                    
                    Esprima.Ast.Node? item = arg1[_listIndex++];
                    
                    return item;
                case 2:
                    _propertyIndex++;
                    
                    return arg2;
                default:
                    return null;
            }
        }

        internal Esprima.Ast.Node? MoveNextNullableAt2(Esprima.Ast.Node arg0, Esprima.Ast.Node arg1, Esprima.Ast.Node? arg2)
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

        internal Esprima.Ast.Node? MoveNextNullableAt2<T0>(in Esprima.Ast.NodeList<T0> arg0, Esprima.Ast.Node arg1, Esprima.Ast.Node? arg2)
            where T0 : Esprima.Ast.Node
        {
            switch (_propertyIndex)
            {
                case 0:
                    if (_listIndex >= arg0.Count)
                    {
                        _listIndex = 0;
                        _propertyIndex++;
                        goto case 1;
                    }
                    
                    Esprima.Ast.Node? item = arg0[_listIndex++];
                    
                    return item;
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

        internal Esprima.Ast.Node? MoveNextNullableAt2<T1>(Esprima.Ast.Node arg0, in Esprima.Ast.NodeList<T1> arg1, Esprima.Ast.Node? arg2)
            where T1 : Esprima.Ast.Node
        {
            switch (_propertyIndex)
            {
                case 0:
                    _propertyIndex++;
                    
                    return arg0;
                case 1:
                    if (_listIndex >= arg1.Count)
                    {
                        _listIndex = 0;
                        _propertyIndex++;
                        goto case 2;
                    }
                    
                    Esprima.Ast.Node? item = arg1[_listIndex++];
                    
                    return item;
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

        internal Esprima.Ast.Node? MoveNextNullableAt0_2<T1>(Esprima.Ast.Node? arg0, in Esprima.Ast.NodeList<T1> arg1, Esprima.Ast.Node? arg2)
            where T1 : Esprima.Ast.Node
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
                    if (_listIndex >= arg1.Count)
                    {
                        _listIndex = 0;
                        _propertyIndex++;
                        goto case 2;
                    }
                    
                    Esprima.Ast.Node? item = arg1[_listIndex++];
                    
                    return item;
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

        internal Esprima.Ast.Node? MoveNextNullableAt1_2(Esprima.Ast.Node arg0, Esprima.Ast.Node? arg1, Esprima.Ast.Node? arg2)
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

        internal Esprima.Ast.Node? MoveNextNullableAt1_2<T0>(in Esprima.Ast.NodeList<T0> arg0, Esprima.Ast.Node? arg1, Esprima.Ast.Node? arg2, Esprima.Ast.Node arg3)
            where T0 : Esprima.Ast.Node
        {
            switch (_propertyIndex)
            {
                case 0:
                    if (_listIndex >= arg0.Count)
                    {
                        _listIndex = 0;
                        _propertyIndex++;
                        goto case 1;
                    }
                    
                    Esprima.Ast.Node? item = arg0[_listIndex++];
                    
                    return item;
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

        internal Esprima.Ast.Node? MoveNextNullableAt0_1_2(Esprima.Ast.Node? arg0, Esprima.Ast.Node? arg1, Esprima.Ast.Node? arg2, Esprima.Ast.Node arg3)
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
