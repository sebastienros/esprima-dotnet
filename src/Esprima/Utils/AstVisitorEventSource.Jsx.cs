using System;
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
        
    protected internal override void VisitJsxSpreadAttribute(JsxSpreadAttribute jsxSpreadAttribute)
    {
        VisitingJsxSpreadAttribute?.Invoke(this, jsxSpreadAttribute);
        base.VisitJsxSpreadAttribute(jsxSpreadAttribute);
        VisitedJsxSpreadAttribute?.Invoke(this, jsxSpreadAttribute);
    }

    protected internal override void VisitJsxElement(JsxElement jsxElement)
    {
        VisitingJsxElement?.Invoke(this, jsxElement);
        base.VisitJsxElement(jsxElement);
        VisitedJsxElement?.Invoke(this, jsxElement);
    }

    protected internal override void VisitJsxAttribute(JsxAttribute jsxAttribute)
    {
        VisitingJsxAttribute?.Invoke(this, jsxAttribute);
        base.VisitJsxAttribute(jsxAttribute);
        VisitedJsxAttribute?.Invoke(this, jsxAttribute);
    }

    protected internal override void VisitJsxIdentifier(JsxIdentifier jsxIdentifier)
    {
        VisitingJsxIdentifier?.Invoke(this, jsxIdentifier);
        base.VisitJsxIdentifier(jsxIdentifier);
        VisitedJsxIdentifier?.Invoke(this, jsxIdentifier);
    }

    protected internal override void VisitJsxClosingElement(JsxClosingElement jsxClosingElement)
    {
        VisitingJsxClosingElement?.Invoke(this, jsxClosingElement);
        base.VisitJsxClosingElement(jsxClosingElement);
        VisitedJsxClosingElement?.Invoke(this, jsxClosingElement);
    }

    protected internal override void VisitJsxText(JsxText jsxText)
    {
        VisitingJsxText?.Invoke(this, jsxText);
        base.VisitJsxText(jsxText);
        VisitedJsxText?.Invoke(this, jsxText);
    }

    protected internal override void VisitJsxClosingFragment(JsxClosingFragment jsxClosingFragment)
    {
        VisitingJsxClosingFragment?.Invoke(this, jsxClosingFragment);
        base.VisitJsxClosingFragment(jsxClosingFragment);
        VisitedJsxClosingFragment?.Invoke(this, jsxClosingFragment);
    }

    protected internal override void VisitJsxOpeningFragment(JsxOpeningFragment jsxOpeningFragment)
    {
        VisitingJsxOpeningFragment?.Invoke(this, jsxOpeningFragment);
        base.VisitJsxOpeningFragment(jsxOpeningFragment);
        VisitedJsxOpeningFragment?.Invoke(this, jsxOpeningFragment);
    }

    protected internal override void VisitJsxOpeningElement(JsxOpeningElement jsxOpeningElement)
    {
        VisitingJsxOpeningElement?.Invoke(this, jsxOpeningElement);
        base.VisitJsxOpeningElement(jsxOpeningElement);
        VisitedJsxOpeningElement?.Invoke(this, jsxOpeningElement);
    }

    protected internal override void VisitJsxNamespacedName(JsxNamespacedName jsxNamespacedName)
    {
        VisitingJsxNamespacedName?.Invoke(this, jsxNamespacedName);
        base.VisitJsxNamespacedName(jsxNamespacedName);
        VisitedJsxNamespacedName?.Invoke(this, jsxNamespacedName);
    }

    protected internal override void VisitJsxMemberExpression(JsxMemberExpression jsxMemberExpression)
    {
        VisitingJsxMemberExpression?.Invoke(this, jsxMemberExpression);
        base.VisitJsxMemberExpression(jsxMemberExpression);
        VisitedJsxMemberExpression?.Invoke(this, jsxMemberExpression);
    }

    protected internal override void VisitJsxEmptyExpression(JsxEmptyExpression jsxEmptyExpression)
    {
        VisitingJsxEmptyExpression?.Invoke(this, jsxEmptyExpression);
        base.VisitJsxEmptyExpression(jsxEmptyExpression);
        VisitedJsxEmptyExpression?.Invoke(this, jsxEmptyExpression);
    }

    protected internal override void VisitJsxExpressionContainer(JsxExpressionContainer jsxExpressionContainer)
    {
        VisitingJsxExpressionContainer?.Invoke(this, jsxExpressionContainer);
        base.VisitJsxExpressionContainer(jsxExpressionContainer);
        VisitedJsxExpressionContainer?.Invoke(this, jsxExpressionContainer);
    }
}
