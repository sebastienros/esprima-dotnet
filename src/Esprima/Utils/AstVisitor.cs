using Esprima.Ast;

namespace Esprima.Utils;

public partial class AstVisitor
{
    public virtual Node? Visit(Node? node)
    {
        return node?.Accept(this);
    }

    protected internal virtual bool HasNodeListChanged<T>(in NodeList<T> nodes, out NodeList<T> newNodes)
        where T : Node?
    {
        for (var i = 0; i < nodes.Count; i++)
        {
            var node = nodes[i];
            if (node is not null)
            {
                Visit(node as Node);
            }
        }

        newNodes = nodes;
        return false;
    }

    protected internal virtual Program VisitProgram(Program program)
    {
        var isNewStatements = HasNodeListChanged(program.Body, out var statements);
        return UpdateProgram(program, isNewStatements, ref statements);
    }

    [Obsolete(
        "This method may be removed in a future version as it will not be called anymore due to employing double dispatch (instead of switch dispatch).")]
    protected virtual void VisitUnknownNode(Node node)
    {
        throw new NotImplementedException(
            $"AST visitor doesn't support nodes of type {node.Type}, you can override VisitUnknownNode to handle this case.");
    }

    protected internal virtual CatchClause VisitCatchClause(CatchClause catchClause)
    {
        Expression? param = null;
        if (catchClause.Param is not null)
        {
            param = Visit(catchClause.Param) as Expression;
        }

        BlockStatement? body = Visit(catchClause.Body) as BlockStatement;
        return UpdateCatchClause(catchClause, param, body !);
    }

    protected internal virtual FunctionDeclaration VisitFunctionDeclaration(FunctionDeclaration functionDeclaration)
    {
        Identifier? id = null;
        if (functionDeclaration.Id is not null)
        {
            id = Visit(functionDeclaration.Id) as Identifier;
        }

        var isNewParameters = HasNodeListChanged(functionDeclaration.Params, out var parameters);
        BlockStatement? body = Visit(functionDeclaration.Body) as BlockStatement;
        return UpdateFunctionDeclaration(functionDeclaration, id, isNewParameters, ref parameters, body!);
    }

    protected internal virtual WithStatement VisitWithStatement(WithStatement withStatement)
    {
        var obj = Visit(withStatement.Object) as Expression;
        var body = Visit(withStatement.Body) as Statement;
        return UpdateWithStatement(withStatement, obj!, body!);
    }

    protected internal virtual WhileStatement VisitWhileStatement(WhileStatement whileStatement)
    {
        var test = Visit(whileStatement.Test) as Expression;
        var body = Visit(whileStatement.Body) as Statement;
        return UpdateWhileStatement(whileStatement, test!, body!);
    }

    protected internal virtual VariableDeclaration VisitVariableDeclaration(VariableDeclaration variableDeclaration)
    {
        var isNewDeclarations = HasNodeListChanged(variableDeclaration.Declarations, out var declarations);
        return UpdateVariableDeclaration(variableDeclaration, isNewDeclarations, ref declarations);
    }

    protected internal virtual TryStatement VisitTryStatement(TryStatement tryStatement)
    {
        var block = Visit(tryStatement.Block) as BlockStatement;

        CatchClause? handler = null;
        if (tryStatement.Handler is not null)
        {
            handler = Visit(tryStatement.Handler) as CatchClause;
        }

        BlockStatement? finalizer = null;
        if (tryStatement.Finalizer is not null)
        {
            finalizer = Visit(tryStatement.Finalizer) as BlockStatement;
        }

        return UpdateTryStatement(tryStatement, block!, handler, finalizer);
    }

    protected internal virtual ThrowStatement VisitThrowStatement(ThrowStatement throwStatement)
    {
        var argument = Visit(throwStatement.Argument) as Expression;
        return UpdateThrowStatement(throwStatement, argument!);
    }

    protected internal virtual SwitchStatement VisitSwitchStatement(SwitchStatement switchStatement)
    {
        var discriminant = Visit(switchStatement.Discriminant) as Expression;
        var isNewCases = HasNodeListChanged(switchStatement.Cases, out var cases);
        return UpdateSwitchStatement(switchStatement, discriminant!, isNewCases, ref cases);
    }

