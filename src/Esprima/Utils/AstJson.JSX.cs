using Esprima.Ast;

namespace Esprima.Utils
{
    public static partial class AstJson
    {
        private sealed partial class Visitor
        {
            protected internal override void VisitJSXSpreadAttribute(JSXSpreadAttribute jsxSpreadAttribute)
            {
                using (StartNodeObject(jsxSpreadAttribute))
                {
                    Member("argument", jsxSpreadAttribute.Argument);
                }
            }

            protected internal override void VisitJSXElement(JSXElement jsxElement)
            {
                using (StartNodeObject(jsxElement))
                {
                    Member("openingElement", jsxElement.OpeningElement);
                    Member("children", jsxElement.Children);
                    Member("closingElement", jsxElement.ClosingElement);
                }
            }

            protected internal override void VisitJSXAttribute(JSXAttribute jsxAttribute)
            {
                using (StartNodeObject(jsxAttribute))
                {
                    Member("name", jsxAttribute.Name);
                    Member("value", jsxAttribute.Value);
                }
            }

            protected internal override void VisitJSXIdentifier(JSXIdentifier jsxIdentifier)
            {
                using (StartNodeObject(jsxIdentifier))
                {
                    Member("name", jsxIdentifier.Name);
                }
            }

            protected internal override void VisitJSXClosingElement(JSXClosingElement jsxClosingElement)
            {
                using (StartNodeObject(jsxClosingElement))
                {
                    Member("name", jsxClosingElement.Name);
                }
            }

            protected internal override void VisitJSXText(JSXText jsxText)
            {
                using (StartNodeObject(jsxText))
                {
                    Member("value", jsxText.Value);
                    Member("raw", jsxText.Raw);
                }
            }

            protected internal override void VisitJSXClosingFragment(JSXClosingFragment jsxClosingFragment)
            {
                using (StartNodeObject(jsxClosingFragment))
                {
                }
            }

            protected internal override void VisitJSXOpeningFragment(JSXOpeningFragment jsxOpeningFragment)
            {
                using (StartNodeObject(jsxOpeningFragment))
                {
                    Member("selfClosing", jsxOpeningFragment.SelfClosing);
                }
            }

            protected internal override void VisitJSXOpeningElement(JSXOpeningElement jsxOpeningElement)
            {
                using (StartNodeObject(jsxOpeningElement))
                {
                    Member("name", jsxOpeningElement.Name);
                    Member("selfClosing", jsxOpeningElement.SelfClosing);
                    Member("attributes", jsxOpeningElement.Attributes);
                }
            }

            protected internal override void VisitJSXNamespacedName(JSXNamespacedName jsxNamespacedName)
            {
                using (StartNodeObject(jsxNamespacedName))
                {
                    Member("namespace", jsxNamespacedName.Namespace);
                    Member("name", jsxNamespacedName.Name);
                }
            }

            protected internal override void VisitJSXMemberExpression(JSXMemberExpression jsxMemberExpression)
            {
                using (StartNodeObject(jsxMemberExpression))
                {
                    Member("object", jsxMemberExpression.Object);
                    Member("property", jsxMemberExpression.Property);
                }
            }

            protected internal override void VisitJSXEmptyExpression(JSXEmptyExpression jsxEmptyExpression)
            {
                using (StartNodeObject(jsxEmptyExpression))
                {
                }
            }

            protected internal override void VisitJSXExpressionContainer(JSXExpressionContainer jsxExpressionContainer)
            {
                using (StartNodeObject(jsxExpressionContainer))
                {
                    Member("expression",jsxExpressionContainer.Expression);
                }
            }
        }
    }
}
