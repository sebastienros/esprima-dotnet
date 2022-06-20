using Esprima.Ast;

namespace Esprima.Utils;

/// <summary>
/// An AST visitor that raises events before and after visiting each node
/// and its descendants.
/// </summary>
public class AstVisitorEventSource : AstVisitor
{
    public event EventHandler<ArrayExpression>? VisitingArrayExpression;
    public event EventHandler<ArrayExpression>? VisitedArrayExpression;
    public event EventHandler<ArrayPattern>? VisitedArrayPattern;
    public event EventHandler<ArrayPattern>? VisitingArrayPattern;
    public event EventHandler<ArrowFunctionExpression>? VisitedArrowFunctionExpression;
    public event EventHandler<ArrowFunctionExpression>? VisitingArrowFunctionExpression;
    public event EventHandler<AssignmentExpression>? VisitedAssignmentExpression;
    public event EventHandler<AssignmentExpression>? VisitingAssignmentExpression;
    public event EventHandler<AssignmentPattern>? VisitedAssignmentPattern;
    public event EventHandler<AssignmentPattern>? VisitingAssignmentPattern;
    public event EventHandler<AwaitExpression>? VisitingAwaitExpression;
    public event EventHandler<AwaitExpression>? VisitedAwaitExpression;
    public event EventHandler<BinaryExpression>? VisitingBinaryExpression;
    public event EventHandler<BinaryExpression>? VisitedBinaryExpression;
    public event EventHandler<BlockStatement>? VisitingBlockStatement;
    public event EventHandler<BlockStatement>? VisitedBlockStatement;
    public event EventHandler<BreakStatement>? VisitingBreakStatement;
    public event EventHandler<BreakStatement>? VisitedBreakStatement;
    public event EventHandler<CallExpression>? VisitingCallExpression;
    public event EventHandler<CallExpression>? VisitedCallExpression;
    public event EventHandler<CatchClause>? VisitingCatchClause;
    public event EventHandler<CatchClause>? VisitedCatchClause;
    public event EventHandler<ChainExpression>? VisitingChainExpression;
    public event EventHandler<ChainExpression>? VisitedChainExpression;
    public event EventHandler<ClassBody>? VisitingClassBody;
    public event EventHandler<ClassBody>? VisitedClassBody;
    public event EventHandler<ClassDeclaration>? VisitingClassDeclaration;
    public event EventHandler<ClassDeclaration>? VisitedClassDeclaration;
    public event EventHandler<ClassExpression>? VisitingClassExpression;
    public event EventHandler<ClassExpression>? VisitedClassExpression;
    public event EventHandler<ConditionalExpression>? VisitingConditionalExpression;
    public event EventHandler<ConditionalExpression>? VisitedConditionalExpression;
    public event EventHandler<ContinueStatement>? VisitingContinueStatement;
    public event EventHandler<ContinueStatement>? VisitedContinueStatement;
    public event EventHandler<DebuggerStatement>? VisitingDebuggerStatement;
    public event EventHandler<DebuggerStatement>? VisitedDebuggerStatement;
    public event EventHandler<Decorator>? VisitingDecorator;
    public event EventHandler<Decorator>? VisitedDecorator;
    public event EventHandler<DoWhileStatement>? VisitingDoWhileStatement;
    public event EventHandler<DoWhileStatement>? VisitedDoWhileStatement;
    public event EventHandler<EmptyStatement>? VisitingEmptyStatement;
    public event EventHandler<EmptyStatement>? VisitedEmptyStatement;
    public event EventHandler<ExportAllDeclaration>? VisitingExportAllDeclaration;
    public event EventHandler<ExportAllDeclaration>? VisitedExportAllDeclaration;
    public event EventHandler<ExportDefaultDeclaration>? VisitingExportDefaultDeclaration;
    public event EventHandler<ExportDefaultDeclaration>? VisitedExportDefaultDeclaration;
    public event EventHandler<ExportNamedDeclaration>? VisitingExportNamedDeclaration;
    public event EventHandler<ExportNamedDeclaration>? VisitedExportNamedDeclaration;
    public event EventHandler<ExportSpecifier>? VisitingExportSpecifier;
    public event EventHandler<ExportSpecifier>? VisitedExportSpecifier;
    public event EventHandler<ExpressionStatement>? VisitingExpressionStatement;
    public event EventHandler<ExpressionStatement>? VisitedExpressionStatement;
    public event EventHandler<ForInStatement>? VisitingForInStatement;
    public event EventHandler<ForInStatement>? VisitedForInStatement;
    public event EventHandler<ForOfStatement>? VisitingForOfStatement;
    public event EventHandler<ForOfStatement>? VisitedForOfStatement;
    public event EventHandler<ForStatement>? VisitingForStatement;
    public event EventHandler<ForStatement>? VisitedForStatement;
    public event EventHandler<FunctionDeclaration>? VisitingFunctionDeclaration;
    public event EventHandler<FunctionDeclaration>? VisitedFunctionDeclaration;
    public event EventHandler<FunctionExpression>? VisitingFunctionExpression;
    public event EventHandler<FunctionExpression>? VisitedFunctionExpression;
    public event EventHandler<Identifier>? VisitingIdentifier;
    public event EventHandler<Identifier>? VisitedIdentifier;
    public event EventHandler<IfStatement>? VisitingIfStatement;
    public event EventHandler<IfStatement>? VisitedIfStatement;
    public event EventHandler<Import>? VisitingImport;
    public event EventHandler<Import>? VisitedImport;
    public event EventHandler<ImportDeclaration>? VisitingImportDeclaration;
    public event EventHandler<ImportDeclaration>? VisitedImportDeclaration;
    public event EventHandler<ImportDefaultSpecifier>? VisitingImportDefaultSpecifier;
    public event EventHandler<ImportDefaultSpecifier>? VisitedImportDefaultSpecifier;
    public event EventHandler<ImportNamespaceSpecifier>? VisitingImportNamespaceSpecifier;
    public event EventHandler<ImportNamespaceSpecifier>? VisitedImportNamespaceSpecifier;
    public event EventHandler<ImportSpecifier>? VisitingImportSpecifier;
    public event EventHandler<ImportSpecifier>? VisitedImportSpecifier;
    public event EventHandler<LabeledStatement>? VisitingLabeledStatement;
    public event EventHandler<LabeledStatement>? VisitedLabeledStatement;
    public event EventHandler<Literal>? VisitingLiteral;
    public event EventHandler<Literal>? VisitedLiteral;
    public event EventHandler<MemberExpression>? VisitingMemberExpression;
    public event EventHandler<MemberExpression>? VisitedMemberExpression;
    public event EventHandler<MetaProperty>? VisitingMetaProperty;
    public event EventHandler<MetaProperty>? VisitedMetaProperty;
    public event EventHandler<MethodDefinition>? VisitingMethodDefinition;
    public event EventHandler<MethodDefinition>? VisitedMethodDefinition;
    public event EventHandler<NewExpression>? VisitingNewExpression;
    public event EventHandler<NewExpression>? VisitedNewExpression;
    public event EventHandler<Node>? VisitingNode;
    public event EventHandler<Node>? VisitedNode;
    public event EventHandler<ObjectExpression>? VisitingObjectExpression;
    public event EventHandler<ObjectExpression>? VisitedObjectExpression;
    public event EventHandler<ObjectPattern>? VisitingObjectPattern;
    public event EventHandler<ObjectPattern>? VisitedObjectPattern;
    public event EventHandler<PrivateIdentifier>? VisitingPrivateIdentifier;
    public event EventHandler<PrivateIdentifier>? VisitedPrivateIdentifier;
    public event EventHandler<Program>? VisitingProgram;
    public event EventHandler<Program>? VisitedProgram;
    public event EventHandler<Property>? VisitingProperty;
    public event EventHandler<Property>? VisitedProperty;
    public event EventHandler<PropertyDefinition>? VisitingPropertyDefinition;
    public event EventHandler<PropertyDefinition>? VisitedPropertyDefinition;
    public event EventHandler<RestElement>? VisitingRestElement;
    public event EventHandler<RestElement>? VisitedRestElement;
    public event EventHandler<ReturnStatement>? VisitingReturnStatement;
    public event EventHandler<ReturnStatement>? VisitedReturnStatement;
    public event EventHandler<SequenceExpression>? VisitingSequenceExpression;
    public event EventHandler<SequenceExpression>? VisitedSequenceExpression;
    public event EventHandler<SpreadElement>? VisitingSpreadElement;
    public event EventHandler<SpreadElement>? VisitedSpreadElement;
    public event EventHandler<StaticBlock>? VisitingStaticBlock;
    public event EventHandler<StaticBlock>? VisitedStaticBlock;
    public event EventHandler<Super>? VisitingSuper;
    public event EventHandler<Super>? VisitedSuper;
    public event EventHandler<SwitchCase>? VisitingSwitchCase;
    public event EventHandler<SwitchCase>? VisitedSwitchCase;
    public event EventHandler<SwitchStatement>? VisitingSwitchStatement;
    public event EventHandler<SwitchStatement>? VisitedSwitchStatement;
    public event EventHandler<TaggedTemplateExpression>? VisitingTaggedTemplateExpression;
    public event EventHandler<TaggedTemplateExpression>? VisitedTaggedTemplateExpression;
    public event EventHandler<TemplateElement>? VisitingTemplateElement;
    public event EventHandler<TemplateElement>? VisitedTemplateElement;
    public event EventHandler<TemplateLiteral>? VisitingTemplateLiteral;
    public event EventHandler<TemplateLiteral>? VisitedTemplateLiteral;
    public event EventHandler<ThisExpression>? VisitingThisExpression;
    public event EventHandler<ThisExpression>? VisitedThisExpression;
    public event EventHandler<ThrowStatement>? VisitingThrowStatement;
    public event EventHandler<ThrowStatement>? VisitedThrowStatement;
    public event EventHandler<TryStatement>? VisitingTryStatement;
    public event EventHandler<TryStatement>? VisitedTryStatement;
    public event EventHandler<UnaryExpression>? VisitingUnaryExpression;
    public event EventHandler<UnaryExpression>? VisitedUnaryExpression;
    public event EventHandler<VariableDeclaration>? VisitingVariableDeclaration;
    public event EventHandler<VariableDeclaration>? VisitedVariableDeclaration;
    public event EventHandler<VariableDeclarator>? VisitingVariableDeclarator;
    public event EventHandler<VariableDeclarator>? VisitedVariableDeclarator;
    public event EventHandler<WhileStatement>? VisitingWhileStatement;
    public event EventHandler<WhileStatement>? VisitedWhileStatement;
    public event EventHandler<WithStatement>? VisitingWithStatement;
    public event EventHandler<WithStatement>? VisitedWithStatement;
    public event EventHandler<YieldExpression>? VisitingYieldExpression;
    public event EventHandler<YieldExpression>? VisitedYieldExpression;

