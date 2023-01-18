using System.Runtime.CompilerServices;
using Esprima.Ast;

namespace Esprima.Utils;

public abstract class AstVisitor<T>
{
    internal static Exception UnsupportedNodeType(Type nodeType, [CallerMemberName] string? callerName = null) =>
        new NotImplementedException($"The visitor does not support nodes of type {nodeType}. You can override {callerName} to handle this case.");

    public virtual T Visit(Node node)
    {
        return node.Accept(this);
    }

    protected internal virtual T VisitAccessorProperty(AccessorProperty accessorProperty)
    {
        throw UnsupportedNodeType(accessorProperty.GetType());
    }

    protected internal virtual T VisitArrayExpression(ArrayExpression arrayExpression)
    {
        throw UnsupportedNodeType(arrayExpression.GetType());
    }

    protected internal virtual T VisitArrayPattern(ArrayPattern arrayPattern)
    {
        throw UnsupportedNodeType(arrayPattern.GetType());
    }

    protected internal virtual T VisitArrowFunctionExpression(ArrowFunctionExpression arrowFunctionExpression)
    {
        throw UnsupportedNodeType(arrowFunctionExpression.GetType());
    }

    protected internal virtual T VisitAssignmentExpression(AssignmentExpression assignmentExpression)
    {
        throw UnsupportedNodeType(assignmentExpression.GetType());
    }

    protected internal virtual T VisitAssignmentPattern(AssignmentPattern assignmentPattern)
    {
        throw UnsupportedNodeType(assignmentPattern.GetType());
    }

    protected internal virtual T VisitAwaitExpression(AwaitExpression awaitExpression)
    {
        throw UnsupportedNodeType(awaitExpression.GetType());
    }

    protected internal virtual T VisitBinaryExpression(BinaryExpression binaryExpression)
    {
        throw UnsupportedNodeType(binaryExpression.GetType());
    }

    protected internal virtual T VisitBlockStatement(BlockStatement blockStatement)
    {
        throw UnsupportedNodeType(blockStatement.GetType());
    }

    protected internal virtual T VisitBreakStatement(BreakStatement breakStatement)
    {
        throw UnsupportedNodeType(breakStatement.GetType());
    }

    protected internal virtual T VisitCallExpression(CallExpression callExpression)
    {
        throw UnsupportedNodeType(callExpression.GetType());
    }

    protected internal virtual T VisitCatchClause(CatchClause catchClause)
    {
        throw UnsupportedNodeType(catchClause.GetType());
    }

    protected internal virtual T VisitChainExpression(ChainExpression chainExpression)
    {
        throw UnsupportedNodeType(chainExpression.GetType());
    }

    protected internal virtual T VisitClassBody(ClassBody classBody)
    {
        throw UnsupportedNodeType(classBody.GetType());
    }

    protected internal virtual T VisitClassDeclaration(ClassDeclaration classDeclaration)
    {
        throw UnsupportedNodeType(classDeclaration.GetType());
    }

    protected internal virtual T VisitClassExpression(ClassExpression classExpression)
    {
        throw UnsupportedNodeType(classExpression.GetType());
    }

    protected internal virtual T VisitConditionalExpression(ConditionalExpression conditionalExpression)
    {
        throw UnsupportedNodeType(conditionalExpression.GetType());
    }

    protected internal virtual T VisitContinueStatement(ContinueStatement continueStatement)
    {
        throw UnsupportedNodeType(continueStatement.GetType());
    }

    protected internal virtual T VisitDebuggerStatement(DebuggerStatement debuggerStatement)
    {
        throw UnsupportedNodeType(debuggerStatement.GetType());
    }

    protected internal virtual T VisitDecorator(Decorator decorator)
    {
        throw UnsupportedNodeType(decorator.GetType());
    }

    protected internal virtual T VisitDoWhileStatement(DoWhileStatement doWhileStatement)
    {
        throw UnsupportedNodeType(doWhileStatement.GetType());
    }

    protected internal virtual T VisitEmptyStatement(EmptyStatement emptyStatement)
    {
        throw UnsupportedNodeType(emptyStatement.GetType());
    }

