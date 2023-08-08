//HintName: Esprima.Utils.Jsx.JsxAstVisitor.g.cs
#nullable enable

namespace Esprima.Utils.Jsx;

partial class JsxAstVisitor
{
    public virtual object? VisitJsxAttribute(Esprima.Ast.Jsx.JsxAttribute jsxAttribute)
    {
        _visitor.Visit(jsxAttribute.Name);

        if (jsxAttribute.Value is not null)
        {
            _visitor.Visit(jsxAttribute.Value);
        }

        return jsxAttribute;
    }

    public virtual object? VisitJsxClosingElement(Esprima.Ast.Jsx.JsxClosingElement jsxClosingElement)
    {
        _visitor.Visit(jsxClosingElement.Name);

        return jsxClosingElement;
    }

    public virtual object? VisitJsxClosingFragment(Esprima.Ast.Jsx.JsxClosingFragment jsxClosingFragment)
    {
        return jsxClosingFragment;
    }

    public virtual object? VisitJsxElement(Esprima.Ast.Jsx.JsxElement jsxElement)
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

    public virtual object? VisitJsxEmptyExpression(Esprima.Ast.Jsx.JsxEmptyExpression jsxEmptyExpression)
    {
        return jsxEmptyExpression;
    }

    public virtual object? VisitJsxExpressionContainer(Esprima.Ast.Jsx.JsxExpressionContainer jsxExpressionContainer)
    {
        _visitor.Visit(jsxExpressionContainer.Expression);

        return jsxExpressionContainer;
    }

    public virtual object? VisitJsxIdentifier(Esprima.Ast.Jsx.JsxIdentifier jsxIdentifier)
    {
        return jsxIdentifier;
    }

    public virtual object? VisitJsxMemberExpression(Esprima.Ast.Jsx.JsxMemberExpression jsxMemberExpression)
    {
        _visitor.Visit(jsxMemberExpression.Object);

        _visitor.Visit(jsxMemberExpression.Property);

        return jsxMemberExpression;
    }

    public virtual object? VisitJsxNamespacedName(Esprima.Ast.Jsx.JsxNamespacedName jsxNamespacedName)
    {
        _visitor.Visit(jsxNamespacedName.Name);

        _visitor.Visit(jsxNamespacedName.Namespace);

        return jsxNamespacedName;
    }

    public virtual object? VisitJsxOpeningElement(Esprima.Ast.Jsx.JsxOpeningElement jsxOpeningElement)
    {
        _visitor.Visit(jsxOpeningElement.Name);

        ref readonly var attributes = ref jsxOpeningElement.Attributes;
        for (var i = 0; i < attributes.Count; i++)
        {
            _visitor.Visit(attributes[i]);
        }

        return jsxOpeningElement;
    }

    public virtual object? VisitJsxOpeningFragment(Esprima.Ast.Jsx.JsxOpeningFragment jsxOpeningFragment)
    {
        return jsxOpeningFragment;
    }

    public virtual object? VisitJsxSpreadAttribute(Esprima.Ast.Jsx.JsxSpreadAttribute jsxSpreadAttribute)
    {
        _visitor.Visit(jsxSpreadAttribute.Argument);

        return jsxSpreadAttribute;
    }

    public virtual object? VisitJsxText(Esprima.Ast.Jsx.JsxText jsxText)
    {
        return jsxText;
    }
}
