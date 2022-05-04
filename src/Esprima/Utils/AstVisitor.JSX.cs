using Esprima.Ast;

namespace Esprima.Utils
{
    public partial class AstVisitor
    {
        protected internal virtual void VisitJSXMemberExpression(JSXMemberExpression jsxMemberExpression)
        {
            Visit(jsxMemberExpression.Object);
            Visit(jsxMemberExpression.Property);
        }

        protected internal virtual void VisitJSXText(JSXText jsxText)
        {
        }

        protected internal virtual void VisitJSXOpeningFragment(JSXOpeningFragment jsxOpeningFragment)
        {
        }

        protected internal virtual void VisitJSXClosingFragment(JSXClosingFragment jsxClosingFragment)
        {
        }


        protected internal virtual void VisitJSXIdentifier(JSXIdentifier jsxIdentifier)
        {
        }

        protected internal virtual void VisitJSXElement(JSXElement jsxElement)
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

        protected internal virtual void VisitJSXOpeningElement(JSXOpeningElement jsxOpeningElement)
        {
            Visit(jsxOpeningElement.Name);
            ref readonly var attributes = ref jsxOpeningElement.Attributes;
            for (var i = 0; i < attributes.Count; i++)
            {
                Visit(attributes[i]);
            }
        }

        protected internal virtual void VisitJSXClosingElement(JSXClosingElement jsxClosingElement)
        {
            Visit(jsxClosingElement.Name);
        }

        protected internal virtual void VisitJSXEmptyExpression(JSXEmptyExpression jsxEmptyExpression)
        {
        }

        protected internal virtual void VisitJSXNamespacedName(JSXNamespacedName jsxNamespacedName)
        {
            Visit(jsxNamespacedName.Name);
            Visit(jsxNamespacedName.Namespace);
        }

        protected internal virtual void VisitJSXSpreadAttribute(JSXSpreadAttribute jsxSpreadAttribute)
        {
            Visit(jsxSpreadAttribute.Argument);
        }

        protected internal virtual void VisitJSXAttribute(JSXAttribute jsxAttribute)
        {
            Visit(jsxAttribute.Name);
            if (jsxAttribute.Value is not null)
            {
                Visit(jsxAttribute.Value);
            }
        }

        protected internal virtual void VisitJSXExpressionContainer(JSXExpressionContainer jsxExpressionContainer)
        {
            Visit(jsxExpressionContainer.Expression);
        }
    }
}