    public override object? Visit(Node node, object? context = null)
    {
        VisitingNode?.Invoke(this, node);
        var result = base.Visit(node, context);
        VisitedNode?.Invoke(this, node);
        return result;
    }

    protected internal override object? VisitArrayExpression(ArrayExpression arrayExpression, object? context)
    {
        VisitingArrayExpression?.Invoke(this, arrayExpression);
        var result = base.VisitArrayExpression(arrayExpression, context);
        VisitedArrayExpression?.Invoke(this, arrayExpression);
        return result;
    }

    protected internal override object? VisitArrayPattern(ArrayPattern arrayPattern, object? context)
    {
        VisitingArrayPattern?.Invoke(this, arrayPattern);
        var result = base.VisitArrayPattern(arrayPattern, context);
        VisitedArrayPattern?.Invoke(this, arrayPattern);
        return result;
    }

    protected internal override object? VisitArrowFunctionExpression(ArrowFunctionExpression arrowFunctionExpression, object? context)
    {
        VisitingArrowFunctionExpression?.Invoke(this, arrowFunctionExpression);
        var result = base.VisitArrowFunctionExpression(arrowFunctionExpression, context);
        VisitedArrowFunctionExpression?.Invoke(this, arrowFunctionExpression);
        return result;
    }

    protected internal override object? VisitAssignmentExpression(AssignmentExpression assignmentExpression, object? context)
    {
        VisitingAssignmentExpression?.Invoke(this, assignmentExpression);
        var result = base.VisitAssignmentExpression(assignmentExpression, context);
        VisitedAssignmentExpression?.Invoke(this, assignmentExpression);
        return result;
    }

