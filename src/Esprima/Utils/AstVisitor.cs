using System.Runtime.CompilerServices;
using Esprima.Ast;

namespace Esprima.Utils;

public class AstVisitor
{
    private static Exception UnsupportedNodeType(Type nodeType, [CallerMemberName] string? callerName = null) =>
        new NotImplementedException($"The visitor does not support nodes of type {nodeType}. You can override {callerName} to handle this case.");

    public virtual object? Visit(Node node, object? context = null)
    {
        return node.Accept(this, context);
    }

    protected internal virtual object? VisitArrayExpression(ArrayExpression arrayExpression, object? context)
    {
        ref readonly var elements = ref arrayExpression.Elements;
        for (var i = 0; i < elements.Count; i++)
        {
            var expr = elements[i];
            if (expr is not null)
            {
                Visit(expr, context);
            }
        }

        return arrayExpression;
    }

    protected internal virtual object? VisitArrayPattern(ArrayPattern arrayPattern, object? context)
    {
        ref readonly var elements = ref arrayPattern.Elements;
        for (var i = 0; i < elements.Count; i++)
        {
            var expr = elements[i];
            if (expr is not null)
            {
                Visit(expr, context);
            }
        }

        return arrayPattern;
    }

    protected internal virtual object? VisitArrowFunctionExpression(ArrowFunctionExpression arrowFunctionExpression, object? context)
    {
        ref readonly var parameters = ref arrowFunctionExpression.Params;
        for (var i = 0; i < parameters.Count; i++)
        {
            Visit(parameters[i], context);
        }

        Visit(arrowFunctionExpression.Body, context);

        return arrowFunctionExpression;
    }

    protected internal virtual object? VisitArrowParameterPlaceHolder(ArrowParameterPlaceHolder arrowParameterPlaceHolder, object? context)
    {
        // Seems that ArrowParameterPlaceHolder nodes never appear in the final tree and only used during the construction of a tree.
        // Though we provide the VisitArrowParameterPlaceHolder method for inheritors in case they still needed it.

        throw UnsupportedNodeType(arrowParameterPlaceHolder.GetType());
    }

    protected internal virtual object? VisitAssignmentExpression(AssignmentExpression assignmentExpression, object? context)
    {
        Visit(assignmentExpression.Left, context);
        Visit(assignmentExpression.Right, context);

        return assignmentExpression;
    }

    protected internal virtual object? VisitAssignmentPattern(AssignmentPattern assignmentPattern, object? context)
    {
        Visit(assignmentPattern.Left, context);
        Visit(assignmentPattern.Right, context);

        return assignmentPattern;
    }

    protected internal virtual object? VisitAwaitExpression(AwaitExpression awaitExpression, object? context)
    {
        Visit(awaitExpression.Argument, context);

        return awaitExpression;
    }

    protected internal virtual object? VisitBinaryExpression(BinaryExpression binaryExpression, object? context)
    {
        Visit(binaryExpression.Left, context);
        Visit(binaryExpression.Right, context);

        return binaryExpression;
    }

    protected internal virtual object? VisitBlockStatement(BlockStatement blockStatement, object? context)
    {
        ref readonly var body = ref blockStatement.Body;
        for (var i = 0; i < body.Count; i++)
        {
            Visit(body[i], context);
        }

        return blockStatement;
    }

    protected internal virtual object? VisitBreakStatement(BreakStatement breakStatement, object? context)
    {
        if (breakStatement.Label is not null)
        {
            Visit(breakStatement.Label, context);
        }

        return breakStatement;
    }

    protected internal virtual object? VisitCallExpression(CallExpression callExpression, object? context)
    {
        Visit(callExpression.Callee, context);
        ref readonly var arguments = ref callExpression.Arguments;
        for (var i = 0; i < arguments.Count; i++)
        {
            Visit(arguments[i], context);
        }

        return callExpression;
    }

    protected internal virtual object? VisitCatchClause(CatchClause catchClause, object? context)
    {
        if (catchClause.Param is not null)
        {
            Visit(catchClause.Param, context);
        }

        Visit(catchClause.Body, context);

        return catchClause;
    }

    protected internal virtual object? VisitChainExpression(ChainExpression chainExpression, object? context)
    {
        Visit(chainExpression.Expression, context);

        return chainExpression;
    }

