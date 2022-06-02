using Esprima.Ast;

namespace Esprima.Utils;

public abstract partial class AstConverter
{
    protected internal override object? VisitJsxMemberExpression(JsxMemberExpression jsxMemberExpression)
    {
        var obj = Visit(jsxMemberExpression.Object) as JsxExpression;
        var property = Visit(jsxMemberExpression.Property) as JsxIdentifier;
        return UpdateJsxMemberExpression(jsxMemberExpression, obj!, property!);
    }

    protected internal override object? VisitJsxText(JsxText jsxText)
    {
        return UpdateJsxText(jsxText);
    }

    protected internal override object? VisitJsxOpeningFragment(JsxOpeningFragment jsxOpeningFragment)
    {
        return UpdateJsxOpeningFragment(jsxOpeningFragment);
    }

    protected internal override object? VisitJsxClosingFragment(JsxClosingFragment jsxClosingFragment)
    {
        return UpdateJsxClosingFragment(jsxClosingFragment);
    }

    protected internal override object? VisitJsxIdentifier(JsxIdentifier jsxIdentifier)
    {
        return UpdateJsxIdentifier(jsxIdentifier);
    }

    protected internal override object? VisitJsxElement(JsxElement jsxElement)
    {
        var openingElement = Visit(jsxElement.OpeningElement) as Node;
        var isNewChildren = HasNodeListChanged(jsxElement.Children, out var children);
        Node? closingElement = null;
        if (jsxElement.ClosingElement is not null)
        {
            closingElement = Visit(jsxElement.ClosingElement) as Node;
        }

        return UpdateJsxElement(jsxElement, openingElement!, isNewChildren, ref children, closingElement);
    }

    protected internal override object? VisitJsxOpeningElement(JsxOpeningElement jsxOpeningElement)
    {
        var name = Visit(jsxOpeningElement.Name) as JsxExpression;
        var isNewAttributes = HasNodeListChanged(jsxOpeningElement.Attributes, out var attributes);
        return UpdateJsxOpeningElement(jsxOpeningElement, name!, isNewAttributes, ref attributes);
    }

    protected internal override object? VisitJsxClosingElement(JsxClosingElement jsxClosingElement)
    {
        var name = Visit(jsxClosingElement.Name) as JsxExpression;
        return UpdateJsxClosingElement(jsxClosingElement, name!);
    }

    protected internal override object? VisitJsxEmptyExpression(JsxEmptyExpression jsxEmptyExpression)
    {
        return UpdateJsxEmptyExpression(jsxEmptyExpression);
    }

    protected internal override object? VisitJsxNamespacedName(JsxNamespacedName jsxNamespacedName)
    {
        var name = Visit(jsxNamespacedName.Name) as JsxIdentifier;
        var @namespace = Visit(jsxNamespacedName.Namespace) as JsxIdentifier;
        return UpdateJsxNamespacedName(jsxNamespacedName, name!, @namespace!);
    }

    protected internal override object? VisitJsxSpreadAttribute(JsxSpreadAttribute jsxSpreadAttribute)
    {
        var argument = Visit(jsxSpreadAttribute.Argument) as JsxExpression;
        return UpdateJsxSpreadAttribute(jsxSpreadAttribute, argument!);
    }

    protected internal override object? VisitJsxAttribute(JsxAttribute jsxAttribute)
    {
        var name = Visit(jsxAttribute.Name) as JsxExpression;
        Expression? value = null;
        if (jsxAttribute.Value is not null)
        {
            value = Visit(jsxAttribute.Value) as Expression;
        }

        return UpdateJsxAttribute(jsxAttribute, name!, value);
    }

    protected internal override object? VisitJsxExpressionContainer(JsxExpressionContainer jsxExpressionContainer)
    {
        var expression = Visit(jsxExpressionContainer.Expression) as Expression;
        return UpdateJsxExpressionContainer(jsxExpressionContainer, expression!);
    }

    #region Update methods

    protected virtual JsxAttribute UpdateJsxAttribute(JsxAttribute jsxAttribute, JsxExpression name, Expression? value)
    {
        if (jsxAttribute.Name == name && jsxAttribute.Value == value)
        {
            return jsxAttribute;
        }

        return new JsxAttribute(name, value);
    }

    protected virtual JsxElement UpdateJsxElement(JsxElement jsxElement, Node openingElement, bool isNewChildren, ref NodeList<JsxExpression> children,
        Node? closingElement)
    {
        if (jsxElement.OpeningElement == openingElement && !isNewChildren &&
            jsxElement.ClosingElement == closingElement)
        {
            return jsxElement;
        }

        return new JsxElement(openingElement, children, closingElement);
    }

    protected virtual JsxIdentifier UpdateJsxIdentifier(JsxIdentifier jsxIdentifier)
    {
        return jsxIdentifier;
    }

    protected virtual JsxText UpdateJsxText(JsxText jsxText)
    {
        return jsxText;
    }

    protected virtual JsxClosingElement UpdateJsxClosingElement(JsxClosingElement jsxClosingElement, JsxExpression name)
    {
        if (jsxClosingElement.Name == name)
        {
            return jsxClosingElement;
        }
        
        return new JsxClosingElement(name);
    }

    protected virtual JsxClosingFragment UpdateJsxClosingFragment(JsxClosingFragment jsxClosingFragment)
    {
        return jsxClosingFragment;
    }

    protected virtual JsxEmptyExpression UpdateJsxEmptyExpression(JsxEmptyExpression jsxEmptyExpression)
    {
        return jsxEmptyExpression;
    }

    protected virtual JsxExpressionContainer UpdateJsxExpressionContainer(JsxExpressionContainer jsxExpressionContainer,
        Expression expression)
    {
        if (jsxExpressionContainer.Expression == expression)
        {
            return jsxExpressionContainer;
        }
        
        return new JsxExpressionContainer(expression);
    }

    protected virtual JsxMemberExpression UpdateJsxMemberExpression(JsxMemberExpression jsxMemberExpression, JsxExpression obj,
        JsxIdentifier property)
    {
        if (jsxMemberExpression.Object == obj && jsxMemberExpression.Property == property)
        {
            return jsxMemberExpression;
        }

        return new JsxMemberExpression(obj, property);
    }

    protected virtual JsxNamespacedName UpdateJsxNamespacedName(JsxNamespacedName jsxNamespacedName, JsxIdentifier name,
        JsxIdentifier @namespace)
    {
        if (jsxNamespacedName.Name == name && jsxNamespacedName.Namespace == @namespace)
        {
            return jsxNamespacedName;
        }

        return new JsxNamespacedName(@namespace, name);
    }

    protected virtual JsxOpeningElement UpdateJsxOpeningElement(JsxOpeningElement jsxOpeningElement, JsxExpression name, bool isNewAttributes,
        ref NodeList<JsxExpression> attributes)
    {
        if (jsxOpeningElement.Name == name && !isNewAttributes)
        {
            return jsxOpeningElement;
        }

        return new JsxOpeningElement(name, jsxOpeningElement.SelfClosing, attributes);
    }

    protected virtual JsxOpeningFragment UpdateJsxOpeningFragment(JsxOpeningFragment jsxOpeningFragment)
    {
        return jsxOpeningFragment;
    }

    protected virtual JsxSpreadAttribute UpdateJsxSpreadAttribute(JsxSpreadAttribute jsxSpreadAttribute, Expression argument)
    {
        if (jsxSpreadAttribute.Argument == argument)
        {
            return jsxSpreadAttribute;
        }

        return new JsxSpreadAttribute(argument);
    }

    #endregion
}
