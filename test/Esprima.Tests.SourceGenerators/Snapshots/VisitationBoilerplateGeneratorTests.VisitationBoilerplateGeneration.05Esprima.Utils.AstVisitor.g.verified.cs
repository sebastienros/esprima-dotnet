//HintName: Esprima.Utils.AstVisitor.g.cs
#nullable enable

namespace Esprima.Utils;

partial class AstVisitor
{
    protected internal virtual object? VisitAccessorProperty(Esprima.Ast.AccessorProperty accessorProperty)
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

    protected internal virtual object? VisitArrayExpression(Esprima.Ast.ArrayExpression arrayExpression)
    {
        ref readonly var elements = ref arrayExpression.Elements;
        for (var i = 0; i < elements.Count; i++)
        {
            var elementsItem = elements[i];
            if (elementsItem is not null)
            {
                Visit(elementsItem);
            }
        }

        return arrayExpression;
    }

    protected internal virtual object? VisitArrayPattern(Esprima.Ast.ArrayPattern arrayPattern)
    {
        ref readonly var elements = ref arrayPattern.Elements;
        for (var i = 0; i < elements.Count; i++)
        {
            var elementsItem = elements[i];
            if (elementsItem is not null)
            {
                Visit(elementsItem);
            }
        }

        return arrayPattern;
    }

    protected internal virtual object? VisitArrowFunctionExpression(Esprima.Ast.ArrowFunctionExpression arrowFunctionExpression)
    {
        ref readonly var @params = ref arrowFunctionExpression.Params;
        for (var i = 0; i < @params.Count; i++)
        {
            Visit(@params[i]);
        }

        Visit(arrowFunctionExpression.Body);

        return arrowFunctionExpression;
    }

    protected internal virtual object? VisitAssignmentExpression(Esprima.Ast.AssignmentExpression assignmentExpression)
    {
        Visit(assignmentExpression.Left);

        Visit(assignmentExpression.Right);

        return assignmentExpression;
    }

    protected internal virtual object? VisitAssignmentPattern(Esprima.Ast.AssignmentPattern assignmentPattern)
    {
        Visit(assignmentPattern.Left);

        Visit(assignmentPattern.Right);

        return assignmentPattern;
    }

    protected internal virtual object? VisitAwaitExpression(Esprima.Ast.AwaitExpression awaitExpression)
    {
        Visit(awaitExpression.Argument);

        return awaitExpression;
    }

    protected internal virtual object? VisitBinaryExpression(Esprima.Ast.BinaryExpression binaryExpression)
    {
        Visit(binaryExpression.Left);

        Visit(binaryExpression.Right);

        return binaryExpression;
    }

    protected internal virtual object? VisitBlockStatement(Esprima.Ast.BlockStatement blockStatement)
    {
        ref readonly var body = ref blockStatement.Body;
        for (var i = 0; i < body.Count; i++)
        {
            Visit(body[i]);
        }

        return blockStatement;
    }

    protected internal virtual object? VisitBreakStatement(Esprima.Ast.BreakStatement breakStatement)
    {
        if (breakStatement.Label is not null)
        {
            Visit(breakStatement.Label);
        }

        return breakStatement;
    }

    protected internal virtual object? VisitCallExpression(Esprima.Ast.CallExpression callExpression)
    {
        Visit(callExpression.Callee);

        ref readonly var arguments = ref callExpression.Arguments;
        for (var i = 0; i < arguments.Count; i++)
        {
            Visit(arguments[i]);
        }

        return callExpression;
    }

    protected internal virtual object? VisitCatchClause(Esprima.Ast.CatchClause catchClause)
    {
        if (catchClause.Param is not null)
        {
            Visit(catchClause.Param);
        }

        Visit(catchClause.Body);

        return catchClause;
    }

    protected internal virtual object? VisitChainExpression(Esprima.Ast.ChainExpression chainExpression)
    {
        Visit(chainExpression.Expression);

        return chainExpression;
    }

