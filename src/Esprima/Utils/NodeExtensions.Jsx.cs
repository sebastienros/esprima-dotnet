using Esprima.Ast;

namespace Esprima.Utils
{
    public static partial class NodeExtensions
    {
        public static JsxAttribute Update(this JsxAttribute jsxAttribute, JsxExpression name, Expression? value)
        {
            if (name == jsxAttribute.Name && value == jsxAttribute.Value)
            {
                return jsxAttribute;
            }

            return new JsxAttribute(name, value);
        }

        public static JsxElement Update(this JsxElement jsxElement, Node openingElement, in NodeList<JsxExpression> children, Node? closingElement)
        {
            if (openingElement == jsxElement.OpeningElement && NodeList.AreSame(children, jsxElement.Children) && closingElement == jsxElement.ClosingElement)
            {
                return jsxElement;
            }

            return new JsxElement(openingElement, children, closingElement);
        }

        public static JsxClosingElement Update(this JsxClosingElement jsxClosingElement, JsxExpression name)
        {
            if (name == jsxClosingElement.Name)
            {
                return jsxClosingElement;
            }

            return new JsxClosingElement(name);
        }

        public static JsxExpressionContainer Update(this JsxExpressionContainer jsxExpressionContainer, Expression expression)
        {
            if (expression == jsxExpressionContainer.Expression)
            {
                return jsxExpressionContainer;
            }

            return new JsxExpressionContainer(expression);
        }

        public static JsxMemberExpression Update(this JsxMemberExpression jsxMemberExpression, JsxExpression obj, JsxIdentifier property)
        {
            if (obj == jsxMemberExpression.Object && property == jsxMemberExpression.Property)
            {
                return jsxMemberExpression;
            }

            return new JsxMemberExpression(obj, property);
        }

        public static JsxNamespacedName Update(this JsxNamespacedName jsxNamespacedName, JsxIdentifier name, JsxIdentifier @namespace)
        {
            if (name == jsxNamespacedName.Name && @namespace == jsxNamespacedName.Namespace)
            {
                return jsxNamespacedName;
            }

            return new JsxNamespacedName(@namespace, name);
        }

        public static JsxOpeningElement Update(this JsxOpeningElement jsxOpeningElement, JsxExpression name, in NodeList<JsxExpression> attributes)
        {
            if (name == jsxOpeningElement.Name && NodeList.AreSame(attributes, jsxOpeningElement.Attributes))
            {
                return jsxOpeningElement;
            }

            return new JsxOpeningElement(name, jsxOpeningElement.SelfClosing, attributes);
        }

        public static JsxSpreadAttribute Update(this JsxSpreadAttribute jsxSpreadAttribute, Expression argument)
        {
            if (argument == jsxSpreadAttribute.Argument)
            {
                return jsxSpreadAttribute;
            }

            return new JsxSpreadAttribute(argument);
        }
    }
}
