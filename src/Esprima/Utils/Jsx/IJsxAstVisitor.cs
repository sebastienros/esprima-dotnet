using Esprima.Ast.Jsx;

namespace Esprima.Utils.Jsx;

public interface IJsxAstVisitor
{
    object? VisitJsxAttribute(JsxAttribute jsxAttribute, object? context);
    object? VisitJsxClosingElement(JsxClosingElement jsxClosingElement, object? context);
    object? VisitJsxClosingFragment(JsxClosingFragment jsxClosingFragment, object? context);
    object? VisitJsxElement(JsxElement jsxElement, object? context);
    object? VisitJsxEmptyExpression(JsxEmptyExpression jsxEmptyExpression, object? context);
    object? VisitJsxExpressionContainer(JsxExpressionContainer jsxExpressionContainer, object? context);
    object? VisitJsxIdentifier(JsxIdentifier jsxIdentifier, object? context);
    object? VisitJsxMemberExpression(JsxMemberExpression jsxMemberExpression, object? context);
    object? VisitJsxNamespacedName(JsxNamespacedName jsxNamespacedName, object? context);
    object? VisitJsxOpeningElement(JsxOpeningElement jsxOpeningElement, object? context);
    object? VisitJsxOpeningFragment(JsxOpeningFragment jsxOpeningFragment, object? context);
    object? VisitJsxSpreadAttribute(JsxSpreadAttribute jsxSpreadAttribute, object? context);
    object? VisitJsxText(JsxText jsxText, object? context);
}
