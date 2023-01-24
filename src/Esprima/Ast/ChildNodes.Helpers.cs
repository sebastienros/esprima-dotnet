namespace Esprima.Ast;

public readonly partial struct ChildNodes : IEnumerable<Node>
{
    public partial struct Enumerator : IEnumerator<Node>
    {
        // This file contains helper methods used for implementing Node.MoveNextChild. These methods contain the actual implementation
        // (finite state machines) of the enumerations. Only special cases are implemented here, non-special cases are auto-generated
        // based on the VisitableNode annotations of the AST nodes (see VisitationBoilerplateGenerator in the Esprima.SourceGenerators project).

        internal Node? MoveNextExportSpecifier(Expression local, Expression exported)
        {
            switch (_propertyIndex)
            {
                case 0:
                    _propertyIndex++;

                    return local;
                case 1:
                    _propertyIndex++;

                    if (exported == local)
                    {
                        goto default;
                    }

                    return exported;
                default:
                    return null;
            }
        }

        internal Node? MoveNextImportSpecifier(Expression imported, Identifier local)
        {
            switch (_propertyIndex)
            {
                case 0:
                    _propertyIndex++;

                    if (imported == local)
                    {
                        goto case 1;
                    }

                    return imported;
                case 1:
                    _propertyIndex++;

                    return local;
                default:
                    return null;
            }
        }

        internal Node? MoveNextProperty(Expression? key, Node value, bool shorthand)
        {
            switch (_propertyIndex)
            {
                case 0:
                    _propertyIndex++;

                    if (shorthand)
                    {
                        goto case 1;
                    }

                    return key;
                case 1:
                    _propertyIndex++;

                    return value;
                default:
                    return null;
            }
        }

        internal Node? MoveNextTemplateLiteral(in NodeList<TemplateElement> quasis, in NodeList<Expression> expressions)
        {
            // Equivalent to:

            // TemplateElement quasi;
            // for (var i = 0; !(quasi = Quasis[i]).Tail; i++)
            // {
            //     yield return quasi;
            //     yield return Expressions[i];
            // }
            // yield return quasi;

            switch (_propertyIndex)
            {
                case 0:
                    _propertyIndex++;

                    var quasi = quasis[_listIndex];
                    if (quasi.Tail)
                    {
                        _propertyIndex++;
                        _listIndex = 0;
                    }

                    return quasi;
                case 1:
                    _propertyIndex--;

                    return expressions[_listIndex++];
                default:
                    return null;
            }
        }
    }
}
