using System.Runtime.CompilerServices;
using Esprima.Ast;

namespace Esprima.Utils;

public abstract partial class AstConverter : AstVisitor
{
    public T VisitAndConvert<T>(T node, [CallerMemberName] string? callerName = null) where T : Node
    {
        node = (Visit(node) as T)!;
        if (node is null)
        {
            throw new InvalidOperationException($"When called from {callerName}, rewriting a node of type '{typeof(T)}' must return a non-null value of the same type. Alternatively, override {callerName} and change it to not visit children of this type.");
        }
        return node;
    }

    protected internal override object? VisitProgram(Program program)
    {
        var isNewStatements = HasNodeListChanged(program.Body, out var statements);
        return UpdateProgram(program, isNewStatements, ref statements);
    }

    [Obsolete("This method may be removed in a future version as it will not be called anymore due to employing double dispatch (instead of switch dispatch).")]
    protected internal override object? VisitUnknownNode(Node node)
    {
        throw new NotImplementedException(
            $"AST visitor doesn't support nodes of type {node.Type}, you can override VisitUnknownNode to handle this case.");
    }

    protected internal override object? VisitCatchClause(CatchClause catchClause)
    {
        Expression? param = null;
        if (catchClause.Param is not null)
        {
            param = Visit(catchClause.Param) as Expression;
        }

        BlockStatement? body = Visit(catchClause.Body) as BlockStatement;
        return UpdateCatchClause(catchClause, param, body!);
    }

    protected internal override object? VisitFunctionDeclaration(FunctionDeclaration functionDeclaration)
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

    protected internal override object? VisitWithStatement(WithStatement withStatement)
    {
        var obj = Visit(withStatement.Object) as Expression;
        var body = Visit(withStatement.Body) as Statement;
        return UpdateWithStatement(withStatement, obj!, body!);
    }

    protected internal override object? VisitWhileStatement(WhileStatement whileStatement)
    {
        var test = Visit(whileStatement.Test) as Expression;
        var body = Visit(whileStatement.Body) as Statement;
        return UpdateWhileStatement(whileStatement, test!, body!);
    }

    protected internal override object? VisitVariableDeclaration(VariableDeclaration variableDeclaration)
    {
        var isNewDeclarations = HasNodeListChanged(variableDeclaration.Declarations, out var declarations);
        return UpdateVariableDeclaration(variableDeclaration, isNewDeclarations, ref declarations);
    }

    protected internal override object? VisitTryStatement(TryStatement tryStatement)
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

    protected internal override object? VisitThrowStatement(ThrowStatement throwStatement)
    {
        var argument = Visit(throwStatement.Argument) as Expression;
        return UpdateThrowStatement(throwStatement, argument!);
    }

    protected internal override object? VisitSwitchStatement(SwitchStatement switchStatement)
    {
        var discriminant = Visit(switchStatement.Discriminant) as Expression;
        var isNewCases = HasNodeListChanged(switchStatement.Cases, out var cases);
        return UpdateSwitchStatement(switchStatement, discriminant!, isNewCases, ref cases);
    }

    protected internal override object? VisitSwitchCase(SwitchCase switchCase)
    {
        Expression? test = null;
        if (switchCase.Test is not null)
        {
            test = Visit(switchCase.Test) as Expression;
        }

        var isNewConsequent = HasNodeListChanged(switchCase.Consequent, out var consequent);
        return UpdateSwitchCase(switchCase, test, isNewConsequent, ref consequent);
    }

    protected internal override object? VisitReturnStatement(ReturnStatement returnStatement)
    {
        Expression? argument = null;
        if (returnStatement.Argument is not null)
        {
            argument = Visit(returnStatement.Argument) as Expression;
        }

        return UpdateReturnStatement(returnStatement, argument);
    }

    protected internal override object? VisitLabeledStatement(LabeledStatement labeledStatement)
    {
        var label = Visit(labeledStatement.Label) as Identifier;
        var body = Visit(labeledStatement.Body) as Statement;
        return UpdateLabeledStatement(labeledStatement, label!, body!);
    }

    protected internal override object? VisitIfStatement(IfStatement ifStatement)
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

    protected internal override object? VisitEmptyStatement(EmptyStatement emptyStatement)
    {
        return UpdateEmptyStatement(emptyStatement);
    }

    protected internal override object? VisitDebuggerStatement(DebuggerStatement debuggerStatement)
    {
        return UpdateDebuggerStatement(debuggerStatement);
    }

    protected internal override object? VisitExpressionStatement(ExpressionStatement expressionStatement)
    {
        var expression = Visit(expressionStatement.Expression) as Expression;
        return UpdateExpressionStatement(expressionStatement, expression!);
    }

    protected internal override object? VisitForStatement(ForStatement forStatement)
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

    protected internal override object? VisitForInStatement(ForInStatement forInStatement)
    {
        var left = Visit(forInStatement.Left) as Node;
        var right = Visit(forInStatement.Right) as Expression;
        var body = Visit(forInStatement.Body) as Statement;
        return UpdateForInStatement(forInStatement, left!, right!, body!);
    }

    protected internal override object? VisitDoWhileStatement(DoWhileStatement doWhileStatement)
    {
        var body = Visit(doWhileStatement.Body) as Statement;
        var test = Visit(doWhileStatement.Test) as Expression;
        return UpdateDoWhileStatement(doWhileStatement, body!, test!);
    }

