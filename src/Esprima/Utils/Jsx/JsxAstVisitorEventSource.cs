using Esprima.Ast.Jsx;

namespace Esprima.Utils.Jsx;

[Obsolete("This class is planned to be removed from the next stable version.")]
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

    public virtual object? VisitJsxAttribute(JsxAttribute jsxAttribute)
    {
        VisitingJsxAttribute?.Invoke(this, jsxAttribute);
        var node = _jsxVisitor.VisitJsxAttribute(jsxAttribute);
        VisitedJsxAttribute?.Invoke(this, jsxAttribute);
        return node;
    }

    public virtual object? VisitJsxClosingElement(JsxClosingElement jsxClosingElement)
    {
        VisitingJsxClosingElement?.Invoke(this, jsxClosingElement);
        var node = _jsxVisitor.VisitJsxClosingElement(jsxClosingElement);
        VisitedJsxClosingElement?.Invoke(this, jsxClosingElement);
        return node;
    }

    public virtual object? VisitJsxClosingFragment(JsxClosingFragment jsxClosingFragment)
    {
        VisitingJsxClosingFragment?.Invoke(this, jsxClosingFragment);
        var node = _jsxVisitor.VisitJsxClosingFragment(jsxClosingFragment);
        VisitedJsxClosingFragment?.Invoke(this, jsxClosingFragment);
        return node;
    }

    public virtual object? VisitJsxElement(JsxElement jsxElement)
    {
        VisitingJsxElement?.Invoke(this, jsxElement);
        var node = _jsxVisitor.VisitJsxElement(jsxElement);
        VisitedJsxElement?.Invoke(this, jsxElement);
        return node;
    }

    public virtual object? VisitJsxEmptyExpression(JsxEmptyExpression jsxEmptyExpression)
    {
        VisitingJsxEmptyExpression?.Invoke(this, jsxEmptyExpression);
        var node = _jsxVisitor.VisitJsxEmptyExpression(jsxEmptyExpression);
        VisitedJsxEmptyExpression?.Invoke(this, jsxEmptyExpression);
        return node;
    }

    public virtual object? VisitJsxExpressionContainer(JsxExpressionContainer jsxExpressionContainer)
    {
        VisitingJsxExpressionContainer?.Invoke(this, jsxExpressionContainer);
        var node = _jsxVisitor.VisitJsxExpressionContainer(jsxExpressionContainer);
        VisitedJsxExpressionContainer?.Invoke(this, jsxExpressionContainer);
        return node;
    }

    public virtual object? VisitJsxIdentifier(JsxIdentifier jsxIdentifier)
    {
        VisitingJsxIdentifier?.Invoke(this, jsxIdentifier);
        var node = _jsxVisitor.VisitJsxIdentifier(jsxIdentifier);
        VisitedJsxIdentifier?.Invoke(this, jsxIdentifier);
        return node;
    }

    public virtual object? VisitJsxMemberExpression(JsxMemberExpression jsxMemberExpression)
    {
        VisitingJsxMemberExpression?.Invoke(this, jsxMemberExpression);
        var node = _jsxVisitor.VisitJsxMemberExpression(jsxMemberExpression);
        VisitedJsxMemberExpression?.Invoke(this, jsxMemberExpression);
        return node;
    }

    public virtual object? VisitJsxNamespacedName(JsxNamespacedName jsxNamespacedName)
    {
        VisitingJsxNamespacedName?.Invoke(this, jsxNamespacedName);
        var node = _jsxVisitor.VisitJsxNamespacedName(jsxNamespacedName);
        VisitedJsxNamespacedName?.Invoke(this, jsxNamespacedName);
        return node;
    }

    public virtual object? VisitJsxOpeningElement(JsxOpeningElement jsxOpeningElement)
    {
        VisitingJsxOpeningElement?.Invoke(this, jsxOpeningElement);
        var node = _jsxVisitor.VisitJsxOpeningElement(jsxOpeningElement);
        VisitedJsxOpeningElement?.Invoke(this, jsxOpeningElement);
        return node;
    }

    public virtual object? VisitJsxOpeningFragment(JsxOpeningFragment jsxOpeningFragment)
    {
        VisitingJsxOpeningFragment?.Invoke(this, jsxOpeningFragment);
        var node = _jsxVisitor.VisitJsxOpeningFragment(jsxOpeningFragment);
        VisitedJsxOpeningFragment?.Invoke(this, jsxOpeningFragment);
        return node;
    }

    public virtual object? VisitJsxSpreadAttribute(JsxSpreadAttribute jsxSpreadAttribute)
    {
        VisitingJsxSpreadAttribute?.Invoke(this, jsxSpreadAttribute);
        var node = _jsxVisitor.VisitJsxSpreadAttribute(jsxSpreadAttribute);
        VisitedJsxSpreadAttribute?.Invoke(this, jsxSpreadAttribute);
        return node;
    }

    public virtual object? VisitJsxText(JsxText jsxText)
    {
        VisitingJsxText?.Invoke(this, jsxText);
        var node = _jsxVisitor.VisitJsxText(jsxText);
        VisitedJsxText?.Invoke(this, jsxText);
        return node;
    }
}