    protected internal virtual object? VisitClassBody(Esprima.Ast.ClassBody classBody)
    {
        ref readonly var body = ref classBody.Body;
        for (var i = 0; i < body.Count; i++)
        {
            Visit(body[i]);
        }

        return classBody;
    }

    protected internal virtual object? VisitClassDeclaration(Esprima.Ast.ClassDeclaration classDeclaration)
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

    protected internal virtual object? VisitClassExpression(Esprima.Ast.ClassExpression classExpression)
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

    protected internal virtual object? VisitConditionalExpression(Esprima.Ast.ConditionalExpression conditionalExpression)
    {
        Visit(conditionalExpression.Test);

        Visit(conditionalExpression.Consequent);

        Visit(conditionalExpression.Alternate);

        return conditionalExpression;
    }

    protected internal virtual object? VisitContinueStatement(Esprima.Ast.ContinueStatement continueStatement)
    {
        if (continueStatement.Label is not null)
        {
            Visit(continueStatement.Label);
        }

        return continueStatement;
    }

    protected internal virtual object? VisitDebuggerStatement(Esprima.Ast.DebuggerStatement debuggerStatement)
    {
        return debuggerStatement;
    }

    protected internal virtual object? VisitDecorator(Esprima.Ast.Decorator decorator)
    {
        Visit(decorator.Expression);

        return decorator;
    }

    protected internal virtual object? VisitDoWhileStatement(Esprima.Ast.DoWhileStatement doWhileStatement)
    {
        Visit(doWhileStatement.Body);

        Visit(doWhileStatement.Test);

        return doWhileStatement;
    }

    protected internal virtual object? VisitEmptyStatement(Esprima.Ast.EmptyStatement emptyStatement)
    {
        return emptyStatement;
    }

    protected internal virtual object? VisitExportAllDeclaration(Esprima.Ast.ExportAllDeclaration exportAllDeclaration)
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

    protected internal virtual object? VisitExportDefaultDeclaration(Esprima.Ast.ExportDefaultDeclaration exportDefaultDeclaration)
    {
        Visit(exportDefaultDeclaration.Declaration);

        return exportDefaultDeclaration;
    }

    protected internal virtual object? VisitExportNamedDeclaration(Esprima.Ast.ExportNamedDeclaration exportNamedDeclaration)
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

    protected internal virtual object? VisitExpressionStatement(Esprima.Ast.ExpressionStatement expressionStatement)
    {
        Visit(expressionStatement.Expression);

        return expressionStatement;
    }

    protected internal virtual object? VisitForInStatement(Esprima.Ast.ForInStatement forInStatement)
    {
        Visit(forInStatement.Left);

        Visit(forInStatement.Right);

        Visit(forInStatement.Body);

        return forInStatement;
    }

    protected internal virtual object? VisitForOfStatement(Esprima.Ast.ForOfStatement forOfStatement)
    {
        Visit(forOfStatement.Left);

        Visit(forOfStatement.Right);

        Visit(forOfStatement.Body);

        return forOfStatement;
    }

    protected internal virtual object? VisitForStatement(Esprima.Ast.ForStatement forStatement)
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

    protected internal virtual object? VisitFunctionDeclaration(Esprima.Ast.FunctionDeclaration functionDeclaration)
    {
        if (functionDeclaration.Id is not null)
        {
            Visit(functionDeclaration.Id);
        }

        ref readonly var @params = ref functionDeclaration.Params;
        for (var i = 0; i < @params.Count; i++)
        {
            Visit(@params[i]);
        }

        Visit(functionDeclaration.Body);

        return functionDeclaration;
    }

    protected internal virtual object? VisitFunctionExpression(Esprima.Ast.FunctionExpression functionExpression)
    {
        if (functionExpression.Id is not null)
        {
            Visit(functionExpression.Id);
        }

        ref readonly var @params = ref functionExpression.Params;
        for (var i = 0; i < @params.Count; i++)
        {
            Visit(@params[i]);
        }

        Visit(functionExpression.Body);

        return functionExpression;
    }

    protected internal virtual object? VisitIdentifier(Esprima.Ast.Identifier identifier)
    {
        return identifier;
    }