    protected internal override object? VisitAssignmentPattern(AssignmentPattern assignmentPattern, object? context)
    {
        VisitingAssignmentPattern?.Invoke(this, assignmentPattern);
        var result = base.VisitAssignmentPattern(assignmentPattern, context);
        VisitedAssignmentPattern?.Invoke(this, assignmentPattern);
        return result;
    }

    protected internal override object? VisitAwaitExpression(AwaitExpression awaitExpression, object? context)
    {
        VisitingAwaitExpression?.Invoke(this, awaitExpression);
        var result = base.VisitAwaitExpression(awaitExpression, context);
        VisitedAwaitExpression?.Invoke(this, awaitExpression);
        return result;
    }

    protected internal override object? VisitBinaryExpression(BinaryExpression binaryExpression, object? context)
    {
        VisitingBinaryExpression?.Invoke(this, binaryExpression);
        var result = base.VisitBinaryExpression(binaryExpression, context);
        VisitedBinaryExpression?.Invoke(this, binaryExpression);
        return result;
    }

    protected internal override object? VisitBlockStatement(BlockStatement blockStatement, object? context)
    {
        VisitingBlockStatement?.Invoke(this, blockStatement);
        var result = base.VisitBlockStatement(blockStatement, context);
        VisitedBlockStatement?.Invoke(this, blockStatement);
        return result;
    }

