using Esprima.Ast;

namespace Esprima.Utils;

public static partial class AstJson
{
    private sealed partial class Visitor
    {
        protected internal override object? VisitJsxSpreadAttribute(JsxSpreadAttribute jsxSpreadAttribute)
        {
            using (StartNodeObject(jsxSpreadAttribute))
            {
                Member("argument", jsxSpreadAttribute.Argument);
            }

            return jsxSpreadAttribute;
        }

        protected internal override object? VisitJsxElement(JsxElement jsxElement)
        {
            using (StartNodeObject(jsxElement))
            {
                Member("openingElement", jsxElement.OpeningElement);
                Member("children", jsxElement.Children);
                Member("closingElement", jsxElement.ClosingElement);
            }

            return jsxElement;
        }

        protected internal override object? VisitJsxAttribute(JsxAttribute jsxAttribute)
        {
            using (StartNodeObject(jsxAttribute))
            {
                Member("name", jsxAttribute.Name);
                Member("value", jsxAttribute.Value);
            }

            return jsxAttribute;
        }

        protected internal override object? VisitJsxIdentifier(JsxIdentifier jsxIdentifier)
        {
            using (StartNodeObject(jsxIdentifier))
            {
                Member("name", jsxIdentifier.Name);
            }

            return jsxIdentifier;
        }

        protected internal override object? VisitJsxClosingElement(JsxClosingElement jsxClosingElement)
        {
            using (StartNodeObject(jsxClosingElement))
            {
                Member("name", jsxClosingElement.Name);
            }

            return jsxClosingElement;
        }

        protected internal override object? VisitJsxText(JsxText jsxText)
        {
            using (StartNodeObject(jsxText))
            {
                Member("value", jsxText.Value);
                Member("raw", jsxText.Raw);
            }

            return jsxText;
        }

        protected internal override object? VisitJsxClosingFragment(JsxClosingFragment jsxClosingFragment)
        {
            using (StartNodeObject(jsxClosingFragment))
            {
            }

            return jsxClosingFragment;
        }

        protected internal override object? VisitJsxOpeningFragment(JsxOpeningFragment jsxOpeningFragment)
        {
            using (StartNodeObject(jsxOpeningFragment))
            {
                Member("selfClosing", jsxOpeningFragment.SelfClosing);
            }

            return jsxOpeningFragment;
        }

        protected internal override object? VisitJsxOpeningElement(JsxOpeningElement jsxOpeningElement)
        {
            using (StartNodeObject(jsxOpeningElement))
            {
                Member("name", jsxOpeningElement.Name);
                Member("selfClosing", jsxOpeningElement.SelfClosing);
                Member("attributes", jsxOpeningElement.Attributes);
            }

            return jsxOpeningElement;
        }

        protected internal override object? VisitJsxNamespacedName(JsxNamespacedName jsxNamespacedName)
        {
            using (StartNodeObject(jsxNamespacedName))
            {
                Member("namespace", jsxNamespacedName.Namespace);
                Member("name", jsxNamespacedName.Name);
            }

            return jsxNamespacedName;
        }

        protected internal override object? VisitJsxMemberExpression(JsxMemberExpression jsxMemberExpression)
        {
            using (StartNodeObject(jsxMemberExpression))
            {
                Member("object", jsxMemberExpression.Object);
                Member("property", jsxMemberExpression.Property);
            }

            return jsxMemberExpression;
        }

        protected internal override object? VisitJsxEmptyExpression(JsxEmptyExpression jsxEmptyExpression)
        {
            using (StartNodeObject(jsxEmptyExpression))
            {
            }

            return jsxEmptyExpression;
        }

        protected internal override object? VisitJsxExpressionContainer(JsxExpressionContainer jsxExpressionContainer)
        {
            using (StartNodeObject(jsxExpressionContainer))
            {
                Member("expression",jsxExpressionContainer.Expression);
            }

            return jsxExpressionContainer;
        }
    }
}
