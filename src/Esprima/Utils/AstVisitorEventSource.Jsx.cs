using Esprima.Jsx.Ast;

namespace Esprima.Utils;

public partial class AstVisitorEventSource
{
    public event EventHandler<JsxSpreadAttribute>? VisitingJsxSpreadAttribute;
    public event EventHandler<JsxSpreadAttribute>? VisitedJsxSpreadAttribute;
    public event EventHandler<JsxElement>? VisitingJsxElement;
    public event EventHandler<JsxElement>? VisitedJsxElement;
    public event EventHandler<JsxAttribute>? VisitingJsxAttribute;
    public event EventHandler<JsxAttribute>? VisitedJsxAttribute;
    public event EventHandler<JsxIdentifier>? VisitingJsxIdentifier;
    public event EventHandler<JsxIdentifier>? VisitedJsxIdentifier;
    public event EventHandler<JsxClosingElement>? VisitingJsxClosingElement;
    public event EventHandler<JsxClosingElement>? VisitedJsxClosingElement;
    public event EventHandler<JsxText>? VisitingJsxText;
    public event EventHandler<JsxText>? VisitedJsxText;
    public event EventHandler<JsxClosingFragment>? VisitingJsxClosingFragment;
    public event EventHandler<JsxClosingFragment>? VisitedJsxClosingFragment;
    public event EventHandler<JsxOpeningFragment>? VisitingJsxOpeningFragment;
    public event EventHandler<JsxOpeningFragment>? VisitedJsxOpeningFragment;
    public event EventHandler<JsxOpeningElement>? VisitingJsxOpeningElement;
    public event EventHandler<JsxOpeningElement>? VisitedJsxOpeningElement;
    public event EventHandler<JsxNamespacedName>? VisitingJsxNamespacedName;
    public event EventHandler<JsxNamespacedName>? VisitedJsxNamespacedName;
    public event EventHandler<JsxMemberExpression>? VisitingJsxMemberExpression;
    public event EventHandler<JsxMemberExpression>? VisitedJsxMemberExpression;
    public event EventHandler<JsxEmptyExpression>? VisitingJsxEmptyExpression;
    public event EventHandler<JsxEmptyExpression>? VisitedJsxEmptyExpression;
    public event EventHandler<JsxExpressionContainer>? VisitingJsxExpressionContainer;
    public event EventHandler<JsxExpressionContainer>? VisitedJsxExpressionContainer;

    protected internal override object? VisitJsxSpreadAttribute(JsxSpreadAttribute jsxSpreadAttribute)
    {
        VisitingJsxSpreadAttribute?.Invoke(this, jsxSpreadAttribute);
        var node = base.VisitJsxSpreadAttribute(jsxSpreadAttribute);
        VisitedJsxSpreadAttribute?.Invoke(this, jsxSpreadAttribute);
        return node;
    }

    protected internal override object? VisitJsxElement(JsxElement jsxElement)
    {
        VisitingJsxElement?.Invoke(this, jsxElement);
        var node = base.VisitJsxElement(jsxElement);
        VisitedJsxElement?.Invoke(this, jsxElement);
        return node;
    }

    protected internal override object? VisitJsxAttribute(JsxAttribute jsxAttribute)
    {
        VisitingJsxAttribute?.Invoke(this, jsxAttribute);
        var node = base.VisitJsxAttribute(jsxAttribute);
        VisitedJsxAttribute?.Invoke(this, jsxAttribute);
        return node;
    }

    protected internal override object? VisitJsxIdentifier(JsxIdentifier jsxIdentifier)
    {
        VisitingJsxIdentifier?.Invoke(this, jsxIdentifier);
        var node = base.VisitJsxIdentifier(jsxIdentifier);
        VisitedJsxIdentifier?.Invoke(this, jsxIdentifier);
        return node;
    }

    protected internal override object? VisitJsxClosingElement(JsxClosingElement jsxClosingElement)
    {
        VisitingJsxClosingElement?.Invoke(this, jsxClosingElement);
        var node = base.VisitJsxClosingElement(jsxClosingElement);
        VisitedJsxClosingElement?.Invoke(this, jsxClosingElement);
        return node;
    }

    protected internal override object? VisitJsxText(JsxText jsxText)
    {
        VisitingJsxText?.Invoke(this, jsxText);
        var node = base.VisitJsxText(jsxText);
        VisitedJsxText?.Invoke(this, jsxText);
        return node;
    }

    protected internal override object? VisitJsxClosingFragment(JsxClosingFragment jsxClosingFragment)
    {
        VisitingJsxClosingFragment?.Invoke(this, jsxClosingFragment);
        var node = base.VisitJsxClosingFragment(jsxClosingFragment);
        VisitedJsxClosingFragment?.Invoke(this, jsxClosingFragment);
        return node;
    }

    protected internal override object? VisitJsxOpeningFragment(JsxOpeningFragment jsxOpeningFragment)
    {
        VisitingJsxOpeningFragment?.Invoke(this, jsxOpeningFragment);
        var node = base.VisitJsxOpeningFragment(jsxOpeningFragment);
        VisitedJsxOpeningFragment?.Invoke(this, jsxOpeningFragment);
        return node;
    }

    protected internal override object? VisitJsxOpeningElement(JsxOpeningElement jsxOpeningElement)
    {
        VisitingJsxOpeningElement?.Invoke(this, jsxOpeningElement);
        var node = base.VisitJsxOpeningElement(jsxOpeningElement);
        VisitedJsxOpeningElement?.Invoke(this, jsxOpeningElement);
        return node;
    }

    protected internal override object? VisitJsxNamespacedName(JsxNamespacedName jsxNamespacedName)
    {
        VisitingJsxNamespacedName?.Invoke(this, jsxNamespacedName);
        var node = base.VisitJsxNamespacedName(jsxNamespacedName);
        VisitedJsxNamespacedName?.Invoke(this, jsxNamespacedName);
        return node;
    }

    protected internal override object? VisitJsxMemberExpression(JsxMemberExpression jsxMemberExpression)
    {
        VisitingJsxMemberExpression?.Invoke(this, jsxMemberExpression);
        var node = base.VisitJsxMemberExpression(jsxMemberExpression);
        VisitedJsxMemberExpression?.Invoke(this, jsxMemberExpression);
        return node;
    }

    protected internal override object? VisitJsxEmptyExpression(JsxEmptyExpression jsxEmptyExpression)
    {
        VisitingJsxEmptyExpression?.Invoke(this, jsxEmptyExpression);
        var node = base.VisitJsxEmptyExpression(jsxEmptyExpression);
        VisitedJsxEmptyExpression?.Invoke(this, jsxEmptyExpression);
        return node;
    }

    protected internal override object? VisitJsxExpressionContainer(JsxExpressionContainer jsxExpressionContainer)
    {
        VisitingJsxExpressionContainer?.Invoke(this, jsxExpressionContainer);
        var node = base.VisitJsxExpressionContainer(jsxExpressionContainer);
        VisitedJsxExpressionContainer?.Invoke(this, jsxExpressionContainer);
        return node;
    }
}