    protected internal virtual object? VisitIfStatement(Esprima.Ast.IfStatement ifStatement)
    {
        Visit(ifStatement.Test);

        Visit(ifStatement.Consequent);

        if (ifStatement.Alternate is not null)
        {
            Visit(ifStatement.Alternate);
        }

        return ifStatement;
    }

    protected internal virtual object? VisitImportAttribute(Esprima.Ast.ImportAttribute importAttribute)
    {
        Visit(importAttribute.Key);

        Visit(importAttribute.Value);

        return importAttribute;
    }

    protected internal virtual object? VisitImportDeclaration(Esprima.Ast.ImportDeclaration importDeclaration)
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

    protected internal virtual object? VisitImportDefaultSpecifier(Esprima.Ast.ImportDefaultSpecifier importDefaultSpecifier)
    {
        Visit(importDefaultSpecifier.Local);

        return importDefaultSpecifier;
    }

    protected internal virtual object? VisitImportExpression(Esprima.Ast.ImportExpression importExpression)
    {
        Visit(importExpression.Source);

        if (importExpression.Attributes is not null)
        {
            Visit(importExpression.Attributes);
        }

        return importExpression;
    }

    protected internal virtual object? VisitImportNamespaceSpecifier(Esprima.Ast.ImportNamespaceSpecifier importNamespaceSpecifier)
    {
        Visit(importNamespaceSpecifier.Local);

        return importNamespaceSpecifier;
    }

    protected internal virtual object? VisitLabeledStatement(Esprima.Ast.LabeledStatement labeledStatement)
    {
        Visit(labeledStatement.Label);

        Visit(labeledStatement.Body);

        return labeledStatement;
    }

    protected internal virtual object? VisitLiteral(Esprima.Ast.Literal literal)
    {
        return literal;
    }

    protected internal virtual object? VisitMemberExpression(Esprima.Ast.MemberExpression memberExpression)
    {
        Visit(memberExpression.Object);

        Visit(memberExpression.Property);

        return memberExpression;
    }

    protected internal virtual object? VisitMetaProperty(Esprima.Ast.MetaProperty metaProperty)
    {
        Visit(metaProperty.Meta);

        Visit(metaProperty.Property);

        return metaProperty;
    }

    protected internal virtual object? VisitMethodDefinition(Esprima.Ast.MethodDefinition methodDefinition)
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

    protected internal virtual object? VisitNewExpression(Esprima.Ast.NewExpression newExpression)
    {
        Visit(newExpression.Callee);

        ref readonly var arguments = ref newExpression.Arguments;
        for (var i = 0; i < arguments.Count; i++)
        {
            Visit(arguments[i]);
        }

        return newExpression;
    }

    protected internal virtual object? VisitObjectExpression(Esprima.Ast.ObjectExpression objectExpression)
    {
        ref readonly var properties = ref objectExpression.Properties;
        for (var i = 0; i < properties.Count; i++)
        {
            Visit(properties[i]);
        }

        return objectExpression;
    }

    protected internal virtual object? VisitObjectPattern(Esprima.Ast.ObjectPattern objectPattern)
    {
        ref readonly var properties = ref objectPattern.Properties;
        for (var i = 0; i < properties.Count; i++)
        {
            Visit(properties[i]);
        }

        return objectPattern;
    }

    protected internal virtual object? VisitPrivateIdentifier(Esprima.Ast.PrivateIdentifier privateIdentifier)
    {
        return privateIdentifier;
    }

    protected internal virtual object? VisitProgram(Esprima.Ast.Program program)
    {
        ref readonly var body = ref program.Body;
        for (var i = 0; i < body.Count; i++)
        {
            Visit(body[i]);
        }

        return program;
    }

    protected internal virtual object? VisitPropertyDefinition(Esprima.Ast.PropertyDefinition propertyDefinition)
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

    protected internal virtual object? VisitRestElement(Esprima.Ast.RestElement restElement)
    {
        Visit(restElement.Argument);

        return restElement;
    }