    protected internal virtual object? VisitClassBody(ClassBody classBody, object? context)
    {
        ref readonly var body = ref classBody.Body;
        for (var i = 0; i < body.Count; i++)
        {
            Visit(body[i], context);
        }

        return classBody;
    }

    protected internal virtual object? VisitClassDeclaration(ClassDeclaration classDeclaration, object? context)
    {
        if (classDeclaration.Id is not null)
        {
            Visit(classDeclaration.Id, context);
        }

        if (classDeclaration.SuperClass is not null)
        {
            Visit(classDeclaration.SuperClass, context);
        }

        Visit(classDeclaration.Body, context);

        ref readonly var decorators = ref classDeclaration.Decorators;
        for (var i = 0; i < decorators.Count; i++)
        {
            Visit(decorators[i], context);
        }

        return classDeclaration;
    }

    protected internal virtual object? VisitClassExpression(ClassExpression classExpression, object? context)
    {
        if (classExpression.Id is not null)
        {
            Visit(classExpression.Id, context);
        }

        if (classExpression.SuperClass is not null)
        {
            Visit(classExpression.SuperClass, context);
        }

        Visit(classExpression.Body, context);

        ref readonly var decorators = ref classExpression.Decorators;
        for (var i = 0; i < decorators.Count; i++)
        {
            Visit(decorators[i], context);
        }

        return classExpression;
    }

    protected internal virtual object? VisitConditionalExpression(ConditionalExpression conditionalExpression, object? context)
    {
        Visit(conditionalExpression.Test, context);
        Visit(conditionalExpression.Consequent, context);
        Visit(conditionalExpression.Alternate, context);

        return conditionalExpression;
    }

    protected internal virtual object? VisitContinueStatement(ContinueStatement continueStatement, object? context)
    {
        if (continueStatement.Label is not null)
        {
            Visit(continueStatement.Label, context);
        }

        return continueStatement;
    }

    protected internal virtual object? VisitDebuggerStatement(DebuggerStatement debuggerStatement, object? context)
    {
        return debuggerStatement;
    }

    protected internal virtual object? VisitDecorator(Decorator decorator, object? context)
    {
        Visit(decorator.Expression, context);

        return decorator;
    }

    protected internal virtual object? VisitDoWhileStatement(DoWhileStatement doWhileStatement, object? context)
    {
        Visit(doWhileStatement.Body, context);
        Visit(doWhileStatement.Test, context);

        return doWhileStatement;
    }

    protected internal virtual object? VisitEmptyStatement(EmptyStatement emptyStatement, object? context)
    {
        return emptyStatement;
    }

    protected internal virtual object? VisitExportAllDeclaration(ExportAllDeclaration exportAllDeclaration, object? context)
    {
        if (exportAllDeclaration.Exported is not null)
        {
            Visit(exportAllDeclaration.Exported, context);
        }

        Visit(exportAllDeclaration.Source, context);

        ref readonly var assertions = ref exportAllDeclaration.Assertions;
        for (var i = 0; i < assertions.Count; i++)
        {
            Visit(assertions[i], context);
        }

        return exportAllDeclaration;
    }

    protected internal virtual object? VisitExportDefaultDeclaration(ExportDefaultDeclaration exportDefaultDeclaration, object? context)
    {
        Visit(exportDefaultDeclaration.Declaration, context);

        return exportDefaultDeclaration;
    }

    protected internal virtual object? VisitExportNamedDeclaration(ExportNamedDeclaration exportNamedDeclaration, object? context)
    {
        if (exportNamedDeclaration.Declaration is not null)
        {
            Visit(exportNamedDeclaration.Declaration, context);
        }

        ref readonly var specifiers = ref exportNamedDeclaration.Specifiers;
        for (var i = 0; i < specifiers.Count; i++)
        {
            Visit(specifiers[i], context);
        }

        if (exportNamedDeclaration.Source is not null)
        {
            Visit(exportNamedDeclaration.Source, context);
        }

        ref readonly var assertions = ref exportNamedDeclaration.Assertions;
        for (var i = 0; i < assertions.Count; i++)
        {
            Visit(assertions[i], context);
        }

        return exportNamedDeclaration;
    }

    protected internal virtual object? VisitExportSpecifier(ExportSpecifier exportSpecifier, object? context)
    {
        Visit(exportSpecifier.Local, context);
        Visit(exportSpecifier.Exported, context);

        return exportSpecifier;
    }