    protected internal virtual SwitchCase VisitSwitchCase(SwitchCase switchCase)
    {
        Expression? test = null;
        if (switchCase.Test is not null)
        {
            test = Visit(switchCase.Test) as Expression;
        }

        var isNewConsequent = HasNodeListChanged(switchCase.Consequent, out var consequent);
        return UpdateSwitchCase(switchCase, test, isNewConsequent, ref consequent);
    }

    protected internal virtual ReturnStatement VisitReturnStatement(ReturnStatement returnStatement)
    {
        Expression? argument = null;
        if (returnStatement.Argument is not null)
        {
            argument = Visit(returnStatement.Argument) as Expression;
        }

        return UpdateReturnStatement(returnStatement, argument);
    }

    protected internal virtual LabeledStatement VisitLabeledStatement(LabeledStatement labeledStatement)
    {
        var label = Visit(labeledStatement.Label) as Identifier;
        var body = Visit(labeledStatement.Body) as Statement;
        return UpdateLabeledStatement(labeledStatement, label!, body!);
    }

    protected internal virtual IfStatement VisitIfStatement(IfStatement ifStatement)
    {
        var test = Visit(ifStatement.Test) as Expression;
        var consequent = Visit(ifStatement.Consequent) as Statement;
        Statement? alternate = null;
        if (ifStatement.Alternate is not null)
        {
            alternate = Visit(ifStatement.Alternate) as Statement;
        }

        return UpdateIfStatement(ifStatement, test!, consequent!, alternate);
    }

    protected internal virtual EmptyStatement VisitEmptyStatement(EmptyStatement emptyStatement)
    {
        return UpdateEmptyStatement(emptyStatement);
    }

    protected internal virtual DebuggerStatement VisitDebuggerStatement(DebuggerStatement debuggerStatement)
    {
        return UpdateDebuggerStatement(debuggerStatement);
    }

    protected internal virtual ExpressionStatement VisitExpressionStatement(ExpressionStatement expressionStatement)
    {
        var expression = Visit(expressionStatement.Expression) as Expression;
        return UpdateExpressionStatement(expressionStatement, expression!);
    }

    protected internal virtual ForStatement VisitForStatement(ForStatement forStatement)
    {
        StatementListItem? init = null;
        if (forStatement.Init is not null)
        {
            init = Visit(forStatement.Init) as StatementListItem;
        }

        Expression? test = null;
        if (forStatement.Test is not null)
        {
            test = Visit(forStatement.Test) as Expression;
        }

        Expression? update = null;
        if (forStatement.Update is not null)
        {
            update = Visit(forStatement.Update) as Expression;
        }

        var body = Visit(forStatement.Body) as Statement;

        return UpdateForStatement(forStatement, init, test, update, body!);
    }

    protected internal virtual ForInStatement VisitForInStatement(ForInStatement forInStatement)
    {
        var left = Visit(forInStatement.Left);
        var right = Visit(forInStatement.Right) as Expression;
        var body = Visit(forInStatement.Body) as Statement;
        return UpdateForInStatement(forInStatement, left!, right!, body!);
    }

    protected internal virtual DoWhileStatement VisitDoWhileStatement(DoWhileStatement doWhileStatement)
    {
        var body = Visit(doWhileStatement.Body) as Statement;
        var test = Visit(doWhileStatement.Test) as Expression;
        return UpdateDoWhileStatement(doWhileStatement, body!, test!);
    }

    protected internal virtual ArrowFunctionExpression VisitArrowFunctionExpression(
        ArrowFunctionExpression arrowFunctionExpression)
    {
        var isNewParameters = HasNodeListChanged(arrowFunctionExpression.Params, out var parameters);
        var body = Visit(arrowFunctionExpression.Body);
        return UpdateArrowFunctionExpression(arrowFunctionExpression, isNewParameters, ref parameters, body!);
    }