    protected internal override object? VisitBreakStatement(BreakStatement breakStatement, object? context)
    {
        VisitingBreakStatement?.Invoke(this, breakStatement);
        var result = base.VisitBreakStatement(breakStatement, context);
        VisitedBreakStatement?.Invoke(this, breakStatement);
        return result;
    }

    protected internal override object? VisitCallExpression(CallExpression callExpression, object? context)
    {
        VisitingCallExpression?.Invoke(this, callExpression);
        var result = base.VisitCallExpression(callExpression, context);
        VisitedCallExpression?.Invoke(this, callExpression);
        return result;
    }

    protected internal override object? VisitCatchClause(CatchClause catchClause, object? context)
    {
        VisitingCatchClause?.Invoke(this, catchClause);
        var result = base.VisitCatchClause(catchClause, context);
        VisitedCatchClause?.Invoke(this, catchClause);
        return result;
    }

    protected internal override object? VisitChainExpression(ChainExpression chainExpression, object? context)
    {
        VisitingChainExpression?.Invoke(this, chainExpression);
        var result = base.VisitChainExpression(chainExpression, context);
        VisitedChainExpression?.Invoke(this, chainExpression);
        return result;
    }

    protected internal override object? VisitClassBody(ClassBody classBody, object? context)
    {
        VisitingClassBody?.Invoke(this, classBody);
        var result = base.VisitClassBody(classBody, context);
        VisitedClassBody?.Invoke(this, classBody);
        return result;
    }

    protected internal override object? VisitClassDeclaration(ClassDeclaration classDeclaration, object? context)
    {
        VisitingClassDeclaration?.Invoke(this, classDeclaration);
        var result = base.VisitClassDeclaration(classDeclaration, context);
        VisitedClassDeclaration?.Invoke(this, classDeclaration);
        return result;
    }

    protected internal override object? VisitClassExpression(ClassExpression classExpression, object? context)
    {
        VisitingClassExpression?.Invoke(this, classExpression);
        var result = base.VisitClassExpression(classExpression, context);
        VisitedClassExpression?.Invoke(this, classExpression);
        return result;
    }

    protected internal override object? VisitConditionalExpression(ConditionalExpression conditionalExpression, object? context)
    {
        VisitingConditionalExpression?.Invoke(this, conditionalExpression);
        var result = base.VisitConditionalExpression(conditionalExpression, context);
        VisitedConditionalExpression?.Invoke(this, conditionalExpression);
        return result;
    }

    protected internal override object? VisitContinueStatement(ContinueStatement continueStatement, object? context)
    {
        VisitingContinueStatement?.Invoke(this, continueStatement);
        var result = base.VisitContinueStatement(continueStatement, context);
        VisitedContinueStatement?.Invoke(this, continueStatement);
        return result;
    }

    protected internal override object? VisitDebuggerStatement(DebuggerStatement debuggerStatement, object? context)
    {
        VisitingDebuggerStatement?.Invoke(this, debuggerStatement);
        var result = base.VisitDebuggerStatement(debuggerStatement, context);
        VisitedDebuggerStatement?.Invoke(this, debuggerStatement);
        return result;
    }

