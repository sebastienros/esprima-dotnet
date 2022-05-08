using Esprima.Ast;

namespace Esprima.Utils;

public partial class AstVisitor
{
    protected virtual JsxMemberExpression UpdateJsxMemberExpression(JsxMemberExpression jsxMemberExpression, JsxExpression obj, JsxIdentifier property)
    {
        return jsxMemberExpression;
    }

    protected virtual JsxText UpdateJsxText(JsxText jsxText)
    {
        return jsxText;
    }

    protected virtual JsxOpeningFragment UpdateJsxOpeningFragment(JsxOpeningFragment jsxOpeningFragment)
    {
        return jsxOpeningFragment;
    }

    protected virtual JsxClosingFragment UpdateJsxClosingFragment(JsxClosingFragment jsxClosingFragment)
    {
        return jsxClosingFragment;
    }

    protected virtual JsxIdentifier UpdateJsxIdentifier(JsxIdentifier jsxIdentifier)
    {
        return jsxIdentifier;
    }

    protected virtual JsxElement UpdateJsxElement(JsxElement jsxElement, Node openingElement, bool isNewChildren, ref NodeList<JsxExpression> children, Node? closingElement)
    {
        return jsxElement;
    }

    protected virtual JsxOpeningElement UpdateJsxOpeningElement(JsxOpeningElement jsxOpeningElement, JsxExpression name, bool isNewAttributes, ref NodeList<JsxExpression> attributes)
    {
        return jsxOpeningElement;
    }

    protected virtual JsxClosingElement UpdateJsxClosingElement(JsxClosingElement jsxClosingElement, JsxExpression name)
    {
        return jsxClosingElement;
    }

    protected virtual JsxEmptyExpression UpdateJsxEmptyExpression(JsxEmptyExpression jsxEmptyExpression)
    {
        return jsxEmptyExpression;
    }

    protected virtual JsxNamespacedName UpdateJsxNamespacedName(JsxNamespacedName jsxNamespacedName, JsxIdentifier name, JsxIdentifier @namespace)
    {
        return jsxNamespacedName;
    }

    protected virtual JsxSpreadAttribute UpdateJsxSpreadAttribute(JsxSpreadAttribute jsxSpreadAttribute, Expression attribute)
    {
        return jsxSpreadAttribute;
    }

    protected virtual JsxAttribute UpdateJsxAttribute(JsxAttribute jsxAttribute, JsxExpression name, Expression? value)
    {
        return jsxAttribute;
    }

    protected virtual JsxExpressionContainer UpdateJsxExpressionContainer(JsxExpressionContainer jsxExpressionContainer, Expression argument)
    {
        return jsxExpressionContainer;
    }
}