    protected internal virtual UnaryExpression VisitUnaryExpression(UnaryExpression unaryExpression)
    {
        var argument = Visit(unaryExpression.Argument) as Expression;
        return UpdateUnaryExpression(unaryExpression, argument!);
    }

    protected internal virtual UpdateExpression VisitUpdateExpression(UpdateExpression updateExpression)
    {
        var argument = Visit(updateExpression.Argument) as Expression;
        return UpdateUpdateExpression(updateExpression, argument!);
    }

    protected internal virtual ThisExpression VisitThisExpression(ThisExpression thisExpression)
    {
        return UpdateThisExpression(thisExpression);
    }

    protected internal virtual SequenceExpression VisitSequenceExpression(SequenceExpression sequenceExpression)
    {
        var isNewExpressions = HasNodeListChanged(sequenceExpression.Expressions, out var expressions);
        return UpdateSequenceExpression(sequenceExpression, isNewExpressions, ref expressions);
    }

    protected internal virtual ObjectExpression VisitObjectExpression(ObjectExpression objectExpression)
    {
        var isNewProperties = HasNodeListChanged(objectExpression.Properties, out var properties);
        return UpdateObjectExpression(objectExpression, isNewProperties, ref properties);
    }

    protected internal virtual NewExpression VisitNewExpression(NewExpression newExpression)
    {
        var callee = Visit(newExpression.Callee) as Expression;
        var isNewArguments = HasNodeListChanged(newExpression.Arguments, out var arguments);
        return UpdateNewExpression(newExpression, callee!, isNewArguments, ref arguments);
    }

    protected internal virtual MemberExpression VisitMemberExpression(MemberExpression memberExpression)
    {
        var obj = Visit(memberExpression.Object) as Expression;
        var property = Visit(memberExpression.Property) as Expression;
        return UpdateMemberExpression(memberExpression, obj!, property!);
    }

    protected internal virtual BinaryExpression VisitLogicalExpression(BinaryExpression binaryExpression)
    {
        var left = Visit(binaryExpression.Left) as Expression;
        var right = Visit(binaryExpression.Right) as Expression;
        return UpdateLogicalExpression(binaryExpression, left!, right!);
    }

    protected internal virtual Literal VisitLiteral(Literal literal)
    {
        return UpdateLiteral(literal);
    }

    protected internal virtual Identifier VisitIdentifier(Identifier identifier)
    {
        return UpdateIdentifier(identifier);
    }

    protected internal virtual PrivateIdentifier VisitPrivateIdentifier(PrivateIdentifier privateIdentifier)
    {
        return UpdatePrivateIdentifier(privateIdentifier);
    }

    protected internal virtual IFunction VisitFunctionExpression(IFunction function)
    {
        Identifier? id = null;
        if (function.Id is not null)
        {
            id = Visit(function.Id) as Identifier;
        }

        var isNewParameters = HasNodeListChanged(function.Params, out var parameters);

        var body = Visit(function.Body) as Node;
        return UpdateFunctionExpression(function, id, isNewParameters, ref parameters, body!);
    }

    protected internal virtual PropertyDefinition VisitPropertyDefinition(PropertyDefinition propertyDefinition)
    {
        var key = Visit(propertyDefinition.Key) as Expression;

        Expression? value = null;
        if (propertyDefinition.Value is not null)
        {
            value = Visit(propertyDefinition.Value) as Expression;
        }

        return UpdatePropertyDefinition(propertyDefinition, key!, value);
    }

    protected internal virtual ChainExpression VisitChainExpression(ChainExpression chainExpression)
    {
        var expression = Visit(chainExpression.Expression) as Expression;
        return UpdateChainExpression(chainExpression, expression!);
    }

    protected internal virtual ClassExpression VisitClassExpression(ClassExpression classExpression)
    {
        Identifier? id = null;
        if (classExpression.Id is not null)
        {
            id = Visit(classExpression.Id) as Identifier;
        }

        Expression? superClass = null;
        if (classExpression.SuperClass is not null)
        {
            superClass = Visit(classExpression.SuperClass) as Expression;
        }

        var body = Visit(classExpression.Body) as ClassBody;
        return UpdateClassExpression(classExpression, id, superClass, body!);
    }

