using System.Runtime.CompilerServices;
using Esprima.Ast;

namespace Esprima.Utils;

public class AstRewriter : AstVisitor
{
    public virtual T VisitAndConvert<T>(T node, object? context = null, bool allowNull = false, [CallerMemberName] string? callerName = null)
        where T : Node?
    {
        if (node is null)
        {
            return allowNull ? null! : throw new ArgumentNullException(nameof(node));
        }

        return Visit(node, context) switch
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

    public virtual bool VisitAndConvert<T>(in NodeList<T> nodes, out NodeList<T> newNodes, object? context = null, bool allowNullElement = false, [CallerMemberName] string? callerName = null)
        where T : Node?
    {
        List<T>? newNodeList = null;
        for (var i = 0; i < nodes.Count; i++)
        {
            var node = nodes[i];

            var newNode = VisitAndConvert(node, context, allowNull: allowNullElement, callerName);

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

    protected internal override object? VisitArrayExpression(ArrayExpression arrayExpression, object? context)
    {
        VisitAndConvert(arrayExpression.Elements, out var elements, context, allowNullElement: true);

        return arrayExpression.UpdateWith(elements);
    }

    protected internal override object? VisitArrayPattern(ArrayPattern arrayPattern, object? context)
    {
        VisitAndConvert(arrayPattern.Elements, out var elements, context, allowNullElement: true);

        return arrayPattern.UpdateWith(elements);
    }

    protected internal override object? VisitArrowFunctionExpression(ArrowFunctionExpression arrowFunctionExpression, object? context)
    {
        VisitAndConvert(arrowFunctionExpression.Params, out var parameters, context);
        var body = VisitAndConvert(arrowFunctionExpression.Body, context);

        return arrowFunctionExpression.UpdateWith(parameters, body);
    }

    protected internal override object? VisitAssignmentExpression(AssignmentExpression assignmentExpression, object? context)
    {
        var left = VisitAndConvert(assignmentExpression.Left, context);
        var right = VisitAndConvert(assignmentExpression.Right, context);

        return assignmentExpression.UpdateWith(left, right);
    }

    protected internal override object? VisitAssignmentPattern(AssignmentPattern assignmentPattern, object? context)
    {
        var left = VisitAndConvert(assignmentPattern.Left, context);
        var right = VisitAndConvert(assignmentPattern.Right, context);

        return assignmentPattern.UpdateWith(left, right);
    }

    protected internal override object? VisitAwaitExpression(AwaitExpression awaitExpression, object? context)
    {
        var argument = VisitAndConvert(awaitExpression.Argument, context);

        return awaitExpression.UpdateWith(argument);
    }

    protected internal override object? VisitBinaryExpression(BinaryExpression binaryExpression, object? context)
    {
        var left = VisitAndConvert(binaryExpression.Left, context);
        var right = VisitAndConvert(binaryExpression.Right, context);

        return binaryExpression.UpdateWith(left, right);
    }

    protected internal override object? VisitBlockStatement(BlockStatement blockStatement, object? context)
    {
        VisitAndConvert(blockStatement.Body, out var body, context);

        return blockStatement.UpdateWith(body);
    }

    protected internal override object? VisitBreakStatement(BreakStatement breakStatement, object? context)
    {
        var label = VisitAndConvert(breakStatement.Label, context, allowNull: true);

        return breakStatement.UpdateWith(label);
    }

    protected internal override object? VisitCallExpression(CallExpression callExpression, object? context)
    {
        var callee = VisitAndConvert(callExpression.Callee, context);
        VisitAndConvert(callExpression.Arguments, out var arguments, context);

        return callExpression.UpdateWith(callee, arguments);
    }

    protected internal override object? VisitCatchClause(CatchClause catchClause, object? context)
    {
        var param = VisitAndConvert(catchClause.Param, context, allowNull: true);
        var body = VisitAndConvert(catchClause.Body, context);

        return catchClause.UpdateWith(param, body);
    }

    protected internal override object? VisitChainExpression(ChainExpression chainExpression, object? context)
    {
        var expression = VisitAndConvert(chainExpression.Expression, context);

        return chainExpression.UpdateWith(expression);
    }

    protected internal override object? VisitClassBody(ClassBody classBody, object? context)
    {
        VisitAndConvert(classBody.Body, out var body, context);

        return classBody.UpdateWith(body);
    }

    protected internal override object? VisitClassDeclaration(ClassDeclaration classDeclaration, object? context)
    {
        var id = VisitAndConvert(classDeclaration.Id, context, allowNull: true);
        var superClass = VisitAndConvert(classDeclaration.SuperClass, context, allowNull: true);
        var body = VisitAndConvert(classDeclaration.Body, context);
        VisitAndConvert(classDeclaration.Decorators, out var decorators, context);

        return classDeclaration.UpdateWith(id, superClass, body, decorators);
    }

    protected internal override object? VisitClassExpression(ClassExpression classExpression, object? context)
    {
        var id = VisitAndConvert(classExpression.Id, context, allowNull: true);
        var superClass = VisitAndConvert(classExpression.SuperClass, context, allowNull: true);
        var body = VisitAndConvert(classExpression.Body, context);
        VisitAndConvert(classExpression.Decorators, out var decorators, context);

        return classExpression.UpdateWith(id, superClass, body, decorators);
    }

    protected internal override object? VisitConditionalExpression(ConditionalExpression conditionalExpression, object? context)
    {
        var test = VisitAndConvert(conditionalExpression.Test, context);
        var consequent = VisitAndConvert(conditionalExpression.Consequent, context);
        var alternate = VisitAndConvert(conditionalExpression.Alternate, context);

        return conditionalExpression.UpdateWith(test, consequent, alternate);
    }

    protected internal override object? VisitContinueStatement(ContinueStatement continueStatement, object? context)
    {
        var label = VisitAndConvert(continueStatement.Label, context, allowNull: true);

        return continueStatement.UpdateWith(label);
    }

    protected internal override object? VisitDecorator(Decorator decorator, object? context)
    {
        var expression = VisitAndConvert(decorator.Expression, context);

        return decorator.UpdateWith(expression);
    }

    protected internal override object? VisitDoWhileStatement(DoWhileStatement doWhileStatement, object? context)
    {
        var body = VisitAndConvert(doWhileStatement.Body, context);
        var test = VisitAndConvert(doWhileStatement.Test, context);

        return doWhileStatement.UpdateWith(body, test);
    }

    protected internal override object? VisitExportAllDeclaration(ExportAllDeclaration exportAllDeclaration, object? context)
    {
        var exported = VisitAndConvert(exportAllDeclaration.Exported, context, allowNull: true);
        var source = VisitAndConvert(exportAllDeclaration.Source, context);
        VisitAndConvert(exportAllDeclaration.Assertions, out var assertions, context);

        return exportAllDeclaration.UpdateWith(exported, source, assertions);
    }

    protected internal override object? VisitExportDefaultDeclaration(ExportDefaultDeclaration exportDefaultDeclaration, object? context)
    {
        var declaration = VisitAndConvert(exportDefaultDeclaration.Declaration, context);

        return exportDefaultDeclaration.UpdateWith(declaration);
    }

    protected internal override object? VisitExportNamedDeclaration(ExportNamedDeclaration exportNamedDeclaration, object? context)
    {
        var declaration = VisitAndConvert(exportNamedDeclaration.Declaration, context, allowNull: true);
        VisitAndConvert(exportNamedDeclaration.Specifiers, out var specifiers, context);
        var source = VisitAndConvert(exportNamedDeclaration.Source, context, allowNull: true);
        VisitAndConvert(exportNamedDeclaration.Assertions, out var assertions, context);

        return exportNamedDeclaration.UpdateWith(declaration, specifiers, source, assertions);
    }

    protected internal override object? VisitExportSpecifier(ExportSpecifier exportSpecifier, object? context)
    {
        var local = VisitAndConvert(exportSpecifier.Local, context);
        var exported = VisitAndConvert(exportSpecifier.Exported, context);

        return exportSpecifier.UpdateWith(local, exported);
    }

    protected internal override object? VisitExpressionStatement(ExpressionStatement expressionStatement, object? context)
    {
        var expression = VisitAndConvert(expressionStatement.Expression, context);

        return expressionStatement.UpdateWith(expression);
    }

    protected internal override object? VisitForInStatement(ForInStatement forInStatement, object? context)
    {
        var left = VisitAndConvert(forInStatement.Left, context);
        var right = VisitAndConvert(forInStatement.Right, context);
        var body = VisitAndConvert(forInStatement.Body, context);

        return forInStatement.UpdateWith(left, right, body);
    }

    protected internal override object? VisitForOfStatement(ForOfStatement forOfStatement, object? context)
    {
        var left = VisitAndConvert(forOfStatement.Left, context);
        var right = VisitAndConvert(forOfStatement.Right, context);
        var body = VisitAndConvert(forOfStatement.Body, context);

        return forOfStatement.UpdateWith(left, right, body);
    }

    protected internal override object? VisitForStatement(ForStatement forStatement, object? context)
    {
        var init = VisitAndConvert(forStatement.Init, context, allowNull: true);
        var test = VisitAndConvert(forStatement.Test, context, allowNull: true);
        var update = VisitAndConvert(forStatement.Update, context, allowNull: true);
        var body = VisitAndConvert(forStatement.Body, context);

        return forStatement.UpdateWith(init, test, update, body);
    }

    protected internal override object? VisitFunctionDeclaration(FunctionDeclaration functionDeclaration, object? context)
    {
        var id = VisitAndConvert(functionDeclaration.Id, context, allowNull: true);
        VisitAndConvert(functionDeclaration.Params, out var parameters, context);
        var body = VisitAndConvert(functionDeclaration.Body, context);

        return functionDeclaration.UpdateWith(id, parameters, body);
    }

    protected internal override object? VisitFunctionExpression(FunctionExpression functionExpression, object? context)
    {
        var id = VisitAndConvert(functionExpression.Id, context, allowNull: true);
        VisitAndConvert(functionExpression.Params, out var parameters, context);
        var body = VisitAndConvert(functionExpression.Body, context);

        return functionExpression.UpdateWith(id, parameters, body);
    }

    protected internal override object? VisitIfStatement(IfStatement ifStatement, object? context)
    {
        var test = VisitAndConvert(ifStatement.Test, context);
        var consequent = VisitAndConvert(ifStatement.Consequent, context);
        var alternate = VisitAndConvert(ifStatement.Alternate, context, allowNull: true);

        return ifStatement.UpdateWith(test, consequent, alternate);
    }

    protected internal override object? VisitImport(Import import, object? context)
    {
        var source = VisitAndConvert(import.Source, context);
        var attributes = VisitAndConvert(import.Attributes, context, allowNull: true);

        return import.UpdateWith(source, attributes);
    }

    protected internal override object? VisitImportAttribute(ImportAttribute importAttribute, object? context)
    {
        var key = VisitAndConvert(importAttribute.Key, context);
        var value = VisitAndConvert(importAttribute.Value, context);

        return importAttribute.UpdateWith(key, value);
    }

    protected internal override object? VisitImportDeclaration(ImportDeclaration importDeclaration, object? context)
    {
        VisitAndConvert(importDeclaration.Specifiers, out var specifiers, context);

        var source = VisitAndConvert(importDeclaration.Source, context);

        VisitAndConvert(importDeclaration.Assertions, out var assertions, context);

        return importDeclaration.UpdateWith(specifiers, source, assertions);
    }

    protected internal override object? VisitImportDefaultSpecifier(ImportDefaultSpecifier importDefaultSpecifier, object? context)
    {
        var local = VisitAndConvert(importDefaultSpecifier.Local, context);

        return importDefaultSpecifier.UpdateWith(local);
    }

    protected internal override object? VisitImportNamespaceSpecifier(ImportNamespaceSpecifier importNamespaceSpecifier, object? context)
    {
        var local = VisitAndConvert(importNamespaceSpecifier.Local, context);

        return importNamespaceSpecifier.UpdateWith(local);
    }

    protected internal override object? VisitImportSpecifier(ImportSpecifier importSpecifier, object? context)
    {
        var imported = VisitAndConvert(importSpecifier.Imported, context);
        var local = VisitAndConvert(importSpecifier.Local, context);

        return importSpecifier.UpdateWith(imported, local);
    }

    protected internal override object? VisitLabeledStatement(LabeledStatement labeledStatement, object? context)
    {
        var label = VisitAndConvert(labeledStatement.Label, context);
        var body = VisitAndConvert(labeledStatement.Body, context);

        return labeledStatement.UpdateWith(label, body);
    }

    protected internal override object? VisitMemberExpression(MemberExpression memberExpression, object? context)
    {
        var obj = VisitAndConvert(memberExpression.Object, context);
        var property = VisitAndConvert(memberExpression.Property, context);

        return memberExpression.UpdateWith(obj, property);
    }

    protected internal override object? VisitMetaProperty(MetaProperty metaProperty, object? context)
    {
        var meta = VisitAndConvert(metaProperty.Meta, context);
        var property = VisitAndConvert(metaProperty.Property, context);

        return metaProperty.UpdateWith(meta, property);
    }

    protected internal override object? VisitMethodDefinition(MethodDefinition methodDefinition, object? context)
    {
        var key = VisitAndConvert(methodDefinition.Key, context);
        var value = VisitAndConvert(methodDefinition.Value, context);
        VisitAndConvert(methodDefinition.Decorators, out var decorators, context);

        return methodDefinition.UpdateWith(key, value, decorators);
    }

    protected internal override object? VisitNewExpression(NewExpression newExpression, object? context)
    {
        var callee = VisitAndConvert(newExpression.Callee, context);
        VisitAndConvert(newExpression.Arguments, out var arguments, context);

        return newExpression.UpdateWith(callee, arguments);
    }

    protected internal override object? VisitObjectExpression(ObjectExpression objectExpression, object? context)
    {
        VisitAndConvert(objectExpression.Properties, out var properties, context);

        return objectExpression.UpdateWith(properties);
    }

    protected internal override object? VisitObjectPattern(ObjectPattern objectPattern, object? context)
    {
        VisitAndConvert(objectPattern.Properties, out var properties, context);

        return objectPattern.UpdateWith(properties);
    }

    protected internal override object? VisitProgram(Program program, object? context)
    {
        VisitAndConvert(program.Body, out var body, context);

        return program.UpdateWith(body);
    }

    protected internal override object? VisitProperty(Property property, object? context)
    {
        var key = VisitAndConvert(property.Key, context);
        var value = VisitAndConvert(property.Value, context);

        return property.UpdateWith(key, value);
    }

    protected internal override object? VisitPropertyDefinition(PropertyDefinition propertyDefinition, object? context)
    {
        var key = VisitAndConvert(propertyDefinition.Key, context);
        var value = VisitAndConvert(propertyDefinition.Value, context, allowNull: true);
        VisitAndConvert(propertyDefinition.Decorators, out var decorators, context);

        return propertyDefinition.UpdateWith(key, value, decorators);
    }

    protected internal override object? VisitRestElement(RestElement restElement, object? context)
    {
        var argument = VisitAndConvert(restElement.Argument, context);

        return restElement.UpdateWith(argument);
    }

    protected internal override object? VisitReturnStatement(ReturnStatement returnStatement, object? context)
    {
        var argument = VisitAndConvert(returnStatement.Argument, context, allowNull: true);

        return returnStatement.UpdateWith(argument);
    }

    protected internal override object? VisitSequenceExpression(SequenceExpression sequenceExpression, object? context)
    {
        VisitAndConvert(sequenceExpression.Expressions, out var expressions, context);

        return sequenceExpression.UpdateWith(expressions);
    }

    protected internal override object? VisitSpreadElement(SpreadElement spreadElement, object? context)
    {
        var argument = VisitAndConvert(spreadElement.Argument, context);

        return spreadElement.UpdateWith(argument);
    }

    protected internal override object? VisitStaticBlock(StaticBlock staticBlock, object? context)
    {
        VisitAndConvert(staticBlock.Body, out var body, context);

        return staticBlock.UpdateWith(body);
    }

    protected internal override object? VisitSwitchCase(SwitchCase switchCase, object? context)
    {
        var test = VisitAndConvert(switchCase.Test, context, allowNull: true);
        VisitAndConvert(switchCase.Consequent, out var consequent, context);

        return switchCase.UpdateWith(test, consequent);
    }

    protected internal override object? VisitSwitchStatement(SwitchStatement switchStatement, object? context)
    {
        var discriminant = VisitAndConvert(switchStatement.Discriminant, context);
        VisitAndConvert(switchStatement.Cases, out var cases, context);

        return switchStatement.UpdateWith(discriminant, cases);
    }

    protected internal override object? VisitTaggedTemplateExpression(TaggedTemplateExpression taggedTemplateExpression, object? context)
    {
        var tag = VisitAndConvert(taggedTemplateExpression.Tag, context);
        var quasi = VisitAndConvert(taggedTemplateExpression.Quasi, context);

        return taggedTemplateExpression.UpdateWith(tag, quasi);
    }

    protected internal override object? VisitTemplateLiteral(TemplateLiteral templateLiteral, object? context)
    {
        VisitAndConvert(templateLiteral.Quasis, out var quasis, context);
        VisitAndConvert(templateLiteral.Expressions, out var expressions, context);

        return templateLiteral.UpdateWith(quasis, expressions);
    }

    protected internal override object? VisitThrowStatement(ThrowStatement throwStatement, object? context)
    {
        var argument = VisitAndConvert(throwStatement.Argument, context);

        return throwStatement.UpdateWith(argument);
    }

    protected internal override object? VisitTryStatement(TryStatement tryStatement, object? context)
    {
        var block = VisitAndConvert(tryStatement.Block, context);
        var handler = VisitAndConvert(tryStatement.Handler, context, allowNull: true);
        var finalizer = VisitAndConvert(tryStatement.Finalizer, context, allowNull: true);

        return tryStatement.UpdateWith(block, handler, finalizer);
    }

    protected internal override object? VisitUnaryExpression(UnaryExpression unaryExpression, object? context)
    {
        var argument = VisitAndConvert(unaryExpression.Argument, context);

        return unaryExpression.UpdateWith(argument);
    }

    protected internal override object? VisitVariableDeclaration(VariableDeclaration variableDeclaration, object? context)
    {
        VisitAndConvert(variableDeclaration.Declarations, out var declarations, context);

        return variableDeclaration.UpdateWith(declarations);
    }

    protected internal override object? VisitVariableDeclarator(VariableDeclarator variableDeclarator, object? context)
    {
        var id = VisitAndConvert(variableDeclarator.Id, context);
        var init = VisitAndConvert(variableDeclarator.Init, context, allowNull: true);

        return variableDeclarator.UpdateWith(id, init);
    }

    protected internal override object? VisitWhileStatement(WhileStatement whileStatement, object? context)
    {
        var test = VisitAndConvert(whileStatement.Test, context);
        var body = VisitAndConvert(whileStatement.Body, context);

        return whileStatement.UpdateWith(test, body);
    }

    protected internal override object? VisitWithStatement(WithStatement withStatement, object? context)
    {
        var obj = VisitAndConvert(withStatement.Object, context);
        var body = VisitAndConvert(withStatement.Body, context);

        return withStatement.UpdateWith(obj, body);
    }

    protected internal override object? VisitYieldExpression(YieldExpression yieldExpression, object? context)
    {
        var argument = VisitAndConvert(yieldExpression.Argument, context, allowNull: true);

        return yieldExpression.UpdateWith(argument);
    }
}
