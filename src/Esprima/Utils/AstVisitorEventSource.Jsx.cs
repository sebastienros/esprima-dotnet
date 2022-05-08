using Esprima.Ast;

namespace Esprima.Utils;

public partial class AstVisitorEventSource
{
    public event EventHandler<Node>? VisitingJsxSpreadAttribute;
    public event EventHandler<Node>? VisitedJsxSpreadAttribute;
    public event EventHandler<Node>? VisitingJsxElement;
    public event EventHandler<Node>? VisitedJsxElement;
    public event EventHandler<Node>? VisitingJsxAttribute;
    public event EventHandler<Node>? VisitedJsxAttribute;
    public event EventHandler<Node>? VisitingJsxIdentifier;
    public event EventHandler<Node>? VisitedJsxIdentifier;
    public event EventHandler<Node>? VisitingJsxClosingElement;
    public event EventHandler<Node>? VisitedJsxClosingElement;
    public event EventHandler<Node>? VisitingJsxText;
    public event EventHandler<Node>? VisitedJsxText;
    public event EventHandler<Node>? VisitingJsxClosingFragment;
    public event EventHandler<Node>? VisitedJsxClosingFragment;
    public event EventHandler<Node>? VisitingJsxOpeningFragment;
    public event EventHandler<Node>? VisitedJsxOpeningFragment;
    public event EventHandler<Node>? VisitingJsxOpeningElement;
    public event EventHandler<Node>? VisitedJsxOpeningElement;
    public event EventHandler<Node>? VisitingJsxNamespacedName;
    public event EventHandler<Node>? VisitedJsxNamespacedName;
    public event EventHandler<Node>? VisitingJsxMemberExpression;
    public event EventHandler<Node>? VisitedJsxMemberExpression;
    public event EventHandler<Node>? VisitingJsxEmptyExpression;
    public event EventHandler<Node>? VisitedJsxEmptyExpression;
    public event EventHandler<Node>? VisitingJsxExpressionContainer;
    public event EventHandler<Node>? VisitedJsxExpressionContainer;
        
    protected internal override JsxSpreadAttribute VisitJsxSpreadAttribute(JsxSpreadAttribute jsxSpreadAttribute)
    {
        VisitingJsxSpreadAttribute?.Invoke(this, jsxSpreadAttribute);
        var node = base.VisitJsxSpreadAttribute(jsxSpreadAttribute);
        VisitedJsxSpreadAttribute?.Invoke(this, jsxSpreadAttribute);
        return node;
    }

    protected internal override JsxElement VisitJsxElement(JsxElement jsxElement)
    {
        VisitingJsxElement?.Invoke(this, jsxElement);
        var node = base.VisitJsxElement(jsxElement);
        VisitedJsxElement?.Invoke(this, jsxElement);
        return node;
    }

    protected internal override JsxAttribute VisitJsxAttribute(JsxAttribute jsxAttribute)
    {
        VisitingJsxAttribute?.Invoke(this, jsxAttribute);
        var node = base.VisitJsxAttribute(jsxAttribute);
        VisitedJsxAttribute?.Invoke(this, jsxAttribute);
        return node;
    }

    protected internal override JsxIdentifier VisitJsxIdentifier(JsxIdentifier jsxIdentifier)
    {
        VisitingJsxIdentifier?.Invoke(this, jsxIdentifier);
        var node = base.VisitJsxIdentifier(jsxIdentifier);
        VisitedJsxIdentifier?.Invoke(this, jsxIdentifier);
        return node;
    }

    protected internal override JsxClosingElement VisitJsxClosingElement(JsxClosingElement jsxClosingElement)
    {
        VisitingJsxClosingElement?.Invoke(this, jsxClosingElement);
        var node = base.VisitJsxClosingElement(jsxClosingElement);
        VisitedJsxClosingElement?.Invoke(this, jsxClosingElement);
        return node;
    }

    protected internal override JsxText VisitJsxText(JsxText jsxText)
    {
        VisitingJsxText?.Invoke(this, jsxText);
        var node = base.VisitJsxText(jsxText);
        VisitedJsxText?.Invoke(this, jsxText);
        return node;
    }

    protected internal override JsxClosingFragment VisitJsxClosingFragment(JsxClosingFragment jsxClosingFragment)
    {
        VisitingJsxClosingFragment?.Invoke(this, jsxClosingFragment);
        var node = base.VisitJsxClosingFragment(jsxClosingFragment);
        VisitedJsxClosingFragment?.Invoke(this, jsxClosingFragment);
        return node;
    }

    protected internal override JsxOpeningFragment VisitJsxOpeningFragment(JsxOpeningFragment jsxOpeningFragment)
    {
        VisitingJsxOpeningFragment?.Invoke(this, jsxOpeningFragment);
        var node = base.VisitJsxOpeningFragment(jsxOpeningFragment);
        VisitedJsxOpeningFragment?.Invoke(this, jsxOpeningFragment);
        return node;
    }

    protected internal override JsxOpeningElement VisitJsxOpeningElement(JsxOpeningElement jsxOpeningElement)
    {
        VisitingJsxOpeningElement?.Invoke(this, jsxOpeningElement);
        var node = base.VisitJsxOpeningElement(jsxOpeningElement);
        VisitedJsxOpeningElement?.Invoke(this, jsxOpeningElement);
        return node;
    }

    protected internal override JsxNamespacedName VisitJsxNamespacedName(JsxNamespacedName jsxNamespacedName)
    {
        VisitingJsxNamespacedName?.Invoke(this, jsxNamespacedName);
        var node = base.VisitJsxNamespacedName(jsxNamespacedName);
        VisitedJsxNamespacedName?.Invoke(this, jsxNamespacedName);
        return node;
    }

    protected internal override JsxMemberExpression VisitJsxMemberExpression(JsxMemberExpression jsxMemberExpression)
    {
        VisitingJsxMemberExpression?.Invoke(this, jsxMemberExpression);
        var node = base.VisitJsxMemberExpression(jsxMemberExpression);
        VisitedJsxMemberExpression?.Invoke(this, jsxMemberExpression);
        return node;
    }

    protected internal override JsxEmptyExpression VisitJsxEmptyExpression(JsxEmptyExpression jsxEmptyExpression)
    {
        VisitingJsxEmptyExpression?.Invoke(this, jsxEmptyExpression);
        var node = base.VisitJsxEmptyExpression(jsxEmptyExpression);
        VisitedJsxEmptyExpression?.Invoke(this, jsxEmptyExpression);
        return node;
    }

    protected internal override JsxExpressionContainer VisitJsxExpressionContainer(JsxExpressionContainer jsxExpressionContainer)
    {
        VisitingJsxExpressionContainer?.Invoke(this, jsxExpressionContainer);
        var node = base.VisitJsxExpressionContainer(jsxExpressionContainer);
        VisitedJsxExpressionContainer?.Invoke(this, jsxExpressionContainer);
        return node;
    }
}
