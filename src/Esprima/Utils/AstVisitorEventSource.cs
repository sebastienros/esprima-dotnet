using Esprima.Ast;

namespace Esprima.Utils;

/// <summary>
/// An AST visitor that raises events before and after visiting each node
/// and its descendants.
/// </summary>
public partial class AstVisitorEventSource : AstVisitor
{
    public event EventHandler<Node>? VisitingNode;
    public event EventHandler<Node>? VisitedNode;
    public event EventHandler<Program>? VisitingProgram;
    public event EventHandler<Program>? VisitedProgram;
    public event EventHandler<Node>? VisitingUnknownNode;
    public event EventHandler<Node>? VisitedUnknownNode;
    public event EventHandler<CatchClause>? VisitingCatchClause;
    public event EventHandler<CatchClause>? VisitedCatchClause;
    public event EventHandler<FunctionDeclaration>? VisitingFunctionDeclaration;
    public event EventHandler<FunctionDeclaration>? VisitedFunctionDeclaration;
    public event EventHandler<WithStatement>? VisitingWithStatement;
    public event EventHandler<WithStatement>? VisitedWithStatement;
    public event EventHandler<WhileStatement>? VisitingWhileStatement;
    public event EventHandler<WhileStatement>? VisitedWhileStatement;
    public event EventHandler<VariableDeclaration>? VisitingVariableDeclaration;
    public event EventHandler<VariableDeclaration>? VisitedVariableDeclaration;
    public event EventHandler<TryStatement>? VisitingTryStatement;
    public event EventHandler<TryStatement>? VisitedTryStatement;
    public event EventHandler<ThrowStatement>? VisitingThrowStatement;
    public event EventHandler<ThrowStatement>? VisitedThrowStatement;
    public event EventHandler<SwitchStatement>? VisitingSwitchStatement;
    public event EventHandler<SwitchStatement>? VisitedSwitchStatement;
    public event EventHandler<SwitchCase>? VisitingSwitchCase;
    public event EventHandler<SwitchCase>? VisitedSwitchCase;
    public event EventHandler<ReturnStatement>? VisitingReturnStatement;
    public event EventHandler<ReturnStatement>? VisitedReturnStatement;
    public event EventHandler<LabeledStatement>? VisitingLabeledStatement;
    public event EventHandler<LabeledStatement>? VisitedLabeledStatement;
    public event EventHandler<IfStatement>? VisitingIfStatement;
    public event EventHandler<IfStatement>? VisitedIfStatement;
    public event EventHandler<EmptyStatement>? VisitingEmptyStatement;
    public event EventHandler<EmptyStatement>? VisitedEmptyStatement;
    public event EventHandler<DebuggerStatement>? VisitingDebuggerStatement;
    public event EventHandler<DebuggerStatement>? VisitedDebuggerStatement;
    public event EventHandler<ExpressionStatement>? VisitingExpressionStatement;
    public event EventHandler<ExpressionStatement>? VisitedExpressionStatement;
    public event EventHandler<ForStatement>? VisitingForStatement;
    public event EventHandler<ForStatement>? VisitedForStatement;
    public event EventHandler<ForInStatement>? VisitingForInStatement;
    public event EventHandler<ForInStatement>? VisitedForInStatement;
    public event EventHandler<DoWhileStatement>? VisitingDoWhileStatement;
    public event EventHandler<DoWhileStatement>? VisitedDoWhileStatement;
    public event EventHandler<ArrowFunctionExpression>? VisitingArrowFunctionExpression;
    public event EventHandler<ArrowFunctionExpression>? VisitedArrowFunctionExpression;
    public event EventHandler<UnaryExpression>? VisitingUnaryExpression;
    public event EventHandler<UnaryExpression>? VisitedUnaryExpression;
    public event EventHandler<ThisExpression>? VisitingThisExpression;
    public event EventHandler<ThisExpression>? VisitedThisExpression;
    public event EventHandler<SequenceExpression>? VisitingSequenceExpression;
    public event EventHandler<SequenceExpression>? VisitedSequenceExpression;
    public event EventHandler<ObjectExpression>? VisitingObjectExpression;
    public event EventHandler<ObjectExpression>? VisitedObjectExpression;
    public event EventHandler<NewExpression>? VisitingNewExpression;
    public event EventHandler<NewExpression>? VisitedNewExpression;
    public event EventHandler<MemberExpression>? VisitingMemberExpression;
    public event EventHandler<MemberExpression>? VisitedMemberExpression;
    public event EventHandler<Literal>? VisitingLiteral;
    public event EventHandler<Literal>? VisitedLiteral;
    public event EventHandler<Identifier>? VisitingIdentifier;
    public event EventHandler<Identifier>? VisitedIdentifier;
    public event EventHandler<PrivateIdentifier>? VisitingPrivateIdentifier;
    public event EventHandler<PrivateIdentifier>? VisitedPrivateIdentifier;
    public event EventHandler<FunctionExpression>? VisitingFunctionExpression;
    public event EventHandler<FunctionExpression>? VisitedFunctionExpression;
    public event EventHandler<PropertyDefinition>? VisitingPropertyDefinition;
    public event EventHandler<PropertyDefinition>? VisitedPropertyDefinition;
    public event EventHandler<ChainExpression>? VisitingChainExpression;
    public event EventHandler<ChainExpression>? VisitedChainExpression;
    public event EventHandler<ClassExpression>? VisitingClassExpression;
    public event EventHandler<ClassExpression>? VisitedClassExpression;
    public event EventHandler<ExportDefaultDeclaration>? VisitingExportDefaultDeclaration;
    public event EventHandler<ExportDefaultDeclaration>? VisitedExportDefaultDeclaration;
    public event EventHandler<ExportAllDeclaration>? VisitingExportAllDeclaration;
    public event EventHandler<ExportAllDeclaration>? VisitedExportAllDeclaration;
    public event EventHandler<ExportNamedDeclaration>? VisitingExportNamedDeclaration;
    public event EventHandler<ExportNamedDeclaration>? VisitedExportNamedDeclaration;
    public event EventHandler<ExportSpecifier>? VisitingExportSpecifier;
    public event EventHandler<ExportSpecifier>? VisitedExportSpecifier;
    public event EventHandler<Import>? VisitingImport;
    public event EventHandler<Import>? VisitedImport;
    public event EventHandler<ImportDeclaration>? VisitingImportDeclaration;
    public event EventHandler<ImportDeclaration>? VisitedImportDeclaration;
    public event EventHandler<ImportNamespaceSpecifier>? VisitingImportNamespaceSpecifier;
    public event EventHandler<ImportNamespaceSpecifier>? VisitedImportNamespaceSpecifier;
    public event EventHandler<ImportDefaultSpecifier>? VisitingImportDefaultSpecifier;
    public event EventHandler<ImportDefaultSpecifier>? VisitedImportDefaultSpecifier;
    public event EventHandler<ImportSpecifier>? VisitingImportSpecifier;
    public event EventHandler<ImportSpecifier>? VisitedImportSpecifier;
    public event EventHandler<MethodDefinition>? VisitingMethodDefinition;
    public event EventHandler<MethodDefinition>? VisitedMethodDefinition;
    public event EventHandler<ForOfStatement>? VisitingForOfStatement;
    public event EventHandler<ForOfStatement>? VisitedForOfStatement;
    public event EventHandler<ClassDeclaration>? VisitingClassDeclaration;
    public event EventHandler<ClassDeclaration>? VisitedClassDeclaration;
    public event EventHandler<ClassBody>? VisitingClassBody;
    public event EventHandler<ClassBody>? VisitedClassBody;
    public event EventHandler<YieldExpression>? VisitingYieldExpression;
    public event EventHandler<YieldExpression>? VisitedYieldExpression;
    public event EventHandler<TaggedTemplateExpression>? VisitingTaggedTemplateExpression;
    public event EventHandler<TaggedTemplateExpression>? VisitedTaggedTemplateExpression;
    public event EventHandler<Super>? VisitingSuper;
    public event EventHandler<Super>? VisitedSuper;
    public event EventHandler<MetaProperty>? VisitingMetaProperty;
    public event EventHandler<MetaProperty>? VisitedMetaProperty;
    public event EventHandler<ObjectPattern>? VisitingObjectPattern;
    public event EventHandler<ObjectPattern>? VisitedObjectPattern;
    public event EventHandler<SpreadElement>? VisitingSpreadElement;
    public event EventHandler<SpreadElement>? VisitedSpreadElement;
    public event EventHandler<AssignmentPattern>? VisitingAssignmentPattern;
    public event EventHandler<AssignmentPattern>? VisitedAssignmentPattern;
    public event EventHandler<ArrayPattern>? VisitingArrayPattern;
    public event EventHandler<ArrayPattern>? VisitedArrayPattern;
    public event EventHandler<VariableDeclarator>? VisitingVariableDeclarator;
    public event EventHandler<VariableDeclarator>? VisitedVariableDeclarator;
    public event EventHandler<TemplateLiteral>? VisitingTemplateLiteral;
    public event EventHandler<TemplateLiteral>? VisitedTemplateLiteral;
    public event EventHandler<TemplateElement>? VisitingTemplateElement;
    public event EventHandler<TemplateElement>? VisitedTemplateElement;
    public event EventHandler<RestElement>? VisitingRestElement;
    public event EventHandler<RestElement>? VisitedRestElement;
    public event EventHandler<Property>? VisitingProperty;
    public event EventHandler<Property>? VisitedProperty;
    public event EventHandler<AwaitExpression>? VisitingAwaitExpression;
    public event EventHandler<AwaitExpression>? VisitedAwaitExpression;
    public event EventHandler<ConditionalExpression>? VisitingConditionalExpression;
    public event EventHandler<ConditionalExpression>? VisitedConditionalExpression;
    public event EventHandler<CallExpression>? VisitingCallExpression;
    public event EventHandler<CallExpression>? VisitedCallExpression;
    public event EventHandler<BinaryExpression>? VisitingBinaryExpression;
    public event EventHandler<BinaryExpression>? VisitedBinaryExpression;
    public event EventHandler<ArrayExpression>? VisitingArrayExpression;
    public event EventHandler<ArrayExpression>? VisitedArrayExpression;
    public event EventHandler<AssignmentExpression>? VisitingAssignmentExpression;
    public event EventHandler<AssignmentExpression>? VisitedAssignmentExpression;
    public event EventHandler<ContinueStatement>? VisitingContinueStatement;
    public event EventHandler<ContinueStatement>? VisitedContinueStatement;
    public event EventHandler<BreakStatement>? VisitingBreakStatement;
    public event EventHandler<BreakStatement>? VisitedBreakStatement;
    public event EventHandler<BlockStatement>? VisitingBlockStatement;
    public event EventHandler<BlockStatement>? VisitedBlockStatement;

