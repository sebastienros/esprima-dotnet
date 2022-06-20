using Esprima.Ast.Jsx;

namespace Esprima.Utils.Jsx;

public class JsxAstRewriter : AstRewriter, IJsxAstVisitor
{
    /// <summary>
    /// Creates an <see cref="IJsxAstVisitor"/> instance which can be used for working around multiple inheritance:
    /// the returned instance re-routes visitations of JSX nodes to the specified <paramref name="rewriter"/>,
    /// thus it can be used for emulating base class method calls.
    /// </summary>
    public static IJsxAstVisitor CreateJsxRewriterFor(AstRewriter rewriter)
    {
        return new JsxAstRewriter(rewriter);
    }

    private readonly AstRewriter _rewriter;
    private readonly IJsxAstVisitor _jsxVisitor;

    public JsxAstRewriter()
    {
        _rewriter = this;
        _jsxVisitor = JsxAstVisitor.CreateJsxVisitorFor(this);
    }

    private JsxAstRewriter(AstRewriter rewriter)
    {
        _rewriter = rewriter;
        _jsxVisitor = JsxAstVisitor.CreateJsxVisitorFor(this);
    }

    public virtual object? VisitJsxAttribute(JsxAttribute jsxAttribute, object? context)
    {
        var name = _rewriter.VisitAndConvert(jsxAttribute.Name, context);
        var value = _rewriter.VisitAndConvert(jsxAttribute.Value, context, allowNull: true);

        return jsxAttribute.UpdateWith(name, value);
    }

    public virtual object? VisitJsxClosingElement(JsxClosingElement jsxClosingElement, object? context)
    {
        var name = _rewriter.VisitAndConvert(jsxClosingElement.Name, context);

        return jsxClosingElement.UpdateWith(name);
    }

    public virtual object? VisitJsxClosingFragment(JsxClosingFragment jsxClosingFragment, object? context)
    {
        return _jsxVisitor.VisitJsxClosingFragment(jsxClosingFragment, context);
    }

    public virtual object? VisitJsxElement(JsxElement jsxElement, object? context)
    {
        var openingElement = _rewriter.VisitAndConvert(jsxElement.OpeningElement, context);
        _rewriter.VisitAndConvert(jsxElement.Children, out var children, context);
        var closingElement = _rewriter.VisitAndConvert(jsxElement.ClosingElement, context, allowNull: true);

        return jsxElement.UpdateWith(openingElement, children, closingElement);
    }

    public virtual object? VisitJsxEmptyExpression(JsxEmptyExpression jsxEmptyExpression, object? context)
    {
        return _jsxVisitor.VisitJsxEmptyExpression(jsxEmptyExpression, context);
    }

    public virtual object? VisitJsxExpressionContainer(JsxExpressionContainer jsxExpressionContainer, object? context)
    {
        var expression = _rewriter.VisitAndConvert(jsxExpressionContainer.Expression, context);

        return jsxExpressionContainer.UpdateWith(expression);
    }

    public virtual object? VisitJsxIdentifier(JsxIdentifier jsxIdentifier, object? context)
    {
        return _jsxVisitor.VisitJsxIdentifier(jsxIdentifier, context);
    }

    public virtual object? VisitJsxMemberExpression(JsxMemberExpression jsxMemberExpression, object? context)
    {
        var obj = _rewriter.VisitAndConvert(jsxMemberExpression.Object, context);
        var property = _rewriter.VisitAndConvert(jsxMemberExpression.Property, context);

        return jsxMemberExpression.UpdateWith(obj, property);
    }

    public virtual object? VisitJsxNamespacedName(JsxNamespacedName jsxNamespacedName, object? context)
    {
        var name = _rewriter.VisitAndConvert(jsxNamespacedName.Name, context);
        var @namespace = _rewriter.VisitAndConvert(jsxNamespacedName.Namespace, context);

        return jsxNamespacedName.UpdateWith(name, @namespace);
    }

    public virtual object? VisitJsxOpeningElement(JsxOpeningElement jsxOpeningElement, object? context)
    {
        var name = _rewriter.VisitAndConvert(jsxOpeningElement.Name, context);
        _rewriter.VisitAndConvert(jsxOpeningElement.Attributes, out var attributes, context);

        return jsxOpeningElement.UpdateWith(name, attributes);
    }

    public virtual object? VisitJsxOpeningFragment(JsxOpeningFragment jsxOpeningFragment, object? context)
    {
        return _jsxVisitor.VisitJsxOpeningFragment(jsxOpeningFragment, context);
    }

    public virtual object? VisitJsxSpreadAttribute(JsxSpreadAttribute jsxSpreadAttribute, object? context)
    {
        var argument = _rewriter.VisitAndConvert(jsxSpreadAttribute.Argument, context);

        return jsxSpreadAttribute.UpdateWith(argument);
    }

    public virtual object? VisitJsxText(JsxText jsxText, object? context)
    {
        return _jsxVisitor.VisitJsxText(jsxText, context);
    }
}
