using Esprima.Ast;

namespace Esprima.Utils;

public partial class AstVisitor
{
    protected internal virtual JsxMemberExpression UpdateJsxMemberExpression(JsxMemberExpression jsxMemberExpression, JsxExpression obj, JsxIdentifier property)
    {
        return jsxMemberExpression;
    }

    protected internal virtual JsxText UpdateJsxText(JsxText jsxText)
    {
        return jsxText;
    }

    protected internal virtual JsxOpeningFragment UpdateJsxOpeningFragment(JsxOpeningFragment jsxOpeningFragment)
    {
        return jsxOpeningFragment;
    }

    protected internal virtual JsxClosingFragment UpdateJsxClosingFragment(JsxClosingFragment jsxClosingFragment)
    {
        return jsxClosingFragment;
    }

    protected internal virtual JsxIdentifier UpdateJsxIdentifier(JsxIdentifier jsxIdentifier)
    {
        return jsxIdentifier;
    }

    protected internal virtual JsxElement UpdateJsxElement(JsxElement jsxElement, Node openingElement, bool isNewChildren, ref NodeList<JsxExpression> children, Node? closingElement)
    {
        return jsxElement;
    }

    protected internal virtual JsxOpeningElement UpdateJsxOpeningElement(JsxOpeningElement jsxOpeningElement, JsxExpression name, bool isNewAttributes, ref NodeList<JsxExpression> attributes)
    {
        return jsxOpeningElement;
    }

    protected internal virtual JsxClosingElement UpdateJsxClosingElement(JsxClosingElement jsxClosingElement, JsxExpression name)
    {
        return jsxClosingElement;
    }

    protected internal virtual JsxEmptyExpression UpdateJsxEmptyExpression(JsxEmptyExpression jsxEmptyExpression)
    {
        return jsxEmptyExpression;
    }

    protected internal virtual JsxNamespacedName UpdateJsxNamespacedName(JsxNamespacedName jsxNamespacedName, JsxIdentifier name, JsxIdentifier @namespace)
    {
        return jsxNamespacedName;
    }

    protected internal virtual JsxSpreadAttribute UpdateJsxSpreadAttribute(JsxSpreadAttribute jsxSpreadAttribute, Expression attribute)
    {
        return jsxSpreadAttribute;
    }

    protected internal virtual JsxAttribute UpdateJsxAttribute(JsxAttribute jsxAttribute, JsxExpression name, Expression? value)
    {
        return jsxAttribute;
    }

    protected internal virtual JsxExpressionContainer UpdateJsxExpressionContainer(JsxExpressionContainer jsxExpressionContainer, Expression expression)
    {
        return jsxExpressionContainer;
    }
}