    protected internal virtual T VisitExportAllDeclaration(ExportAllDeclaration exportAllDeclaration)
    {
        throw UnsupportedNodeType(exportAllDeclaration.GetType());
    }

    protected internal virtual T VisitExportDefaultDeclaration(ExportDefaultDeclaration exportDefaultDeclaration)
    {
        throw UnsupportedNodeType(exportDefaultDeclaration.GetType());
    }

    protected internal virtual T VisitExportNamedDeclaration(ExportNamedDeclaration exportNamedDeclaration)
    {
        throw UnsupportedNodeType(exportNamedDeclaration.GetType());
    }

    protected internal virtual T VisitExportSpecifier(ExportSpecifier exportSpecifier)
    {
        throw UnsupportedNodeType(exportSpecifier.GetType());
    }

    protected internal virtual T VisitExpressionStatement(ExpressionStatement expressionStatement)
    {
        throw UnsupportedNodeType(expressionStatement.GetType());
    }

    protected internal virtual T VisitExtension(Node node)
    {
        throw UnsupportedNodeType(node.GetType());
    }

    protected internal virtual T VisitForInStatement(ForInStatement forInStatement)
    {
        throw UnsupportedNodeType(forInStatement.GetType());
    }

    protected internal virtual T VisitForOfStatement(ForOfStatement forOfStatement)
    {
        throw UnsupportedNodeType(forOfStatement.GetType());
    }

    protected internal virtual T VisitForStatement(ForStatement forStatement)
    {
        throw UnsupportedNodeType(forStatement.GetType());
    }

    protected internal virtual T VisitFunctionDeclaration(FunctionDeclaration functionDeclaration)
    {
        throw UnsupportedNodeType(functionDeclaration.GetType());
    }

    protected internal virtual T VisitFunctionExpression(FunctionExpression functionExpression)
    {
        throw UnsupportedNodeType(functionExpression.GetType());
    }

    protected internal virtual T VisitIdentifier(Identifier identifier)
    {
        throw UnsupportedNodeType(identifier.GetType());
    }

    protected internal virtual T VisitIfStatement(IfStatement ifStatement)
    {
        throw UnsupportedNodeType(ifStatement.GetType());
    }

    protected internal virtual T VisitImport(Import import)
    {
        throw UnsupportedNodeType(import.GetType());
    }

    protected internal virtual T VisitImportAttribute(ImportAttribute importAttribute)
    {
        throw UnsupportedNodeType(importAttribute.GetType());
    }

    protected internal virtual T VisitImportDeclaration(ImportDeclaration importDeclaration)
    {
        throw UnsupportedNodeType(importDeclaration.GetType());
    }

    protected internal virtual T VisitImportDefaultSpecifier(ImportDefaultSpecifier importDefaultSpecifier)
    {
        throw UnsupportedNodeType(importDefaultSpecifier.GetType());
    }

    protected internal virtual T VisitImportNamespaceSpecifier(ImportNamespaceSpecifier importNamespaceSpecifier)
    {
        throw UnsupportedNodeType(importNamespaceSpecifier.GetType());
    }

    protected internal virtual T VisitImportSpecifier(ImportSpecifier importSpecifier)
    {
        throw UnsupportedNodeType(importSpecifier.GetType());
    }

    protected internal virtual T VisitLabeledStatement(LabeledStatement labeledStatement)
    {
        throw UnsupportedNodeType(labeledStatement.GetType());
    }

    protected internal virtual T VisitLiteral(Literal literal)
    {
        throw UnsupportedNodeType(literal.GetType());
    }

    protected internal virtual T VisitMemberExpression(MemberExpression memberExpression)
    {
        throw UnsupportedNodeType(memberExpression.GetType());
    }

    protected internal virtual T VisitMetaProperty(MetaProperty metaProperty)
    {
        throw UnsupportedNodeType(metaProperty.GetType());
    }

    protected internal virtual T VisitMethodDefinition(MethodDefinition methodDefinition)
    {
        throw UnsupportedNodeType(methodDefinition.GetType());
    }

    protected internal virtual T VisitNewExpression(NewExpression newExpression)
    {
        throw UnsupportedNodeType(newExpression.GetType());
    }

