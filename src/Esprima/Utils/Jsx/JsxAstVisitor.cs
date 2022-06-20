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

    public virtual object? VisitJsxAttribute(JsxAttribute jsxAttribute, object? context)
    {
        _visitor.Visit(jsxAttribute.Name, context);
        if (jsxAttribute.Value is not null)
        {
            _visitor.Visit(jsxAttribute.Value, context);
        }

        return jsxAttribute;
    }

    public virtual object? VisitJsxClosingElement(JsxClosingElement jsxClosingElement, object? context)
    {
        _visitor.Visit(jsxClosingElement.Name, context);

        return jsxClosingElement;
    }

    public virtual object? VisitJsxClosingFragment(JsxClosingFragment jsxClosingFragment, object? context)
    {
        return jsxClosingFragment;
    }

    public virtual object? VisitJsxElement(JsxElement jsxElement, object? context)
    {
        _visitor.Visit(jsxElement.OpeningElement, context);
        ref readonly var children = ref jsxElement.Children;
        for (var i = 0; i < children.Count; i++)
        {
            _visitor.Visit(children[i], context);
        }

        if (jsxElement.ClosingElement is not null)
        {
            _visitor.Visit(jsxElement.ClosingElement, context);
        }

        return jsxElement;
    }

    public virtual object? VisitJsxEmptyExpression(JsxEmptyExpression jsxEmptyExpression, object? context)
    {
        return jsxEmptyExpression;
    }

    public virtual object? VisitJsxExpressionContainer(JsxExpressionContainer jsxExpressionContainer, object? context)
    {
        _visitor.Visit(jsxExpressionContainer.Expression, context);

        return jsxExpressionContainer;
    }

    public virtual object? VisitJsxIdentifier(JsxIdentifier jsxIdentifier, object? context)
    {
        return jsxIdentifier;
    }

    public virtual object? VisitJsxMemberExpression(JsxMemberExpression jsxMemberExpression, object? context)
    {
        _visitor.Visit(jsxMemberExpression.Object, context);
        _visitor.Visit(jsxMemberExpression.Property, context);

        return jsxMemberExpression;
    }

    public virtual object? VisitJsxNamespacedName(JsxNamespacedName jsxNamespacedName, object? context)
    {
        _visitor.Visit(jsxNamespacedName.Name, context);
        _visitor.Visit(jsxNamespacedName.Namespace, context);

        return jsxNamespacedName;
    }

    public virtual object? VisitJsxOpeningElement(JsxOpeningElement jsxOpeningElement, object? context)
    {
        _visitor.Visit(jsxOpeningElement.Name, context);
        ref readonly var attributes = ref jsxOpeningElement.Attributes;
        for (var i = 0; i < attributes.Count; i++)
        {
            _visitor.Visit(attributes[i], context);
        }

        return jsxOpeningElement;
    }

    public virtual object? VisitJsxOpeningFragment(JsxOpeningFragment jsxOpeningFragment, object? context)
    {
        return jsxOpeningFragment;
    }

    public virtual object? VisitJsxSpreadAttribute(JsxSpreadAttribute jsxSpreadAttribute, object? context)
    {
        _visitor.Visit(jsxSpreadAttribute.Argument, context);

        return jsxSpreadAttribute;
    }

    public virtual object? VisitJsxText(JsxText jsxText, object? context)
    {
        return jsxText;
    }
}
