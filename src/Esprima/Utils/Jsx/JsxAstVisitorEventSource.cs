using Esprima.Ast.Jsx;

namespace Esprima.Utils.Jsx;

public class JsxAstVisitorEventSource : AstVisitorEventSource, IJsxAstVisitor
{
    public event EventHandler<JsxAttribute>? VisitingJsxAttribute;
    public event EventHandler<JsxAttribute>? VisitedJsxAttribute;
    public event EventHandler<JsxClosingElement>? VisitingJsxClosingElement;
    public event EventHandler<JsxClosingElement>? VisitedJsxClosingElement;
    public event EventHandler<JsxClosingFragment>? VisitingJsxClosingFragment;
    public event EventHandler<JsxClosingFragment>? VisitedJsxClosingFragment;
    public event EventHandler<JsxElement>? VisitingJsxElement;
    public event EventHandler<JsxElement>? VisitedJsxElement;
    public event EventHandler<JsxEmptyExpression>? VisitingJsxEmptyExpression;
    public event EventHandler<JsxEmptyExpression>? VisitedJsxEmptyExpression;
    public event EventHandler<JsxExpressionContainer>? VisitingJsxExpressionContainer;
    public event EventHandler<JsxExpressionContainer>? VisitedJsxExpressionContainer;
    public event EventHandler<JsxIdentifier>? VisitingJsxIdentifier;
    public event EventHandler<JsxIdentifier>? VisitedJsxIdentifier;
    public event EventHandler<JsxMemberExpression>? VisitingJsxMemberExpression;
    public event EventHandler<JsxMemberExpression>? VisitedJsxMemberExpression;
    public event EventHandler<JsxNamespacedName>? VisitingJsxNamespacedName;
    public event EventHandler<JsxNamespacedName>? VisitedJsxNamespacedName;
    public event EventHandler<JsxOpeningElement>? VisitingJsxOpeningElement;
    public event EventHandler<JsxOpeningElement>? VisitedJsxOpeningElement;
    public event EventHandler<JsxOpeningFragment>? VisitingJsxOpeningFragment;
    public event EventHandler<JsxOpeningFragment>? VisitedJsxOpeningFragment;
    public event EventHandler<JsxSpreadAttribute>? VisitingJsxSpreadAttribute;
    public event EventHandler<JsxSpreadAttribute>? VisitedJsxSpreadAttribute;
    public event EventHandler<JsxText>? VisitingJsxText;
    public event EventHandler<JsxText>? VisitedJsxText;

    private readonly IJsxAstVisitor _jsxVisitor;

    public JsxAstVisitorEventSource()
    {
        _jsxVisitor = JsxAstVisitor.CreateJsxVisitorFor(this);
    }

    public virtual object? VisitJsxAttribute(JsxAttribute jsxAttribute, object? context)
    {
        VisitingJsxAttribute?.Invoke(this, jsxAttribute);
        var node = _jsxVisitor.VisitJsxAttribute(jsxAttribute, context);
        VisitedJsxAttribute?.Invoke(this, jsxAttribute);
        return node;
    }

    public virtual object? VisitJsxClosingElement(JsxClosingElement jsxClosingElement, object? context)
    {
        VisitingJsxClosingElement?.Invoke(this, jsxClosingElement);
        var node = _jsxVisitor.VisitJsxClosingElement(jsxClosingElement, context);
        VisitedJsxClosingElement?.Invoke(this, jsxClosingElement);
        return node;
    }

    public virtual object? VisitJsxClosingFragment(JsxClosingFragment jsxClosingFragment, object? context)
    {
        VisitingJsxClosingFragment?.Invoke(this, jsxClosingFragment);
        var node = _jsxVisitor.VisitJsxClosingFragment(jsxClosingFragment, context);
        VisitedJsxClosingFragment?.Invoke(this, jsxClosingFragment);
        return node;
    }