    public override object? Visit(Node node)
    {
        VisitingNode?.Invoke(this, node);
        var result = base.Visit(node);
        VisitedNode?.Invoke(this, node);
        return result;
    }

    protected internal override object? VisitProgram(Program program)
    {
        VisitingProgram?.Invoke(this, program);
        var result = base.VisitProgram(program);
        VisitedProgram?.Invoke(this, program);
        return result;
    }

    [Obsolete("This method may be removed in a future version as it will not be called anymore due to employing double dispatch (instead of switch dispatch).")]
    protected internal override object? VisitUnknownNode(Node node)
    {
        VisitingUnknownNode?.Invoke(this, node);
        var result = base.VisitUnknownNode(node);
        VisitedUnknownNode?.Invoke(this, node);
        return result;
    }

    protected internal override object? VisitCatchClause(CatchClause catchClause)
    {
        VisitingCatchClause?.Invoke(this, catchClause);
        var result = base.VisitCatchClause(catchClause);
        VisitedCatchClause?.Invoke(this, catchClause);
        return result;
    }

    protected internal override object? VisitFunctionDeclaration(FunctionDeclaration functionDeclaration)
    {
        VisitingFunctionDeclaration?.Invoke(this, functionDeclaration);
        var result = base.VisitFunctionDeclaration(functionDeclaration);
        VisitedFunctionDeclaration?.Invoke(this, functionDeclaration);
        return result;
    }

