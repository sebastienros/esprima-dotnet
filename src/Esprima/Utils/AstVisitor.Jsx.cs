using Esprima.Ast;

namespace Esprima.Utils;

public partial class AstVisitor
{
    protected internal virtual void VisitJsxMemberExpression(JsxMemberExpression jsxMemberExpression)
    {
        Visit(jsxMemberExpression.Object);
        Visit(jsxMemberExpression.Property);
    }

    protected internal virtual void VisitJsxText(JsxText jsxText)
    {
    }

    protected internal virtual void VisitJsxOpeningFragment(JsxOpeningFragment jsxOpeningFragment)
    {
    }

    protected internal virtual void VisitJsxClosingFragment(JsxClosingFragment jsxClosingFragment)
    {
    }


    protected internal virtual void VisitJsxIdentifier(JsxIdentifier jsxIdentifier)
    {
    }

    protected internal virtual void VisitJsxElement(JsxElement jsxElement)
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
    }

    protected internal virtual void VisitJsxOpeningElement(JsxOpeningElement jsxOpeningElement)
    {
        Visit(jsxOpeningElement.Name);
        ref readonly var attributes = ref jsxOpeningElement.Attributes;
        for (var i = 0; i < attributes.Count; i++)
        {
            Visit(attributes[i]);
        }
    }

    protected internal virtual void VisitJsxClosingElement(JsxClosingElement jsxClosingElement)
    {
        Visit(jsxClosingElement.Name);
    }

    protected internal virtual void VisitJsxEmptyExpression(JsxEmptyExpression jsxEmptyExpression)
    {
    }

    protected internal virtual void VisitJsxNamespacedName(JsxNamespacedName jsxNamespacedName)
    {
        Visit(jsxNamespacedName.Name);
        Visit(jsxNamespacedName.Namespace);
    }

    protected internal virtual void VisitJsxSpreadAttribute(JsxSpreadAttribute jsxSpreadAttribute)
    {
        Visit(jsxSpreadAttribute.Argument);
    }

    protected internal virtual void VisitJsxAttribute(JsxAttribute jsxAttribute)
    {
        Visit(jsxAttribute.Name);
        if (jsxAttribute.Value is not null)
        {
            Visit(jsxAttribute.Value);
        }
    }

    protected internal virtual void VisitJsxExpressionContainer(JsxExpressionContainer jsxExpressionContainer)
    {
        Visit(jsxExpressionContainer.Expression);
    }
}