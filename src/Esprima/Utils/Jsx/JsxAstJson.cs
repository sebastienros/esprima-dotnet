using Esprima.Ast;
using Esprima.Ast.Jsx;

namespace Esprima.Utils.Jsx;

public sealed class JsxAstToJsonConverter : AstToJsonConverter
{
    public static new readonly JsxAstToJsonConverter Default = new();

    private JsxAstToJsonConverter() { }

    private protected override VisitorBase CreateVisitor(JsonWriter writer, AstJson.Options options)
    {
        return new Visitor(writer, options);
    }

    private sealed class Visitor : VisitorBase, IJsxAstVisitor
    {
        public Visitor(JsonWriter writer, AstJson.Options options)
            : base(writer, options)
        {
        }

        protected override string GetNodeType(Node node)
        {
            if (node is JsxExpression jsxExpression)
            {
                // Due to the borrowed test fixtures, it's important to use the 'JSX' prefix to stay consistent with the naming used by original Esprima
                // (see https://github.com/jquery/esprima/blob/4.0.1/src/jsx-nodes.ts).
                return "JSX" + jsxExpression.Type.ToString();
            }

            return base.GetNodeType(node);
        }

        object? IJsxAstVisitor.VisitJsxAttribute(JsxAttribute jsxAttribute)
        {
            using (StartNodeObject(jsxAttribute))
            {
                Member("name", jsxAttribute.Name);
                Member("value", jsxAttribute.Value);
            }

            return jsxAttribute;
        }

        object? IJsxAstVisitor.VisitJsxClosingElement(JsxClosingElement jsxClosingElement)
        {
            using (StartNodeObject(jsxClosingElement))
            {
                Member("name", jsxClosingElement.Name);
            }

            return jsxClosingElement;
        }

        object? IJsxAstVisitor.VisitJsxClosingFragment(JsxClosingFragment jsxClosingFragment)
        {
            using (StartNodeObject(jsxClosingFragment))
            {
            }

            return jsxClosingFragment;
        }

        object? IJsxAstVisitor.VisitJsxElement(JsxElement jsxElement)
        {
            using (StartNodeObject(jsxElement))
            {
                Member("openingElement", jsxElement.OpeningElement);
                Member("children", jsxElement.Children);
                Member("closingElement", jsxElement.ClosingElement);
            }

            return jsxElement;
        }

        object? IJsxAstVisitor.VisitJsxEmptyExpression(JsxEmptyExpression jsxEmptyExpression)
        {
            using (StartNodeObject(jsxEmptyExpression))
            {
            }

            return jsxEmptyExpression;
        }

        object? IJsxAstVisitor.VisitJsxExpressionContainer(JsxExpressionContainer jsxExpressionContainer)
        {
            using (StartNodeObject(jsxExpressionContainer))
            {
                Member("expression", jsxExpressionContainer.Expression);
            }

            return jsxExpressionContainer;
        }

        object? IJsxAstVisitor.VisitJsxIdentifier(JsxIdentifier jsxIdentifier)
        {
            using (StartNodeObject(jsxIdentifier))
            {
                Member("name", jsxIdentifier.Name);
            }

            return jsxIdentifier;
        }

        object? IJsxAstVisitor.VisitJsxMemberExpression(JsxMemberExpression jsxMemberExpression)
        {
            using (StartNodeObject(jsxMemberExpression))
            {
                Member("object", jsxMemberExpression.Object);
                Member("property", jsxMemberExpression.Property);
            }

            return jsxMemberExpression;
        }

        object? IJsxAstVisitor.VisitJsxNamespacedName(JsxNamespacedName jsxNamespacedName)
        {
            using (StartNodeObject(jsxNamespacedName))
            {
                Member("namespace", jsxNamespacedName.Namespace);
                Member("name", jsxNamespacedName.Name);
            }

            return jsxNamespacedName;
        }

        object? IJsxAstVisitor.VisitJsxOpeningElement(JsxOpeningElement jsxOpeningElement)
        {
            using (StartNodeObject(jsxOpeningElement))
            {
                Member("name", jsxOpeningElement.Name);
                Member("selfClosing", jsxOpeningElement.SelfClosing);
                Member("attributes", jsxOpeningElement.Attributes);
            }

            return jsxOpeningElement;
        }

        object? IJsxAstVisitor.VisitJsxOpeningFragment(JsxOpeningFragment jsxOpeningFragment)
        {
            using (StartNodeObject(jsxOpeningFragment))
            {
                Member("selfClosing", jsxOpeningFragment.SelfClosing);
            }

            return jsxOpeningFragment;
        }

        object? IJsxAstVisitor.VisitJsxSpreadAttribute(JsxSpreadAttribute jsxSpreadAttribute)
        {
            using (StartNodeObject(jsxSpreadAttribute))
            {
                Member("argument", jsxSpreadAttribute.Argument);
            }

            return jsxSpreadAttribute;
        }

        object? IJsxAstVisitor.VisitJsxText(JsxText jsxText)
        {
            using (StartNodeObject(jsxText))
            {
                Member("value", jsxText.Value);
                Member("raw", jsxText.Raw);
            }

            return jsxText;
        }
    }
}
