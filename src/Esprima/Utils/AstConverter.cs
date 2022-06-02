﻿using System.Runtime.CompilerServices;
using Esprima.Ast;

namespace Esprima.Utils;

public abstract partial class AstConverter : AstVisitor
{
    public virtual T VisitAndConvert<T>(T node, bool allowNull = false, [CallerMemberName] string? callerName = null)
        where T : Node?
    {
        if (node is null)
        {
            return allowNull ? null! : throw new ArgumentNullException(nameof(node));
        }

        return Visit(node) switch
        {
            T convertedNode => convertedNode,
            null when allowNull => null!,
            null => throw MustRewriteToSameNodeNonNullable(typeof(T), callerName),
            _ => throw (allowNull ? MustRewriteToSameNodeNullable(typeof(T), callerName) : MustRewriteToSameNodeNonNullable(typeof(T), callerName))
        };

        static Exception MustRewriteToSameNodeNonNullable(Type nodeType, string? callerName) =>
            new InvalidOperationException($"When called from {callerName}, rewriting a node of type {nodeType} must return a non-null value of the same type. Alternatively, override {callerName} and change it to not visit children of this type.");

        static Exception MustRewriteToSameNodeNullable(Type nodeType, string? callerName) =>
            new InvalidOperationException($"When called from {callerName}, rewriting a node of type {nodeType} must return null or a non-null value of the same type. Alternatively, override {callerName} and change it to not visit children of this type.");
    }

    public virtual bool VisitAndConvert<T>(in NodeList<T> nodes, out NodeList<T> newNodes, bool allowNullElement = false, [CallerMemberName] string? callerName = null)
        where T : Node?
    {
        List<T>? newNodeList = null;
        for (var i = 0; i < nodes.Count; i++)
        {
            var node = nodes[i];

            var newNode = VisitAndConvert(node, allowNull: allowNullElement, callerName);

            if (newNodeList is not null)
            {
                newNodeList.Add(newNode);
            }
            else if (newNode != node)
            {
                newNodeList = new List<T>();
                for (var j = 0; j < i; j++)
                {
                    newNodeList.Add(nodes[j]);
                }

                newNodeList.Add(newNode);
            }
        }

        if (newNodeList is not null)
        {
            newNodes = new NodeList<T>(newNodeList);
            return true;
        }

        newNodes = nodes;
        return false;
    }

    protected internal override object? VisitProgram(Program program)
    {
        VisitAndConvert(program.Body, out var body);

        return program.Update(body);
    }

    [Obsolete("This method may be removed in a future version as it will not be called anymore due to employing double dispatch (instead of switch dispatch).")]
    protected internal override object? VisitUnknownNode(Node node)
    {
        throw new NotImplementedException(
            $"AST visitor doesn't support nodes of type {node.Type}, you can override VisitUnknownNode to handle this case.");
    }

    protected internal override object? VisitCatchClause(CatchClause catchClause)
    {
        var param = VisitAndConvert(catchClause.Param, allowNull: true);
        var body = VisitAndConvert(catchClause.Body);

        return catchClause.Update(param, body);
    }

    protected internal override object? VisitFunctionDeclaration(FunctionDeclaration functionDeclaration)
    {
        var id = VisitAndConvert(functionDeclaration.Id, allowNull: true);
        VisitAndConvert(functionDeclaration.Params, out var parameters);
        var body = VisitAndConvert((BlockStatement) functionDeclaration.Body);

        return functionDeclaration.Update(id, parameters, body);
    }

    protected internal override object? VisitWithStatement(WithStatement withStatement)
    {
        var obj = VisitAndConvert(withStatement.Object);
        var body = VisitAndConvert(withStatement.Body);

        return withStatement.Update(obj, body);
    }

    protected internal override object? VisitWhileStatement(WhileStatement whileStatement)
    {
        var test = VisitAndConvert(whileStatement.Test);
        var body = VisitAndConvert(whileStatement.Body);

        return whileStatement.Update(test, body);
    }

    protected internal override object? VisitVariableDeclaration(VariableDeclaration variableDeclaration)
    {
        VisitAndConvert(variableDeclaration.Declarations, out var declarations);

        return variableDeclaration.Update(declarations);
    }

    protected internal override object? VisitTryStatement(TryStatement tryStatement)
    {
        var block = VisitAndConvert(tryStatement.Block);
        var handler = VisitAndConvert(tryStatement.Handler, allowNull: true);
        var finalizer = VisitAndConvert(tryStatement.Finalizer, allowNull: true);

        return tryStatement.Update(block, handler, finalizer);
    }

    protected internal override object? VisitThrowStatement(ThrowStatement throwStatement)
    {
        var argument = VisitAndConvert(throwStatement.Argument);

        return throwStatement.Update(argument);
    }