    protected internal virtual object? VisitReturnStatement(Esprima.Ast.ReturnStatement returnStatement)
    {
        if (returnStatement.Argument is not null)
        {
            Visit(returnStatement.Argument);
        }

        return returnStatement;
    }

    protected internal virtual object? VisitSequenceExpression(Esprima.Ast.SequenceExpression sequenceExpression)
    {
        ref readonly var expressions = ref sequenceExpression.Expressions;
        for (var i = 0; i < expressions.Count; i++)
        {
            Visit(expressions[i]);
        }

        return sequenceExpression;
    }

    protected internal virtual object? VisitSpreadElement(Esprima.Ast.SpreadElement spreadElement)
    {
        Visit(spreadElement.Argument);

        return spreadElement;
    }

    protected internal virtual object? VisitStaticBlock(Esprima.Ast.StaticBlock staticBlock)
    {
        ref readonly var body = ref staticBlock.Body;
        for (var i = 0; i < body.Count; i++)
        {
            Visit(body[i]);
        }

        return staticBlock;
    }

    protected internal virtual object? VisitSuper(Esprima.Ast.Super super)
    {
        return super;
    }

    protected internal virtual object? VisitSwitchCase(Esprima.Ast.SwitchCase switchCase)
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

    protected internal virtual object? VisitSwitchStatement(Esprima.Ast.SwitchStatement switchStatement)
    {
        Visit(switchStatement.Discriminant);

        ref readonly var cases = ref switchStatement.Cases;
        for (var i = 0; i < cases.Count; i++)
        {
            Visit(cases[i]);
        }

        return switchStatement;
    }

    protected internal virtual object? VisitTaggedTemplateExpression(Esprima.Ast.TaggedTemplateExpression taggedTemplateExpression)
    {
        Visit(taggedTemplateExpression.Tag);

        Visit(taggedTemplateExpression.Quasi);

        return taggedTemplateExpression;
    }

    protected internal virtual object? VisitTemplateElement(Esprima.Ast.TemplateElement templateElement)
    {
        return templateElement;
    }

    protected internal virtual object? VisitThisExpression(Esprima.Ast.ThisExpression thisExpression)
    {
        return thisExpression;
    }

    protected internal virtual object? VisitThrowStatement(Esprima.Ast.ThrowStatement throwStatement)
    {
        Visit(throwStatement.Argument);

        return throwStatement;
    }

    protected internal virtual object? VisitTryStatement(Esprima.Ast.TryStatement tryStatement)
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

    protected internal virtual object? VisitUnaryExpression(Esprima.Ast.UnaryExpression unaryExpression)
    {
        Visit(unaryExpression.Argument);

        return unaryExpression;
    }

    protected internal virtual object? VisitVariableDeclaration(Esprima.Ast.VariableDeclaration variableDeclaration)
    {
        ref readonly var declarations = ref variableDeclaration.Declarations;
        for (var i = 0; i < declarations.Count; i++)
        {
            Visit(declarations[i]);
        }

        return variableDeclaration;
    }

    protected internal virtual object? VisitVariableDeclarator(Esprima.Ast.VariableDeclarator variableDeclarator)
    {
        Visit(variableDeclarator.Id);

        if (variableDeclarator.Init is not null)
        {
            Visit(variableDeclarator.Init);
        }

        return variableDeclarator;
    }

    protected internal virtual object? VisitWhileStatement(Esprima.Ast.WhileStatement whileStatement)
    {
        Visit(whileStatement.Test);

        Visit(whileStatement.Body);

        return whileStatement;
    }

    protected internal virtual object? VisitWithStatement(Esprima.Ast.WithStatement withStatement)
    {
        Visit(withStatement.Object);

        Visit(withStatement.Body);

        return withStatement;
    }

    protected internal virtual object? VisitYieldExpression(Esprima.Ast.YieldExpression yieldExpression)
    {
        if (yieldExpression.Argument is not null)
        {
            Visit(yieldExpression.Argument);
        }

        return yieldExpression;
    }
}