    protected internal override object? VisitArrowFunctionExpression(ArrowFunctionExpression arrowFunctionExpression)
    {
        var isNewParameters = HasNodeListChanged(arrowFunctionExpression.Params, out var parameters);
        var body = Visit(arrowFunctionExpression.Body) as Node;
        return UpdateArrowFunctionExpression(arrowFunctionExpression, isNewParameters, ref parameters, body!);
    }

    protected internal override object? VisitUnaryExpression(UnaryExpression unaryExpression)
    {
        var argument = Visit(unaryExpression.Argument) as Expression;
        return UpdateUnaryExpression(unaryExpression, argument!);
    }

    protected internal override object? VisitUpdateExpression(UpdateExpression updateExpression)
    {
        var argument = Visit(updateExpression.Argument) as Expression;
        return UpdateUpdateExpression(updateExpression, argument!);
    }

    protected internal override object? VisitThisExpression(ThisExpression thisExpression)
    {
        return UpdateThisExpression(thisExpression);
    }

    protected internal override object? VisitSequenceExpression(SequenceExpression sequenceExpression)
    {
        var isNewExpressions = HasNodeListChanged(sequenceExpression.Expressions, out var expressions);
        return UpdateSequenceExpression(sequenceExpression, isNewExpressions, ref expressions);
    }

    protected internal override object? VisitObjectExpression(ObjectExpression objectExpression)
    {
        var isNewProperties = HasNodeListChanged(objectExpression.Properties, out var properties);
        return UpdateObjectExpression(objectExpression, isNewProperties, ref properties);
    }

    protected internal override object? VisitNewExpression(NewExpression newExpression)
    {
        var callee = Visit(newExpression.Callee) as Expression;
        var isNewArguments = HasNodeListChanged(newExpression.Arguments, out var arguments);
        return UpdateNewExpression(newExpression, callee!, isNewArguments, ref arguments);
    }

    protected internal override object? VisitMemberExpression(MemberExpression memberExpression)
    {
        var obj = Visit(memberExpression.Object) as Expression;
        var property = Visit(memberExpression.Property) as Expression;
        return UpdateMemberExpression(memberExpression, obj!, property!);
    }

    protected internal override object? VisitLogicalExpression(BinaryExpression binaryExpression)
    {
        var left = Visit(binaryExpression.Left) as Expression;
        var right = Visit(binaryExpression.Right) as Expression;
        return UpdateLogicalExpression(binaryExpression, left!, right!);
    }

    protected internal override object? VisitLiteral(Literal literal)
    {
        return UpdateLiteral(literal);
    }

    protected internal override object? VisitIdentifier(Identifier identifier)
    {
        return UpdateIdentifier(identifier);
    }

    protected internal override object? VisitPrivateIdentifier(PrivateIdentifier privateIdentifier)
    {
        return UpdatePrivateIdentifier(privateIdentifier);
    }

    protected internal override object? VisitFunctionExpression(IFunction function)
    {
        Identifier? id = null;
        if (function.Id is not null)
        {
            id = Visit(function.Id) as Identifier;
        }

        var isNewParameters = HasNodeListChanged(function.Params, out var parameters);

        var body = Visit(function.Body) as Node;
        return (Node)UpdateFunctionExpression(function, id, isNewParameters, ref parameters, body!);
    }

    protected internal override object? VisitPropertyDefinition(PropertyDefinition propertyDefinition)
    {
        var key = Visit(propertyDefinition.Key) as Expression;

        Expression? value = null;
        if (propertyDefinition.Value is not null)
        {
            value = Visit(propertyDefinition.Value) as Expression;
        }

        return UpdatePropertyDefinition(propertyDefinition, key!, value);
    }

    protected internal override object? VisitChainExpression(ChainExpression chainExpression)
    {
        var expression = Visit(chainExpression.Expression) as Expression;
        return UpdateChainExpression(chainExpression, expression!);
    }

    protected internal override object? VisitClassExpression(ClassExpression classExpression)
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

    protected internal override object? VisitExportDefaultDeclaration(ExportDefaultDeclaration exportDefaultDeclaration)
    {
        var declaration = Visit(exportDefaultDeclaration.Declaration) as StatementListItem;
        return UpdateExportDefaultDeclaration(exportDefaultDeclaration, declaration!);
    }

    protected internal override object? VisitExportAllDeclaration(ExportAllDeclaration exportAllDeclaration)
    {
        Expression? exported = null;
        if (exportAllDeclaration.Exported is not null)
        {
            exported = Visit(exportAllDeclaration.Exported) as Expression;
        }

        var source = Visit(exportAllDeclaration.Source) as Literal;
        return UpdateExportAllDeclaration(exportAllDeclaration, exported, source!);
    }

    protected internal override object? VisitExportNamedDeclaration(ExportNamedDeclaration exportNamedDeclaration)
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

    protected internal override object? VisitExportSpecifier(ExportSpecifier exportSpecifier)
    {
        var local = Visit(exportSpecifier.Local) as Expression;
        var exported = Visit(exportSpecifier.Exported) as Expression;
        return UpdateExportSpecifier(exportSpecifier, local!, exported!);
    }

