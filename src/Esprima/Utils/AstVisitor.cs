using System.Runtime.CompilerServices;
using Esprima.Ast;

namespace Esprima.Utils;

public class AstVisitor
{
    private static Exception UnsupportedNodeType(Type nodeType, [CallerMemberName] string? callerName = null) =>
        new NotImplementedException($"The visitor does not support nodes of type {nodeType}. You can override {callerName} to handle this case.");

    public virtual object? Visit(Node node)
    {
        return node.Accept(this);
    }

    protected internal virtual object? VisitArrayExpression(ArrayExpression arrayExpression)
    {
        foreach (var expression in arrayExpression.Elements)
        {
            if (expression is not null)
            {
                Visit(expression);
            }
        }

        return arrayExpression;
    }

    protected internal virtual object? VisitArrayPattern(ArrayPattern arrayPattern)
    {
        foreach (var expression in arrayPattern.Elements)
        {
            if (expression is not null)
            {
                Visit(expression);
            }
        }

        return arrayPattern;
    }

    protected internal virtual object? VisitArrowFunctionExpression(ArrowFunctionExpression arrowFunctionExpression)
    {
        foreach (var parameter in arrowFunctionExpression.Params)
        {
            Visit(parameter);
        }

        Visit(arrowFunctionExpression.Body);

        return arrowFunctionExpression;
    }

    protected internal virtual object? VisitArrowParameterPlaceHolder(ArrowParameterPlaceHolder arrowParameterPlaceHolder)
    {
        // Seems that ArrowParameterPlaceHolder nodes never appear in the final tree and only used during the construction of a tree.
        // Though we provide the VisitArrowParameterPlaceHolder method for inheritors in case they still needed it.

        throw UnsupportedNodeType(arrowParameterPlaceHolder.GetType());
    }

    protected internal virtual object? VisitAssignmentExpression(AssignmentExpression assignmentExpression)
    {
        Visit(assignmentExpression.Left);
        Visit(assignmentExpression.Right);

        return assignmentExpression;
    }

    protected internal virtual object? VisitAssignmentPattern(AssignmentPattern assignmentPattern)
    {
        Visit(assignmentPattern.Left);
        Visit(assignmentPattern.Right);

        return assignmentPattern;
    }

    protected internal virtual object? VisitAwaitExpression(AwaitExpression awaitExpression)
    {
        Visit(awaitExpression.Argument);

        return awaitExpression;
    }

    protected internal virtual object? VisitBinaryExpression(BinaryExpression binaryExpression)
    {
        Visit(binaryExpression.Left);
        Visit(binaryExpression.Right);

        return binaryExpression;
    }

    protected internal virtual object? VisitBlockStatement(BlockStatement blockStatement)
    {
        foreach (var statement in blockStatement.Body)
        {
            Visit(statement);
        }

        return blockStatement;
    }

    protected internal virtual object? VisitBreakStatement(BreakStatement breakStatement)
    {
        if (breakStatement.Label is not null)
        {
            Visit(breakStatement.Label);
        }

        return breakStatement;
    }

    protected internal virtual object? VisitCallExpression(CallExpression callExpression)
    {
        Visit(callExpression.Callee);
        foreach (var argument in callExpression.Arguments)
        {
            Visit(argument);
        }

        return callExpression;
    }

    protected internal virtual object? VisitCatchClause(CatchClause catchClause)
    {
        if (catchClause.Param is not null)
        {
            Visit(catchClause.Param);
        }

        Visit(catchClause.Body);

        return catchClause;
    }

    protected internal virtual object? VisitChainExpression(ChainExpression chainExpression)
    {
        Visit(chainExpression.Expression);

        return chainExpression;
    }

    protected internal virtual object? VisitClassBody(ClassBody classBody)
    {
        foreach (var statement in classBody.Body)
        {
            Visit(statement);
        }

        return classBody;
    }

    protected internal virtual object? VisitClassDeclaration(ClassDeclaration classDeclaration)
    {
        if (classDeclaration.Id is not null)
        {
            Visit(classDeclaration.Id);
        }

        if (classDeclaration.SuperClass is not null)
        {
            Visit(classDeclaration.SuperClass);
        }

        Visit(classDeclaration.Body);

        foreach (var decorator in classDeclaration.Decorators)
        {
            Visit(decorator);
        }

        return classDeclaration;
    }

    protected internal virtual object? VisitClassExpression(ClassExpression classExpression)
    {
        if (classExpression.Id is not null)
        {
            Visit(classExpression.Id);
        }

        if (classExpression.SuperClass is not null)
        {
            Visit(classExpression.SuperClass);
        }

        Visit(classExpression.Body);

        foreach (var decorator in classExpression.Decorators)
        {
            Visit(decorator);
        }

        return classExpression;
    }