    protected internal override object? VisitWithStatement(WithStatement withStatement)
    {
        VisitingWithStatement?.Invoke(this, withStatement);
        var result = base.VisitWithStatement(withStatement);
        VisitedWithStatement?.Invoke(this, withStatement);
        return result;
    }

    protected internal override object? VisitWhileStatement(WhileStatement whileStatement)
    {
        VisitingWhileStatement?.Invoke(this, whileStatement);
        var result = base.VisitWhileStatement(whileStatement);
        VisitedWhileStatement?.Invoke(this, whileStatement);
        return result;
    }

    protected internal override object? VisitVariableDeclaration(VariableDeclaration variableDeclaration)
    {
        VisitingVariableDeclaration?.Invoke(this, variableDeclaration);
        var result = base.VisitVariableDeclaration(variableDeclaration);
        VisitedVariableDeclaration?.Invoke(this, variableDeclaration);
        return result;
    }

    protected internal override object? VisitTryStatement(TryStatement tryStatement)
    {
        VisitingTryStatement?.Invoke(this, tryStatement);
        var result = base.VisitTryStatement(tryStatement);
        VisitedTryStatement?.Invoke(this, tryStatement);
        return result;
    }

    protected internal override object? VisitThrowStatement(ThrowStatement throwStatement)
    {
        VisitingThrowStatement?.Invoke(this, throwStatement);
        var result = base.VisitThrowStatement(throwStatement);
        VisitedThrowStatement?.Invoke(this, throwStatement);
        return result;
    }