    protected internal override object? VisitSwitchStatement(SwitchStatement switchStatement)
    {
        var discriminant = VisitAndConvert(switchStatement.Discriminant);
        VisitAndConvert(switchStatement.Cases, out var cases);

        return switchStatement.Update(discriminant, cases);
    }

    protected internal override object? VisitSwitchCase(SwitchCase switchCase)
    {
        var test = VisitAndConvert(switchCase.Test, allowNull: true);
        VisitAndConvert(switchCase.Consequent, out var consequent);

        return switchCase.Update(test, consequent);
    }

    protected internal override object? VisitReturnStatement(ReturnStatement returnStatement)
    {
        var argument = VisitAndConvert(returnStatement.Argument, allowNull: true);

        return returnStatement.Update(argument);
    }

    protected internal override object? VisitLabeledStatement(LabeledStatement labeledStatement)
    {
        var label = VisitAndConvert(labeledStatement.Label);
        var body = VisitAndConvert(labeledStatement.Body);

        return labeledStatement.Update(label, body);
    }

    protected internal override object? VisitIfStatement(IfStatement ifStatement)
    {
        var test = VisitAndConvert(ifStatement.Test);
        var consequent = VisitAndConvert(ifStatement.Consequent);
        var alternate = VisitAndConvert(ifStatement.Alternate, allowNull: true);

        return ifStatement.Update(test, consequent, alternate);
    }

    protected internal override object? VisitExpressionStatement(ExpressionStatement expressionStatement)
    {
        var expression = VisitAndConvert(expressionStatement.Expression);

        return expressionStatement.Update(expression);
    }

    protected internal override object? VisitForStatement(ForStatement forStatement)
    {
        var init = VisitAndConvert(forStatement.Init, allowNull: true);
        var test = VisitAndConvert(forStatement.Test, allowNull: true);
        var update = VisitAndConvert(forStatement.Update, allowNull: true);
        var body = VisitAndConvert(forStatement.Body);

        return forStatement.Update(init, test, update, body);
    }

    protected internal override object? VisitForInStatement(ForInStatement forInStatement)
    {
        var left = VisitAndConvert(forInStatement.Left);
        var right = VisitAndConvert(forInStatement.Right);
        var body = VisitAndConvert(forInStatement.Body);

        return forInStatement.Update(left, right, body);
    }

    protected internal override object? VisitDoWhileStatement(DoWhileStatement doWhileStatement)
    {
        var body = VisitAndConvert(doWhileStatement.Body);
        var test = VisitAndConvert(doWhileStatement.Test);

        return doWhileStatement.Update(body, test);
    }

    protected internal override object? VisitArrowFunctionExpression(ArrowFunctionExpression arrowFunctionExpression)
    {
        VisitAndConvert(arrowFunctionExpression.Params, out var parameters);
        var body = VisitAndConvert(arrowFunctionExpression.Body);

        return arrowFunctionExpression.Update(parameters, body);
    }

    protected internal override object? VisitUnaryExpression(UnaryExpression unaryExpression)
    {
        var argument = VisitAndConvert(unaryExpression.Argument);

        return unaryExpression.Update(argument);
    }

    protected internal override object? VisitSequenceExpression(SequenceExpression sequenceExpression)
    {
        VisitAndConvert(sequenceExpression.Expressions, out var expressions);

        return sequenceExpression.Update(expressions);
    }

    protected internal override object? VisitObjectExpression(ObjectExpression objectExpression)
    {
        VisitAndConvert(objectExpression.Properties, out var properties);

        return objectExpression.Update(properties);
    }

    protected internal override object? VisitNewExpression(NewExpression newExpression)
    {
        var callee = VisitAndConvert(newExpression.Callee);
        VisitAndConvert(newExpression.Arguments, out var arguments);

        return newExpression.Update(callee, arguments);
    }

    protected internal override object? VisitMemberExpression(MemberExpression memberExpression)
    {
        var obj = VisitAndConvert(memberExpression.Object);
        var property = VisitAndConvert(memberExpression.Property);

        return memberExpression.Update(obj, property);
    }

    protected internal override object? VisitFunctionExpression(FunctionExpression functionExpression)
    {
        var id = VisitAndConvert(functionExpression.Id, allowNull: true);
        VisitAndConvert(functionExpression.Params, out var parameters);
        var body = VisitAndConvert((BlockStatement) functionExpression.Body);

        return functionExpression.Update(id, parameters, body);
    }

