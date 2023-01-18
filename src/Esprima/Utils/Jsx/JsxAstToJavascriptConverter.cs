using Esprima.Ast;
using Esprima.Ast.Jsx;
using static Esprima.Utils.JavaScriptTextWriter;

namespace Esprima.Utils.Jsx;

public record class JsxAstToJavaScriptOptions : AstToJavaScriptOptions
{
    public static new readonly JsxAstToJavaScriptOptions Default = new();

    protected internal override AstToJavaScriptConverter CreateConverter(JavaScriptTextWriter writer) => new JsxAstToJavaScriptConverter(writer, this);
}

// JSX spec: https://facebook.github.io/jsx/
public class JsxAstToJavaScriptConverter : AstToJavaScriptConverter, IJsxAstVisitor
{
    public JsxAstToJavaScriptConverter(JavaScriptTextWriter writer, JsxAstToJavaScriptOptions options) : base(writer, options)
    {
    }

    object? IJsxAstVisitor<object?>.VisitJsxAttribute(JsxAttribute jsxAttribute)
    {
        Writer.SpaceRecommendedAfterLastToken();

        WriteContext.SetNodeProperty(nameof(jsxAttribute.Name), static node => node.As<JsxAttribute>().Name);
        VisitAuxiliaryNode(jsxAttribute.Name);

        if (jsxAttribute.Value is not null)
        {
            WriteContext.ClearNodeProperty();
            Writer.WritePunctuator("=", TokenFlags.InBetween, ref WriteContext);

            WriteContext.SetNodeProperty(nameof(jsxAttribute.Value), static node => node.As<JsxAttribute>().Value);
            VisitAuxiliaryNode(jsxAttribute.Value);
        }

        return jsxAttribute;
    }

    object? IJsxAstVisitor<object?>.VisitJsxClosingElement(JsxClosingElement jsxClosingElement)
    {
        Writer.WritePunctuator("<", TokenFlags.Leading, ref WriteContext);
        Writer.WritePunctuator("/", ref WriteContext);

        WriteContext.SetNodeProperty(nameof(jsxClosingElement.Name), static node => node.As<JsxClosingElement>().Name);
        VisitAuxiliaryNode(jsxClosingElement.Name);

        WriteContext.ClearNodeProperty();
        Writer.WritePunctuator(">", TokenFlags.Trailing, ref WriteContext);

        return jsxClosingElement;
    }

    object? IJsxAstVisitor<object?>.VisitJsxClosingFragment(JsxClosingFragment jsxClosingFragment)
    {
        Writer.WritePunctuator("<", TokenFlags.Leading, ref WriteContext);
        Writer.WritePunctuator("/", ref WriteContext);
        Writer.WritePunctuator(">", TokenFlags.Trailing, ref WriteContext);

        return jsxClosingFragment;
    }

    object? IJsxAstVisitor<object?>.VisitJsxElement(JsxElement jsxElement)
    {
        WriteContext.SetNodeProperty(nameof(jsxElement.OpeningElement), static node => node.As<JsxElement>().OpeningElement);
        VisitAuxiliaryNode(jsxElement.OpeningElement);

        WriteContext.SetNodeProperty(nameof(jsxElement.Children), static node => ref node.As<JsxElement>().Children);
        VisitAuxiliaryNodeList(in jsxElement.Children, separator: string.Empty);

        if (jsxElement.ClosingElement is not null)
        {
            WriteContext.SetNodeProperty(nameof(jsxElement.ClosingElement), static node => node.As<JsxElement>().ClosingElement);
            VisitAuxiliaryNode(jsxElement.ClosingElement);
        }

        return jsxElement;
    }

    object? IJsxAstVisitor<object?>.VisitJsxEmptyExpression(JsxEmptyExpression jsxEmptyExpression)
    {
        return jsxEmptyExpression;
    }

    object? IJsxAstVisitor<object?>.VisitJsxExpressionContainer(JsxExpressionContainer jsxExpressionContainer)
    {
        Writer.WritePunctuator("{", TokenFlags.Leading, ref WriteContext);

        WriteContext.SetNodeProperty(nameof(jsxExpressionContainer.Expression), static node => node.As<JsxExpressionContainer>().Expression);
        VisitRootExpression(jsxExpressionContainer.Expression, RootExpressionFlags(needsBrackets: false));

        WriteContext.ClearNodeProperty();
        Writer.WritePunctuator("}", TokenFlags.Trailing, ref WriteContext);

        return jsxExpressionContainer;
    }

    object? IJsxAstVisitor<object?>.VisitJsxIdentifier(JsxIdentifier jsxIdentifier)
    {
        WriteContext.SetNodeProperty(nameof(jsxIdentifier.Name), static node => node.As<JsxIdentifier>().Name);
        Writer.WriteIdentifier(jsxIdentifier.Name, ref WriteContext);

        return jsxIdentifier;
    }