    protected internal virtual object? VisitConditionalExpression(ConditionalExpression conditionalExpression)
    {
        Visit(conditionalExpression.Test);
        Visit(conditionalExpression.Consequent);
        Visit(conditionalExpression.Alternate);

        return conditionalExpression;
    }

    protected internal virtual object? VisitContinueStatement(ContinueStatement continueStatement)
    {
        if (continueStatement.Label is not null)
        {
            Visit(continueStatement.Label);
        }

        return continueStatement;
    }

    protected internal virtual object? VisitDebuggerStatement(DebuggerStatement debuggerStatement)
    {
        return debuggerStatement;
    }

    protected internal virtual object? VisitDecorator(Decorator decorator)
    {
        Visit(decorator.Expression);

        return decorator;
    }

    protected internal virtual object? VisitDoWhileStatement(DoWhileStatement doWhileStatement)
    {
        Visit(doWhileStatement.Body);
        Visit(doWhileStatement.Test);

        return doWhileStatement;
    }

    protected internal virtual object? VisitEmptyStatement(EmptyStatement emptyStatement)
    {
        return emptyStatement;
    }

    protected internal virtual object? VisitExportAllDeclaration(ExportAllDeclaration exportAllDeclaration)
    {
        if (exportAllDeclaration.Exported is not null)
        {
            Visit(exportAllDeclaration.Exported);
        }

        Visit(exportAllDeclaration.Source);

        foreach (var assertion in exportAllDeclaration.Assertions)
        {
            Visit(assertion);
        }

        return exportAllDeclaration;
    }

    protected internal virtual object? VisitExportDefaultDeclaration(ExportDefaultDeclaration exportDefaultDeclaration)
    {
        Visit(exportDefaultDeclaration.Declaration);

        return exportDefaultDeclaration;
    }

    protected internal virtual object? VisitExportNamedDeclaration(ExportNamedDeclaration exportNamedDeclaration)
    {
        if (exportNamedDeclaration.Declaration is not null)
        {
            Visit(exportNamedDeclaration.Declaration);
        }

        foreach (var specifier in exportNamedDeclaration.Specifiers)
        {
            Visit(specifier);
        }

        if (exportNamedDeclaration.Source is not null)
        {
            Visit(exportNamedDeclaration.Source);
        }

        foreach (var assertion in exportNamedDeclaration.Assertions)
        {
            Visit(assertion);
        }

        return exportNamedDeclaration;
    }

    protected internal virtual object? VisitExportSpecifier(ExportSpecifier exportSpecifier)
    {
        Visit(exportSpecifier.Local);
        Visit(exportSpecifier.Exported);

        return exportSpecifier;
    }

    protected internal virtual object? VisitExpressionStatement(ExpressionStatement expressionStatement)
    {
        Visit(expressionStatement.Expression);

        return expressionStatement;
    }

    protected internal virtual object? VisitExtension(Node node)
    {
        // Node type Extension is used to represent extensions to the standard AST (for example, see JSX parsing).
        // Nodes of this type never appear in the tree returned by the core parser (JavaScriptParser),
        // thus the visitor doesn't deal with this type by default. Inheritors either need to override this method,
        // or inherit from another visitor which was built to handle extension nodes (e.g. JsxAstVisitor in the case of JSX).

        throw UnsupportedNodeType(node.GetType());
    }

    protected internal virtual object? VisitForInStatement(ForInStatement forInStatement)
    {
        Visit(forInStatement.Left);
        Visit(forInStatement.Right);
        Visit(forInStatement.Body);

        return forInStatement;
    }

    protected internal virtual object? VisitForOfStatement(ForOfStatement forOfStatement)
    {
        Visit(forOfStatement.Left);
        Visit(forOfStatement.Right);
        Visit(forOfStatement.Body);

        return forOfStatement;
    }

    protected internal virtual object? VisitForStatement(ForStatement forStatement)
    {
        if (forStatement.Init is not null)
        {
            Visit(forStatement.Init);
        }

        if (forStatement.Test is not null)
        {
            Visit(forStatement.Test);
        }

        if (forStatement.Update is not null)
        {
            Visit(forStatement.Update);
        }

        Visit(forStatement.Body);

        return forStatement;
    }

    protected internal virtual object? VisitFunctionDeclaration(FunctionDeclaration functionDeclaration)
    {
        if (functionDeclaration.Id is not null)
        {
            Visit(functionDeclaration.Id);
        }

        foreach (var parameter in functionDeclaration.Params)
        {
            Visit(parameter);
        }

        Visit(functionDeclaration.Body);

        return functionDeclaration;
    }