    protected internal override object? VisitPropertyDefinition(PropertyDefinition propertyDefinition)
    {
        var key = VisitAndConvert(propertyDefinition.Key);
        var value = VisitAndConvert(propertyDefinition.Value, allowNull: true);

        return propertyDefinition.Update(key, value);
    }

    protected internal override object? VisitChainExpression(ChainExpression chainExpression)
    {
        var expression = VisitAndConvert(chainExpression.Expression);

        return chainExpression.Update(expression);
    }

    protected internal override object? VisitClassExpression(ClassExpression classExpression)
    {
        var id = VisitAndConvert(classExpression.Id, allowNull: true);
        var superClass = VisitAndConvert(classExpression.SuperClass, allowNull: true);
        var body = VisitAndConvert(classExpression.Body);

        return classExpression.Update(id, superClass, body);
    }

    protected internal override object? VisitExportDefaultDeclaration(ExportDefaultDeclaration exportDefaultDeclaration)
    {
        var declaration = VisitAndConvert(exportDefaultDeclaration.Declaration);

        return exportDefaultDeclaration.Update(declaration);
    }

    protected internal override object? VisitExportAllDeclaration(ExportAllDeclaration exportAllDeclaration)
    {
        var exported = VisitAndConvert(exportAllDeclaration.Exported, allowNull: true);
        var source = VisitAndConvert(exportAllDeclaration.Source);

        return exportAllDeclaration.Update(exported, source);
    }

    protected internal override object? VisitExportNamedDeclaration(ExportNamedDeclaration exportNamedDeclaration)
    {
        var declaration = VisitAndConvert(exportNamedDeclaration.Declaration, allowNull: true);
        VisitAndConvert(exportNamedDeclaration.Specifiers, out var specifiers);
        var source = VisitAndConvert(exportNamedDeclaration.Source, allowNull: true);

        return exportNamedDeclaration.Update(declaration, specifiers, source);
    }

    protected internal override object? VisitExportSpecifier(ExportSpecifier exportSpecifier)
    {
        var local = VisitAndConvert(exportSpecifier.Local);
        var exported = VisitAndConvert(exportSpecifier.Exported);

        return exportSpecifier.Update(local, exported);
    }

    protected internal override object? VisitImport(Import import)
    {
        var source = VisitAndConvert(import.Source, allowNull: true);

        return import.Update(source);
    }

    protected internal override object? VisitImportDeclaration(ImportDeclaration importDeclaration)
    {
        VisitAndConvert(importDeclaration.Specifiers, out var specifiers);

        var source = VisitAndConvert(importDeclaration.Source);

        return importDeclaration.Update(specifiers, source);
    }

    protected internal override object? VisitImportNamespaceSpecifier(ImportNamespaceSpecifier importNamespaceSpecifier)
    {
        var local = VisitAndConvert(importNamespaceSpecifier.Local);

        return importNamespaceSpecifier.Update(local);
    }

    protected internal override object? VisitImportDefaultSpecifier(ImportDefaultSpecifier importDefaultSpecifier)
    {
        var local = VisitAndConvert(importDefaultSpecifier.Local);

        return importDefaultSpecifier.Update(local);
    }

    protected internal override object? VisitImportSpecifier(ImportSpecifier importSpecifier)
    {
        var imported = VisitAndConvert(importSpecifier.Imported);
        var local = VisitAndConvert(importSpecifier.Local);

        return importSpecifier.Update(imported, local);
    }

    protected internal override object? VisitMethodDefinition(MethodDefinition methodDefinition)
    {
        var key = VisitAndConvert(methodDefinition.Key);
        var value = VisitAndConvert((FunctionExpression) methodDefinition.Value);

        return methodDefinition.Update(key, value);
    }

    protected internal override object? VisitForOfStatement(ForOfStatement forOfStatement)
    {
        var left = VisitAndConvert(forOfStatement.Left);
        var right = VisitAndConvert(forOfStatement.Right);
        var body = VisitAndConvert(forOfStatement.Body);

        return forOfStatement.Update(left, right, body);
    }

    protected internal override object? VisitClassDeclaration(ClassDeclaration classDeclaration)
    {
        var id = VisitAndConvert(classDeclaration.Id, allowNull: true);
        var superClass = VisitAndConvert(classDeclaration.SuperClass, allowNull: true);
        var body = VisitAndConvert(classDeclaration.Body);

        return classDeclaration.Update(id, superClass, body);
    }

    protected internal override object? VisitClassBody(ClassBody classBody)
    {
        VisitAndConvert(classBody.Body, out var body);

        return classBody.Update(body);
    }

    protected internal override object? VisitYieldExpression(YieldExpression yieldExpression)
    {
        var argument = VisitAndConvert(yieldExpression.Argument, allowNull: true);

        return yieldExpression.Update(argument);
    }