    protected internal override object? VisitImport(Import import)
    {
        Expression? source = null;
        if (import.Source is not null)
        {
            source = Visit(import.Source) as Expression;
        }

        return UpdateImport(import, source);
    }

    protected internal override object? VisitImportDeclaration(ImportDeclaration importDeclaration)
    {
        var isNewSpecifiers = HasNodeListChanged(importDeclaration.Specifiers, out var specifiers);
        var source = Visit(importDeclaration.Source) as Literal;
        return UpdateImportDeclaration(importDeclaration, isNewSpecifiers, ref specifiers, source!);
    }

    protected internal override object? VisitImportNamespaceSpecifier(ImportNamespaceSpecifier importNamespaceSpecifier)
    {
        var local = Visit(importNamespaceSpecifier.Local) as Identifier;
        return UpdateImportNamespaceSpecifier(importNamespaceSpecifier, local!);
    }

    protected internal override object? VisitImportDefaultSpecifier(ImportDefaultSpecifier importDefaultSpecifier)
    {
        var local = Visit(importDefaultSpecifier.Local) as Identifier;
        return UpdateImportDefaultSpecifier(importDefaultSpecifier, local!);
    }

    protected internal override object? VisitImportSpecifier(ImportSpecifier importSpecifier)
    {
        var imported = Visit(importSpecifier.Imported) as Expression;
        var local = Visit(importSpecifier.Local) as Identifier;
        return UpdateImportSpecifier(importSpecifier, imported!, local!);
    }

    protected internal override object? VisitMethodDefinition(MethodDefinition methodDefinition)
    {
        var key = Visit(methodDefinition.Key) as Expression;
        var value = Visit(methodDefinition.Value) as Expression;
        return UpdateMethodDefinition(methodDefinition, key!, value!);
    }

    protected internal override object? VisitForOfStatement(ForOfStatement forOfStatement)
    {
        var left = Visit(forOfStatement.Left) as Node;
        var right = Visit(forOfStatement.Right) as Expression;
        var body = Visit(forOfStatement.Body) as Statement;
        return UpdateForOfStatement(forOfStatement, left!, right!, body!);
    }

    protected internal override object? VisitClassDeclaration(ClassDeclaration classDeclaration)
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

    protected internal override object? VisitClassBody(ClassBody classBody)
    {
        var isNewBody = HasNodeListChanged(classBody.Body, out var body);
        return UpdateClassBody(classBody, isNewBody, ref body);
    }

    protected internal override object? VisitYieldExpression(YieldExpression yieldExpression)
    {
        Expression? argument = null;
        if (yieldExpression.Argument is not null)
        {
            argument = Visit(yieldExpression.Argument) as Expression;
        }

        return UpdateYieldExpression(yieldExpression, argument);
    }

    protected internal override object? VisitTaggedTemplateExpression(TaggedTemplateExpression taggedTemplateExpression)
    {
        var tag = Visit(taggedTemplateExpression.Tag) as Expression;
        var quasi = Visit(taggedTemplateExpression.Quasi) as TemplateLiteral;
        return UpdateTaggedTemplateExpression(taggedTemplateExpression, tag!, quasi!);
    }

    protected internal override object? VisitSuper(Super super)
    {
        return UpdateSuper(super);
    }

    protected internal override object? VisitMetaProperty(MetaProperty metaProperty)
    {
        var meta = Visit(metaProperty.Meta) as Identifier;
        var property = Visit(metaProperty.Property) as Identifier;
        return UpdateMetaProperty(metaProperty, meta!, property!);
    }

    protected internal override object? VisitArrowParameterPlaceHolder(ArrowParameterPlaceHolder arrowParameterPlaceHolder)
    {
        // ArrowParameterPlaceHolder nodes never appear in the final tree and only used during the construction of a tree.
        return UpdateArrowParameterPlaceHolder(arrowParameterPlaceHolder);
    }

    protected internal override object? VisitObjectPattern(ObjectPattern objectPattern)
    {
        var isNewProperties = HasNodeListChanged(objectPattern.Properties, out var properties);
        return UpdateObjectPattern(objectPattern, isNewProperties, ref properties);
    }

    protected internal override object? VisitSpreadElement(SpreadElement spreadElement)
    {
        var argument = Visit(spreadElement.Argument) as Expression;
        return UpdateSpreadElement(spreadElement, argument!);
    }

    protected internal override object? VisitAssignmentPattern(AssignmentPattern assignmentPattern)
    {
        var left = Visit(assignmentPattern.Left) as Expression;
        var right = Visit(assignmentPattern.Right) as Expression;
        return UpdateAssignmentPattern(assignmentPattern, left!, right!);
    }

    protected internal override object? VisitArrayPattern(ArrayPattern arrayPattern)
    {
        var isNewElements = HasNodeListChanged(arrayPattern.Elements, out var elements);
        return UpdateArrayPattern(arrayPattern, isNewElements, ref elements);
    }

    protected internal override object? VisitVariableDeclarator(VariableDeclarator variableDeclarator)
    {
        var id = Visit(variableDeclarator.Id) as Expression;
        Expression? init = null;
        if (variableDeclarator.Init is not null)
        {
            init = Visit(variableDeclarator.Init) as Expression;
        }

        return UpdateVariableDeclarator(variableDeclarator, id!, init);
    }

