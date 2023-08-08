//HintName: Esprima.Utils.Jsx.IJsxAstVisitor.g.cs
#nullable enable

namespace Esprima.Utils.Jsx;

partial interface IJsxAstVisitor
{
    object? VisitJsxAttribute(Esprima.Ast.Jsx.JsxAttribute jsxAttribute);

    object? VisitJsxClosingElement(Esprima.Ast.Jsx.JsxClosingElement jsxClosingElement);

    object? VisitJsxClosingFragment(Esprima.Ast.Jsx.JsxClosingFragment jsxClosingFragment);

    object? VisitJsxElement(Esprima.Ast.Jsx.JsxElement jsxElement);

    object? VisitJsxEmptyExpression(Esprima.Ast.Jsx.JsxEmptyExpression jsxEmptyExpression);

    object? VisitJsxExpressionContainer(Esprima.Ast.Jsx.JsxExpressionContainer jsxExpressionContainer);

    object? VisitJsxIdentifier(Esprima.Ast.Jsx.JsxIdentifier jsxIdentifier);

    object? VisitJsxMemberExpression(Esprima.Ast.Jsx.JsxMemberExpression jsxMemberExpression);

    object? VisitJsxNamespacedName(Esprima.Ast.Jsx.JsxNamespacedName jsxNamespacedName);

    object? VisitJsxOpeningElement(Esprima.Ast.Jsx.JsxOpeningElement jsxOpeningElement);

    object? VisitJsxOpeningFragment(Esprima.Ast.Jsx.JsxOpeningFragment jsxOpeningFragment);

    object? VisitJsxSpreadAttribute(Esprima.Ast.Jsx.JsxSpreadAttribute jsxSpreadAttribute);

    object? VisitJsxText(Esprima.Ast.Jsx.JsxText jsxText);
}
