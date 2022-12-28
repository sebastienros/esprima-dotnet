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

    protected internal virtual object? VisitAccessorProperty(AccessorProperty accessorProperty)
    {
        ref readonly var decorators = ref accessorProperty.Decorators;
        for (var i = 0; i < decorators.Count; i++)
        {
            Visit(decorators[i]);
        }

        Visit(accessorProperty.Key);

        if (accessorProperty.Value is not null)
        {
            Visit(accessorProperty.Value);
        }

        return accessorProperty;
    }

    protected internal virtual object? VisitArrayExpression(ArrayExpression arrayExpression)
    {
        ref readonly var elements = ref arrayExpression.Elements;
        for (var i = 0; i < elements.Count; i++)
        {
            var expr = elements[i];
            if (expr is not null)
            {
                Visit(expr);
            }
        }

        return arrayExpression;
    }

    protected internal virtual object? VisitArrayPattern(ArrayPattern arrayPattern)
    {
        ref readonly var elements = ref arrayPattern.Elements;
        for (var i = 0; i < elements.Count; i++)
        {
            var expr = elements[i];
            if (expr is not null)
            {
                Visit(expr);
            }
        }

        return arrayPattern;
    }

    protected internal virtual object? VisitArrowFunctionExpression(ArrowFunctionExpression arrowFunctionExpression)
    {
        ref readonly var parameters = ref arrowFunctionExpression.Params;
        for (var i = 0; i < parameters.Count; i++)
        {
            Visit(parameters[i]);
        }

        Visit(arrowFunctionExpression.Body);

        return arrowFunctionExpression;
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
        ref readonly var body = ref blockStatement.Body;
        for (var i = 0; i < body.Count; i++)
        {
            Visit(body[i]);
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
        ref readonly var arguments = ref callExpression.Arguments;
        for (var i = 0; i < arguments.Count; i++)
        {
            Visit(arguments[i]);
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
        ref readonly var body = ref classBody.Body;
        for (var i = 0; i < body.Count; i++)
        {
            Visit(body[i]);
        }

        return classBody;
    }

    protected internal virtual object? VisitClassDeclaration(ClassDeclaration classDeclaration)
    {
        ref readonly var decorators = ref classDeclaration.Decorators;
        for (var i = 0; i < decorators.Count; i++)
        {
            Visit(decorators[i]);
        }

        if (classDeclaration.Id is not null)
        {
            Visit(classDeclaration.Id);
        }

        if (classDeclaration.SuperClass is not null)
        {
            Visit(classDeclaration.SuperClass);
        }

        Visit(classDeclaration.Body);

        return classDeclaration;
    }

    protected internal virtual object? VisitClassExpression(ClassExpression classExpression)
    {
        ref readonly var decorators = ref classExpression.Decorators;
        for (var i = 0; i < decorators.Count; i++)
        {
            Visit(decorators[i]);
        }

        if (classExpression.Id is not null)
        {
            Visit(classExpression.Id);
        }

        if (classExpression.SuperClass is not null)
        {
            Visit(classExpression.SuperClass);
        }

        Visit(classExpression.Body);

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

        ref readonly var assertions = ref exportAllDeclaration.Assertions;
        for (var i = 0; i < assertions.Count; i++)
        {
            Visit(assertions[i]);
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

        ref readonly var specifiers = ref exportNamedDeclaration.Specifiers;
        for (var i = 0; i < specifiers.Count; i++)
        {
            Visit(specifiers[i]);
        }

        if (exportNamedDeclaration.Source is not null)
        {
            Visit(exportNamedDeclaration.Source);
        }

        ref readonly var assertions = ref exportNamedDeclaration.Assertions;
        for (var i = 0; i < assertions.Count; i++)
        {
            Visit(assertions[i]);
        }

        return exportNamedDeclaration;
    }

    protected internal virtual object? VisitExportSpecifier(ExportSpecifier exportSpecifier)
    {
        Visit(exportSpecifier.Local);

        if (exportSpecifier.Exported != exportSpecifier.Local)
        {
            Visit(exportSpecifier.Exported);
        }

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

        ref readonly var parameters = ref functionDeclaration.Params;
        for (var i = 0; i < parameters.Count; i++)
        {
            Visit(parameters[i]);
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

        ref readonly var parameters = ref functionExpression.Params;
        for (var i = 0; i < parameters.Count; i++)
        {
            Visit(parameters[i]);
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
        ref readonly var specifiers = ref importDeclaration.Specifiers;
        for (var i = 0; i < specifiers.Count; i++)
        {
            Visit(specifiers[i]);
        }

        Visit(importDeclaration.Source);

        ref readonly var assertions = ref importDeclaration.Assertions;
        for (var i = 0; i < assertions.Count; i++)
        {
            Visit(assertions[i]);
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
        if (importSpecifier.Imported != importSpecifier.Local)
        {
            Visit(importSpecifier.Imported);
        }

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
        ref readonly var decorators = ref methodDefinition.Decorators;
        for (var i = 0; i < decorators.Count; i++)
        {
            Visit(decorators[i]);
        }

        Visit(methodDefinition.Key);
        Visit(methodDefinition.Value);

        return methodDefinition;
    }

    protected internal virtual object? VisitNewExpression(NewExpression newExpression)
    {
        Visit(newExpression.Callee);
        ref readonly var arguments = ref newExpression.Arguments;
        for (var i = 0; i < arguments.Count; i++)
        {
            Visit(arguments[i]);
        }

        return newExpression;
    }

    protected internal virtual object? VisitObjectExpression(ObjectExpression objectExpression)
    {
        ref readonly var properties = ref objectExpression.Properties;
        for (var i = 0; i < properties.Count; i++)
        {
            Visit(properties[i]);
        }

        return objectExpression;
    }

    protected internal virtual object? VisitObjectPattern(ObjectPattern objectPattern)
    {
        ref readonly var properties = ref objectPattern.Properties;
        for (var i = 0; i < properties.Count; i++)
        {
            Visit(properties[i]);
        }

        return objectPattern;
    }

    protected internal virtual object? VisitPrivateIdentifier(PrivateIdentifier privateIdentifier)
    {
        return privateIdentifier;
    }

    protected internal virtual object? VisitProgram(Program program)
    {
        ref readonly var statements = ref program.Body;
        for (var i = 0; i < statements.Count; i++)
        {
            Visit(statements[i]);
        }

        return program;
    }

    protected internal virtual object? VisitProperty(Property property)
    {
        if (!property.Shorthand)
        {
            Visit(property.Key);
        }

        Visit(property.Value);

        return property;
    }

    protected internal virtual object? VisitPropertyDefinition(PropertyDefinition propertyDefinition)
    {
        ref readonly var decorators = ref propertyDefinition.Decorators;
        for (var i = 0; i < decorators.Count; i++)
        {
            Visit(decorators[i]);
        }

        Visit(propertyDefinition.Key);

        if (propertyDefinition.Value is not null)
        {
            Visit(propertyDefinition.Value);
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
        ref readonly var expressions = ref sequenceExpression.Expressions;
        for (var i = 0; i < expressions.Count; i++)
        {
            Visit(expressions[i]);
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
        ref readonly var body = ref staticBlock.Body;
        for (var i = 0; i < body.Count; i++)
        {
            Visit(body[i]);
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

        ref readonly var consequent = ref switchCase.Consequent;
        for (var i = 0; i < consequent.Count; i++)
        {
            Visit(consequent[i]);
        }

        return switchCase;
    }

    protected internal virtual object? VisitSwitchStatement(SwitchStatement switchStatement)
    {
        Visit(switchStatement.Discriminant);
        ref readonly var cases = ref switchStatement.Cases;
        for (var i = 0; i < cases.Count; i++)
        {
            Visit(cases[i]);
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
        ref readonly var quasis = ref templateLiteral.Quasis;
        ref readonly var expressions = ref templateLiteral.Expressions;

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
        ref readonly var declarations = ref variableDeclaration.Declarations;
        for (var i = 0; i < declarations.Count; i++)
        {
            Visit(declarations[i]);
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