    protected internal override object? VisitSwitchStatement(SwitchStatement switchStatement)
    {
        VisitingSwitchStatement?.Invoke(this, switchStatement);
        var result = base.VisitSwitchStatement(switchStatement);
        VisitedSwitchStatement?.Invoke(this, switchStatement);
        return result;
    }

    protected internal override object? VisitSwitchCase(SwitchCase switchCase)
    {
        VisitingSwitchCase?.Invoke(this, switchCase);
        var result = base.VisitSwitchCase(switchCase);
        VisitedSwitchCase?.Invoke(this, switchCase);
        return result;
    }

    protected internal override object? VisitReturnStatement(ReturnStatement returnStatement)
    {
        VisitingReturnStatement?.Invoke(this, returnStatement);
        var result = base.VisitReturnStatement(returnStatement);
        VisitedReturnStatement?.Invoke(this, returnStatement);
        return result;
    }

    protected internal override object? VisitLabeledStatement(LabeledStatement labeledStatement)
    {
        VisitingLabeledStatement?.Invoke(this, labeledStatement);
        var result = base.VisitLabeledStatement(labeledStatement);
        VisitedLabeledStatement?.Invoke(this, labeledStatement);
        return result;
    }

    protected internal override object? VisitIfStatement(IfStatement ifStatement)
    {
        VisitingIfStatement?.Invoke(this, ifStatement);
        var result = base.VisitIfStatement(ifStatement);
        VisitedIfStatement?.Invoke(this, ifStatement);
        return result;
    }

    protected internal override object? VisitEmptyStatement(EmptyStatement emptyStatement)
    {
        VisitingEmptyStatement?.Invoke(this, emptyStatement);
        var result = base.VisitEmptyStatement(emptyStatement);
        VisitedEmptyStatement?.Invoke(this, emptyStatement);
        return result;
    }

    protected internal override object? VisitDebuggerStatement(DebuggerStatement debuggerStatement)
    {
        VisitingDebuggerStatement?.Invoke(this, debuggerStatement);
        var result = base.VisitDebuggerStatement(debuggerStatement);
        VisitedDebuggerStatement?.Invoke(this, debuggerStatement);
        return result;
    }

    protected internal override object? VisitExpressionStatement(ExpressionStatement expressionStatement)
    {
        VisitingExpressionStatement?.Invoke(this, expressionStatement);
        var result = base.VisitExpressionStatement(expressionStatement);
        VisitedExpressionStatement?.Invoke(this, expressionStatement);
        return result;
    }