    protected internal virtual object? VisitExpressionStatement(ExpressionStatement expressionStatement, object? context)
    {
        Visit(expressionStatement.Expression, context);

        return expressionStatement;
    }

    protected internal virtual object? VisitExtension(Node node, object? context)
    {
        // Node type Extension is used to represent extensions to the standard AST (for example, see JSX parsing).
        // Nodes of this type never appear in the tree returned by the core parser (JavaScriptParser),
        // thus the visitor doesn't deal with this type by default. Inheritors either need to override this method,
        // or inherit from another visitor which was built to handle extension nodes (e.g. JsxAstVisitor in the case of JSX).

        throw UnsupportedNodeType(node.GetType());
    }

    protected internal virtual object? VisitForInStatement(ForInStatement forInStatement, object? context)
    {
        Visit(forInStatement.Left, context);
        Visit(forInStatement.Right, context);
        Visit(forInStatement.Body, context);

        return forInStatement;
    }

    protected internal virtual object? VisitForOfStatement(ForOfStatement forOfStatement, object? context)
    {
        Visit(forOfStatement.Left, context);
        Visit(forOfStatement.Right, context);
        Visit(forOfStatement.Body, context);

        return forOfStatement;
    }

    protected internal virtual object? VisitForStatement(ForStatement forStatement, object? context)
    {
        if (forStatement.Init is not null)
        {
            Visit(forStatement.Init, context);
        }

        if (forStatement.Test is not null)
        {
            Visit(forStatement.Test, context);
        }

        if (forStatement.Update is not null)
        {
            Visit(forStatement.Update, context);
        }

        Visit(forStatement.Body, context);

        return forStatement;
    }

    protected internal virtual object? VisitFunctionDeclaration(FunctionDeclaration functionDeclaration, object? context)
    {
        if (functionDeclaration.Id is not null)
        {
            Visit(functionDeclaration.Id, context);
        }

        ref readonly var parameters = ref functionDeclaration.Params;
        for (var i = 0; i < parameters.Count; i++)
        {
            Visit(parameters[i], context);
        }

        Visit(functionDeclaration.Body, context);

        return functionDeclaration;
    }

    protected internal virtual object? VisitFunctionExpression(FunctionExpression functionExpression, object? context)
    {
        if (functionExpression.Id is not null)
        {
            Visit(functionExpression.Id, context);
        }

        ref readonly var parameters = ref functionExpression.Params;
        for (var i = 0; i < parameters.Count; i++)
        {
            Visit(parameters[i], context);
        }

        Visit(functionExpression.Body, context);

        return functionExpression;
    }

    protected internal virtual object? VisitIdentifier(Identifier identifier, object? context)
    {
        return identifier;
    }

    protected internal virtual object? VisitIfStatement(IfStatement ifStatement, object? context)
    {
        Visit(ifStatement.Test, context);
        Visit(ifStatement.Consequent, context);
        if (ifStatement.Alternate is not null)
        {
            Visit(ifStatement.Alternate, context);
        }

        return ifStatement;
    }

    protected internal virtual object? VisitImport(Import import, object? context)
    {
        Visit(import.Source, context);

        if (import.Attributes is not null)
        {
            Visit(import.Attributes, context);
        }

        return import;
    }

    protected internal virtual object? VisitImportAttribute(ImportAttribute importAttribute, object? context)
    {
        Visit(importAttribute.Key, context);
        Visit(importAttribute.Value, context);

        return importAttribute;
    }

    protected internal virtual object? VisitImportDeclaration(ImportDeclaration importDeclaration, object? context)
    {
        ref readonly var specifiers = ref importDeclaration.Specifiers;
        for (var i = 0; i < specifiers.Count; i++)
        {
            Visit(specifiers[i], context);
        }

        Visit(importDeclaration.Source, context);

        ref readonly var assertions = ref importDeclaration.Assertions;
        for (var i = 0; i < assertions.Count; i++)
        {
            Visit(assertions[i], context);
        }

        return importDeclaration;
    }

    protected internal virtual object? VisitImportDefaultSpecifier(ImportDefaultSpecifier importDefaultSpecifier, object? context)
    {
        Visit(importDefaultSpecifier.Local, context);

        return importDefaultSpecifier;
    }