    protected internal override object? VisitDecorator(Decorator decorator, object? context)
    {
        VisitingDecorator?.Invoke(this, decorator);
        var result = base.VisitDecorator(decorator, context);
        VisitedDecorator?.Invoke(this, decorator);
        return result;
    }

    protected internal override object? VisitDoWhileStatement(DoWhileStatement doWhileStatement, object? context)
    {
        VisitingDoWhileStatement?.Invoke(this, doWhileStatement);
        var result = base.VisitDoWhileStatement(doWhileStatement, context);
        VisitedDoWhileStatement?.Invoke(this, doWhileStatement);
        return result;
    }

    protected internal override object? VisitEmptyStatement(EmptyStatement emptyStatement, object? context)
    {
        VisitingEmptyStatement?.Invoke(this, emptyStatement);
        var result = base.VisitEmptyStatement(emptyStatement, context);
        VisitedEmptyStatement?.Invoke(this, emptyStatement);
        return result;
    }

    protected internal override object? VisitExportAllDeclaration(ExportAllDeclaration exportAllDeclaration, object? context)
    {
        VisitingExportAllDeclaration?.Invoke(this, exportAllDeclaration);
        var result = base.VisitExportAllDeclaration(exportAllDeclaration, context);
        VisitedExportAllDeclaration?.Invoke(this, exportAllDeclaration);
        return result;
    }

    protected internal override object? VisitExportDefaultDeclaration(ExportDefaultDeclaration exportDefaultDeclaration, object? context)
    {
        VisitingExportDefaultDeclaration?.Invoke(this, exportDefaultDeclaration);
        var result = base.VisitExportDefaultDeclaration(exportDefaultDeclaration, context);
        VisitedExportDefaultDeclaration?.Invoke(this, exportDefaultDeclaration);
        return result;
    }

    protected internal override object? VisitExportNamedDeclaration(ExportNamedDeclaration exportNamedDeclaration, object? context)
    {
        VisitingExportNamedDeclaration?.Invoke(this, exportNamedDeclaration);
        var result = base.VisitExportNamedDeclaration(exportNamedDeclaration, context);
        VisitedExportNamedDeclaration?.Invoke(this, exportNamedDeclaration);
        return result;
    }

    protected internal override object? VisitExportSpecifier(ExportSpecifier exportSpecifier, object? context)
    {
        VisitingExportSpecifier?.Invoke(this, exportSpecifier);
        var result = base.VisitExportSpecifier(exportSpecifier, context);
        VisitedExportSpecifier?.Invoke(this, exportSpecifier);
        return result;
    }

    protected internal override object? VisitExpressionStatement(ExpressionStatement expressionStatement, object? context)
    {
        VisitingExpressionStatement?.Invoke(this, expressionStatement);
        var result = base.VisitExpressionStatement(expressionStatement, context);
        VisitedExpressionStatement?.Invoke(this, expressionStatement);
        return result;
    }

    protected internal override object? VisitForInStatement(ForInStatement forInStatement, object? context)
    {
        VisitingForInStatement?.Invoke(this, forInStatement);
        var result = base.VisitForInStatement(forInStatement, context);
        VisitedForInStatement?.Invoke(this, forInStatement);
        return result;
    }

    protected internal override object? VisitForOfStatement(ForOfStatement forOfStatement, object? context)
    {
        VisitingForOfStatement?.Invoke(this, forOfStatement);
        var result = base.VisitForOfStatement(forOfStatement, context);
        VisitedForOfStatement?.Invoke(this, forOfStatement);
        return result;
    }

    protected internal override object? VisitForStatement(ForStatement forStatement, object? context)
    {
        VisitingForStatement?.Invoke(this, forStatement);
        var result = base.VisitForStatement(forStatement, context);
        VisitedForStatement?.Invoke(this, forStatement);
        return result;
    }

    protected internal override object? VisitFunctionDeclaration(FunctionDeclaration functionDeclaration, object? context)
    {
        VisitingFunctionDeclaration?.Invoke(this, functionDeclaration);
        var result = base.VisitFunctionDeclaration(functionDeclaration, context);
        VisitedFunctionDeclaration?.Invoke(this, functionDeclaration);
        return result;
    }

