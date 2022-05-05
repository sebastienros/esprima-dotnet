using Esprima.Ast.Jsx;

namespace Esprima.Utils.Jsx;

public class JsxAstVisitor : AstVisitor, IJsxAstVisitor
{
    /// <summary>
    /// Creates an <see cref="IJsxAstVisitor"/> instance which can be used for working around multiple inheritance:
    /// the returned instance re-routes visitations of JSX nodes to the specified <paramref name="visitor"/>,
    /// thus it can be used for emulating base class method calls.
    /// </summary>
    public static IJsxAstVisitor CreateJsxVisitorFor(AstVisitor visitor)
    {
        return new JsxAstVisitor(visitor);
    }

    private readonly AstVisitor _visitor;

    public JsxAstVisitor()
    {
        _visitor = this;
    }

    private JsxAstVisitor(AstVisitor visitor)
    {
        _visitor = visitor;
    }

    public virtual object? VisitJsxAttribute(JsxAttribute jsxAttribute)
    {
        _visitor.Visit(jsxAttribute.Name);
        if (jsxAttribute.Value is not null)
        {
            _visitor.Visit(jsxAttribute.Value);
        }

        return jsxAttribute;
    }

    public virtual object? VisitJsxClosingElement(JsxClosingElement jsxClosingElement)
    {
        _visitor.Visit(jsxClosingElement.Name);

        return jsxClosingElement;
    }

    public virtual object? VisitJsxClosingFragment(JsxClosingFragment jsxClosingFragment)
    {
        return jsxClosingFragment;
    }

    public virtual object? VisitJsxElement(JsxElement jsxElement)
    {
        _visitor.Visit(jsxElement.OpeningElement);
        foreach (var child in jsxElement.Children)
        {
            _visitor.Visit(child);
        }

        if (jsxElement.ClosingElement is not null)
        {
            _visitor.Visit(jsxElement.ClosingElement);
        }

        return jsxElement;
    }

    public virtual object? VisitJsxEmptyExpression(JsxEmptyExpression jsxEmptyExpression)
    {
        return jsxEmptyExpression;
    }

    public virtual object? VisitJsxExpressionContainer(JsxExpressionContainer jsxExpressionContainer)
    {
        _visitor.Visit(jsxExpressionContainer.Expression);

        return jsxExpressionContainer;
    }

    public virtual object? VisitJsxIdentifier(JsxIdentifier jsxIdentifier)
    {
        return jsxIdentifier;
    }

    public virtual object? VisitJsxMemberExpression(JsxMemberExpression jsxMemberExpression)
    {
        _visitor.Visit(jsxMemberExpression.Object);
        _visitor.Visit(jsxMemberExpression.Property);

        return jsxMemberExpression;
    }

    public virtual object? VisitJsxNamespacedName(JsxNamespacedName jsxNamespacedName)
    {
        _visitor.Visit(jsxNamespacedName.Name);
        _visitor.Visit(jsxNamespacedName.Namespace);

        return jsxNamespacedName;
    }

    public virtual object? VisitJsxOpeningElement(JsxOpeningElement jsxOpeningElement)
    {
        _visitor.Visit(jsxOpeningElement.Name);
        foreach (var attribute in jsxOpeningElement.Attributes)
        {
            _visitor.Visit(attribute);
        }

        return jsxOpeningElement;
    }

    public virtual object? VisitJsxOpeningFragment(JsxOpeningFragment jsxOpeningFragment)
    {
        return jsxOpeningFragment;
    }

    public virtual object? VisitJsxSpreadAttribute(JsxSpreadAttribute jsxSpreadAttribute)
    {
        _visitor.Visit(jsxSpreadAttribute.Argument);

        return jsxSpreadAttribute;
    }

    public virtual object? VisitJsxText(JsxText jsxText)
    {
        return jsxText;
    }
}