    protected internal virtual object? VisitImportNamespaceSpecifier(ImportNamespaceSpecifier importNamespaceSpecifier, object? context)
    {
        Visit(importNamespaceSpecifier.Local, context);

        return importNamespaceSpecifier;
    }

    protected internal virtual object? VisitImportSpecifier(ImportSpecifier importSpecifier, object? context)
    {
        Visit(importSpecifier.Imported, context);
        Visit(importSpecifier.Local, context);

        return importSpecifier;
    }

    protected internal virtual object? VisitLabeledStatement(LabeledStatement labeledStatement, object? context)
    {
        Visit(labeledStatement.Label, context);
        Visit(labeledStatement.Body, context);

        return labeledStatement;
    }

    protected internal virtual object? VisitLiteral(Literal literal, object? context)
    {
        return literal;
    }

    protected internal virtual object? VisitMemberExpression(MemberExpression memberExpression, object? context)
    {
        Visit(memberExpression.Object, context);
        Visit(memberExpression.Property, context);

        return memberExpression;
    }

    protected internal virtual object? VisitMetaProperty(MetaProperty metaProperty, object? context)
    {
        Visit(metaProperty.Meta, context);
        Visit(metaProperty.Property, context);

        return metaProperty;
    }

    protected internal virtual object? VisitMethodDefinition(MethodDefinition methodDefinition, object? context)
    {
        Visit(methodDefinition.Key, context);
        Visit(methodDefinition.Value, context);

        ref readonly var decorators = ref methodDefinition.Decorators;
        for (var i = 0; i < decorators.Count; i++)
        {
            Visit(decorators[i], context);
        }

        return methodDefinition;
    }

    protected internal virtual object? VisitNewExpression(NewExpression newExpression, object? context)
    {
        Visit(newExpression.Callee, context);
        ref readonly var arguments = ref newExpression.Arguments;
        for (var i = 0; i < arguments.Count; i++)
        {
            Visit(arguments[i], context);
        }

        return newExpression;
    }

    protected internal virtual object? VisitObjectExpression(ObjectExpression objectExpression, object? context)
    {
        ref readonly var properties = ref objectExpression.Properties;
        for (var i = 0; i < properties.Count; i++)
        {
            Visit(properties[i], context);
        }

        return objectExpression;
    }

    protected internal virtual object? VisitObjectPattern(ObjectPattern objectPattern, object? context)
    {
        ref readonly var properties = ref objectPattern.Properties;
        for (var i = 0; i < properties.Count; i++)
        {
            Visit(properties[i], context);
        }

        return objectPattern;
    }

    protected internal virtual object? VisitPrivateIdentifier(PrivateIdentifier privateIdentifier, object? context)
    {
        return privateIdentifier;
    }

    protected internal virtual object? VisitProgram(Program program, object? context)
    {
        ref readonly var statements = ref program.Body;
        for (var i = 0; i < statements.Count; i++)
        {
            Visit(statements[i], context);
        }

        return program;
    }

    protected internal virtual object? VisitProperty(Property property, object? context)
    {
        Visit(property.Key, context);
        Visit(property.Value, context);

        return property;
    }

    protected internal virtual object? VisitPropertyDefinition(PropertyDefinition propertyDefinition, object? context)
    {
        Visit(propertyDefinition.Key, context);

        if (propertyDefinition.Value is not null)
        {
            Visit(propertyDefinition.Value, context);
        }

        ref readonly var decorators = ref propertyDefinition.Decorators;
        for (var i = 0; i < decorators.Count; i++)
        {
            Visit(decorators[i], context);
        }

        return propertyDefinition;
    }

    protected internal virtual object? VisitRestElement(RestElement restElement, object? context)
    {
        Visit(restElement.Argument, context);

        return restElement;
    }

    protected internal virtual object? VisitReturnStatement(ReturnStatement returnStatement, object? context)
    {
        if (returnStatement.Argument is not null)
        {
            Visit(returnStatement.Argument, context);
        }

        return returnStatement;
    }

    protected internal virtual object? VisitSequenceExpression(SequenceExpression sequenceExpression, object? context)
    {
        ref readonly var expressions = ref sequenceExpression.Expressions;
        for (var i = 0; i < expressions.Count; i++)
        {
            Visit(expressions[i], context);
        }

        return sequenceExpression;
    }

