using System.Runtime.CompilerServices;
using Esprima.Ast;

namespace Esprima.Utils;

public class AstRewriter : AstVisitor
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

    protected internal override object? VisitAccessorProperty(AccessorProperty accessorProperty)
    {
        VisitAndConvert(accessorProperty.Decorators, out var decorators);
        var key = VisitAndConvert(accessorProperty.Key);
        var value = VisitAndConvert(accessorProperty.Value, allowNull: true);

        return accessorProperty.UpdateWith(key, value, decorators);
    }

    protected internal override object? VisitArrayExpression(ArrayExpression arrayExpression)
    {
        VisitAndConvert(arrayExpression.Elements, out var elements, allowNullElement: true);

        return arrayExpression.UpdateWith(elements);
    }

    protected internal override object? VisitArrayPattern(ArrayPattern arrayPattern)
    {
        VisitAndConvert(arrayPattern.Elements, out var elements, allowNullElement: true);

        return arrayPattern.UpdateWith(elements);
    }

    protected internal override object? VisitArrowFunctionExpression(ArrowFunctionExpression arrowFunctionExpression)
    {
        VisitAndConvert(arrowFunctionExpression.Params, out var parameters);
        var body = VisitAndConvert(arrowFunctionExpression.Body);

        return arrowFunctionExpression.UpdateWith(parameters, body);
    }

    protected internal override object? VisitAssignmentExpression(AssignmentExpression assignmentExpression)
    {
        var left = VisitAndConvert(assignmentExpression.Left);
        var right = VisitAndConvert(assignmentExpression.Right);

        return assignmentExpression.UpdateWith(left, right);
    }

    protected internal override object? VisitAssignmentPattern(AssignmentPattern assignmentPattern)
    {
        var left = VisitAndConvert(assignmentPattern.Left);
        var right = VisitAndConvert(assignmentPattern.Right);

        return assignmentPattern.UpdateWith(left, right);
    }

    protected internal override object? VisitAwaitExpression(AwaitExpression awaitExpression)
    {
        var argument = VisitAndConvert(awaitExpression.Argument);

        return awaitExpression.UpdateWith(argument);
    }

    protected internal override object? VisitBinaryExpression(BinaryExpression binaryExpression)
    {
        var left = VisitAndConvert(binaryExpression.Left);
        var right = VisitAndConvert(binaryExpression.Right);

        return binaryExpression.UpdateWith(left, right);
    }

    protected internal override object? VisitBlockStatement(BlockStatement blockStatement)
    {
        VisitAndConvert(blockStatement.Body, out var body);

        return blockStatement.UpdateWith(body);
    }

    protected internal override object? VisitBreakStatement(BreakStatement breakStatement)
    {
        var label = VisitAndConvert(breakStatement.Label, allowNull: true);

        return breakStatement.UpdateWith(label);
    }

    protected internal override object? VisitCallExpression(CallExpression callExpression)
    {
        var callee = VisitAndConvert(callExpression.Callee);
        VisitAndConvert(callExpression.Arguments, out var arguments);

        return callExpression.UpdateWith(callee, arguments);
    }

    protected internal override object? VisitCatchClause(CatchClause catchClause)
    {
        var param = VisitAndConvert(catchClause.Param, allowNull: true);
        var body = VisitAndConvert(catchClause.Body);

        return catchClause.UpdateWith(param, body);
    }

    protected internal override object? VisitChainExpression(ChainExpression chainExpression)
    {
        var expression = VisitAndConvert(chainExpression.Expression);

        return chainExpression.UpdateWith(expression);
    }

    protected internal override object? VisitClassBody(ClassBody classBody)
    {
        VisitAndConvert(classBody.Body, out var body);

        return classBody.UpdateWith(body);
    }

    protected internal override object? VisitClassDeclaration(ClassDeclaration classDeclaration)
    {
        VisitAndConvert(classDeclaration.Decorators, out var decorators);
        var id = VisitAndConvert(classDeclaration.Id, allowNull: true);
        var superClass = VisitAndConvert(classDeclaration.SuperClass, allowNull: true);
        var body = VisitAndConvert(classDeclaration.Body);

        return classDeclaration.UpdateWith(id, superClass, body, decorators);
    }

    protected internal override object? VisitClassExpression(ClassExpression classExpression)
    {
        VisitAndConvert(classExpression.Decorators, out var decorators);
        var id = VisitAndConvert(classExpression.Id, allowNull: true);
        var superClass = VisitAndConvert(classExpression.SuperClass, allowNull: true);
        var body = VisitAndConvert(classExpression.Body);

        return classExpression.UpdateWith(id, superClass, body, decorators);
    }

    protected internal override object? VisitConditionalExpression(ConditionalExpression conditionalExpression)
    {
        var test = VisitAndConvert(conditionalExpression.Test);
        var consequent = VisitAndConvert(conditionalExpression.Consequent);
        var alternate = VisitAndConvert(conditionalExpression.Alternate);

        return conditionalExpression.UpdateWith(test, consequent, alternate);
    }

    protected internal override object? VisitContinueStatement(ContinueStatement continueStatement)
    {
        var label = VisitAndConvert(continueStatement.Label, allowNull: true);

        return continueStatement.UpdateWith(label);
    }

    protected internal override object? VisitDecorator(Decorator decorator)
    {
        var expression = VisitAndConvert(decorator.Expression);

        return decorator.UpdateWith(expression);
    }

    protected internal override object? VisitDoWhileStatement(DoWhileStatement doWhileStatement)
    {
        var body = VisitAndConvert(doWhileStatement.Body);
        var test = VisitAndConvert(doWhileStatement.Test);

        return doWhileStatement.UpdateWith(body, test);
    }

    protected internal override object? VisitExportAllDeclaration(ExportAllDeclaration exportAllDeclaration)
    {
        var exported = VisitAndConvert(exportAllDeclaration.Exported, allowNull: true);
        var source = VisitAndConvert(exportAllDeclaration.Source);
        VisitAndConvert(exportAllDeclaration.Assertions, out var assertions);

        return exportAllDeclaration.UpdateWith(exported, source, assertions);
    }

    protected internal override object? VisitExportDefaultDeclaration(ExportDefaultDeclaration exportDefaultDeclaration)
    {
        var declaration = VisitAndConvert(exportDefaultDeclaration.Declaration);

        return exportDefaultDeclaration.UpdateWith(declaration);
    }

    protected internal override object? VisitExportNamedDeclaration(ExportNamedDeclaration exportNamedDeclaration)
    {
        var declaration = VisitAndConvert(exportNamedDeclaration.Declaration, allowNull: true);
        VisitAndConvert(exportNamedDeclaration.Specifiers, out var specifiers);
        var source = VisitAndConvert(exportNamedDeclaration.Source, allowNull: true);
        VisitAndConvert(exportNamedDeclaration.Assertions, out var assertions);

        return exportNamedDeclaration.UpdateWith(declaration, specifiers, source, assertions);
    }

    protected internal override object? VisitExportSpecifier(ExportSpecifier exportSpecifier)
    {
        Expression local;
        Expression exported;

        if (exportSpecifier.Exported == exportSpecifier.Local)
        {
            exported = local = VisitAndConvert(exportSpecifier.Local);
        }
        else
        {
            local = VisitAndConvert(exportSpecifier.Local);
            exported = VisitAndConvert(exportSpecifier.Exported);
        }

        return exportSpecifier.UpdateWith(local, exported);
    }

    protected internal override object? VisitExpressionStatement(ExpressionStatement expressionStatement)
    {
        var expression = VisitAndConvert(expressionStatement.Expression);

        return expressionStatement.UpdateWith(expression);
    }

    protected internal override object? VisitForInStatement(ForInStatement forInStatement)
    {
        var left = VisitAndConvert(forInStatement.Left);
        var right = VisitAndConvert(forInStatement.Right);
        var body = VisitAndConvert(forInStatement.Body);

        return forInStatement.UpdateWith(left, right, body);
    }

    protected internal override object? VisitForOfStatement(ForOfStatement forOfStatement)
    {
        var left = VisitAndConvert(forOfStatement.Left);
        var right = VisitAndConvert(forOfStatement.Right);
        var body = VisitAndConvert(forOfStatement.Body);

        return forOfStatement.UpdateWith(left, right, body);
    }

    protected internal override object? VisitForStatement(ForStatement forStatement)
    {
        var init = VisitAndConvert(forStatement.Init, allowNull: true);
        var test = VisitAndConvert(forStatement.Test, allowNull: true);
        var update = VisitAndConvert(forStatement.Update, allowNull: true);
        var body = VisitAndConvert(forStatement.Body);

        return forStatement.UpdateWith(init, test, update, body);
    }

    protected internal override object? VisitFunctionDeclaration(FunctionDeclaration functionDeclaration)
    {
        var id = VisitAndConvert(functionDeclaration.Id, allowNull: true);
        VisitAndConvert(functionDeclaration.Params, out var parameters);
        var body = VisitAndConvert(functionDeclaration.Body);

        return functionDeclaration.UpdateWith(id, parameters, body);
    }

    protected internal override object? VisitFunctionExpression(FunctionExpression functionExpression)
    {
        var id = VisitAndConvert(functionExpression.Id, allowNull: true);
        VisitAndConvert(functionExpression.Params, out var parameters);
        var body = VisitAndConvert(functionExpression.Body);

        return functionExpression.UpdateWith(id, parameters, body);
    }

    protected internal override object? VisitIfStatement(IfStatement ifStatement)
    {
        var test = VisitAndConvert(ifStatement.Test);
        var consequent = VisitAndConvert(ifStatement.Consequent);
        var alternate = VisitAndConvert(ifStatement.Alternate, allowNull: true);

        return ifStatement.UpdateWith(test, consequent, alternate);
    }

    protected internal override object? VisitImport(Import import)
    {
        var source = VisitAndConvert(import.Source);
        var attributes = VisitAndConvert(import.Attributes, allowNull: true);

        return import.UpdateWith(source, attributes);
    }

    protected internal override object? VisitImportAttribute(ImportAttribute importAttribute)
    {
        var key = VisitAndConvert(importAttribute.Key);
        var value = VisitAndConvert(importAttribute.Value);

        return importAttribute.UpdateWith(key, value);
    }

    protected internal override object? VisitImportDeclaration(ImportDeclaration importDeclaration)
    {
        VisitAndConvert(importDeclaration.Specifiers, out var specifiers);

        var source = VisitAndConvert(importDeclaration.Source);

        VisitAndConvert(importDeclaration.Assertions, out var assertions);

        return importDeclaration.UpdateWith(specifiers, source, assertions);
    }

    protected internal override object? VisitImportDefaultSpecifier(ImportDefaultSpecifier importDefaultSpecifier)
    {
        var local = VisitAndConvert(importDefaultSpecifier.Local);

        return importDefaultSpecifier.UpdateWith(local);
    }

    protected internal override object? VisitImportNamespaceSpecifier(ImportNamespaceSpecifier importNamespaceSpecifier)
    {
        var local = VisitAndConvert(importNamespaceSpecifier.Local);

        return importNamespaceSpecifier.UpdateWith(local);
    }

    protected internal override object? VisitImportSpecifier(ImportSpecifier importSpecifier)
    {
        Expression imported;
        Identifier local;

        if (importSpecifier.Imported == importSpecifier.Local)
        {
            imported = local = VisitAndConvert(importSpecifier.Local);
        }
        else
        {
            imported = VisitAndConvert(importSpecifier.Imported);
            local = VisitAndConvert(importSpecifier.Local);
        }

        return importSpecifier.UpdateWith(imported, local);
    }

    protected internal override object? VisitLabeledStatement(LabeledStatement labeledStatement)
    {
        var label = VisitAndConvert(labeledStatement.Label);
        var body = VisitAndConvert(labeledStatement.Body);

        return labeledStatement.UpdateWith(label, body);
    }

    protected internal override object? VisitMemberExpression(MemberExpression memberExpression)
    {
        var obj = VisitAndConvert(memberExpression.Object);
        var property = VisitAndConvert(memberExpression.Property);

        return memberExpression.UpdateWith(obj, property);
    }

    protected internal override object? VisitMetaProperty(MetaProperty metaProperty)
    {
        var meta = VisitAndConvert(metaProperty.Meta);
        var property = VisitAndConvert(metaProperty.Property);

        return metaProperty.UpdateWith(meta, property);
    }

    protected internal override object? VisitMethodDefinition(MethodDefinition methodDefinition)
    {
        VisitAndConvert(methodDefinition.Decorators, out var decorators);
        var key = VisitAndConvert(methodDefinition.Key);
        var value = VisitAndConvert(methodDefinition.Value);

        return methodDefinition.UpdateWith(key, value, decorators);
    }

    protected internal override object? VisitNewExpression(NewExpression newExpression)
    {
        var callee = VisitAndConvert(newExpression.Callee);
        VisitAndConvert(newExpression.Arguments, out var arguments);

        return newExpression.UpdateWith(callee, arguments);
    }

    protected internal override object? VisitObjectExpression(ObjectExpression objectExpression)
    {
        VisitAndConvert(objectExpression.Properties, out var properties);

        return objectExpression.UpdateWith(properties);
    }

    protected internal override object? VisitObjectPattern(ObjectPattern objectPattern)
    {
        VisitAndConvert(objectPattern.Properties, out var properties);

        return objectPattern.UpdateWith(properties);
    }

    protected internal override object? VisitProgram(Program program)
    {
        VisitAndConvert(program.Body, out var body);

        return program.UpdateWith(body);
    }

    protected internal override object? VisitProperty(Property property)
    {
        Expression? key;
        Node value;

        if (property.Shorthand)
        {
            value = VisitAndConvert(property.Value);
            key = (value is AssignmentPattern assignmentPattern
                ? assignmentPattern.Left
                : value).As<Identifier>();
        }
        else
        {
            key = VisitAndConvert(property.Key);
            value = VisitAndConvert(property.Value);
        }

        return property.UpdateWith(key, value);
    }

    protected internal override object? VisitPropertyDefinition(PropertyDefinition propertyDefinition)
    {
        VisitAndConvert(propertyDefinition.Decorators, out var decorators);
        var key = VisitAndConvert(propertyDefinition.Key);
        var value = VisitAndConvert(propertyDefinition.Value, allowNull: true);

        return propertyDefinition.UpdateWith(key, value, decorators);
    }

    protected internal override object? VisitRestElement(RestElement restElement)
    {
        var argument = VisitAndConvert(restElement.Argument);

        return restElement.UpdateWith(argument);
    }

    protected internal override object? VisitReturnStatement(ReturnStatement returnStatement)
    {
        var argument = VisitAndConvert(returnStatement.Argument, allowNull: true);

        return returnStatement.UpdateWith(argument);
    }

    protected internal override object? VisitSequenceExpression(SequenceExpression sequenceExpression)
    {
        VisitAndConvert(sequenceExpression.Expressions, out var expressions);

        return sequenceExpression.UpdateWith(expressions);
    }

    protected internal override object? VisitSpreadElement(SpreadElement spreadElement)
    {
        var argument = VisitAndConvert(spreadElement.Argument);

        return spreadElement.UpdateWith(argument);
    }

    protected internal override object? VisitStaticBlock(StaticBlock staticBlock)
    {
        VisitAndConvert(staticBlock.Body, out var body);

        return staticBlock.UpdateWith(body);
    }

    protected internal override object? VisitSwitchCase(SwitchCase switchCase)
    {
        var test = VisitAndConvert(switchCase.Test, allowNull: true);
        VisitAndConvert(switchCase.Consequent, out var consequent);

        return switchCase.UpdateWith(test, consequent);
    }

    protected internal override object? VisitSwitchStatement(SwitchStatement switchStatement)
    {
        var discriminant = VisitAndConvert(switchStatement.Discriminant);
        VisitAndConvert(switchStatement.Cases, out var cases);

        return switchStatement.UpdateWith(discriminant, cases);
    }

    protected internal override object? VisitTaggedTemplateExpression(TaggedTemplateExpression taggedTemplateExpression)
    {
        var tag = VisitAndConvert(taggedTemplateExpression.Tag);
        var quasi = VisitAndConvert(taggedTemplateExpression.Quasi);

        return taggedTemplateExpression.UpdateWith(tag, quasi);
    }

    protected internal override object? VisitTemplateLiteral(TemplateLiteral templateLiteral)
    {
        VisitAndConvert(templateLiteral.Quasis, out var quasis);
        VisitAndConvert(templateLiteral.Expressions, out var expressions);

        return templateLiteral.UpdateWith(quasis, expressions);
    }

    protected internal override object? VisitThrowStatement(ThrowStatement throwStatement)
    {
        var argument = VisitAndConvert(throwStatement.Argument);

        return throwStatement.UpdateWith(argument);
    }

    protected internal override object? VisitTryStatement(TryStatement tryStatement)
    {
        var block = VisitAndConvert(tryStatement.Block);
        var handler = VisitAndConvert(tryStatement.Handler, allowNull: true);
        var finalizer = VisitAndConvert(tryStatement.Finalizer, allowNull: true);

        return tryStatement.UpdateWith(block, handler, finalizer);
    }

    protected internal override object? VisitUnaryExpression(UnaryExpression unaryExpression)
    {
        var argument = VisitAndConvert(unaryExpression.Argument);

        return unaryExpression.UpdateWith(argument);
    }

    protected internal override object? VisitVariableDeclaration(VariableDeclaration variableDeclaration)
    {
        VisitAndConvert(variableDeclaration.Declarations, out var declarations);

        return variableDeclaration.UpdateWith(declarations);
    }

    protected internal override object? VisitVariableDeclarator(VariableDeclarator variableDeclarator)
    {
        var id = VisitAndConvert(variableDeclarator.Id);
        var init = VisitAndConvert(variableDeclarator.Init, allowNull: true);

        return variableDeclarator.UpdateWith(id, init);
    }

    protected internal override object? VisitWhileStatement(WhileStatement whileStatement)
    {
        var test = VisitAndConvert(whileStatement.Test);
        var body = VisitAndConvert(whileStatement.Body);

        return whileStatement.UpdateWith(test, body);
    }

    protected internal override object? VisitWithStatement(WithStatement withStatement)
    {
        var obj = VisitAndConvert(withStatement.Object);
        var body = VisitAndConvert(withStatement.Body);

        return withStatement.UpdateWith(obj, body);
    }

    protected internal override object? VisitYieldExpression(YieldExpression yieldExpression)
    {
        var argument = VisitAndConvert(yieldExpression.Argument, allowNull: true);

        return yieldExpression.UpdateWith(argument);
    }
}