    protected internal virtual ExportDefaultDeclaration VisitExportDefaultDeclaration(
        ExportDefaultDeclaration exportDefaultDeclaration)
    {
        var declaration = Visit(exportDefaultDeclaration.Declaration) as StatementListItem;
        return UpdateExportDefaultDeclaration(exportDefaultDeclaration, declaration!);
    }

    protected internal virtual ExportAllDeclaration VisitExportAllDeclaration(
        ExportAllDeclaration exportAllDeclaration)
    {
        Expression? exported = null;
        if (exportAllDeclaration.Exported is not null)
        {
            exported = Visit(exportAllDeclaration.Exported) as Expression;
        }

        var source = Visit(exportAllDeclaration.Source) as Literal;
        return UpdateExportAllDeclaration(exportAllDeclaration, exported, source!);
    }

    protected internal virtual ExportNamedDeclaration VisitExportNamedDeclaration(
        ExportNamedDeclaration exportNamedDeclaration)
    {
        StatementListItem? declaration = null;
        if (exportNamedDeclaration.Declaration is not null)
        {
            declaration = Visit(exportNamedDeclaration.Declaration) as StatementListItem;
        }

        var isNewSpecifiers = HasNodeListChanged(exportNamedDeclaration.Specifiers, out var specifiers);

        Literal? source = null;
        if (exportNamedDeclaration.Source is not null)
        {
            source = Visit(exportNamedDeclaration.Source) as Literal;
        }

        return UpdateExportNamedDeclaration(exportNamedDeclaration, declaration, isNewSpecifiers, ref specifiers,
            source);
    }

    protected internal virtual ExportSpecifier VisitExportSpecifier(ExportSpecifier exportSpecifier)
    {
        var local = Visit(exportSpecifier.Local) as Expression;
        var exported = Visit(exportSpecifier.Exported) as Expression;
        return UpdateExportSpecifier(exportSpecifier, local!, exported!);
    }

    protected internal virtual Import VisitImport(Import import)
    {
        Expression? source = null;
        if (import.Source is not null)
        {
            source = Visit(import.Source) as Expression;
        }

        return UpdateImport(import, source);
    }

    protected internal virtual ImportDeclaration VisitImportDeclaration(ImportDeclaration importDeclaration)
    {
        var isNewSpecifiers = HasNodeListChanged(importDeclaration.Specifiers, out var specifiers);
        var source = Visit(importDeclaration.Source) as Literal;
        return UpdateImportDeclaration(importDeclaration, isNewSpecifiers, ref specifiers, source!);
    }

    protected internal virtual ImportNamespaceSpecifier VisitImportNamespaceSpecifier(
        ImportNamespaceSpecifier importNamespaceSpecifier)
    {
        var local = Visit(importNamespaceSpecifier.Local) as Identifier;
        return UpdateImportNamespaceSpecifier(importNamespaceSpecifier, local!);
    }

    protected internal virtual ImportDefaultSpecifier VisitImportDefaultSpecifier(
        ImportDefaultSpecifier importDefaultSpecifier)
    {
        var local = Visit(importDefaultSpecifier.Local) as Identifier;
        return UpdateImportDefaultSpecifier(importDefaultSpecifier, local!);
    }

    protected internal virtual ImportSpecifier VisitImportSpecifier(ImportSpecifier importSpecifier)
    {
        var imported = Visit(importSpecifier.Imported) as Expression;
        var local = Visit(importSpecifier.Local) as Identifier;
        return UpdateImportSpecifier(importSpecifier, imported!, local!);
    }

    protected internal virtual MethodDefinition VisitMethodDefinition(MethodDefinition methodDefinition)
    {
        var key = Visit(methodDefinition.Key) as Expression;
        var value = Visit(methodDefinition.Value) as Expression;
        return UpdateMethodDefinition(methodDefinition, key!, value!);
    }

