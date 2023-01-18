using Esprima.Ast.Jsx;

namespace Esprima.Utils.Jsx;

public class JsxAstVisitor<T> : AstVisitor<T>, IJsxAstVisitor<T>
{
    public virtual T VisitJsxAttribute(JsxAttribute jsxAttribute)
    {
        throw UnsupportedNodeType(jsxAttribute.GetType());
    }

    public virtual T VisitJsxClosingElement(JsxClosingElement jsxClosingElement)
    {
        throw UnsupportedNodeType(jsxClosingElement.GetType());
    }

    public virtual T VisitJsxClosingFragment(JsxClosingFragment jsxClosingFragment)
    {
        throw UnsupportedNodeType(jsxClosingFragment.GetType());
    }

    public virtual T VisitJsxElement(JsxElement jsxElement)
    {
        throw UnsupportedNodeType(jsxElement.GetType());
    }

    public virtual T VisitJsxEmptyExpression(JsxEmptyExpression jsxEmptyExpression)
    {
        throw UnsupportedNodeType(jsxEmptyExpression.GetType());
    }

    public virtual T VisitJsxExpressionContainer(JsxExpressionContainer jsxExpressionContainer)
    {
        throw UnsupportedNodeType(jsxExpressionContainer.GetType());
    }

    public virtual T VisitJsxIdentifier(JsxIdentifier jsxIdentifier)
    {
        throw UnsupportedNodeType(jsxIdentifier.GetType());
    }

    public virtual T VisitJsxMemberExpression(JsxMemberExpression jsxMemberExpression)
    {
        throw UnsupportedNodeType(jsxMemberExpression.GetType());
    }

    public virtual T VisitJsxNamespacedName(JsxNamespacedName jsxNamespacedName)
    {
        throw UnsupportedNodeType(jsxNamespacedName.GetType());
    }

    public virtual T VisitJsxOpeningElement(JsxOpeningElement jsxOpeningElement)
    {
        throw UnsupportedNodeType(jsxOpeningElement.GetType());
    }

    public virtual T VisitJsxOpeningFragment(JsxOpeningFragment jsxOpeningFragment)
    {
        throw UnsupportedNodeType(jsxOpeningFragment.GetType());
    }

    public virtual T VisitJsxSpreadAttribute(JsxSpreadAttribute jsxSpreadAttribute)
    {
        throw UnsupportedNodeType(jsxSpreadAttribute.GetType());
    }

    public virtual T VisitJsxText(JsxText jsxText)
    {
        throw UnsupportedNodeType(jsxText.GetType());
    }
}