    protected internal override object? VisitForStatement(ForStatement forStatement)
    {
        VisitingForStatement?.Invoke(this, forStatement);
        var result = base.VisitForStatement(forStatement);
        VisitedForStatement?.Invoke(this, forStatement);
        return result;
    }

    protected internal override object? VisitForInStatement(ForInStatement forInStatement)
    {
        VisitingForInStatement?.Invoke(this, forInStatement);
        var result = base.VisitForInStatement(forInStatement);
        VisitedForInStatement?.Invoke(this, forInStatement);
        return result;
    }

    protected internal override object? VisitDoWhileStatement(DoWhileStatement doWhileStatement)
    {
        VisitingDoWhileStatement?.Invoke(this, doWhileStatement);
        var result = base.VisitDoWhileStatement(doWhileStatement);
        VisitedDoWhileStatement?.Invoke(this, doWhileStatement);
        return result;
    }

    protected internal override object? VisitArrowFunctionExpression(ArrowFunctionExpression arrowFunctionExpression)
    {
        VisitingArrowFunctionExpression?.Invoke(this, arrowFunctionExpression);
        var result = base.VisitArrowFunctionExpression(arrowFunctionExpression);
        VisitedArrowFunctionExpression?.Invoke(this, arrowFunctionExpression);
        return result;
    }

    protected internal override object? VisitUnaryExpression(UnaryExpression unaryExpression)
    {
        VisitingUnaryExpression?.Invoke(this, unaryExpression);
        var result = base.VisitUnaryExpression(unaryExpression);
        VisitedUnaryExpression?.Invoke(this, unaryExpression);
        return result;
    }

    protected internal override object? VisitThisExpression(ThisExpression thisExpression)
    {
        VisitingThisExpression?.Invoke(this, thisExpression);
        var result = base.VisitThisExpression(thisExpression);
        VisitedThisExpression?.Invoke(this, thisExpression);
        return result;
    }

    protected internal override object? VisitSequenceExpression(SequenceExpression sequenceExpression)
    {
        VisitingSequenceExpression?.Invoke(this, sequenceExpression);
        var result = base.VisitSequenceExpression(sequenceExpression);
        VisitedSequenceExpression?.Invoke(this, sequenceExpression);
        return result;
    }

    protected internal override object? VisitObjectExpression(ObjectExpression objectExpression)
    {
        VisitingObjectExpression?.Invoke(this, objectExpression);
        var result = base.VisitObjectExpression(objectExpression);
        VisitedObjectExpression?.Invoke(this, objectExpression);
        return result;
    }

    protected internal override object? VisitNewExpression(NewExpression newExpression)
    {
        VisitingNewExpression?.Invoke(this, newExpression);
        var result = base.VisitNewExpression(newExpression);
        VisitedNewExpression?.Invoke(this, newExpression);
        return result;
    }

    protected internal override object? VisitMemberExpression(MemberExpression memberExpression)
    {
        VisitingMemberExpression?.Invoke(this, memberExpression);
        var result = base.VisitMemberExpression(memberExpression);
        VisitedMemberExpression?.Invoke(this, memberExpression);
        return result;
    }

    protected internal override object? VisitLiteral(Literal literal)
    {
        VisitingLiteral?.Invoke(this, literal);
        var result = base.VisitLiteral(literal);
        VisitedLiteral?.Invoke(this, literal);
        return result;
    }

    protected internal override object? VisitIdentifier(Identifier identifier)
    {
        VisitingIdentifier?.Invoke(this, identifier);
        var result = base.VisitIdentifier(identifier);
        VisitedIdentifier?.Invoke(this, identifier);
        return result;
    }

    protected internal override object? VisitPrivateIdentifier(PrivateIdentifier privateIdentifier)
    {
        VisitingPrivateIdentifier?.Invoke(this, privateIdentifier);
        var result = base.VisitPrivateIdentifier(privateIdentifier);
        VisitedPrivateIdentifier?.Invoke(this, privateIdentifier);
        return result;
    }

