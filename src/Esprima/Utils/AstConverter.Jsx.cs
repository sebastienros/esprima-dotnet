using Esprima.Ast;

namespace Esprima.Utils;

public abstract partial class AstConverter 
{
    protected override JsxAttribute UpdateJsxAttribute(JsxAttribute jsxAttribute, JsxExpression name, Expression? value)
    {
        if (jsxAttribute.Name == name && jsxAttribute.Value == value)
        {
            return jsxAttribute;
        }

        return new JsxAttribute(name, value);
    }

    protected override JsxElement UpdateJsxElement(JsxElement jsxElement, Node openingElement, bool isNewChildren, ref NodeList<JsxExpression> children,
        Node? closingElement)
    {
        if (jsxElement.OpeningElement == openingElement && !isNewChildren &&
            jsxElement.ClosingElement == closingElement)
        {
            return jsxElement;
        }

        return new JsxElement(openingElement, children, closingElement);
    }

    protected override JsxIdentifier UpdateJsxIdentifier(JsxIdentifier jsxIdentifier)
    {
        return jsxIdentifier;
    }

    protected override JsxText UpdateJsxText(JsxText jsxText)
    {
        return jsxText;
    }

    protected override JsxClosingElement UpdateJsxClosingElement(JsxClosingElement jsxClosingElement, JsxExpression name)
    {
        if (jsxClosingElement.Name == name)
        {
            return jsxClosingElement;
        }
        
        return new JsxClosingElement(name);
    }

    protected override JsxClosingFragment UpdateJsxClosingFragment(JsxClosingFragment jsxClosingFragment)
    {
        return jsxClosingFragment;
    }

    protected override JsxEmptyExpression UpdateJsxEmptyExpression(JsxEmptyExpression jsxEmptyExpression)
    {
        return jsxEmptyExpression;
    }

    protected override JsxExpressionContainer UpdateJsxExpressionContainer(JsxExpressionContainer jsxExpressionContainer,
        Expression expression)
    {
        if (jsxExpressionContainer.Expression == expression)
        {
            return jsxExpressionContainer;
        }
        
        return new JsxExpressionContainer(expression);
    }

    protected override JsxMemberExpression UpdateJsxMemberExpression(JsxMemberExpression jsxMemberExpression, JsxExpression obj,
        JsxIdentifier property)
    {
        if (jsxMemberExpression.Object == obj && jsxMemberExpression.Property == property)
        {
            return jsxMemberExpression;
        }

        return new JsxMemberExpression(obj, property);
    }

    protected override JsxNamespacedName UpdateJsxNamespacedName(JsxNamespacedName jsxNamespacedName, JsxIdentifier name,
        JsxIdentifier @namespace)
    {
        if (jsxNamespacedName.Name == name && jsxNamespacedName.Namespace == @namespace)
        {
            return jsxNamespacedName;
        }

        return new JsxNamespacedName(@namespace, name);
    }

    protected override JsxOpeningElement UpdateJsxOpeningElement(JsxOpeningElement jsxOpeningElement, JsxExpression name, bool isNewAttributes,
        ref NodeList<JsxExpression> attributes)
    {
        if (jsxOpeningElement.Name == name && !isNewAttributes)
        {
            return jsxOpeningElement;
        }

        return new JsxOpeningElement(name, jsxOpeningElement.SelfClosing, attributes);
    }

    protected override JsxOpeningFragment UpdateJsxOpeningFragment(JsxOpeningFragment jsxOpeningFragment)
    {
        return jsxOpeningFragment;
    }

    protected override JsxSpreadAttribute UpdateJsxSpreadAttribute(JsxSpreadAttribute jsxSpreadAttribute, Expression argument)
    {
        if (jsxSpreadAttribute.Argument == argument)
        {
            return jsxSpreadAttribute;
        }

        return new JsxSpreadAttribute(argument);
    }
}