    protected internal virtual ForOfStatement VisitForOfStatement(ForOfStatement forOfStatement)
    {
        var left = Visit(forOfStatement.Left);
        var right = Visit(forOfStatement.Right) as Expression;
        var body = Visit(forOfStatement.Body) as Statement;
        return UpdateForOfStatement(forOfStatement, left!, right!, body!);
    }

    protected internal virtual ClassDeclaration VisitClassDeclaration(ClassDeclaration classDeclaration)
    {
        Identifier? id = null;
        if (classDeclaration.Id is not null)
        {
            id = Visit(classDeclaration.Id) as Identifier;
        }

        Expression? superClass = null;
        if (classDeclaration.SuperClass is not null)
        {
            superClass = Visit(classDeclaration.SuperClass) as Expression;
        }

        var body = Visit(classDeclaration.Body) as ClassBody;
        return UpdateClassDeclaration(classDeclaration, id, superClass, body!);
    }

    protected internal virtual ClassBody VisitClassBody(ClassBody classBody)
    {
        var isNewBody = HasNodeListChanged(classBody.Body, out var body);
        return UpdateClassBody(classBody, isNewBody, ref body);
    }

    protected internal virtual YieldExpression VisitYieldExpression(YieldExpression yieldExpression)
    {
        Expression? argument = null;
        if (yieldExpression.Argument is not null)
        {
            argument = Visit(yieldExpression.Argument) as Expression;
        }

        return UpdateYieldExpression(yieldExpression, argument);
    }

    protected internal virtual TaggedTemplateExpression VisitTaggedTemplateExpression(
        TaggedTemplateExpression taggedTemplateExpression)
    {
        var tag = Visit(taggedTemplateExpression.Tag) as Expression;
        var quasi = Visit(taggedTemplateExpression.Quasi) as TemplateLiteral;
        return UpdateTaggedTemplateExpression(taggedTemplateExpression, tag!, quasi!);
    }

    protected internal virtual Super VisitSuper(Super super)
    {
        return UpdateSuper(super);
    }

    protected internal virtual MetaProperty VisitMetaProperty(MetaProperty metaProperty)
    {
        var meta = Visit(metaProperty.Meta) as Identifier;
        var property = Visit(metaProperty.Property) as Identifier;
        return UpdateMetaProperty(metaProperty, meta!, property!);
    }

    protected internal virtual ArrowParameterPlaceHolder VisitArrowParameterPlaceHolder(
        ArrowParameterPlaceHolder arrowParameterPlaceHolder)
    {
        // ArrowParameterPlaceHolder nodes never appear in the final tree and only used during the construction of a tree.
        return UpdateArrowParameterPlaceHolder(arrowParameterPlaceHolder);
    }

    protected internal virtual ObjectPattern VisitObjectPattern(ObjectPattern objectPattern)
    {
        var isNewProperties = HasNodeListChanged(objectPattern.Properties, out var properties);
        return UpdateObjectPattern(objectPattern, isNewProperties, ref properties);
    }

    protected internal virtual SpreadElement VisitSpreadElement(SpreadElement spreadElement)
    {
        var argument = Visit(spreadElement.Argument) as Expression;
        return UpdateSpreadElement(spreadElement, argument!);
    }

    protected internal virtual AssignmentPattern VisitAssignmentPattern(AssignmentPattern assignmentPattern)
    {
        var left = Visit(assignmentPattern.Left) as Expression;
        var right = Visit(assignmentPattern.Right) as Expression;
        return UpdateAssignmentPattern(assignmentPattern, left!, right!);
    }

    protected internal virtual ArrayPattern VisitArrayPattern(ArrayPattern arrayPattern)
    {
        var isNewElements = HasNodeListChanged(arrayPattern.Elements, out var elements);
        return UpdateArrayPattern(arrayPattern, isNewElements, ref elements);
    }

    protected internal virtual VariableDeclarator VisitVariableDeclarator(VariableDeclarator variableDeclarator)
    {
        var id = Visit(variableDeclarator.Id) as Expression;
        Expression? init = null;
        if (variableDeclarator.Init is not null)
        {
            init = Visit(variableDeclarator.Init) as Expression;
        }

        return UpdateVariableDeclarator(variableDeclarator, id!, init);
    }

