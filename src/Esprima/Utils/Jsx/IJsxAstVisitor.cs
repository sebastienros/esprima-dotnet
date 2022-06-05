using Esprima.Ast.Jsx;

namespace Esprima.Utils.Jsx;

public interface IJsxAstVisitor
{
    object? VisitJsxMemberExpression(JsxMemberExpression jsxMemberExpression);
    object? VisitJsxText(JsxText jsxText);
    object? VisitJsxOpeningFragment(JsxOpeningFragment jsxOpeningFragment);
    object? VisitJsxClosingFragment(JsxClosingFragment jsxClosingFragment);
    object? VisitJsxIdentifier(JsxIdentifier jsxIdentifier);
    object? VisitJsxElement(JsxElement jsxElement);
    object? VisitJsxOpeningElement(JsxOpeningElement jsxOpeningElement);
    object? VisitJsxClosingElement(JsxClosingElement jsxClosingElement);
    object? VisitJsxEmptyExpression(JsxEmptyExpression jsxEmptyExpression);
    object? VisitJsxNamespacedName(JsxNamespacedName jsxNamespacedName);
    object? VisitJsxSpreadAttribute(JsxSpreadAttribute jsxSpreadAttribute);
    object? VisitJsxAttribute(JsxAttribute jsxAttribute);
    object? VisitJsxExpressionContainer(JsxExpressionContainer jsxExpressionContainer);
}