    public virtual object? VisitJsxElement(JsxElement jsxElement, object? context)
    {
        VisitingJsxElement?.Invoke(this, jsxElement);
        var node = _jsxVisitor.VisitJsxElement(jsxElement, context);
        VisitedJsxElement?.Invoke(this, jsxElement);
        return node;
    }

    public virtual object? VisitJsxEmptyExpression(JsxEmptyExpression jsxEmptyExpression, object? context)
    {
        VisitingJsxEmptyExpression?.Invoke(this, jsxEmptyExpression);
        var node = _jsxVisitor.VisitJsxEmptyExpression(jsxEmptyExpression, context);
        VisitedJsxEmptyExpression?.Invoke(this, jsxEmptyExpression);
        return node;
    }

    public virtual object? VisitJsxExpressionContainer(JsxExpressionContainer jsxExpressionContainer, object? context)
    {
        VisitingJsxExpressionContainer?.Invoke(this, jsxExpressionContainer);
        var node = _jsxVisitor.VisitJsxExpressionContainer(jsxExpressionContainer, context);
        VisitedJsxExpressionContainer?.Invoke(this, jsxExpressionContainer);
        return node;
    }

    public virtual object? VisitJsxIdentifier(JsxIdentifier jsxIdentifier, object? context)
    {
        VisitingJsxIdentifier?.Invoke(this, jsxIdentifier);
        var node = _jsxVisitor.VisitJsxIdentifier(jsxIdentifier, context);
        VisitedJsxIdentifier?.Invoke(this, jsxIdentifier);
        return node;
    }

    public virtual object? VisitJsxMemberExpression(JsxMemberExpression jsxMemberExpression, object? context)
    {
        VisitingJsxMemberExpression?.Invoke(this, jsxMemberExpression);
        var node = _jsxVisitor.VisitJsxMemberExpression(jsxMemberExpression, context);
        VisitedJsxMemberExpression?.Invoke(this, jsxMemberExpression);
        return node;
    }

    public virtual object? VisitJsxNamespacedName(JsxNamespacedName jsxNamespacedName, object? context)
    {
        VisitingJsxNamespacedName?.Invoke(this, jsxNamespacedName);
        var node = _jsxVisitor.VisitJsxNamespacedName(jsxNamespacedName, context);
        VisitedJsxNamespacedName?.Invoke(this, jsxNamespacedName);
        return node;
    }

    public virtual object? VisitJsxOpeningElement(JsxOpeningElement jsxOpeningElement, object? context)
    {
        VisitingJsxOpeningElement?.Invoke(this, jsxOpeningElement);
        var node = _jsxVisitor.VisitJsxOpeningElement(jsxOpeningElement, context);
        VisitedJsxOpeningElement?.Invoke(this, jsxOpeningElement);
        return node;
    }

    public virtual object? VisitJsxOpeningFragment(JsxOpeningFragment jsxOpeningFragment, object? context)
    {
        VisitingJsxOpeningFragment?.Invoke(this, jsxOpeningFragment);
        var node = _jsxVisitor.VisitJsxOpeningFragment(jsxOpeningFragment, context);
        VisitedJsxOpeningFragment?.Invoke(this, jsxOpeningFragment);
        return node;
    }

    public virtual object? VisitJsxSpreadAttribute(JsxSpreadAttribute jsxSpreadAttribute, object? context)
    {
        VisitingJsxSpreadAttribute?.Invoke(this, jsxSpreadAttribute);
        var node = _jsxVisitor.VisitJsxSpreadAttribute(jsxSpreadAttribute, context);
        VisitedJsxSpreadAttribute?.Invoke(this, jsxSpreadAttribute);
        return node;
    }

    public virtual object? VisitJsxText(JsxText jsxText, object? context)
    {
        VisitingJsxText?.Invoke(this, jsxText);
        var node = _jsxVisitor.VisitJsxText(jsxText, context);
        VisitedJsxText?.Invoke(this, jsxText);
        return node;
    }
}