    protected internal virtual TemplateLiteral VisitTemplateLiteral(TemplateLiteral templateLiteral)
    {
        var quasis = templateLiteral.Quasis;
        var expressions = templateLiteral.Expressions;

        var n = expressions.Count;

        for (var i = 0; i < n; i++)
        {
            Visit(quasis[i]);
            Visit(expressions[i]);
        }

        Visit(quasis[n]);
        return UpdateTemplateLiteral(templateLiteral, ref quasis, ref expressions);
    }

    protected internal virtual TemplateElement VisitTemplateElement(TemplateElement templateElement)
    {
        return UpdateTemplateElement(templateElement);
    }

    protected internal virtual RestElement VisitRestElement(RestElement restElement)
    {
        var argument = Visit(restElement.Argument) as Expression;
        return UpdateRestElement(restElement, argument!);
    }

    protected internal virtual Property VisitProperty(Property property)
    {
        var key = Visit(property.Key) as Expression;
        var value = Visit(property.Value) as Expression;
        return UpdateProperty(property, key!, value!);
    }

    protected internal virtual AwaitExpression VisitAwaitExpression(AwaitExpression awaitExpression)
    {
        var argument = Visit(awaitExpression.Argument) as Expression;
        return UpdateAwaitExpression(awaitExpression, argument!);
    }

    protected internal virtual ConditionalExpression VisitConditionalExpression(
        ConditionalExpression conditionalExpression)
    {
        var test = Visit(conditionalExpression.Test) as Expression;
        var consequent = Visit(conditionalExpression.Consequent) as Expression;
        var alternate = Visit(conditionalExpression.Alternate) as Expression;
        return UpdateConditionalExpression(conditionalExpression, test!, consequent!, alternate!);
    }

    protected internal virtual CallExpression VisitCallExpression(CallExpression callExpression)
    {
        var callee = Visit(callExpression.Callee) as Expression;
        var isNewArguments = HasNodeListChanged(callExpression.Arguments, out var arguments);
        return UpdateCallExpression(callExpression, callee!, isNewArguments, ref arguments);
    }

    protected internal virtual BinaryExpression VisitBinaryExpression(BinaryExpression binaryExpression)
    {
        var left = Visit(binaryExpression.Left) as Expression;
        var right = Visit(binaryExpression.Right) as Expression;
        return UpdateBinaryExpression(binaryExpression, left!, right!);
    }

    protected internal virtual ArrayExpression VisitArrayExpression(ArrayExpression arrayExpression)
    {
        var isNewElements = HasNodeListChanged(arrayExpression.Elements, out var elements);
        return UpdateArrayExpression(arrayExpression, isNewElements, ref elements);
    }

    protected internal virtual AssignmentExpression VisitAssignmentExpression(
        AssignmentExpression assignmentExpression)
    {
        var left = Visit(assignmentExpression.Left) as Expression;
        var right = Visit(assignmentExpression.Right) as Expression;
        return UpdateAssignmentExpression(assignmentExpression, left!, right!);
    }

    protected internal virtual ContinueStatement VisitContinueStatement(ContinueStatement continueStatement)
    {
        Identifier? label = null;
        if (continueStatement.Label is not null)
        {
            label = Visit(continueStatement.Label) as Identifier;
        }

        return UpdateContinueStatement(continueStatement, label);
    }

    protected internal virtual BreakStatement VisitBreakStatement(BreakStatement breakStatement)
    {
        Identifier? label = null;
        if (breakStatement.Label is not null)
        {
            label = Visit(breakStatement.Label) as Identifier;
        }

        return UpdateBreakStatement(breakStatement, label);
    }

    protected internal virtual BlockStatement VisitBlockStatement(BlockStatement blockStatement)
    {
        var isNewBody = HasNodeListChanged(blockStatement.Body, out var body);
        return UpdateBlockStatement(blockStatement, isNewBody, ref body);
    }
}