    protected internal override object? VisitFunctionExpression(FunctionExpression functionExpression)
    {
        VisitingFunctionExpression?.Invoke(this, functionExpression);
        var result = base.VisitFunctionExpression(functionExpression);
        VisitedFunctionExpression?.Invoke(this, functionExpression);
        return result;
    }

    protected internal override object? VisitPropertyDefinition(PropertyDefinition propertyDefinition)
    {
        VisitingPropertyDefinition?.Invoke(this, propertyDefinition);
        var result = base.VisitPropertyDefinition(propertyDefinition);
        VisitedPropertyDefinition?.Invoke(this, propertyDefinition);
        return result;
    }

    protected internal override object? VisitChainExpression(ChainExpression chainExpression)
    {
        VisitingChainExpression?.Invoke(this, chainExpression);
        var result = base.VisitChainExpression(chainExpression);
        VisitedChainExpression?.Invoke(this, chainExpression);
        return result;
    }

    protected internal override object? VisitClassExpression(ClassExpression classExpression)
    {
        VisitingClassExpression?.Invoke(this, classExpression);
        var result = base.VisitClassExpression(classExpression);
        VisitedClassExpression?.Invoke(this, classExpression);
        return result;
    }

    protected internal override object? VisitExportDefaultDeclaration(ExportDefaultDeclaration exportDefaultDeclaration)
    {
        VisitingExportDefaultDeclaration?.Invoke(this, exportDefaultDeclaration);
        var result = base.VisitExportDefaultDeclaration(exportDefaultDeclaration);
        VisitedExportDefaultDeclaration?.Invoke(this, exportDefaultDeclaration);
        return result;
    }

    protected internal override object? VisitExportAllDeclaration(ExportAllDeclaration exportAllDeclaration)
    {
        VisitingExportAllDeclaration?.Invoke(this, exportAllDeclaration);
        var result = base.VisitExportAllDeclaration(exportAllDeclaration);
        VisitedExportAllDeclaration?.Invoke(this, exportAllDeclaration);
        return result;
    }

    protected internal override object? VisitExportNamedDeclaration(ExportNamedDeclaration exportNamedDeclaration)
    {
        VisitingExportNamedDeclaration?.Invoke(this, exportNamedDeclaration);
        var result = base.VisitExportNamedDeclaration(exportNamedDeclaration);
        VisitedExportNamedDeclaration?.Invoke(this, exportNamedDeclaration);
        return result;
    }

    protected internal override object? VisitExportSpecifier(ExportSpecifier exportSpecifier)
    {
        VisitingExportSpecifier?.Invoke(this, exportSpecifier);
        var result = base.VisitExportSpecifier(exportSpecifier);
        VisitedExportSpecifier?.Invoke(this, exportSpecifier);
        return result;
    }

    protected internal override object? VisitImport(Import import)
    {
        VisitingImport?.Invoke(this, import);
        var result = base.VisitImport(import);
        VisitedImport?.Invoke(this, import);
        return result;
    }

    protected internal override object? VisitImportDeclaration(ImportDeclaration importDeclaration)
    {
        VisitingImportDeclaration?.Invoke(this, importDeclaration);
        var result = base.VisitImportDeclaration(importDeclaration);
        VisitedImportDeclaration?.Invoke(this, importDeclaration);
        return result;
    }

    protected internal override object? VisitImportNamespaceSpecifier(ImportNamespaceSpecifier importNamespaceSpecifier)
    {
        VisitingImportNamespaceSpecifier?.Invoke(this, importNamespaceSpecifier);
        var result = base.VisitImportNamespaceSpecifier(importNamespaceSpecifier);
        VisitedImportNamespaceSpecifier?.Invoke(this, importNamespaceSpecifier);
        return result;
    }

    protected internal override object? VisitImportDefaultSpecifier(ImportDefaultSpecifier importDefaultSpecifier)
    {
        VisitingImportDefaultSpecifier?.Invoke(this, importDefaultSpecifier);
        var result = base.VisitImportDefaultSpecifier(importDefaultSpecifier);
        VisitedImportDefaultSpecifier?.Invoke(this, importDefaultSpecifier);
        return result;
    }

