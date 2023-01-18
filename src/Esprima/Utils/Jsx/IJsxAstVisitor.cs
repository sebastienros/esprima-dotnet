using Esprima.Ast.Jsx;

namespace Esprima.Utils.Jsx;

public interface IJsxAstVisitor : IJsxAstVisitor<object?>
{
}

public interface IJsxAstVisitor<out T>
{
    T VisitJsxAttribute(JsxAttribute jsxAttribute);
    T VisitJsxClosingElement(JsxClosingElement jsxClosingElement);
    T VisitJsxClosingFragment(JsxClosingFragment jsxClosingFragment);
    T VisitJsxElement(JsxElement jsxElement);
    T VisitJsxEmptyExpression(JsxEmptyExpression jsxEmptyExpression);
    T VisitJsxExpressionContainer(JsxExpressionContainer jsxExpressionContainer);
    T VisitJsxIdentifier(JsxIdentifier jsxIdentifier);
    T VisitJsxMemberExpression(JsxMemberExpression jsxMemberExpression);
    T VisitJsxNamespacedName(JsxNamespacedName jsxNamespacedName);
    T VisitJsxOpeningElement(JsxOpeningElement jsxOpeningElement);
    T VisitJsxOpeningFragment(JsxOpeningFragment jsxOpeningFragment);
    T VisitJsxSpreadAttribute(JsxSpreadAttribute jsxSpreadAttribute);
    T VisitJsxText(JsxText jsxText);
}