    object? IJsxAstVisitor<object?>.VisitJsxMemberExpression(JsxMemberExpression jsxMemberExpression)
    {
        WriteContext.SetNodeProperty(nameof(jsxMemberExpression.Object), static node => node.As<JsxMemberExpression>().Object);
        VisitAuxiliaryNode(jsxMemberExpression.Object);

        WriteContext.ClearNodeProperty();
        Writer.WritePunctuator(".", TokenFlags.InBetween, ref WriteContext);

        WriteContext.SetNodeProperty(nameof(jsxMemberExpression.Property), static node => node.As<JsxMemberExpression>().Property);
        VisitAuxiliaryNode(jsxMemberExpression.Property);

        return jsxMemberExpression;
    }

    object? IJsxAstVisitor<object?>.VisitJsxNamespacedName(JsxNamespacedName jsxNamespacedName)
    {
        WriteContext.SetNodeProperty(nameof(jsxNamespacedName.Namespace), static node => node.As<JsxNamespacedName>().Namespace);
        VisitAuxiliaryNode(jsxNamespacedName.Namespace);

        WriteContext.ClearNodeProperty();
        Writer.WritePunctuator(":", TokenFlags.InBetween, ref WriteContext);

        WriteContext.SetNodeProperty(nameof(jsxNamespacedName.Name), static node => node.As<JsxNamespacedName>().Name);
        VisitAuxiliaryNode(jsxNamespacedName.Name);

        return jsxNamespacedName;
    }

    object? IJsxAstVisitor<object?>.VisitJsxOpeningElement(JsxOpeningElement jsxOpeningElement)
    {
        Writer.WritePunctuator("<", TokenFlags.Leading, ref WriteContext);

        WriteContext.SetNodeProperty(nameof(jsxOpeningElement.Name), static node => node.As<JsxOpeningElement>().Name);
        VisitAuxiliaryNode(jsxOpeningElement.Name);

        WriteContext.SetNodeProperty(nameof(jsxOpeningElement.Attributes), static node => ref node.As<JsxOpeningElement>().Attributes);
        VisitAuxiliaryNodeList(in jsxOpeningElement.Attributes, separator: string.Empty);

        if (jsxOpeningElement.SelfClosing)
        {
            WriteContext.SetNodeProperty(nameof(jsxOpeningElement.SelfClosing), static node => node.As<JsxOpeningElement>().SelfClosing);
            Writer.WritePunctuator("/", TokenFlags.LeadingSpaceRecommended, ref WriteContext);
        }
        WriteContext.ClearNodeProperty();
        Writer.WritePunctuator(">", TokenFlags.Trailing, ref WriteContext);

        return jsxOpeningElement;
    }

    object? IJsxAstVisitor<object?>.VisitJsxOpeningFragment(JsxOpeningFragment jsxOpeningFragment)
    {
        Writer.WritePunctuator("<", TokenFlags.Leading, ref WriteContext);

        if (jsxOpeningFragment.SelfClosing)
        {
            WriteContext.SetNodeProperty(nameof(jsxOpeningFragment.SelfClosing), static node => node.As<JsxOpeningFragment>().SelfClosing);
            Writer.WritePunctuator("/", ref WriteContext);

            WriteContext.ClearNodeProperty();
        }
        Writer.WritePunctuator(">", TokenFlags.Trailing, ref WriteContext);

        return jsxOpeningFragment;
    }

    object? IJsxAstVisitor<object?>.VisitJsxSpreadAttribute(JsxSpreadAttribute jsxSpreadAttribute)
    {
        Writer.WritePunctuator("{", TokenFlags.Leading | TokenFlags.LeadingSpaceRecommended, ref WriteContext);

        var argumentNeedsBrackets = UnaryOperandNeedsBrackets(jsxSpreadAttribute, jsxSpreadAttribute.Argument);

        WriteContext.SetNodeProperty(nameof(jsxSpreadAttribute.Argument), static node => node.As<JsxSpreadAttribute>().Argument);
        Writer.WritePunctuator("...", TokenFlags.Leading, ref WriteContext);

        VisitRootExpression(jsxSpreadAttribute.Argument, RootExpressionFlags(argumentNeedsBrackets));

        WriteContext.ClearNodeProperty();
        Writer.WritePunctuator("}", TokenFlags.Trailing, ref WriteContext);

        return jsxSpreadAttribute;
    }

    object? IJsxAstVisitor<object?>.VisitJsxText(JsxText jsxText)
    {
        Writer.WriteLiteral(jsxText.Raw, TokenType.Extension, ref WriteContext);

        return jsxText;
    }

    protected override int GetOperatorPrecedence(Expression expression, out int associativity)
    {
        if (expression.Type == Nodes.Extension && expression is JsxExpression jsxExpression)
        {
            const int undefinedAssociativity = 0;

            switch (jsxExpression.Type)
            {
                case JsxNodeType.Element:
                    associativity = undefinedAssociativity;
                    return int.MaxValue;
                case JsxNodeType.SpreadAttribute:
                    associativity = undefinedAssociativity;
                    return 200;
            }
        }

        return base.GetOperatorPrecedence(expression, out associativity);
    }
}
