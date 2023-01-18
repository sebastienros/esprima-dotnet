using Esprima.Ast.Jsx;

namespace Esprima.Utils.Jsx;

public class JsxAstVisitor : AstVisitor, IJsxAstVisitor
{
    /// <summary>
    /// Creates an <see cref="IJsxAstVisitor"/> instance which can be used for working around multiple inheritance:
    /// the returned instance re-routes visitations of JSX nodes to the specified <paramref name="visitor"/>,
    /// thus it can be used for emulating base class method calls.
    /// </summary>
    public static IJsxAstVisitor CreateJsxVisitorFor<TVisitor>(TVisitor visitor)
        where TVisitor : AstVisitor, IJsxAstVisitor
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

    public object? VisitJsxAttribute(JsxAttribute jsxAttribute)
    {
        _visitor.Visit(jsxAttribute.Name);
        if (jsxAttribute.Value is not null)
        {
            _visitor.Visit(jsxAttribute.Value);
        }

        return jsxAttribute;
    }

    public object? VisitJsxClosingElement(JsxClosingElement jsxClosingElement)
    {
        _visitor.Visit(jsxClosingElement.Name);

        return jsxClosingElement;
    }

    public object? VisitJsxClosingFragment(JsxClosingFragment jsxClosingFragment)
    {
        return jsxClosingFragment;
    }

    public object? VisitJsxElement(JsxElement jsxElement)
    {
        _visitor.Visit(jsxElement.OpeningElement);
        ref readonly var children = ref jsxElement.Children;
        for (var i = 0; i < children.Count; i++)
        {
            _visitor.Visit(children[i]);
        }

        if (jsxElement.ClosingElement is not null)
        {
            _visitor.Visit(jsxElement.ClosingElement);
        }

        return jsxElement;
    }

    public object? VisitJsxEmptyExpression(JsxEmptyExpression jsxEmptyExpression)
    {
        return jsxEmptyExpression;
    }

    public object? VisitJsxExpressionContainer(JsxExpressionContainer jsxExpressionContainer)
    {
        _visitor.Visit(jsxExpressionContainer.Expression);

        return jsxExpressionContainer;
    }

    public object? VisitJsxIdentifier(JsxIdentifier jsxIdentifier)
    {
        return jsxIdentifier;
    }

    public object? VisitJsxMemberExpression(JsxMemberExpression jsxMemberExpression)
    {
        _visitor.Visit(jsxMemberExpression.Object);
        _visitor.Visit(jsxMemberExpression.Property);

        return jsxMemberExpression;
    }

    public object? VisitJsxNamespacedName(JsxNamespacedName jsxNamespacedName)
    {
        _visitor.Visit(jsxNamespacedName.Name);
        _visitor.Visit(jsxNamespacedName.Namespace);

        return jsxNamespacedName;
    }

    public object? VisitJsxOpeningElement(JsxOpeningElement jsxOpeningElement)
    {
        _visitor.Visit(jsxOpeningElement.Name);
        ref readonly var attributes = ref jsxOpeningElement.Attributes;
        for (var i = 0; i < attributes.Count; i++)
        {
            _visitor.Visit(attributes[i]);
        }

        return jsxOpeningElement;
    }

    public object? VisitJsxOpeningFragment(JsxOpeningFragment jsxOpeningFragment)
    {
        return jsxOpeningFragment;
    }

    public object? VisitJsxSpreadAttribute(JsxSpreadAttribute jsxSpreadAttribute)
    {
        _visitor.Visit(jsxSpreadAttribute.Argument);

        return jsxSpreadAttribute;
    }

    public object? VisitJsxText(JsxText jsxText)
    {
        return jsxText;
    }
}