    protected internal override object? VisitFunctionExpression(FunctionExpression functionExpression, object? context)
    {
        VisitingFunctionExpression?.Invoke(this, functionExpression);
        var result = base.VisitFunctionExpression(functionExpression, context);
        VisitedFunctionExpression?.Invoke(this, functionExpression);
        return result;
    }

    protected internal override object? VisitIdentifier(Identifier identifier, object? context)
    {
        VisitingIdentifier?.Invoke(this, identifier);
        var result = base.VisitIdentifier(identifier, context);
        VisitedIdentifier?.Invoke(this, identifier);
        return result;
    }

    protected internal override object? VisitIfStatement(IfStatement ifStatement, object? context)
    {
        VisitingIfStatement?.Invoke(this, ifStatement);
        var result = base.VisitIfStatement(ifStatement, context);
        VisitedIfStatement?.Invoke(this, ifStatement);
        return result;
    }

    protected internal override object? VisitImport(Import import, object? context)
    {
        VisitingImport?.Invoke(this, import);
        var result = base.VisitImport(import, context);
        VisitedImport?.Invoke(this, import);
        return result;
    }

    protected internal override object? VisitImportDeclaration(ImportDeclaration importDeclaration, object? context)
    {
        VisitingImportDeclaration?.Invoke(this, importDeclaration);
        var result = base.VisitImportDeclaration(importDeclaration, context);
        VisitedImportDeclaration?.Invoke(this, importDeclaration);
        return result;
    }

    protected internal override object? VisitImportDefaultSpecifier(ImportDefaultSpecifier importDefaultSpecifier, object? context)
    {
        VisitingImportDefaultSpecifier?.Invoke(this, importDefaultSpecifier);
        var result = base.VisitImportDefaultSpecifier(importDefaultSpecifier, context);
        VisitedImportDefaultSpecifier?.Invoke(this, importDefaultSpecifier);
        return result;
    }

    protected internal override object? VisitImportNamespaceSpecifier(ImportNamespaceSpecifier importNamespaceSpecifier, object? context)
    {
        VisitingImportNamespaceSpecifier?.Invoke(this, importNamespaceSpecifier);
        var result = base.VisitImportNamespaceSpecifier(importNamespaceSpecifier, context);
        VisitedImportNamespaceSpecifier?.Invoke(this, importNamespaceSpecifier);
        return result;
    }

    protected internal override object? VisitImportSpecifier(ImportSpecifier importSpecifier, object? context)
    {
        VisitingImportSpecifier?.Invoke(this, importSpecifier);
        var result = base.VisitImportSpecifier(importSpecifier, context);
        VisitedImportSpecifier?.Invoke(this, importSpecifier);
        return result;
    }

    protected internal override object? VisitLabeledStatement(LabeledStatement labeledStatement, object? context)
    {
        VisitingLabeledStatement?.Invoke(this, labeledStatement);
        var result = base.VisitLabeledStatement(labeledStatement, context);
        VisitedLabeledStatement?.Invoke(this, labeledStatement);
        return result;
    }

    protected internal override object? VisitLiteral(Literal literal, object? context)
    {
        VisitingLiteral?.Invoke(this, literal);
        var result = base.VisitLiteral(literal, context);
        VisitedLiteral?.Invoke(this, literal);
        return result;
    }

    protected internal override object? VisitMemberExpression(MemberExpression memberExpression, object? context)
    {
        VisitingMemberExpression?.Invoke(this, memberExpression);
        var result = base.VisitMemberExpression(memberExpression, context);
        VisitedMemberExpression?.Invoke(this, memberExpression);
        return result;
    }

    protected internal override object? VisitMetaProperty(MetaProperty metaProperty, object? context)
    {
        VisitingMetaProperty?.Invoke(this, metaProperty);
        var result = base.VisitMetaProperty(metaProperty, context);
        VisitedMetaProperty?.Invoke(this, metaProperty);
        return result;
    }

    protected internal override object? VisitMethodDefinition(MethodDefinition methodDefinition, object? context)
    {
        VisitingMethodDefinition?.Invoke(this, methodDefinition);
        var result = base.VisitMethodDefinition(methodDefinition, context);
        VisitedMethodDefinition?.Invoke(this, methodDefinition);
        return result;
    }

