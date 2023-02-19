//HintName: Esprima.Utils.Jsx.JsxAstRewriter.g.cs
#nullable enable

namespace Esprima.Utils.Jsx;

partial class JsxAstRewriter
{
    public virtual object? VisitJsxAttribute(Esprima.Ast.Jsx.JsxAttribute jsxAttribute)
    {
        var name = _rewriter.VisitAndConvert(jsxAttribute.Name);

        var value = _rewriter.VisitAndConvert(jsxAttribute.Value, allowNull: true);

        return jsxAttribute.UpdateWith(name, value);
    }

    public virtual object? VisitJsxClosingElement(Esprima.Ast.Jsx.JsxClosingElement jsxClosingElement)
    {
        var name = _rewriter.VisitAndConvert(jsxClosingElement.Name);

        return jsxClosingElement.UpdateWith(name);
    }

    public virtual object? VisitJsxClosingFragment(Esprima.Ast.Jsx.JsxClosingFragment jsxClosingFragment)
    {
        return _jsxVisitor.VisitJsxClosingFragment(jsxClosingFragment);
    }

    public virtual object? VisitJsxElement(Esprima.Ast.Jsx.JsxElement jsxElement)
    {
        var openingElement = _rewriter.VisitAndConvert(jsxElement.OpeningElement);

        _rewriter.VisitAndConvert(jsxElement.Children, out var children);

        var closingElement = _rewriter.VisitAndConvert(jsxElement.ClosingElement, allowNull: true);

        return jsxElement.UpdateWith(openingElement, children, closingElement);
    }

    public virtual object? VisitJsxEmptyExpression(Esprima.Ast.Jsx.JsxEmptyExpression jsxEmptyExpression)
    {
        return _jsxVisitor.VisitJsxEmptyExpression(jsxEmptyExpression);
    }

    public virtual object? VisitJsxExpressionContainer(Esprima.Ast.Jsx.JsxExpressionContainer jsxExpressionContainer)
    {
        var expression = _rewriter.VisitAndConvert(jsxExpressionContainer.Expression);

        return jsxExpressionContainer.UpdateWith(expression);
    }

    public virtual object? VisitJsxIdentifier(Esprima.Ast.Jsx.JsxIdentifier jsxIdentifier)
    {
        return _jsxVisitor.VisitJsxIdentifier(jsxIdentifier);
    }

    public virtual object? VisitJsxMemberExpression(Esprima.Ast.Jsx.JsxMemberExpression jsxMemberExpression)
    {
        var @object = _rewriter.VisitAndConvert(jsxMemberExpression.Object);

        var property = _rewriter.VisitAndConvert(jsxMemberExpression.Property);

        return jsxMemberExpression.UpdateWith(@object, property);
    }

    public virtual object? VisitJsxNamespacedName(Esprima.Ast.Jsx.JsxNamespacedName jsxNamespacedName)
    {
        var name = _rewriter.VisitAndConvert(jsxNamespacedName.Name);

        var @namespace = _rewriter.VisitAndConvert(jsxNamespacedName.Namespace);

        return jsxNamespacedName.UpdateWith(name, @namespace);
    }

    public virtual object? VisitJsxOpeningElement(Esprima.Ast.Jsx.JsxOpeningElement jsxOpeningElement)
    {
        var name = _rewriter.VisitAndConvert(jsxOpeningElement.Name);

        _rewriter.VisitAndConvert(jsxOpeningElement.Attributes, out var attributes);

        return jsxOpeningElement.UpdateWith(name, attributes);
    }

    public virtual object? VisitJsxOpeningFragment(Esprima.Ast.Jsx.JsxOpeningFragment jsxOpeningFragment)
    {
        return _jsxVisitor.VisitJsxOpeningFragment(jsxOpeningFragment);
    }

    public virtual object? VisitJsxSpreadAttribute(Esprima.Ast.Jsx.JsxSpreadAttribute jsxSpreadAttribute)
    {
        var argument = _rewriter.VisitAndConvert(jsxSpreadAttribute.Argument);

        return jsxSpreadAttribute.UpdateWith(argument);
    }

    public virtual object? VisitJsxText(Esprima.Ast.Jsx.JsxText jsxText)
    {
        return _jsxVisitor.VisitJsxText(jsxText);
    }
}