    protected internal virtual object? VisitSpreadElement(SpreadElement spreadElement, object? context)
    {
        Visit(spreadElement.Argument, context);

        return spreadElement;
    }

    protected internal virtual object? VisitStaticBlock(StaticBlock staticBlock, object? context)
    {
        ref readonly var body = ref staticBlock.Body;
        for (var i = 0; i < body.Count; i++)
        {
            Visit(body[i], context);
        }

        return staticBlock;
    }

    protected internal virtual object? VisitSuper(Super super, object? context)
    {
        return super;
    }

    protected internal virtual object? VisitSwitchCase(SwitchCase switchCase, object? context)
    {
        if (switchCase.Test is not null)
        {
            Visit(switchCase.Test, context);
        }

        ref readonly var consequent = ref switchCase.Consequent;
        for (var i = 0; i < consequent.Count; i++)
        {
            Visit(consequent[i], context);
        }

        return switchCase;
    }

    protected internal virtual object? VisitSwitchStatement(SwitchStatement switchStatement, object? context)
    {
        Visit(switchStatement.Discriminant, context);
        ref readonly var cases = ref switchStatement.Cases;
        for (var i = 0; i < cases.Count; i++)
        {
            Visit(cases[i], context);
        }

        return switchStatement;
    }

    protected internal virtual object? VisitTaggedTemplateExpression(TaggedTemplateExpression taggedTemplateExpression, object? context)
    {
        Visit(taggedTemplateExpression.Tag, context);
        Visit(taggedTemplateExpression.Quasi, context);

        return taggedTemplateExpression;
    }

    protected internal virtual object? VisitTemplateElement(TemplateElement templateElement, object? context)
    {
        return templateElement;
    }

    protected internal virtual object? VisitTemplateLiteral(TemplateLiteral templateLiteral, object? context)
    {
        ref readonly var quasis = ref templateLiteral.Quasis;
        ref readonly var expressions = ref templateLiteral.Expressions;

        TemplateElement quasi;
        for (var i = 0; !(quasi = quasis[i]).Tail; i++)
        {
            Visit(quasi, context);
            Visit(expressions[i], context);
        }
        Visit(quasi, context);

        return templateLiteral;
    }

    protected internal virtual object? VisitThisExpression(ThisExpression thisExpression, object? context)
    {
        return thisExpression;
    }

    protected internal virtual object? VisitThrowStatement(ThrowStatement throwStatement, object? context)
    {
        Visit(throwStatement.Argument, context);

        return throwStatement;
    }

    protected internal virtual object? VisitTryStatement(TryStatement tryStatement, object? context)
    {
        Visit(tryStatement.Block, context);
        if (tryStatement.Handler is not null)
        {
            Visit(tryStatement.Handler, context);
        }

        if (tryStatement.Finalizer is not null)
        {
            Visit(tryStatement.Finalizer, context);
        }

        return tryStatement;
    }

    protected internal virtual object? VisitUnaryExpression(UnaryExpression unaryExpression, object? context)
    {
        Visit(unaryExpression.Argument, context);

        return unaryExpression;
    }

    protected internal virtual object? VisitVariableDeclaration(VariableDeclaration variableDeclaration, object? context)
    {
        ref readonly var declarations = ref variableDeclaration.Declarations;
        for (var i = 0; i < declarations.Count; i++)
        {
            Visit(declarations[i], context);
        }

        return variableDeclaration;
    }

    protected internal virtual object? VisitVariableDeclarator(VariableDeclarator variableDeclarator, object? context)
    {
        Visit(variableDeclarator.Id, context);
        if (variableDeclarator.Init is not null)
        {
            Visit(variableDeclarator.Init, context);
        }

        return variableDeclarator;
    }

    protected internal virtual object? VisitWhileStatement(WhileStatement whileStatement, object? context)
    {
        Visit(whileStatement.Test, context);
        Visit(whileStatement.Body, context);

        return whileStatement;
    }

    protected internal virtual object? VisitWithStatement(WithStatement withStatement, object? context)
    {
        Visit(withStatement.Object, context);
        Visit(withStatement.Body, context);

        return withStatement;
    }

    protected internal virtual object? VisitYieldExpression(YieldExpression yieldExpression, object? context)
    {
        if (yieldExpression.Argument is not null)
        {
            Visit(yieldExpression.Argument, context);
        }

        return yieldExpression;
    }
}
