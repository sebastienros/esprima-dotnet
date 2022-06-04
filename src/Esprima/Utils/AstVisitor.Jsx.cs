using Esprima.Jsx.Ast;

namespace Esprima.Utils;

public partial class AstVisitor
{
    protected internal virtual object? VisitJsxMemberExpression(JsxMemberExpression jsxMemberExpression)
    {
        Visit(jsxMemberExpression.Object);
        Visit(jsxMemberExpression.Property);

        return jsxMemberExpression;
    }

    protected internal virtual object? VisitJsxText(JsxText jsxText)
    {
        return jsxText;
    }

    protected internal virtual object? VisitJsxOpeningFragment(JsxOpeningFragment jsxOpeningFragment)
    {
        return jsxOpeningFragment;
    }

    protected internal virtual object? VisitJsxClosingFragment(JsxClosingFragment jsxClosingFragment)
    {
        return jsxClosingFragment;
    }

    protected internal virtual object? VisitJsxIdentifier(JsxIdentifier jsxIdentifier)
    {
        return jsxIdentifier;
    }

    protected internal virtual object? VisitJsxElement(JsxElement jsxElement)
    {
        Visit(jsxElement.OpeningElement);
        ref readonly var children = ref jsxElement.Children;
        for (var i = 0; i < children.Count; i++)
        {
            Visit(children[i]);
        }

        if (jsxElement.ClosingElement is not null)
        {
            Visit(jsxElement.ClosingElement);
        }

        return jsxElement;
    }

    protected internal virtual object? VisitJsxOpeningElement(JsxOpeningElement jsxOpeningElement)
    {
        Visit(jsxOpeningElement.Name);
        ref readonly var attributes = ref jsxOpeningElement.Attributes;
        for (var i = 0; i < attributes.Count; i++)
        {
            Visit(attributes[i]);
        }

        return jsxOpeningElement;
    }

    protected internal virtual object? VisitJsxClosingElement(JsxClosingElement jsxClosingElement)
    {
        Visit(jsxClosingElement.Name);

        return jsxClosingElement;
    }

    protected internal virtual object? VisitJsxEmptyExpression(JsxEmptyExpression jsxEmptyExpression)
    {
        return jsxEmptyExpression;
    }

    protected internal virtual object? VisitJsxNamespacedName(JsxNamespacedName jsxNamespacedName)
    {
        Visit(jsxNamespacedName.Name);
        Visit(jsxNamespacedName.Namespace);

        return jsxNamespacedName;
    }

    protected internal virtual object? VisitJsxSpreadAttribute(JsxSpreadAttribute jsxSpreadAttribute)
    {
        Visit(jsxSpreadAttribute.Argument);

        return jsxSpreadAttribute;
    }

    protected internal virtual object? VisitJsxAttribute(JsxAttribute jsxAttribute)
    {
        Visit(jsxAttribute.Name);
        if (jsxAttribute.Value is not null)
        {
            Visit(jsxAttribute.Value);
        }

        return jsxAttribute;
    }

    protected internal virtual object? VisitJsxExpressionContainer(JsxExpressionContainer jsxExpressionContainer)
    {
        Visit(jsxExpressionContainer.Expression);

        return jsxExpressionContainer;
    }
}
