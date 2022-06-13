using Esprima.Ast.Jsx;

namespace Esprima.Utils.Jsx;

public interface IJsxAstVisitor
{
    object? VisitJsxAttribute(JsxAttribute jsxAttribute);
    object? VisitJsxClosingElement(JsxClosingElement jsxClosingElement);
    object? VisitJsxClosingFragment(JsxClosingFragment jsxClosingFragment);
    object? VisitJsxElement(JsxElement jsxElement);
    object? VisitJsxEmptyExpression(JsxEmptyExpression jsxEmptyExpression);
    object? VisitJsxExpressionContainer(JsxExpressionContainer jsxExpressionContainer);
    object? VisitJsxIdentifier(JsxIdentifier jsxIdentifier);
    object? VisitJsxMemberExpression(JsxMemberExpression jsxMemberExpression);
    object? VisitJsxNamespacedName(JsxNamespacedName jsxNamespacedName);
    object? VisitJsxOpeningElement(JsxOpeningElement jsxOpeningElement);
    object? VisitJsxOpeningFragment(JsxOpeningFragment jsxOpeningFragment);
    object? VisitJsxSpreadAttribute(JsxSpreadAttribute jsxSpreadAttribute);
    object? VisitJsxText(JsxText jsxText);
}