    protected internal override object? VisitTemplateLiteral(TemplateLiteral templateLiteral)
    {
        var isNewQuasis = HasNodeListChanged(templateLiteral.Quasis, out var quasis);
        var isNewExpressions = HasNodeListChanged(templateLiteral.Expressions, out var expressions);

        return UpdateTemplateLiteral(templateLiteral, isNewQuasis, ref quasis, isNewExpressions, ref expressions);
    }

    protected internal override object? VisitTemplateElement(TemplateElement templateElement)
    {
        return UpdateTemplateElement(templateElement);
    }

    protected internal override object? VisitRestElement(RestElement restElement)
    {
        var argument = Visit(restElement.Argument) as Expression;
        return UpdateRestElement(restElement, argument!);
    }

    protected internal override object? VisitProperty(Property property)
    {
        var key = Visit(property.Key) as Expression;
        var value = Visit(property.Value) as Expression;
        return UpdateProperty(property, key!, value!);
    }

    protected internal override object? VisitAwaitExpression(AwaitExpression awaitExpression)
    {
        var argument = Visit(awaitExpression.Argument) as Expression;
        return UpdateAwaitExpression(awaitExpression, argument!);
    }

    protected internal override object? VisitConditionalExpression(ConditionalExpression conditionalExpression)
    {
        var test = Visit(conditionalExpression.Test) as Expression;
        var consequent = Visit(conditionalExpression.Consequent) as Expression;
        var alternate = Visit(conditionalExpression.Alternate) as Expression;
        return UpdateConditionalExpression(conditionalExpression, test!, consequent!, alternate!);
    }

    protected internal override object? VisitCallExpression(CallExpression callExpression)
    {
        var callee = Visit(callExpression.Callee) as Expression;
        var isNewArguments = HasNodeListChanged(callExpression.Arguments, out var arguments);
        return UpdateCallExpression(callExpression, callee!, isNewArguments, ref arguments);
    }

    protected internal override object? VisitBinaryExpression(BinaryExpression binaryExpression)
    {
        var left = Visit(binaryExpression.Left) as Expression;
        var right = Visit(binaryExpression.Right) as Expression;
        return UpdateBinaryExpression(binaryExpression, left!, right!);
    }

    protected internal override object? VisitArrayExpression(ArrayExpression arrayExpression)
    {
        var isNewElements = HasNodeListChanged(arrayExpression.Elements, out var elements);
        return UpdateArrayExpression(arrayExpression, isNewElements, ref elements);
    }

    protected internal override object? VisitAssignmentExpression(AssignmentExpression assignmentExpression)
    {
        var left = Visit(assignmentExpression.Left) as Expression;
        var right = Visit(assignmentExpression.Right) as Expression;
        return UpdateAssignmentExpression(assignmentExpression, left!, right!);
    }

    protected internal override object? VisitContinueStatement(ContinueStatement continueStatement)
    {
        Identifier? label = null;
        if (continueStatement.Label is not null)
        {
            label = Visit(continueStatement.Label) as Identifier;
        }

        return UpdateContinueStatement(continueStatement, label);
    }

    protected internal override object? VisitBreakStatement(BreakStatement breakStatement)
    {
        Identifier? label = null;
        if (breakStatement.Label is not null)
        {
            label = Visit(breakStatement.Label) as Identifier;
        }

        return UpdateBreakStatement(breakStatement, label);
    }

    protected internal override object? VisitBlockStatement(BlockStatement blockStatement)
    {
        var isNewBody = HasNodeListChanged(blockStatement.Body, out var body);
        return UpdateBlockStatement(blockStatement, isNewBody, ref body);
    }

    #region Update methods