    protected internal override object? VisitImportSpecifier(ImportSpecifier importSpecifier)
    {
        VisitingImportSpecifier?.Invoke(this, importSpecifier);
        var result = base.VisitImportSpecifier(importSpecifier);
        VisitedImportSpecifier?.Invoke(this, importSpecifier);
        return result;
    }

    protected internal override object? VisitMethodDefinition(MethodDefinition methodDefinition)
    {
        VisitingMethodDefinition?.Invoke(this, methodDefinition);
        var result = base.VisitMethodDefinition(methodDefinition);
        VisitedMethodDefinition?.Invoke(this, methodDefinition);
        return result;
    }

    protected internal override object? VisitForOfStatement(ForOfStatement forOfStatement)
    {
        VisitingForOfStatement?.Invoke(this, forOfStatement);
        var result = base.VisitForOfStatement(forOfStatement);
        VisitedForOfStatement?.Invoke(this, forOfStatement);
        return result;
    }

    protected internal override object? VisitClassDeclaration(ClassDeclaration classDeclaration)
    {
        VisitingClassDeclaration?.Invoke(this, classDeclaration);
        var result = base.VisitClassDeclaration(classDeclaration);
        VisitedClassDeclaration?.Invoke(this, classDeclaration);
        return result;
    }

    protected internal override object? VisitClassBody(ClassBody classBody)
    {
        VisitingClassBody?.Invoke(this, classBody);
        var result = base.VisitClassBody(classBody);
        VisitedClassBody?.Invoke(this, classBody);
        return result;
    }

    protected internal override object? VisitYieldExpression(YieldExpression yieldExpression)
    {
        VisitingYieldExpression?.Invoke(this, yieldExpression);
        var result = base.VisitYieldExpression(yieldExpression);
        VisitedYieldExpression?.Invoke(this, yieldExpression);
        return result;
    }

    protected internal override object? VisitTaggedTemplateExpression(TaggedTemplateExpression taggedTemplateExpression)
    {
        VisitingTaggedTemplateExpression?.Invoke(this, taggedTemplateExpression);
        var result = base.VisitTaggedTemplateExpression(taggedTemplateExpression);
        VisitedTaggedTemplateExpression?.Invoke(this, taggedTemplateExpression);
        return result;
    }

    protected internal override object? VisitSuper(Super super)
    {
        VisitingSuper?.Invoke(this, super);
        var result = base.VisitSuper(super);
        VisitedSuper?.Invoke(this, super);
        return result;
    }

    protected internal override object? VisitMetaProperty(MetaProperty metaProperty)
    {
        VisitingMetaProperty?.Invoke(this, metaProperty);
        var result = base.VisitMetaProperty(metaProperty);
        VisitedMetaProperty?.Invoke(this, metaProperty);
        return result;
    }

    protected internal override object? VisitObjectPattern(ObjectPattern objectPattern)
    {
        VisitingObjectPattern?.Invoke(this, objectPattern);
        var result = base.VisitObjectPattern(objectPattern);
        VisitedObjectPattern?.Invoke(this, objectPattern);
        return result;
    }

    protected internal override object? VisitSpreadElement(SpreadElement spreadElement)
    {
        VisitingSpreadElement?.Invoke(this, spreadElement);
        var result = base.VisitSpreadElement(spreadElement);
        VisitedSpreadElement?.Invoke(this, spreadElement);
        return result;
    }

    protected internal override object? VisitAssignmentPattern(AssignmentPattern assignmentPattern)
    {
        VisitingAssignmentPattern?.Invoke(this, assignmentPattern);
        var result = base.VisitAssignmentPattern(assignmentPattern);
        VisitedAssignmentPattern?.Invoke(this, assignmentPattern);
        return result;
    }

    protected internal override object? VisitArrayPattern(ArrayPattern arrayPattern)
    {
        VisitingArrayPattern?.Invoke(this, arrayPattern);
        var result = base.VisitArrayPattern(arrayPattern);
        VisitedArrayPattern?.Invoke(this, arrayPattern);
        return result;
    }