    protected internal override object? VisitNewExpression(NewExpression newExpression, object? context)
    {
        VisitingNewExpression?.Invoke(this, newExpression);
        var result = base.VisitNewExpression(newExpression, context);
        VisitedNewExpression?.Invoke(this, newExpression);
        return result;
    }

    protected internal override object? VisitObjectExpression(ObjectExpression objectExpression, object? context)
    {
        VisitingObjectExpression?.Invoke(this, objectExpression);
        var result = base.VisitObjectExpression(objectExpression, context);
        VisitedObjectExpression?.Invoke(this, objectExpression);
        return result;
    }

    protected internal override object? VisitObjectPattern(ObjectPattern objectPattern, object? context)
    {
        VisitingObjectPattern?.Invoke(this, objectPattern);
        var result = base.VisitObjectPattern(objectPattern, context);
        VisitedObjectPattern?.Invoke(this, objectPattern);
        return result;
    }

    protected internal override object? VisitPrivateIdentifier(PrivateIdentifier privateIdentifier, object? context)
    {
        VisitingPrivateIdentifier?.Invoke(this, privateIdentifier);
        var result = base.VisitPrivateIdentifier(privateIdentifier, context);
        VisitedPrivateIdentifier?.Invoke(this, privateIdentifier);
        return result;
    }

    protected internal override object? VisitProgram(Program program, object? context)
    {
        VisitingProgram?.Invoke(this, program);
        var result = base.VisitProgram(program, context);
        VisitedProgram?.Invoke(this, program);
        return result;
    }

    protected internal override object? VisitProperty(Property property, object? context)
    {
        VisitingProperty?.Invoke(this, property);
        var result = base.VisitProperty(property, context);
        VisitedProperty?.Invoke(this, property);
        return result;
    }

    protected internal override object? VisitPropertyDefinition(PropertyDefinition propertyDefinition, object? context)
    {
        VisitingPropertyDefinition?.Invoke(this, propertyDefinition);
        var result = base.VisitPropertyDefinition(propertyDefinition, context);
        VisitedPropertyDefinition?.Invoke(this, propertyDefinition);
        return result;
    }

    protected internal override object? VisitRestElement(RestElement restElement, object? context)
    {
        VisitingRestElement?.Invoke(this, restElement);
        var result = base.VisitRestElement(restElement, context);
        VisitedRestElement?.Invoke(this, restElement);
        return result;
    }

    protected internal override object? VisitReturnStatement(ReturnStatement returnStatement, object? context)
    {
        VisitingReturnStatement?.Invoke(this, returnStatement);
        var result = base.VisitReturnStatement(returnStatement, context);
        VisitedReturnStatement?.Invoke(this, returnStatement);
        return result;
    }

    protected internal override object? VisitSequenceExpression(SequenceExpression sequenceExpression, object? context)
    {
        VisitingSequenceExpression?.Invoke(this, sequenceExpression);
        var result = base.VisitSequenceExpression(sequenceExpression, context);
        VisitedSequenceExpression?.Invoke(this, sequenceExpression);
        return result;
    }

    protected internal override object? VisitSpreadElement(SpreadElement spreadElement, object? context)
    {
        VisitingSpreadElement?.Invoke(this, spreadElement);
        var result = base.VisitSpreadElement(spreadElement, context);
        VisitedSpreadElement?.Invoke(this, spreadElement);
        return result;
    }

    protected internal override object? VisitStaticBlock(StaticBlock staticBlock, object? context)
    {
        VisitingStaticBlock?.Invoke(this, staticBlock);
        var result = base.VisitStaticBlock(staticBlock, context);
        VisitedStaticBlock?.Invoke(this, staticBlock);
        return result;
    }

    protected internal override object? VisitSuper(Super super, object? context)
    {
        VisitingSuper?.Invoke(this, super);
        var result = base.VisitSuper(super, context);
        VisitedSuper?.Invoke(this, super);
        return result;
    }

    protected internal override object? VisitSwitchCase(SwitchCase switchCase, object? context)
    {
        VisitingSwitchCase?.Invoke(this, switchCase);
        var result = base.VisitSwitchCase(switchCase, context);
        VisitedSwitchCase?.Invoke(this, switchCase);
        return result;
    }

