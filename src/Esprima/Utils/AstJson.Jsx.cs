using Esprima.Ast;

namespace Esprima.Utils;

public static partial class AstJson
{
    private sealed partial class Visitor
    {
        protected internal override void VisitJsxSpreadAttribute(JsxSpreadAttribute jsxSpreadAttribute)
        {
            using (StartNodeObject(jsxSpreadAttribute))
            {
                Member("argument", jsxSpreadAttribute.Argument);
            }
        }

        protected internal override void VisitJsxElement(JsxElement jsxElement)
        {
            using (StartNodeObject(jsxElement))
            {
                Member("openingElement", jsxElement.OpeningElement);
                Member("children", jsxElement.Children);
                Member("closingElement", jsxElement.ClosingElement);
            }
        }

        protected internal override void VisitJsxAttribute(JsxAttribute jsxAttribute)
        {
            using (StartNodeObject(jsxAttribute))
            {
                Member("name", jsxAttribute.Name);
                Member("value", jsxAttribute.Value);
            }
        }

        protected internal override void VisitJsxIdentifier(JsxIdentifier jsxIdentifier)
        {
            using (StartNodeObject(jsxIdentifier))
            {
                Member("name", jsxIdentifier.Name);
            }
        }

        protected internal override void VisitJsxClosingElement(JsxClosingElement jsxClosingElement)
        {
            using (StartNodeObject(jsxClosingElement))
            {
                Member("name", jsxClosingElement.Name);
            }
        }

        protected internal override void VisitJsxText(JsxText jsxText)
        {
            using (StartNodeObject(jsxText))
            {
                Member("value", jsxText.Value);
                Member("raw", jsxText.Raw);
            }
        }

        protected internal override void VisitJsxClosingFragment(JsxClosingFragment jsxClosingFragment)
        {
            using (StartNodeObject(jsxClosingFragment))
            {
            }
        }

        protected internal override void VisitJsxOpeningFragment(JsxOpeningFragment jsxOpeningFragment)
        {
            using (StartNodeObject(jsxOpeningFragment))
            {
                Member("selfClosing", jsxOpeningFragment.SelfClosing);
            }
        }

        protected internal override void VisitJsxOpeningElement(JsxOpeningElement jsxOpeningElement)
        {
            using (StartNodeObject(jsxOpeningElement))
            {
                Member("name", jsxOpeningElement.Name);
                Member("selfClosing", jsxOpeningElement.SelfClosing);
                Member("attributes", jsxOpeningElement.Attributes);
            }
        }

        protected internal override void VisitJsxNamespacedName(JsxNamespacedName jsxNamespacedName)
        {
            using (StartNodeObject(jsxNamespacedName))
            {
                Member("namespace", jsxNamespacedName.Namespace);
                Member("name", jsxNamespacedName.Name);
            }
        }

        protected internal override void VisitJsxMemberExpression(JsxMemberExpression jsxMemberExpression)
        {
            using (StartNodeObject(jsxMemberExpression))
            {
                Member("object", jsxMemberExpression.Object);
                Member("property", jsxMemberExpression.Property);
            }
        }

        protected internal override void VisitJsxEmptyExpression(JsxEmptyExpression jsxEmptyExpression)
        {
            using (StartNodeObject(jsxEmptyExpression))
            {
            }
        }

        protected internal override void VisitJsxExpressionContainer(JsxExpressionContainer jsxExpressionContainer)
        {
            using (StartNodeObject(jsxExpressionContainer))
            {
                Member("expression",jsxExpressionContainer.Expression);
            }
        }
    }
}