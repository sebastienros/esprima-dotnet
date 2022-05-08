using Esprima.Ast;

namespace Esprima.Utils;

public partial class AstVisitor
{
    protected internal virtual JsxMemberExpression VisitJsxMemberExpression(JsxMemberExpression jsxMemberExpression)
    {
        var obj = Visit(jsxMemberExpression.Object);
        var property = Visit(jsxMemberExpression.Property);
        return UpdateJsxMemberExpression(jsxMemberExpression, obj, property);
    }

    protected internal virtual JsxText VisitJsxText(JsxText jsxText)
    {
        return UpdateJsxText(jsxText);
    }

    protected internal virtual JsxOpeningFragment VisitJsxOpeningFragment(JsxOpeningFragment jsxOpeningFragment)
    {
        return UpdateJsxOpeningFragment(jsxOpeningFragment);
    }

    protected internal virtual JsxClosingFragment VisitJsxClosingFragment(JsxClosingFragment jsxClosingFragment)
    {
        return UpdateJsxClosingFragment(jsxClosingFragment);
    }

    protected internal virtual JsxIdentifier VisitJsxIdentifier(JsxIdentifier jsxIdentifier)
    {
        return UpdateJsxIdentifier(jsxIdentifier);
    }

    protected internal virtual JsxElement VisitJsxElement(JsxElement jsxElement)
    {
        var openingElement = Visit(jsxElement.OpeningElement);
        var isNewChildren = HasNodeListChanged(jsxElement.Children, out var children);
        Node? closingElement = null;
        if (jsxElement.ClosingElement is not null)
        {
            closingElement = Visit(jsxElement.ClosingElement);
        }

        return UpdateJsxElement(jsxElement, openingElement, isNewChildren, ref children, closingElement);
    }

    protected internal virtual JsxOpeningElement VisitJsxOpeningElement(JsxOpeningElement jsxOpeningElement)
    {
        var name = Visit(jsxOpeningElement.Name);
        var isNewAttributes = HasNodeListChanged(jsxOpeningElement.Attributes, out var attributes);
        return UpdateJsxOpeningElement(jsxOpeningElement, name, isNewAttributes, ref attributes);
    }

    protected internal virtual JsxClosingElement VisitJsxClosingElement(JsxClosingElement jsxClosingElement)
    {
        var name = Visit(jsxClosingElement.Name);
        return UpdateJsxClosingElement(jsxClosingElement, name);
    }

    protected internal virtual JsxEmptyExpression VisitJsxEmptyExpression(JsxEmptyExpression jsxEmptyExpression)
    {
        return UpdateJsxEmptyExpression(jsxEmptyExpression);
    }

    protected internal virtual JsxNamespacedName VisitJsxNamespacedName(JsxNamespacedName jsxNamespacedName)
    {
        var name = Visit(jsxNamespacedName.Name);
        var @namespace = Visit(jsxNamespacedName.Namespace);
        return UpdateJsxNamespacedName(jsxNamespacedName, name, @namespace);
    }

    protected internal virtual JsxSpreadAttribute VisitJsxSpreadAttribute(JsxSpreadAttribute jsxSpreadAttribute)
    {
        var argument = Visit(jsxSpreadAttribute.Argument);
        return UpdateJsxSpreadAttribute(jsxSpreadAttribute, argument);
    }

    protected internal virtual JsxAttribute VisitJsxAttribute(JsxAttribute jsxAttribute)
    {
        var name = Visit(jsxAttribute.Name);
        Expression? value = null;
        if (jsxAttribute.Value is not null)
        {
            value = Visit(jsxAttribute.Value);
        }

        return UpdateJsxAttribute(jsxAttribute, name, value);
    }

    protected internal virtual JsxExpressionContainer VisitJsxExpressionContainer(
        JsxExpressionContainer jsxExpressionContainer)
    {
        var expression = Visit(jsxExpressionContainer.Expression);
        return UpdateJsxExpressionContainer(jsxExpressionContainer, expression);
    }
}