    protected internal override object? VisitVariableDeclarator(VariableDeclarator variableDeclarator)
    {
        VisitingVariableDeclarator?.Invoke(this, variableDeclarator);
        var result = base.VisitVariableDeclarator(variableDeclarator);
        VisitedVariableDeclarator?.Invoke(this, variableDeclarator);
        return result;
    }

    protected internal override object? VisitTemplateLiteral(TemplateLiteral templateLiteral)
    {
        VisitingTemplateLiteral?.Invoke(this, templateLiteral);
        var result = base.VisitTemplateLiteral(templateLiteral);
        VisitedTemplateLiteral?.Invoke(this, templateLiteral);
        return result;
    }

    protected internal override object? VisitTemplateElement(TemplateElement templateElement)
    {
        VisitingTemplateElement?.Invoke(this, templateElement);
        var result = base.VisitTemplateElement(templateElement);
        VisitedTemplateElement?.Invoke(this, templateElement);
        return result;
    }

    protected internal override object? VisitRestElement(RestElement restElement)
    {
        VisitingRestElement?.Invoke(this, restElement);
        var result = base.VisitRestElement(restElement);
        VisitedRestElement?.Invoke(this, restElement);
        return result;
    }

    protected internal override object? VisitProperty(Property property)
    {
        VisitingProperty?.Invoke(this, property);
        var result = base.VisitProperty(property);
        VisitedProperty?.Invoke(this, property);
        return result;
    }

    protected internal override object? VisitAwaitExpression(AwaitExpression awaitExpression)
    {
        VisitingAwaitExpression?.Invoke(this, awaitExpression);
        var result = base.VisitAwaitExpression(awaitExpression);
        VisitedAwaitExpression?.Invoke(this, awaitExpression);
        return result;
    }

    protected internal override object? VisitConditionalExpression(ConditionalExpression conditionalExpression)
    {
        VisitingConditionalExpression?.Invoke(this, conditionalExpression);
        var result = base.VisitConditionalExpression(conditionalExpression);
        VisitedConditionalExpression?.Invoke(this, conditionalExpression);
        return result;
    }

    protected internal override object? VisitCallExpression(CallExpression callExpression)
    {
        VisitingCallExpression?.Invoke(this, callExpression);
        var result = base.VisitCallExpression(callExpression);
        VisitedCallExpression?.Invoke(this, callExpression);
        return result;
    }

    protected internal override object? VisitBinaryExpression(BinaryExpression binaryExpression)
    {
        VisitingBinaryExpression?.Invoke(this, binaryExpression);
        var result = base.VisitBinaryExpression(binaryExpression);
        VisitedBinaryExpression?.Invoke(this, binaryExpression);
        return result;
    }

    protected internal override object? VisitArrayExpression(ArrayExpression arrayExpression)
    {
        VisitingArrayExpression?.Invoke(this, arrayExpression);
        var result = base.VisitArrayExpression(arrayExpression);
        VisitedArrayExpression?.Invoke(this, arrayExpression);
        return result;
    }

    protected internal override object? VisitAssignmentExpression(AssignmentExpression assignmentExpression)
    {
        VisitingAssignmentExpression?.Invoke(this, assignmentExpression);
        var result = base.VisitAssignmentExpression(assignmentExpression);
        VisitedAssignmentExpression?.Invoke(this, assignmentExpression);
        return result;
    }

    protected internal override object? VisitContinueStatement(ContinueStatement continueStatement)
    {
        VisitingContinueStatement?.Invoke(this, continueStatement);
        var result = base.VisitContinueStatement(continueStatement);
        VisitedContinueStatement?.Invoke(this, continueStatement);
        return result;
    }

    protected internal override object? VisitBreakStatement(BreakStatement breakStatement)
    {
        VisitingBreakStatement?.Invoke(this, breakStatement);
        var result = base.VisitBreakStatement(breakStatement);
        VisitedBreakStatement?.Invoke(this, breakStatement);
        return result;
    }

    protected internal override object? VisitBlockStatement(BlockStatement blockStatement)
    {
        VisitingBlockStatement?.Invoke(this, blockStatement);
        var result = base.VisitBlockStatement(blockStatement);
        VisitedBlockStatement?.Invoke(this, blockStatement);
        return result;
    }
}
