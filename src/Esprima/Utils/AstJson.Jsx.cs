using Esprima.Ast;

namespace Esprima.Utils;

public static partial class AstJson
{
    private sealed partial class Visitor
    {
        protected internal override JsxSpreadAttribute VisitJsxSpreadAttribute(JsxSpreadAttribute jsxSpreadAttribute)
        {
            using (StartNodeObject(jsxSpreadAttribute))
            {
                Member("argument", jsxSpreadAttribute.Argument);
            }

            return jsxSpreadAttribute;
        }

        protected internal override JsxElement VisitJsxElement(JsxElement jsxElement)
        {
            using (StartNodeObject(jsxElement))
            {
                Member("openingElement", jsxElement.OpeningElement);
                Member("children", jsxElement.Children);
                Member("closingElement", jsxElement.ClosingElement);
            }

            return jsxElement;
        }

        protected internal override JsxAttribute VisitJsxAttribute(JsxAttribute jsxAttribute)
        {
            using (StartNodeObject(jsxAttribute))
            {
                Member("name", jsxAttribute.Name);
                Member("value", jsxAttribute.Value);
            }

            return jsxAttribute;
        }

        protected internal override JsxIdentifier VisitJsxIdentifier(JsxIdentifier jsxIdentifier)
        {
            using (StartNodeObject(jsxIdentifier))
            {
                Member("name", jsxIdentifier.Name);
            }

            return jsxIdentifier;
        }

        protected internal override JsxClosingElement VisitJsxClosingElement(JsxClosingElement jsxClosingElement)
        {
            using (StartNodeObject(jsxClosingElement))
            {
                Member("name", jsxClosingElement.Name);
            }

            return jsxClosingElement;
        }

        protected internal override JsxText VisitJsxText(JsxText jsxText)
        {
            using (StartNodeObject(jsxText))
            {
                Member("value", jsxText.Value);
                Member("raw", jsxText.Raw);
            }

            return jsxText;
        }

        protected internal override JsxClosingFragment VisitJsxClosingFragment(JsxClosingFragment jsxClosingFragment)
        {
            using (StartNodeObject(jsxClosingFragment))
            {
            }

            return jsxClosingFragment;
        }

        protected internal override JsxOpeningFragment VisitJsxOpeningFragment(JsxOpeningFragment jsxOpeningFragment)
        {
            using (StartNodeObject(jsxOpeningFragment))
            {
                Member("selfClosing", jsxOpeningFragment.SelfClosing);
            }

            return jsxOpeningFragment;
        }

        protected internal override JsxOpeningElement VisitJsxOpeningElement(JsxOpeningElement jsxOpeningElement)
        {
            using (StartNodeObject(jsxOpeningElement))
            {
                Member("name", jsxOpeningElement.Name);
                Member("selfClosing", jsxOpeningElement.SelfClosing);
                Member("attributes", jsxOpeningElement.Attributes);
            }

            return jsxOpeningElement;
        }

        protected internal override JsxNamespacedName VisitJsxNamespacedName(JsxNamespacedName jsxNamespacedName)
        {
            using (StartNodeObject(jsxNamespacedName))
            {
                Member("namespace", jsxNamespacedName.Namespace);
                Member("name", jsxNamespacedName.Name);
            }

            return jsxNamespacedName;
        }

        protected internal override JsxMemberExpression VisitJsxMemberExpression(JsxMemberExpression jsxMemberExpression)
        {
            using (StartNodeObject(jsxMemberExpression))
            {
                Member("object", jsxMemberExpression.Object);
                Member("property", jsxMemberExpression.Property);
            }

            return jsxMemberExpression;
        }

        protected internal override JsxEmptyExpression VisitJsxEmptyExpression(JsxEmptyExpression jsxEmptyExpression)
        {
            using (StartNodeObject(jsxEmptyExpression))
            {
            }

            return jsxEmptyExpression;
        }

        protected internal override JsxExpressionContainer VisitJsxExpressionContainer(JsxExpressionContainer jsxExpressionContainer)
        {
            using (StartNodeObject(jsxExpressionContainer))
            {
                Member("expression",jsxExpressionContainer.Expression);
            }

            return jsxExpressionContainer;
        }
    }
}