    protected virtual bool HasNodeListChanged<T>(in NodeList<T> nodes, out NodeList<T> newNodes)
        where T : Node?
    {
        List<T>? newNodeList = null;
        for (var i = 0; i < nodes.Count; i++)
        {
            var node = nodes[i];
            if (node is null || node is not Node nodeCast)
            {
                continue;
            }

            var newNode = Visit(nodeCast);
            if (newNodeList is not null)
            {
                if (newNode is not null)
                {
                    newNodeList.Add((T) newNode);
                }
            }
            else if (newNode != nodeCast)
            {
                newNodeList = new List<T>();
                for (var j = 0; j < i; j++)
                {
                    newNodeList.Add(nodes[j]);
                }

                if (newNode is not null)
                {
                    newNodeList.Add((T) newNode);
                }
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

    protected virtual Program UpdateProgram(Program program, bool isNewStatements,
        ref NodeList<Statement> statements)
    {
        if (!isNewStatements)
        {
            return program;
        }

        return program switch
        {
            Module => new Module(statements),
            Script script => new Script(statements, script.Strict),
            _ => throw new NotImplementedException($"{program.SourceType} does not implemented yet.")
        };
    }

    protected virtual CatchClause UpdateCatchClause(CatchClause catchClause, Expression? param,
        BlockStatement body)
    {
        if (param is not null)
        {
            if (param == catchClause.Param && body == catchClause.Body)
            {
                return catchClause;
            }

            return new CatchClause(param, body);
        }

        if (body == catchClause.Body)
        {
            return catchClause;
        }

        return new CatchClause(catchClause.Param, body);
    }

    protected virtual FunctionDeclaration UpdateFunctionDeclaration(FunctionDeclaration functionDeclaration,
        Identifier? id,
        bool isNewParameters, ref NodeList<Expression> parameters, BlockStatement body)
    {
        if (!isNewParameters && id == functionDeclaration.Id && body == functionDeclaration.Body)
        {
            return functionDeclaration;
        }

        return new FunctionDeclaration(id, parameters, body, functionDeclaration.Generator,
            functionDeclaration.Strict, functionDeclaration.Async);
    }

    protected virtual WithStatement UpdateWithStatement(WithStatement withStatement, Expression obj,
        Statement body)
    {
        if (obj == withStatement.Object && body == withStatement.Body)
        {
            return withStatement;
        }

        return new WithStatement(obj, body);
    }

    protected virtual WhileStatement UpdateWhileStatement(WhileStatement whileStatement, Expression test,
        Statement body)
    {
        if (test == whileStatement.Test && body == whileStatement.Body)
        {
            return whileStatement;
        }

        return new WhileStatement(test, body);
    }

    protected virtual VariableDeclaration UpdateVariableDeclaration(VariableDeclaration variableDeclaration,
        bool isNewDeclarations,
        ref NodeList<VariableDeclarator> declarations)
    {
        if (isNewDeclarations)
        {
            return new VariableDeclaration(declarations, variableDeclaration.Kind);
        }

        return variableDeclaration;
    }

    protected virtual TryStatement UpdateTryStatement(TryStatement tryStatement, BlockStatement block,
        CatchClause? handler, BlockStatement? finalizer)
    {
        if (block == tryStatement.Block && handler == tryStatement.Handler && finalizer == tryStatement.Finalizer)
        {
            return tryStatement;
        }

        return new TryStatement(block, handler, finalizer);
    }

    protected virtual ThrowStatement UpdateThrowStatement(ThrowStatement throwStatement, Expression argument)
    {
        if (argument == throwStatement.Argument)
        {
            return throwStatement;
        }

        return new ThrowStatement(argument);
    }

    protected virtual SwitchStatement UpdateSwitchStatement(SwitchStatement switchStatement,
        Expression discriminant, bool isNewCases,
        ref NodeList<SwitchCase> cases)
    {
        if (discriminant == switchStatement.Discriminant && !isNewCases)
        {
            return switchStatement;
        }

        return new SwitchStatement(discriminant, cases);
    }

    protected virtual SwitchCase UpdateSwitchCase(SwitchCase switchCase, Expression? test, bool isNewConsequent,
        ref NodeList<Statement> consequent)
    {
        if (test == switchCase.Test && !isNewConsequent)
        {
            return switchCase;
        }

        return new SwitchCase(test, consequent);
    }

    protected virtual ReturnStatement UpdateReturnStatement(ReturnStatement returnStatement, Expression? argument)
    {
        if (argument == returnStatement.Argument)
        {
            return returnStatement;
        }

        return new ReturnStatement(argument);
    }

    protected virtual LabeledStatement UpdateLabeledStatement(LabeledStatement labeledStatement, Identifier label,
        Statement body)
    {
        if (label == labeledStatement.Label && body == labeledStatement.Body)
        {
            return labeledStatement;
        }

        return new LabeledStatement(label, body);
    }

    protected virtual IfStatement UpdateIfStatement(IfStatement ifStatement, Expression test, Statement consequent,
        Statement? alternate)
    {
        if (test == ifStatement.Test && consequent == ifStatement.Consequent && alternate == ifStatement.Alternate)
        {
            return ifStatement;
        }

        return new IfStatement(test, consequent, alternate);
    }

    protected virtual EmptyStatement UpdateEmptyStatement(EmptyStatement emptyStatement)
    {
        return emptyStatement;
    }

    protected virtual DebuggerStatement UpdateDebuggerStatement(DebuggerStatement debuggerStatement)
    {
        return debuggerStatement;
    }

    protected virtual ExpressionStatement UpdateExpressionStatement(ExpressionStatement expressionStatement,
        Expression expression)
    {
        if (expression == expressionStatement.Expression)
        {
            return expressionStatement;
        }

        return new ExpressionStatement(expression);
    }

    protected virtual ForStatement UpdateForStatement(ForStatement forStatement, StatementListItem? init,
        Expression? test,
        Expression? update, Statement body)
    {
        if (init == forStatement.Init && test == forStatement.Test && update == forStatement.Update &&
            body == forStatement.Body)
        {
            return forStatement;
        }

        return new ForStatement(init, test, update, body);
    }

    protected virtual ForInStatement UpdateForInStatement(ForInStatement forInStatement, Node left, Expression right,
        Statement body)
    {
        if (left == forInStatement.Left && right == forInStatement.Right && body == forInStatement.Body)
        {
            return forInStatement;
        }

        return new ForInStatement(left, right, body);
    }

    protected virtual DoWhileStatement UpdateDoWhileStatement(DoWhileStatement doWhileStatement, Statement body,
        Expression test)
    {
        if (body == doWhileStatement.Body && test == doWhileStatement.Test)
        {
            return doWhileStatement;
        }

        return new DoWhileStatement(body, test);
    }

    protected virtual ArrowFunctionExpression UpdateArrowFunctionExpression(
        ArrowFunctionExpression arrowFunctionExpression,
        bool isNewParameters, ref NodeList<Expression> parameters, Node body)
    {
        if (!isNewParameters && body == arrowFunctionExpression.Body)
        {
            return arrowFunctionExpression;
        }

        return new ArrowFunctionExpression(parameters, body, arrowFunctionExpression.Expression,
            arrowFunctionExpression.Strict, arrowFunctionExpression.Async);
    }

    protected virtual UnaryExpression UpdateUnaryExpression(UnaryExpression unaryExpression, Expression argument)
    {
        if (argument == unaryExpression.Argument)
        {
            return unaryExpression;
        }

        return new UnaryExpression(unaryExpression.Operator, argument);
    }

    protected virtual UpdateExpression UpdateUpdateExpression(UpdateExpression updateExpression, Expression argument)
    {
        if (argument == updateExpression.Argument)
        {
            return updateExpression;
        }

        return new UpdateExpression(updateExpression.Operator, argument, updateExpression.Prefix);
    }

    protected virtual ThisExpression UpdateThisExpression(ThisExpression thisExpression)
    {
        return thisExpression;
    }

    protected virtual SequenceExpression UpdateSequenceExpression(SequenceExpression sequenceExpression,
        bool isNewExpressions,
        ref NodeList<Expression> expressions)
    {
        if (isNewExpressions)
        {
            return new SequenceExpression(expressions);
        }

        return sequenceExpression;
    }

    protected virtual ObjectExpression UpdateObjectExpression(ObjectExpression objectExpression, bool isNewProperties,
        ref NodeList<Expression> properties)
    {
        if (isNewProperties)
        {
            return new ObjectExpression(properties);
        }

        return objectExpression;
    }

    protected virtual NewExpression UpdateNewExpression(NewExpression newExpression, Expression callee,
        bool isNewArguments,
        ref NodeList<Expression> arguments)
    {
        if (!isNewArguments && callee == newExpression.Callee)
        {
            return newExpression;
        }

        return new NewExpression(callee, arguments);
    }

    protected virtual MemberExpression UpdateMemberExpression(MemberExpression memberExpression, Expression obj,
        Expression property)
    {
        if (obj == memberExpression.Object && property == memberExpression.Property)
        {
            return memberExpression;
        }

        return memberExpression.Computed switch
        {
            true => new ComputedMemberExpression(obj, property, memberExpression.Optional),
            false => new StaticMemberExpression(obj, property, memberExpression.Optional),
        };
    }

    protected virtual BinaryExpression UpdateLogicalExpression(BinaryExpression binaryExpression, Expression left,
        Expression right)
    {
        if (left == binaryExpression.Left && right == binaryExpression.Right)
        {
            return binaryExpression;
        }

        return new BinaryExpression(binaryExpression.Operator, left, right);
    }

    protected virtual Literal UpdateLiteral(Literal literal)
    {
        return literal;
    }

    protected virtual Identifier UpdateIdentifier(Identifier identifier)
    {
        return identifier;
    }

    protected virtual PrivateIdentifier UpdatePrivateIdentifier(PrivateIdentifier privateIdentifier)
    {
        return privateIdentifier;
    }

    protected virtual IFunction UpdateFunctionExpression(IFunction function, Identifier? id, bool isNewParameters,
        ref NodeList<Expression> parameters,
        Node body)
    {
        if (id == function.Id && !isNewParameters && body == function.Body)
        {
            return function;
        }

        return function switch
        {
            ArrowFunctionExpression => new ArrowFunctionExpression(parameters, body, function.Expression,
                function.Strict, function.Async),
            FunctionDeclaration => new FunctionDeclaration(id, parameters, (body as BlockStatement) !,
                function.Generator,
                function.Strict, function.Async),
            FunctionExpression => new FunctionExpression(id, parameters, (body as BlockStatement) !, function.Generator,
                function.Strict, function.Async),
            _ => throw new NotImplementedException($"{function.GetType().Name} does not implemented yet.")
        };
    }

    protected virtual PropertyDefinition UpdatePropertyDefinition(PropertyDefinition propertyDefinition,
        Expression key, Expression? value)
    {
        if (key == propertyDefinition.Key && value == propertyDefinition.Value)
        {
            return propertyDefinition;
        }

        return new PropertyDefinition(key, propertyDefinition.Computed, value !, propertyDefinition.Static);
    }

    protected virtual ChainExpression UpdateChainExpression(ChainExpression chainExpression, Expression expression)
    {
        if (expression == chainExpression.Expression)
        {
            return chainExpression;
        }

        return new ChainExpression(expression);
    }

    protected virtual ClassExpression UpdateClassExpression(ClassExpression classExpression, Identifier? id,
        Expression? superClass,
        ClassBody body)
    {
        if (id == classExpression.Id && superClass == classExpression.SuperClass && body == classExpression.Body)
        {
            return classExpression;
        }

        return new ClassExpression(id, superClass, body);
    }

    protected virtual ExportDefaultDeclaration UpdateExportDefaultDeclaration(
        ExportDefaultDeclaration exportDefaultDeclaration,
        StatementListItem declaration)
    {
        if (declaration == exportDefaultDeclaration.Declaration)
        {
            return exportDefaultDeclaration;
        }

        return new ExportDefaultDeclaration(declaration);
    }

    protected virtual ExportAllDeclaration UpdateExportAllDeclaration(ExportAllDeclaration exportAllDeclaration,
        Expression? exported,
        Literal source)
    {
        if (exported == exportAllDeclaration.Exported && source == exportAllDeclaration.Source)
        {
            return exportAllDeclaration;
        }

        return new ExportAllDeclaration(source, exported);
    }

    protected virtual ExportNamedDeclaration UpdateExportNamedDeclaration(
        ExportNamedDeclaration exportNamedDeclaration,
        StatementListItem? declaration, bool isNewSpecifiers, ref NodeList<ExportSpecifier> specifiers, Literal? source)
    {
        if (declaration == exportNamedDeclaration.Declaration && !isNewSpecifiers &&
            source == exportNamedDeclaration.Source)
        {
            return exportNamedDeclaration;
        }

        return new ExportNamedDeclaration(declaration, specifiers, source);
    }

    protected virtual ExportSpecifier UpdateExportSpecifier(ExportSpecifier exportSpecifier, Expression local,
        Expression exported)
    {
        if (local == exportSpecifier.Local && exported == exportSpecifier.Exported)
        {
            return exportSpecifier;
        }

        return new ExportSpecifier(local, exported);
    }

    protected virtual Import UpdateImport(Import import, Expression? source)
    {
        if (source == import.Source)
        {
            return import;
        }

        return new Import(source);
    }

    protected virtual ImportDeclaration UpdateImportDeclaration(ImportDeclaration importDeclaration,
        bool isNewSpecifiers,
        ref NodeList<ImportDeclarationSpecifier> specifiers, Literal source)
    {
        if (!isNewSpecifiers && source == importDeclaration.Source)
        {
            return importDeclaration;
        }

        return new ImportDeclaration(specifiers, source);
    }

    protected virtual ImportNamespaceSpecifier UpdateImportNamespaceSpecifier(
        ImportNamespaceSpecifier importNamespaceSpecifier,
        Identifier local)
    {
        if (local == importNamespaceSpecifier.Local)
        {
            return importNamespaceSpecifier;
        }

        return new ImportNamespaceSpecifier(local);
    }

    protected virtual ImportDefaultSpecifier UpdateImportDefaultSpecifier(
        ImportDefaultSpecifier importDefaultSpecifier, Identifier local)
    {
        if (local == importDefaultSpecifier.Local)
        {
            return importDefaultSpecifier;
        }

        return new ImportDefaultSpecifier(local);
    }

    protected virtual ImportSpecifier UpdateImportSpecifier(ImportSpecifier importSpecifier, Expression imported,
        Identifier local)
    {
        if (imported == importSpecifier.Imported && local == importSpecifier.Local)
        {
            return importSpecifier;
        }

        return new ImportSpecifier(local, imported);
    }

    protected virtual MethodDefinition UpdateMethodDefinition(MethodDefinition methodDefinition, Expression key,
        Expression value)
    {
        if (key == methodDefinition.Key && value == methodDefinition.Value)
        {
            return methodDefinition;
        }

        return new MethodDefinition(key, methodDefinition.Computed, (value as FunctionExpression)!,
            methodDefinition.Kind,
            methodDefinition.Static);
    }

    protected virtual ForOfStatement UpdateForOfStatement(ForOfStatement forOfStatement, Node left, Expression right,
        Statement body)
    {
        if (left == forOfStatement.Left && right == forOfStatement.Right && body == forOfStatement.Body)
        {
            return forOfStatement;
        }

        return new ForOfStatement(left, right, body, forOfStatement.Await);
    }

    protected virtual ClassDeclaration UpdateClassDeclaration(ClassDeclaration classDeclaration, Identifier? id,
        Expression? superClass,
        ClassBody body)
    {
        if (id == classDeclaration.Id && superClass == classDeclaration.SuperClass && body == classDeclaration.Body)
        {
            return classDeclaration;
        }

        return new ClassDeclaration(id, superClass, body);
    }

    protected virtual ClassBody UpdateClassBody(ClassBody classBody, bool isNewBody, ref NodeList<Node> body)
    {
        if (isNewBody)
        {
            return new ClassBody(body);
        }

        return classBody;
    }

    protected virtual YieldExpression UpdateYieldExpression(YieldExpression yieldExpression, Expression? argument)
    {
        if (argument == yieldExpression.Argument)
        {
            return yieldExpression;
        }

        return new YieldExpression(argument, yieldExpression.Delegate);
    }

    protected virtual TaggedTemplateExpression UpdateTaggedTemplateExpression(
        TaggedTemplateExpression taggedTemplateExpression,
        Expression tag, TemplateLiteral quasi)
    {
        if (tag == taggedTemplateExpression.Tag && quasi == taggedTemplateExpression.Quasi)
        {
            return taggedTemplateExpression;
        }

        return new TaggedTemplateExpression(tag, quasi);
    }

    protected virtual Super UpdateSuper(Super super)
    {
        return super;
    }

    protected virtual MetaProperty UpdateMetaProperty(MetaProperty metaProperty, Identifier meta, Identifier property)
    {
        if (meta == metaProperty.Meta && property == metaProperty.Property)
        {
            return metaProperty;
        }

        return new MetaProperty(meta, property);
    }

    protected virtual ArrowParameterPlaceHolder UpdateArrowParameterPlaceHolder(
        ArrowParameterPlaceHolder arrowParameterPlaceHolder)
    {
        return arrowParameterPlaceHolder;
    }

    protected virtual ObjectPattern UpdateObjectPattern(ObjectPattern objectPattern, bool isNewProperties,
        ref NodeList<Node> properties)
    {
        if (isNewProperties)
        {
            return new ObjectPattern(properties);
        }

        return objectPattern;
    }

    protected virtual SpreadElement UpdateSpreadElement(SpreadElement spreadElement, Expression argument)
    {
        if (argument == spreadElement.Argument)
        {
            return spreadElement;
        }

        return new SpreadElement(argument);
    }

    protected virtual AssignmentPattern UpdateAssignmentPattern(AssignmentPattern assignmentPattern, Expression left,
        Expression right)
    {
        if (left == assignmentPattern.Left && right == assignmentPattern.Right)
        {
            return assignmentPattern;
        }

        return new AssignmentPattern(left, right);
    }

    protected virtual ArrayPattern UpdateArrayPattern(ArrayPattern arrayPattern, bool isNewElements,
        ref NodeList<Expression?> elements)
    {
        if (isNewElements)
        {
            return new ArrayPattern(elements);
        }

        return arrayPattern;
    }

    protected virtual VariableDeclarator UpdateVariableDeclarator(VariableDeclarator variableDeclarator, Expression id,
        Expression? init)
    {
        if (id == variableDeclarator.Id && init == variableDeclarator.Init)
        {
            return variableDeclarator;
        }

        return new VariableDeclarator(id, init);
    }

    protected virtual TemplateLiteral UpdateTemplateLiteral(TemplateLiteral templateLiteral, bool isNewQuasis, ref NodeList<TemplateElement> quasis, bool isNewExpression, ref NodeList<Expression> expressions)
    {
        if (!isNewQuasis && !isNewExpression)
        {
            return templateLiteral;
        }
        
        return new TemplateLiteral(quasis, expressions);
    }

    protected virtual TemplateElement UpdateTemplateElement(TemplateElement templateElement)
    {
        return templateElement;
    }

    protected virtual RestElement UpdateRestElement(RestElement restElement, Expression argument)
    {
        if (argument == restElement.Argument)
        {
            return restElement;
        }

        return new RestElement(argument);
    }

    protected virtual Property UpdateProperty(Property property, Expression key, Expression value)
    {
        if (key == property.Key && value == property.Value)
        {
            return property;
        }

        return new Property(property.Kind, key, property.Computed, value, property.Method, property.Shorthand);
    }

    protected virtual AwaitExpression UpdateAwaitExpression(AwaitExpression awaitExpression, Expression argument)
    {
        if (argument == awaitExpression.Argument)
        {
            return awaitExpression;
        }

        return new AwaitExpression(argument);
    }

    protected virtual ConditionalExpression UpdateConditionalExpression(ConditionalExpression conditionalExpression,
        Expression test,
        Expression consequent, Expression alternate)
    {
        if (test == conditionalExpression.Test && consequent == conditionalExpression.Consequent &&
            alternate == conditionalExpression.Alternate)
        {
            return conditionalExpression;
        }

        return new ConditionalExpression(test, consequent, alternate);
    }

    protected virtual CallExpression UpdateCallExpression(CallExpression callExpression, Expression callee,
        bool isNewArguments,
        ref NodeList<Expression> arguments)
    {
        if (!isNewArguments && callee == callExpression.Callee)
        {
            return callExpression;
        }

        return new CallExpression(callee, arguments, callExpression.Optional);
    }

    protected virtual BinaryExpression UpdateBinaryExpression(BinaryExpression binaryExpression, Expression left,
        Expression right)
    {
        if (left == binaryExpression.Left && right == binaryExpression.Right)
        {
            return binaryExpression;
        }

        return new BinaryExpression(binaryExpression.Operator, left, right);
    }

    protected virtual ArrayExpression UpdateArrayExpression(ArrayExpression arrayExpression, bool isNewElements,
        ref NodeList<Expression?> elements)
    {
        if (isNewElements)
        {
            return new ArrayExpression(elements);
        }

        return arrayExpression;
    }

    protected virtual AssignmentExpression UpdateAssignmentExpression(AssignmentExpression assignmentExpression,
        Expression left,
        Expression right)
    {
        if (left == assignmentExpression.Left && right == assignmentExpression.Right)
        {
            return assignmentExpression;
        }

        return new AssignmentExpression(assignmentExpression.Operator, left, right);
    }

    protected virtual ContinueStatement UpdateContinueStatement(ContinueStatement continueStatement, Identifier? label)
    {
        if (label != continueStatement.Label)
        {
            return new ContinueStatement(label);
        }

        return continueStatement;
    }

    protected virtual BreakStatement UpdateBreakStatement(BreakStatement breakStatement, Identifier? label)
    {
        if (label != breakStatement.Label)
        {
            return new BreakStatement(label);
        }

        return breakStatement;
    }

    protected virtual BlockStatement UpdateBlockStatement(BlockStatement blockStatement, bool isNewBody,
        ref NodeList<Statement> body)
    {
        if (isNewBody)
        {
            return new BlockStatement(body);
        }

        return blockStatement;
    }

    #endregion
}
