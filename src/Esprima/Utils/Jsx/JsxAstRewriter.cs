using Esprima.Ast.Jsx;

namespace Esprima.Utils.Jsx;

public class JsxAstRewriter : AstRewriter, IJsxAstVisitor
{
    /// <summary>
    /// Creates an <see cref="IJsxAstVisitor"/> instance which can be used for working around multiple inheritance:
    /// the returned instance re-routes visitations of JSX nodes to the specified <paramref name="rewriter"/>,
    /// thus it can be used for emulating base class method calls.
    /// </summary>
    public static IJsxAstVisitor CreateJsxRewriterFor<TRewriter>(TRewriter rewriter)
        where TRewriter : AstRewriter, IJsxAstVisitor
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

    public virtual object? VisitJsxAttribute(JsxAttribute jsxAttribute)
    {
        var name = _rewriter.VisitAndConvert(jsxAttribute.Name);
        var value = _rewriter.VisitAndConvert(jsxAttribute.Value, allowNull: true);

        return jsxAttribute.UpdateWith(name, value);
    }

    public virtual object? VisitJsxClosingElement(JsxClosingElement jsxClosingElement)
    {
        var name = _rewriter.VisitAndConvert(jsxClosingElement.Name);

        return jsxClosingElement.UpdateWith(name);
    }

    public virtual object? VisitJsxClosingFragment(JsxClosingFragment jsxClosingFragment)
    {
        return _jsxVisitor.VisitJsxClosingFragment(jsxClosingFragment);
    }

    public virtual object? VisitJsxElement(JsxElement jsxElement)
    {
        var openingElement = _rewriter.VisitAndConvert(jsxElement.OpeningElement);
        _rewriter.VisitAndConvert(jsxElement.Children, out var children);
        var closingElement = _rewriter.VisitAndConvert(jsxElement.ClosingElement, allowNull: true);

        return jsxElement.UpdateWith(openingElement, children, closingElement);
    }

    public virtual object? VisitJsxEmptyExpression(JsxEmptyExpression jsxEmptyExpression)
    {
        return _jsxVisitor.VisitJsxEmptyExpression(jsxEmptyExpression);
    }

    public virtual object? VisitJsxExpressionContainer(JsxExpressionContainer jsxExpressionContainer)
    {
        var expression = _rewriter.VisitAndConvert(jsxExpressionContainer.Expression);

        return jsxExpressionContainer.UpdateWith(expression);
    }

    public virtual object? VisitJsxIdentifier(JsxIdentifier jsxIdentifier)
    {
        return _jsxVisitor.VisitJsxIdentifier(jsxIdentifier);
    }

    public virtual object? VisitJsxMemberExpression(JsxMemberExpression jsxMemberExpression)
    {
        var obj = _rewriter.VisitAndConvert(jsxMemberExpression.Object);
        var property = _rewriter.VisitAndConvert(jsxMemberExpression.Property);

        return jsxMemberExpression.UpdateWith(obj, property);
    }

    public virtual object? VisitJsxNamespacedName(JsxNamespacedName jsxNamespacedName)
    {
        var name = _rewriter.VisitAndConvert(jsxNamespacedName.Name);
        var @namespace = _rewriter.VisitAndConvert(jsxNamespacedName.Namespace);

        return jsxNamespacedName.UpdateWith(name, @namespace);
    }

    public virtual object? VisitJsxOpeningElement(JsxOpeningElement jsxOpeningElement)
    {
        var name = _rewriter.VisitAndConvert(jsxOpeningElement.Name);
        _rewriter.VisitAndConvert(jsxOpeningElement.Attributes, out var attributes);

        return jsxOpeningElement.UpdateWith(name, attributes);
    }

    public virtual object? VisitJsxOpeningFragment(JsxOpeningFragment jsxOpeningFragment)
    {
        return _jsxVisitor.VisitJsxOpeningFragment(jsxOpeningFragment);
    }

    public virtual object? VisitJsxSpreadAttribute(JsxSpreadAttribute jsxSpreadAttribute)
    {
        var argument = _rewriter.VisitAndConvert(jsxSpreadAttribute.Argument);

        return jsxSpreadAttribute.UpdateWith(argument);
    }

    public virtual object? VisitJsxText(JsxText jsxText)
    {
        return _jsxVisitor.VisitJsxText(jsxText);
    }
}
