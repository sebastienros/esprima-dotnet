using Esprima.Ast;

namespace Esprima.Utils;

public abstract partial class AstRewriter
{
    protected internal override object? VisitJsxMemberExpression(JsxMemberExpression jsxMemberExpression)
    {
        var obj = VisitAndConvert(jsxMemberExpression.Object);
        var property = VisitAndConvert(jsxMemberExpression.Property);

        return jsxMemberExpression.Update(obj, property);
    }

    protected internal override object? VisitJsxElement(JsxElement jsxElement)
    {
        var openingElement = VisitAndConvert(jsxElement.OpeningElement);
        VisitAndConvert(jsxElement.Children, out var children);
        var closingElement = VisitAndConvert(jsxElement.ClosingElement, allowNull: true);

        return jsxElement.Update(openingElement, children, closingElement);
    }

    protected internal override object? VisitJsxOpeningElement(JsxOpeningElement jsxOpeningElement)
    {
        var name = VisitAndConvert(jsxOpeningElement.Name);
        VisitAndConvert(jsxOpeningElement.Attributes, out var attributes);

        return jsxOpeningElement.Update(name, attributes);
    }

    protected internal override object? VisitJsxClosingElement(JsxClosingElement jsxClosingElement)
    {
        var name = VisitAndConvert(jsxClosingElement.Name);

        return jsxClosingElement.Update(name);
    }

    protected internal override object? VisitJsxNamespacedName(JsxNamespacedName jsxNamespacedName)
    {
        var name = VisitAndConvert(jsxNamespacedName.Name);
        var @namespace = VisitAndConvert(jsxNamespacedName.Namespace);

        return jsxNamespacedName.Update(name, @namespace);
    }

    protected internal override object? VisitJsxSpreadAttribute(JsxSpreadAttribute jsxSpreadAttribute)
    {
        var argument = VisitAndConvert(jsxSpreadAttribute.Argument);

        return jsxSpreadAttribute.Update(argument);
    }

    protected internal override object? VisitJsxAttribute(JsxAttribute jsxAttribute)
    {
        var name = VisitAndConvert(jsxAttribute.Name);
        var value = VisitAndConvert(jsxAttribute.Value, allowNull: true);

        return jsxAttribute.Update(name, value);
    }

    protected internal override object? VisitJsxExpressionContainer(JsxExpressionContainer jsxExpressionContainer)
    {
        var expression = VisitAndConvert(jsxExpressionContainer.Expression);

        return jsxExpressionContainer.Update(expression);
    }
}