    protected internal override object? VisitTaggedTemplateExpression(TaggedTemplateExpression taggedTemplateExpression)
    {
        var tag = VisitAndConvert(taggedTemplateExpression.Tag);
        var quasi = VisitAndConvert(taggedTemplateExpression.Quasi);

        return taggedTemplateExpression.Update(tag, quasi);
    }

    protected internal override object? VisitMetaProperty(MetaProperty metaProperty)
    {
        var meta = VisitAndConvert(metaProperty.Meta);
        var property = VisitAndConvert(metaProperty.Property);

        return metaProperty.Update(meta, property);
    }

    protected internal override object? VisitObjectPattern(ObjectPattern objectPattern)
    {
        VisitAndConvert(objectPattern.Properties, out var properties);

        return objectPattern.Update(properties);
    }

    protected internal override object? VisitSpreadElement(SpreadElement spreadElement)
    {
        var argument = VisitAndConvert(spreadElement.Argument);

        return spreadElement.Update(argument);
    }

    protected internal override object? VisitAssignmentPattern(AssignmentPattern assignmentPattern)
    {
        var left = VisitAndConvert(assignmentPattern.Left);
        var right = VisitAndConvert(assignmentPattern.Right);

        return assignmentPattern.Update(left, right);
    }

    protected internal override object? VisitArrayPattern(ArrayPattern arrayPattern)
    {
        VisitAndConvert(arrayPattern.Elements, out var elements, allowNullElement: true);

        return arrayPattern.Update(elements);
    }

    protected internal override object? VisitVariableDeclarator(VariableDeclarator variableDeclarator)
    {
        var id = VisitAndConvert(variableDeclarator.Id);
        var init = VisitAndConvert(variableDeclarator.Init, allowNull: true);

        return variableDeclarator.Update(id, init);
    }

    protected internal override object? VisitTemplateLiteral(TemplateLiteral templateLiteral)
    {
        VisitAndConvert(templateLiteral.Quasis, out var quasis);
        VisitAndConvert(templateLiteral.Expressions, out var expressions);

        return templateLiteral.Update(quasis, expressions);
    }

    protected internal override object? VisitRestElement(RestElement restElement)
    {
        var argument = VisitAndConvert(restElement.Argument);

        return restElement.Update(argument);
    }

    protected internal override object? VisitProperty(Property property)
    {
        var key = VisitAndConvert(property.Key);
        var value = VisitAndConvert(property.Value);

        return property.Update(key, value);
    }

    protected internal override object? VisitAwaitExpression(AwaitExpression awaitExpression)
    {
        var argument = VisitAndConvert(awaitExpression.Argument);

        return awaitExpression.Update(argument);
    }

    protected internal override object? VisitConditionalExpression(ConditionalExpression conditionalExpression)
    {
        var test = VisitAndConvert(conditionalExpression.Test);
        var consequent = VisitAndConvert(conditionalExpression.Consequent);
        var alternate = VisitAndConvert(conditionalExpression.Alternate);

        return conditionalExpression.Update(test, consequent, alternate);
    }

    protected internal override object? VisitCallExpression(CallExpression callExpression)
    {
        var callee = VisitAndConvert(callExpression.Callee);
        VisitAndConvert(callExpression.Arguments, out var arguments);

        return callExpression.Update(callee, arguments);
    }

    protected internal override object? VisitBinaryExpression(BinaryExpression binaryExpression)
    {
        var left = VisitAndConvert(binaryExpression.Left);
        var right = VisitAndConvert(binaryExpression.Right);

        return binaryExpression.Update(left, right);
    }

    protected internal override object? VisitArrayExpression(ArrayExpression arrayExpression)
    {
        VisitAndConvert(arrayExpression.Elements, out var elements, allowNullElement: true);

        return arrayExpression.Update(elements);
    }

    protected internal override object? VisitAssignmentExpression(AssignmentExpression assignmentExpression)
    {
        var left = VisitAndConvert(assignmentExpression.Left);
        var right = VisitAndConvert(assignmentExpression.Right);

        return assignmentExpression.Update(left, right);
    }

    protected internal override object? VisitContinueStatement(ContinueStatement continueStatement)
    {
        var label = VisitAndConvert(continueStatement.Label, allowNull: true);

        return continueStatement.Update(label);
    }

    protected internal override object? VisitBreakStatement(BreakStatement breakStatement)
    {
        var label = VisitAndConvert(breakStatement.Label, allowNull: true);

        return breakStatement.Update(label);
    }

    protected internal override object? VisitBlockStatement(BlockStatement blockStatement)
    {
        VisitAndConvert(blockStatement.Body, out var body);

        return blockStatement.Update(body);
    }
}