    protected internal virtual object? VisitFunctionExpression(FunctionExpression functionExpression)
    {
        if (functionExpression.Id is not null)
        {
            Visit(functionExpression.Id);
        }

        foreach (var parameter in functionExpression.Params)
        {
            Visit(parameter);
        }

        Visit(functionExpression.Body);

        return functionExpression;
    }

    protected internal virtual object? VisitIdentifier(Identifier identifier)
    {
        return identifier;
    }

    protected internal virtual object? VisitIfStatement(IfStatement ifStatement)
    {
        Visit(ifStatement.Test);
        Visit(ifStatement.Consequent);
        if (ifStatement.Alternate is not null)
        {
            Visit(ifStatement.Alternate);
        }

        return ifStatement;
    }

    protected internal virtual object? VisitImport(Import import)
    {
        Visit(import.Source);

        if (import.Attributes is not null)
        {
            Visit(import.Attributes);
        }

        return import;
    }

    protected internal virtual object? VisitImportAttribute(ImportAttribute importAttribute)
    {
        Visit(importAttribute.Key);
        Visit(importAttribute.Value);

        return importAttribute;
    }

    protected internal virtual object? VisitImportDeclaration(ImportDeclaration importDeclaration)
    {
        foreach (var specifier in importDeclaration.Specifiers)
        {
            Visit(specifier);
        }

        Visit(importDeclaration.Source);

        foreach (var assertion in importDeclaration.Assertions)
        {
            Visit(assertion);
        }

        return importDeclaration;
    }

    protected internal virtual object? VisitImportDefaultSpecifier(ImportDefaultSpecifier importDefaultSpecifier)
    {
        Visit(importDefaultSpecifier.Local);

        return importDefaultSpecifier;
    }

    protected internal virtual object? VisitImportNamespaceSpecifier(ImportNamespaceSpecifier importNamespaceSpecifier)
    {
        Visit(importNamespaceSpecifier.Local);

        return importNamespaceSpecifier;
    }

    protected internal virtual object? VisitImportSpecifier(ImportSpecifier importSpecifier)
    {
        Visit(importSpecifier.Imported);
        Visit(importSpecifier.Local);

        return importSpecifier;
    }

    protected internal virtual object? VisitLabeledStatement(LabeledStatement labeledStatement)
    {
        Visit(labeledStatement.Label);
        Visit(labeledStatement.Body);

        return labeledStatement;
    }

    protected internal virtual object? VisitLiteral(Literal literal)
    {
        return literal;
    }

    protected internal virtual object? VisitMemberExpression(MemberExpression memberExpression)
    {
        Visit(memberExpression.Object);
        Visit(memberExpression.Property);

        return memberExpression;
    }

    protected internal virtual object? VisitMetaProperty(MetaProperty metaProperty)
    {
        Visit(metaProperty.Meta);
        Visit(metaProperty.Property);

        return metaProperty;
    }

    protected internal virtual object? VisitMethodDefinition(MethodDefinition methodDefinition)
    {
        Visit(methodDefinition.Key);
        Visit(methodDefinition.Value);

        foreach (var decorator in methodDefinition.Decorators)
        {
            Visit(decorator);
        }

        return methodDefinition;
    }

    protected internal virtual object? VisitNewExpression(NewExpression newExpression)
    {
        Visit(newExpression.Callee);
        foreach (var argument in newExpression.Arguments)
        {
            Visit(argument);
        }

        return newExpression;
    }

    protected internal virtual object? VisitObjectExpression(ObjectExpression objectExpression)
    {
        foreach (var property in objectExpression.Properties)
        {
            Visit(property);
        }

        return objectExpression;
    }

    protected internal virtual object? VisitObjectPattern(ObjectPattern objectPattern)
    {
        foreach (var property in objectPattern.Properties)
        {
            Visit(property);
        }

        return objectPattern;
    }

    protected internal virtual object? VisitPrivateIdentifier(PrivateIdentifier privateIdentifier)
    {
        return privateIdentifier;
    }

    protected internal virtual object? VisitProgram(Program program)
    {
        foreach (var statement in program.Body)
        {
            Visit(statement);
        }

        return program;
    }

    protected internal virtual object? VisitProperty(Property property)
    {
        Visit(property.Key);
        Visit(property.Value);

        return property;
    }