    protected internal override object? VisitSwitchStatement(SwitchStatement switchStatement, object? context)
    {
        VisitingSwitchStatement?.Invoke(this, switchStatement);
        var result = base.VisitSwitchStatement(switchStatement, context);
        VisitedSwitchStatement?.Invoke(this, switchStatement);
        return result;
    }

    protected internal override object? VisitTaggedTemplateExpression(TaggedTemplateExpression taggedTemplateExpression, object? context)
    {
        VisitingTaggedTemplateExpression?.Invoke(this, taggedTemplateExpression);
        var result = base.VisitTaggedTemplateExpression(taggedTemplateExpression, context);
        VisitedTaggedTemplateExpression?.Invoke(this, taggedTemplateExpression);
        return result;
    }

    protected internal override object? VisitTemplateElement(TemplateElement templateElement, object? context)
    {
        VisitingTemplateElement?.Invoke(this, templateElement);
        var result = base.VisitTemplateElement(templateElement, context);
        VisitedTemplateElement?.Invoke(this, templateElement);
        return result;
    }

    protected internal override object? VisitTemplateLiteral(TemplateLiteral templateLiteral, object? context)
    {
        VisitingTemplateLiteral?.Invoke(this, templateLiteral);
        var result = base.VisitTemplateLiteral(templateLiteral, context);
        VisitedTemplateLiteral?.Invoke(this, templateLiteral);
        return result;
    }

    protected internal override object? VisitThisExpression(ThisExpression thisExpression, object? context)
    {
        VisitingThisExpression?.Invoke(this, thisExpression);
        var result = base.VisitThisExpression(thisExpression, context);
        VisitedThisExpression?.Invoke(this, thisExpression);
        return result;
    }

    protected internal override object? VisitThrowStatement(ThrowStatement throwStatement, object? context)
    {
        VisitingThrowStatement?.Invoke(this, throwStatement);
        var result = base.VisitThrowStatement(throwStatement, context);
        VisitedThrowStatement?.Invoke(this, throwStatement);
        return result;
    }

    protected internal override object? VisitTryStatement(TryStatement tryStatement, object? context)
    {
        VisitingTryStatement?.Invoke(this, tryStatement);
        var result = base.VisitTryStatement(tryStatement, context);
        VisitedTryStatement?.Invoke(this, tryStatement);
        return result;
    }

    protected internal override object? VisitUnaryExpression(UnaryExpression unaryExpression, object? context)
    {
        VisitingUnaryExpression?.Invoke(this, unaryExpression);
        var result = base.VisitUnaryExpression(unaryExpression, context);
        VisitedUnaryExpression?.Invoke(this, unaryExpression);
        return result;
    }

    protected internal override object? VisitVariableDeclaration(VariableDeclaration variableDeclaration, object? context)
    {
        VisitingVariableDeclaration?.Invoke(this, variableDeclaration);
        var result = base.VisitVariableDeclaration(variableDeclaration, context);
        VisitedVariableDeclaration?.Invoke(this, variableDeclaration);
        return result;
    }

    protected internal override object? VisitVariableDeclarator(VariableDeclarator variableDeclarator, object? context)
    {
        VisitingVariableDeclarator?.Invoke(this, variableDeclarator);
        var result = base.VisitVariableDeclarator(variableDeclarator, context);
        VisitedVariableDeclarator?.Invoke(this, variableDeclarator);
        return result;
    }

    protected internal override object? VisitWhileStatement(WhileStatement whileStatement, object? context)
    {
        VisitingWhileStatement?.Invoke(this, whileStatement);
        var result = base.VisitWhileStatement(whileStatement, context);
        VisitedWhileStatement?.Invoke(this, whileStatement);
        return result;
    }

    protected internal override object? VisitWithStatement(WithStatement withStatement, object? context)
    {
        VisitingWithStatement?.Invoke(this, withStatement);
        var result = base.VisitWithStatement(withStatement, context);
        VisitedWithStatement?.Invoke(this, withStatement);
        return result;
    }

    protected internal override object? VisitYieldExpression(YieldExpression yieldExpression, object? context)
    {
        VisitingYieldExpression?.Invoke(this, yieldExpression);
        var result = base.VisitYieldExpression(yieldExpression, context);
        VisitedYieldExpression?.Invoke(this, yieldExpression);
        return result;
    }
}