    protected internal virtual T VisitObjectExpression(ObjectExpression objectExpression)
    {
        throw UnsupportedNodeType(objectExpression.GetType());
    }

    protected internal virtual T VisitObjectPattern(ObjectPattern objectPattern)
    {
        throw UnsupportedNodeType(objectPattern.GetType());
    }

    protected internal virtual T VisitPrivateIdentifier(PrivateIdentifier privateIdentifier)
    {
        throw UnsupportedNodeType(privateIdentifier.GetType());
    }

    protected internal virtual T VisitProgram(Program program)
    {
        throw UnsupportedNodeType(program.GetType());
    }

    protected internal virtual T VisitProperty(Property property)
    {
        throw UnsupportedNodeType(property.GetType());
    }

    protected internal virtual T VisitPropertyDefinition(PropertyDefinition propertyDefinition)
    {
        throw UnsupportedNodeType(propertyDefinition.GetType());
    }

    protected internal virtual T VisitRestElement(RestElement restElement)
    {
        throw UnsupportedNodeType(restElement.GetType());
    }

    protected internal virtual T VisitReturnStatement(ReturnStatement returnStatement)
    {
        throw UnsupportedNodeType(returnStatement.GetType());
    }

    protected internal virtual T VisitSequenceExpression(SequenceExpression sequenceExpression)
    {
        throw UnsupportedNodeType(sequenceExpression.GetType());
    }

    protected internal virtual T VisitSpreadElement(SpreadElement spreadElement)
    {
        throw UnsupportedNodeType(spreadElement.GetType());
    }

    protected internal virtual T VisitStaticBlock(StaticBlock staticBlock)
    {
        throw UnsupportedNodeType(staticBlock.GetType());
    }

    protected internal virtual T VisitSuper(Super super)
    {
        throw UnsupportedNodeType(super.GetType());
    }

    protected internal virtual T VisitSwitchCase(SwitchCase switchCase)
    {
        throw UnsupportedNodeType(switchCase.GetType());
    }

    protected internal virtual T VisitSwitchStatement(SwitchStatement switchStatement)
    {
        throw UnsupportedNodeType(switchStatement.GetType());
    }

    protected internal virtual T VisitTaggedTemplateExpression(TaggedTemplateExpression taggedTemplateExpression)
    {
        throw UnsupportedNodeType(taggedTemplateExpression.GetType());
    }

    protected internal virtual T VisitTemplateElement(TemplateElement templateElement)
    {
        throw UnsupportedNodeType(templateElement.GetType());
    }

    protected internal virtual T VisitTemplateLiteral(TemplateLiteral templateLiteral)
    {
        throw UnsupportedNodeType(templateLiteral.GetType());
    }

    protected internal virtual T VisitThisExpression(ThisExpression thisExpression)
    {
        throw UnsupportedNodeType(thisExpression.GetType());
    }

    protected internal virtual T VisitThrowStatement(ThrowStatement throwStatement)
    {
        throw UnsupportedNodeType(throwStatement.GetType());
    }

    protected internal virtual T VisitTryStatement(TryStatement tryStatement)
    {
        throw UnsupportedNodeType(tryStatement.GetType());
    }

    protected internal virtual T VisitUnaryExpression(UnaryExpression unaryExpression)
    {
        throw UnsupportedNodeType(unaryExpression.GetType());
    }

    protected internal virtual T VisitVariableDeclaration(VariableDeclaration variableDeclaration)
    {
        throw UnsupportedNodeType(variableDeclaration.GetType());
    }

    protected internal virtual T VisitVariableDeclarator(VariableDeclarator variableDeclarator)
    {
        throw UnsupportedNodeType(variableDeclarator.GetType());
    }

    protected internal virtual T VisitWhileStatement(WhileStatement whileStatement)
    {
        throw UnsupportedNodeType(whileStatement.GetType());
    }

    protected internal virtual T VisitWithStatement(WithStatement withStatement)
    {
        throw UnsupportedNodeType(withStatement.GetType());
    }

    protected internal virtual T VisitYieldExpression(YieldExpression yieldExpression)
    {
        throw UnsupportedNodeType(yieldExpression.GetType());
    }
}