    protected internal virtual object? VisitPropertyDefinition(PropertyDefinition propertyDefinition)
    {
        Visit(propertyDefinition.Key);

        if (propertyDefinition.Value is not null)
        {
            Visit(propertyDefinition.Value);
        }

        foreach (var decorator in propertyDefinition.Decorators)
        {
            Visit(decorator);
        }

        return propertyDefinition;
    }

    protected internal virtual object? VisitRestElement(RestElement restElement)
    {
        Visit(restElement.Argument);

        return restElement;
    }

    protected internal virtual object? VisitReturnStatement(ReturnStatement returnStatement)
    {
        if (returnStatement.Argument is not null)
        {
            Visit(returnStatement.Argument);
        }

        return returnStatement;
    }

    protected internal virtual object? VisitSequenceExpression(SequenceExpression sequenceExpression)
    {
        foreach (var expression in sequenceExpression.Expressions)
        {
            Visit(expression);
        }

        return sequenceExpression;
    }

    protected internal virtual object? VisitSpreadElement(SpreadElement spreadElement)
    {
        Visit(spreadElement.Argument);

        return spreadElement;
    }

    protected internal virtual object? VisitStaticBlock(StaticBlock staticBlock)
    {
        foreach (var statement in staticBlock.Body)
        {
            Visit(statement);
        }

        return staticBlock;
    }

    protected internal virtual object? VisitSuper(Super super)
    {
        return super;
    }

    protected internal virtual object? VisitSwitchCase(SwitchCase switchCase)
    {
        if (switchCase.Test is not null)
        {
            Visit(switchCase.Test);
        }

        foreach (var statement in switchCase.Consequent)
        {
            Visit(statement);
        }

        return switchCase;
    }

    protected internal virtual object? VisitSwitchStatement(SwitchStatement switchStatement)
    {
        Visit(switchStatement.Discriminant);
        foreach (var switchCase in switchStatement.Cases)
        {
            Visit(switchCase);
        }

        return switchStatement;
    }

    protected internal virtual object? VisitTaggedTemplateExpression(TaggedTemplateExpression taggedTemplateExpression)
    {
        Visit(taggedTemplateExpression.Tag);
        Visit(taggedTemplateExpression.Quasi);

        return taggedTemplateExpression;
    }

    protected internal virtual object? VisitTemplateElement(TemplateElement templateElement)
    {
        return templateElement;
    }

    protected internal virtual object? VisitTemplateLiteral(TemplateLiteral templateLiteral)
    {
        var quasis = templateLiteral.Quasis;
        var expressions = templateLiteral.Expressions;

        TemplateElement quasi;
        for (var i = 0; !(quasi = quasis[i]).Tail; i++)
        {
            Visit(quasi);
            Visit(expressions[i]);
        }
        Visit(quasi);

        return templateLiteral;
    }

    protected internal virtual object? VisitThisExpression(ThisExpression thisExpression)
    {
        return thisExpression;
    }

    protected internal virtual object? VisitThrowStatement(ThrowStatement throwStatement)
    {
        Visit(throwStatement.Argument);

        return throwStatement;
    }

    protected internal virtual object? VisitTryStatement(TryStatement tryStatement)
    {
        Visit(tryStatement.Block);
        if (tryStatement.Handler is not null)
        {
            Visit(tryStatement.Handler);
        }

        if (tryStatement.Finalizer is not null)
        {
            Visit(tryStatement.Finalizer);
        }

        return tryStatement;
    }

    protected internal virtual object? VisitUnaryExpression(UnaryExpression unaryExpression)
    {
        Visit(unaryExpression.Argument);

        return unaryExpression;
    }

    protected internal virtual object? VisitVariableDeclaration(VariableDeclaration variableDeclaration)
    {
        foreach (var declaration in variableDeclaration.Declarations)
        {
            Visit(declaration);
        }

        return variableDeclaration;
    }

    protected internal virtual object? VisitVariableDeclarator(VariableDeclarator variableDeclarator)
    {
        Visit(variableDeclarator.Id);
        if (variableDeclarator.Init is not null)
        {
            Visit(variableDeclarator.Init);
        }

        return variableDeclarator;
    }

    protected internal virtual object? VisitWhileStatement(WhileStatement whileStatement)
    {
        Visit(whileStatement.Test);
        Visit(whileStatement.Body);

        return whileStatement;
    }

    protected internal virtual object? VisitWithStatement(WithStatement withStatement)
    {
        Visit(withStatement.Object);
        Visit(withStatement.Body);

        return withStatement;
    }

    protected internal virtual object? VisitYieldExpression(YieldExpression yieldExpression)
    {
        if (yieldExpression.Argument is not null)
        {
            Visit(yieldExpression.Argument);
        }

        return yieldExpression;
    }
}
