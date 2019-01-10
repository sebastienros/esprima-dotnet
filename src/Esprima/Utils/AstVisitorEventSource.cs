using System;
using Esprima.Ast;

namespace Esprima.Utils
{
    /// <summary>
    /// An AST visitor that raises events before and after visiting each node
    /// and its descendants.
    /// </summary>

    public class AstVisitorEventSource : AstVisitor
    {
        public event EventHandler<INode> VisitingNode;
        public event EventHandler<INode> VisitedNode;
        public event EventHandler<Program> VisitingProgram;
        public event EventHandler<Program> VisitedProgram;
        public event EventHandler<Statement> VisitingStatement;
        public event EventHandler<Statement> VisitedStatement;
        public event EventHandler<INode> VisitingUnknownNode;
        public event EventHandler<INode> VisitedUnknownNode;
        public event EventHandler<CatchClause> VisitingCatchClause;
        public event EventHandler<CatchClause> VisitedCatchClause;
        public event EventHandler<FunctionDeclaration> VisitingFunctionDeclaration;
        public event EventHandler<FunctionDeclaration> VisitedFunctionDeclaration;
        public event EventHandler<WithStatement> VisitingWithStatement;
        public event EventHandler<WithStatement> VisitedWithStatement;
        public event EventHandler<WhileStatement> VisitingWhileStatement;
        public event EventHandler<WhileStatement> VisitedWhileStatement;
        public event EventHandler<VariableDeclaration> VisitingVariableDeclaration;
        public event EventHandler<VariableDeclaration> VisitedVariableDeclaration;
        public event EventHandler<TryStatement> VisitingTryStatement;
        public event EventHandler<TryStatement> VisitedTryStatement;
        public event EventHandler<ThrowStatement> VisitingThrowStatement;
        public event EventHandler<ThrowStatement> VisitedThrowStatement;
        public event EventHandler<SwitchStatement> VisitingSwitchStatement;
        public event EventHandler<SwitchStatement> VisitedSwitchStatement;
        public event EventHandler<SwitchCase> VisitingSwitchCase;
        public event EventHandler<SwitchCase> VisitedSwitchCase;
        public event EventHandler<ReturnStatement> VisitingReturnStatement;
        public event EventHandler<ReturnStatement> VisitedReturnStatement;
        public event EventHandler<LabeledStatement> VisitingLabeledStatement;
        public event EventHandler<LabeledStatement> VisitedLabeledStatement;
        public event EventHandler<IfStatement> VisitingIfStatement;
        public event EventHandler<IfStatement> VisitedIfStatement;
        public event EventHandler<EmptyStatement> VisitingEmptyStatement;
        public event EventHandler<EmptyStatement> VisitedEmptyStatement;
        public event EventHandler<DebuggerStatement> VisitingDebuggerStatement;
        public event EventHandler<DebuggerStatement> VisitedDebuggerStatement;
        public event EventHandler<ExpressionStatement> VisitingExpressionStatement;
        public event EventHandler<ExpressionStatement> VisitedExpressionStatement;
        public event EventHandler<ForStatement> VisitingForStatement;
        public event EventHandler<ForStatement> VisitedForStatement;
        public event EventHandler<ForInStatement> VisitingForInStatement;
        public event EventHandler<ForInStatement> VisitedForInStatement;
        public event EventHandler<DoWhileStatement> VisitingDoWhileStatement;
        public event EventHandler<DoWhileStatement> VisitedDoWhileStatement;
        public event EventHandler<Expression> VisitingExpression;
        public event EventHandler<Expression> VisitedExpression;
        public event EventHandler<ArrowFunctionExpression> VisitingArrowFunctionExpression;
        public event EventHandler<ArrowFunctionExpression> VisitedArrowFunctionExpression;
        public event EventHandler<UnaryExpression> VisitingUnaryExpression;
        public event EventHandler<UnaryExpression> VisitedUnaryExpression;
        public event EventHandler<UpdateExpression> VisitingUpdateExpression;
        public event EventHandler<UpdateExpression> VisitedUpdateExpression;
        public event EventHandler<ThisExpression> VisitingThisExpression;
        public event EventHandler<ThisExpression> VisitedThisExpression;
        public event EventHandler<SequenceExpression> VisitingSequenceExpression;
        public event EventHandler<SequenceExpression> VisitedSequenceExpression;
        public event EventHandler<ObjectExpression> VisitingObjectExpression;
        public event EventHandler<ObjectExpression> VisitedObjectExpression;
        public event EventHandler<NewExpression> VisitingNewExpression;
        public event EventHandler<NewExpression> VisitedNewExpression;
        public event EventHandler<MemberExpression> VisitingMemberExpression;
        public event EventHandler<MemberExpression> VisitedMemberExpression;
        public event EventHandler<BinaryExpression> VisitingLogicalExpression;
        public event EventHandler<BinaryExpression> VisitedLogicalExpression;
        public event EventHandler<Literal> VisitingLiteral;
        public event EventHandler<Literal> VisitedLiteral;
        public event EventHandler<Identifier> VisitingIdentifier;
        public event EventHandler<Identifier> VisitedIdentifier;
        public event EventHandler<IFunction> VisitingFunctionExpression;
        public event EventHandler<IFunction> VisitedFunctionExpression;
        public event EventHandler<ClassExpression> VisitingClassExpression;
        public event EventHandler<ClassExpression> VisitedClassExpression;
        public event EventHandler<ExportDefaultDeclaration> VisitingExportDefaultDeclaration;
        public event EventHandler<ExportDefaultDeclaration> VisitedExportDefaultDeclaration;
        public event EventHandler<ExportAllDeclaration> VisitingExportAllDeclaration;
        public event EventHandler<ExportAllDeclaration> VisitedExportAllDeclaration;
        public event EventHandler<ExportNamedDeclaration> VisitingExportNamedDeclaration;
        public event EventHandler<ExportNamedDeclaration> VisitedExportNamedDeclaration;
        public event EventHandler<ExportSpecifier> VisitingExportSpecifier;
        public event EventHandler<ExportSpecifier> VisitedExportSpecifier;
        public event EventHandler<ImportDeclaration> VisitingImportDeclaration;
        public event EventHandler<ImportDeclaration> VisitedImportDeclaration;
        public event EventHandler<ImportNamespaceSpecifier> VisitingImportNamespaceSpecifier;
        public event EventHandler<ImportNamespaceSpecifier> VisitedImportNamespaceSpecifier;
        public event EventHandler<ImportDefaultSpecifier> VisitingImportDefaultSpecifier;
        public event EventHandler<ImportDefaultSpecifier> VisitedImportDefaultSpecifier;
        public event EventHandler<ImportSpecifier> VisitingImportSpecifier;
        public event EventHandler<ImportSpecifier> VisitedImportSpecifier;
        public event EventHandler<MethodDefinition> VisitingMethodDefinition;
        public event EventHandler<MethodDefinition> VisitedMethodDefinition;
        public event EventHandler<ForOfStatement> VisitingForOfStatement;
        public event EventHandler<ForOfStatement> VisitedForOfStatement;
        public event EventHandler<ClassDeclaration> VisitingClassDeclaration;
        public event EventHandler<ClassDeclaration> VisitedClassDeclaration;
        public event EventHandler<ClassBody> VisitingClassBody;
        public event EventHandler<ClassBody> VisitedClassBody;
        public event EventHandler<YieldExpression> VisitingYieldExpression;
        public event EventHandler<YieldExpression> VisitedYieldExpression;
        public event EventHandler<TaggedTemplateExpression> VisitingTaggedTemplateExpression;
        public event EventHandler<TaggedTemplateExpression> VisitedTaggedTemplateExpression;
        public event EventHandler<Super> VisitingSuper;
        public event EventHandler<Super> VisitedSuper;
        public event EventHandler<MetaProperty> VisitingMetaProperty;
        public event EventHandler<MetaProperty> VisitedMetaProperty;
        public event EventHandler<ArrowParameterPlaceHolder> VisitingArrowParameterPlaceHolder;
        public event EventHandler<ArrowParameterPlaceHolder> VisitedArrowParameterPlaceHolder;
        public event EventHandler<ObjectPattern> VisitingObjectPattern;
        public event EventHandler<ObjectPattern> VisitedObjectPattern;
        public event EventHandler<SpreadElement> VisitingSpreadElement;
        public event EventHandler<SpreadElement> VisitedSpreadElement;
        public event EventHandler<AssignmentPattern> VisitingAssignmentPattern;
        public event EventHandler<AssignmentPattern> VisitedAssignmentPattern;
        public event EventHandler<ArrayPattern> VisitingArrayPattern;
        public event EventHandler<ArrayPattern> VisitedArrayPattern;
        public event EventHandler<VariableDeclarator> VisitingVariableDeclarator;
        public event EventHandler<VariableDeclarator> VisitedVariableDeclarator;
        public event EventHandler<TemplateLiteral> VisitingTemplateLiteral;
        public event EventHandler<TemplateLiteral> VisitedTemplateLiteral;
        public event EventHandler<TemplateElement> VisitingTemplateElement;
        public event EventHandler<TemplateElement> VisitedTemplateElement;
        public event EventHandler<RestElement> VisitingRestElement;
        public event EventHandler<RestElement> VisitedRestElement;
        public event EventHandler<Property> VisitingProperty;
        public event EventHandler<Property> VisitedProperty;
        public event EventHandler<ConditionalExpression> VisitingConditionalExpression;
        public event EventHandler<ConditionalExpression> VisitedConditionalExpression;
        public event EventHandler<CallExpression> VisitingCallExpression;
        public event EventHandler<CallExpression> VisitedCallExpression;
        public event EventHandler<BinaryExpression> VisitingBinaryExpression;
        public event EventHandler<BinaryExpression> VisitedBinaryExpression;
        public event EventHandler<ArrayExpression> VisitingArrayExpression;
        public event EventHandler<ArrayExpression> VisitedArrayExpression;
        public event EventHandler<AssignmentExpression> VisitingAssignmentExpression;
        public event EventHandler<AssignmentExpression> VisitedAssignmentExpression;
        public event EventHandler<ContinueStatement> VisitingContinueStatement;
        public event EventHandler<ContinueStatement> VisitedContinueStatement;
        public event EventHandler<BreakStatement> VisitingBreakStatement;
        public event EventHandler<BreakStatement> VisitedBreakStatement;
        public event EventHandler<BlockStatement> VisitingBlockStatement;
        public event EventHandler<BlockStatement> VisitedBlockStatement;

        public override void Visit(INode node)
        {
            VisitingNode?.Invoke(this, node);
            base.Visit(node);
            VisitedNode?.Invoke(this, node);
        }

        public override void VisitProgram(Program program)
        {
            VisitingProgram?.Invoke(this, program);
            base.VisitProgram(program);
            VisitedProgram?.Invoke(this, program);
        }

        public override void VisitStatement(Statement statement)
        {
            VisitingStatement?.Invoke(this, statement);
            base.VisitStatement(statement);
            VisitedStatement?.Invoke(this, statement);
        }

        public override void VisitUnknownNode(INode node)
        {
            VisitingUnknownNode?.Invoke(this, node);
            base.VisitUnknownNode(node);
            VisitedUnknownNode?.Invoke(this, node);
        }

        public override void VisitCatchClause(CatchClause catchClause)
        {
            VisitingCatchClause?.Invoke(this, catchClause);
            base.VisitCatchClause(catchClause);
            VisitedCatchClause?.Invoke(this, catchClause);
        }

        public override void VisitFunctionDeclaration(FunctionDeclaration functionDeclaration)
        {
            VisitingFunctionDeclaration?.Invoke(this, functionDeclaration);
            base.VisitFunctionDeclaration(functionDeclaration);
            VisitedFunctionDeclaration?.Invoke(this, functionDeclaration);
        }

        public override void VisitWithStatement(WithStatement withStatement)
        {
            VisitingWithStatement?.Invoke(this, withStatement);
            base.VisitWithStatement(withStatement);
            VisitedWithStatement?.Invoke(this, withStatement);
        }

        public override void VisitWhileStatement(WhileStatement whileStatement)
        {
            VisitingWhileStatement?.Invoke(this, whileStatement);
            base.VisitWhileStatement(whileStatement);
            VisitedWhileStatement?.Invoke(this, whileStatement);
        }

        public override void VisitVariableDeclaration(VariableDeclaration variableDeclaration)
        {
            VisitingVariableDeclaration?.Invoke(this, variableDeclaration);
            base.VisitVariableDeclaration(variableDeclaration);
            VisitedVariableDeclaration?.Invoke(this, variableDeclaration);
        }

        public override void VisitTryStatement(TryStatement tryStatement)
        {
            VisitingTryStatement?.Invoke(this, tryStatement);
            base.VisitTryStatement(tryStatement);
            VisitedTryStatement?.Invoke(this, tryStatement);
        }

        public override void VisitThrowStatement(ThrowStatement throwStatement)
        {
            VisitingThrowStatement?.Invoke(this, throwStatement);
            base.VisitThrowStatement(throwStatement);
            VisitedThrowStatement?.Invoke(this, throwStatement);
        }

        public override void VisitSwitchStatement(SwitchStatement switchStatement)
        {
            VisitingSwitchStatement?.Invoke(this, switchStatement);
            base.VisitSwitchStatement(switchStatement);
            VisitedSwitchStatement?.Invoke(this, switchStatement);
        }

        public override void VisitSwitchCase(SwitchCase switchCase)
        {
            VisitingSwitchCase?.Invoke(this, switchCase);
            base.VisitSwitchCase(switchCase);
            VisitedSwitchCase?.Invoke(this, switchCase);
        }

        public override void VisitReturnStatement(ReturnStatement returnStatement)
        {
            VisitingReturnStatement?.Invoke(this, returnStatement);
            base.VisitReturnStatement(returnStatement);
            VisitedReturnStatement?.Invoke(this, returnStatement);
        }

        public override void VisitLabeledStatement(LabeledStatement labeledStatement)
        {
            VisitingLabeledStatement?.Invoke(this, labeledStatement);
            base.VisitLabeledStatement(labeledStatement);
            VisitedLabeledStatement?.Invoke(this, labeledStatement);
        }

        public override void VisitIfStatement(IfStatement ifStatement)
        {
            VisitingIfStatement?.Invoke(this, ifStatement);
            base.VisitIfStatement(ifStatement);
            VisitedIfStatement?.Invoke(this, ifStatement);
        }

        public override void VisitEmptyStatement(EmptyStatement emptyStatement)
        {
            VisitingEmptyStatement?.Invoke(this, emptyStatement);
            base.VisitEmptyStatement(emptyStatement);
            VisitedEmptyStatement?.Invoke(this, emptyStatement);
        }

        public override void VisitDebuggerStatement(DebuggerStatement debuggerStatement)
        {
            VisitingDebuggerStatement?.Invoke(this, debuggerStatement);
            base.VisitDebuggerStatement(debuggerStatement);
            VisitedDebuggerStatement?.Invoke(this, debuggerStatement);
        }

        public override void VisitExpressionStatement(ExpressionStatement expressionStatement)
        {
            VisitingExpressionStatement?.Invoke(this, expressionStatement);
            base.VisitExpressionStatement(expressionStatement);
            VisitedExpressionStatement?.Invoke(this, expressionStatement);
        }

        public override void VisitForStatement(ForStatement forStatement)
        {
            VisitingForStatement?.Invoke(this, forStatement);
            base.VisitForStatement(forStatement);
            VisitedForStatement?.Invoke(this, forStatement);
        }

        public override void VisitForInStatement(ForInStatement forInStatement)
        {
            VisitingForInStatement?.Invoke(this, forInStatement);
            base.VisitForInStatement(forInStatement);
            VisitedForInStatement?.Invoke(this, forInStatement);
        }

        public override void VisitDoWhileStatement(DoWhileStatement doWhileStatement)
        {
            VisitingDoWhileStatement?.Invoke(this, doWhileStatement);
            base.VisitDoWhileStatement(doWhileStatement);
            VisitedDoWhileStatement?.Invoke(this, doWhileStatement);
        }

        public override void VisitExpression(Expression expression)
        {
            VisitingExpression?.Invoke(this, expression);
            base.VisitExpression(expression);
            VisitedExpression?.Invoke(this, expression);
        }

        public override void VisitArrowFunctionExpression(ArrowFunctionExpression arrowFunctionExpression)
        {
            VisitingArrowFunctionExpression?.Invoke(this, arrowFunctionExpression);
            base.VisitArrowFunctionExpression(arrowFunctionExpression);
            VisitedArrowFunctionExpression?.Invoke(this, arrowFunctionExpression);
        }

        public override void VisitUnaryExpression(UnaryExpression unaryExpression)
        {
            VisitingUnaryExpression?.Invoke(this, unaryExpression);
            base.VisitUnaryExpression(unaryExpression);
            VisitedUnaryExpression?.Invoke(this, unaryExpression);
        }

        public override void VisitUpdateExpression(UpdateExpression updateExpression)
        {
            VisitingUpdateExpression?.Invoke(this, updateExpression);
            base.VisitUpdateExpression(updateExpression);
            VisitedUpdateExpression?.Invoke(this, updateExpression);
        }

        public override void VisitThisExpression(ThisExpression thisExpression)
        {
            VisitingThisExpression?.Invoke(this, thisExpression);
            base.VisitThisExpression(thisExpression);
            VisitedThisExpression?.Invoke(this, thisExpression);
        }

        public override void VisitSequenceExpression(SequenceExpression sequenceExpression)
        {
            VisitingSequenceExpression?.Invoke(this, sequenceExpression);
            base.VisitSequenceExpression(sequenceExpression);
            VisitedSequenceExpression?.Invoke(this, sequenceExpression);
        }

        public override void VisitObjectExpression(ObjectExpression objectExpression)
        {
            VisitingObjectExpression?.Invoke(this, objectExpression);
            base.VisitObjectExpression(objectExpression);
            VisitedObjectExpression?.Invoke(this, objectExpression);
        }

        public override void VisitNewExpression(NewExpression newExpression)
        {
            VisitingNewExpression?.Invoke(this, newExpression);
            base.VisitNewExpression(newExpression);
            VisitedNewExpression?.Invoke(this, newExpression);
        }

        public override void VisitMemberExpression(MemberExpression memberExpression)
        {
            VisitingMemberExpression?.Invoke(this, memberExpression);
            base.VisitMemberExpression(memberExpression);
            VisitedMemberExpression?.Invoke(this, memberExpression);
        }

        public override void VisitLogicalExpression(BinaryExpression binaryExpression)
        {
            VisitingLogicalExpression?.Invoke(this, binaryExpression);
            base.VisitLogicalExpression(binaryExpression);
            VisitedLogicalExpression?.Invoke(this, binaryExpression);
        }

        public override void VisitLiteral(Literal literal)
        {
            VisitingLiteral?.Invoke(this, literal);
            base.VisitLiteral(literal);
            VisitedLiteral?.Invoke(this, literal);
        }

        public override void VisitIdentifier(Identifier identifier)
        {
            VisitingIdentifier?.Invoke(this, identifier);
            base.VisitIdentifier(identifier);
            VisitedIdentifier?.Invoke(this, identifier);
        }

        public override void VisitFunctionExpression(IFunction function)
        {
            VisitingFunctionExpression?.Invoke(this, function);
            base.VisitFunctionExpression(function);
            VisitedFunctionExpression?.Invoke(this, function);
        }

        public override void VisitClassExpression(ClassExpression classExpression)
        {
            VisitingClassExpression?.Invoke(this, classExpression);
            base.VisitClassExpression(classExpression);
            VisitedClassExpression?.Invoke(this, classExpression);
        }

        public override void VisitExportDefaultDeclaration(ExportDefaultDeclaration exportDefaultDeclaration)
        {
            VisitingExportDefaultDeclaration?.Invoke(this, exportDefaultDeclaration);
            base.VisitExportDefaultDeclaration(exportDefaultDeclaration);
            VisitedExportDefaultDeclaration?.Invoke(this, exportDefaultDeclaration);
        }

        public override void VisitExportAllDeclaration(ExportAllDeclaration exportAllDeclaration)
        {
            VisitingExportAllDeclaration?.Invoke(this, exportAllDeclaration);
            base.VisitExportAllDeclaration(exportAllDeclaration);
            VisitedExportAllDeclaration?.Invoke(this, exportAllDeclaration);
        }

        public override void VisitExportNamedDeclaration(ExportNamedDeclaration exportNamedDeclaration)
        {
            VisitingExportNamedDeclaration?.Invoke(this, exportNamedDeclaration);
            base.VisitExportNamedDeclaration(exportNamedDeclaration);
            VisitedExportNamedDeclaration?.Invoke(this, exportNamedDeclaration);
        }

        public override void VisitExportSpecifier(ExportSpecifier exportSpecifier)
        {
            VisitingExportSpecifier?.Invoke(this, exportSpecifier);
            base.VisitExportSpecifier(exportSpecifier);
            VisitedExportSpecifier?.Invoke(this, exportSpecifier);
        }

        public override void VisitImportDeclaration(ImportDeclaration importDeclaration)
        {
            VisitingImportDeclaration?.Invoke(this, importDeclaration);
            base.VisitImportDeclaration(importDeclaration);
            VisitedImportDeclaration?.Invoke(this, importDeclaration);
        }

        public override void VisitImportNamespaceSpecifier(ImportNamespaceSpecifier importNamespaceSpecifier)
        {
            VisitingImportNamespaceSpecifier?.Invoke(this, importNamespaceSpecifier);
            base.VisitImportNamespaceSpecifier(importNamespaceSpecifier);
            VisitedImportNamespaceSpecifier?.Invoke(this, importNamespaceSpecifier);
        }

        public override void VisitImportDefaultSpecifier(ImportDefaultSpecifier importDefaultSpecifier)
        {
            VisitingImportDefaultSpecifier?.Invoke(this, importDefaultSpecifier);
            base.VisitImportDefaultSpecifier(importDefaultSpecifier);
            VisitedImportDefaultSpecifier?.Invoke(this, importDefaultSpecifier);
        }

        public override void VisitImportSpecifier(ImportSpecifier importSpecifier)
        {
            VisitingImportSpecifier?.Invoke(this, importSpecifier);
            base.VisitImportSpecifier(importSpecifier);
            VisitedImportSpecifier?.Invoke(this, importSpecifier);
        }

        public override void VisitMethodDefinition(MethodDefinition methodDefinitions)
        {
            VisitingMethodDefinition?.Invoke(this, methodDefinitions);
            base.VisitMethodDefinition(methodDefinitions);
            VisitedMethodDefinition?.Invoke(this, methodDefinitions);
        }

        public override void VisitForOfStatement(ForOfStatement forOfStatement)
        {
            VisitingForOfStatement?.Invoke(this, forOfStatement);
            base.VisitForOfStatement(forOfStatement);
            VisitedForOfStatement?.Invoke(this, forOfStatement);
        }

        public override void VisitClassDeclaration(ClassDeclaration classDeclaration)
        {
            VisitingClassDeclaration?.Invoke(this, classDeclaration);
            base.VisitClassDeclaration(classDeclaration);
            VisitedClassDeclaration?.Invoke(this, classDeclaration);
        }

        public override void VisitClassBody(ClassBody classBody)
        {
            VisitingClassBody?.Invoke(this, classBody);
            base.VisitClassBody(classBody);
            VisitedClassBody?.Invoke(this, classBody);
        }

        public override void VisitYieldExpression(YieldExpression yieldExpression)
        {
            VisitingYieldExpression?.Invoke(this, yieldExpression);
            base.VisitYieldExpression(yieldExpression);
            VisitedYieldExpression?.Invoke(this, yieldExpression);
        }

        public override void VisitTaggedTemplateExpression(TaggedTemplateExpression taggedTemplateExpression)
        {
            VisitingTaggedTemplateExpression?.Invoke(this, taggedTemplateExpression);
            base.VisitTaggedTemplateExpression(taggedTemplateExpression);
            VisitedTaggedTemplateExpression?.Invoke(this, taggedTemplateExpression);
        }

        public override void VisitSuper(Super super)
        {
            VisitingSuper?.Invoke(this, super);
            base.VisitSuper(super);
            VisitedSuper?.Invoke(this, super);
        }

        public override void VisitMetaProperty(MetaProperty metaProperty)
        {
            VisitingMetaProperty?.Invoke(this, metaProperty);
            base.VisitMetaProperty(metaProperty);
            VisitedMetaProperty?.Invoke(this, metaProperty);
        }

        public override void VisitArrowParameterPlaceHolder(ArrowParameterPlaceHolder arrowParameterPlaceHolder)
        {
            VisitingArrowParameterPlaceHolder?.Invoke(this, arrowParameterPlaceHolder);
            base.VisitArrowParameterPlaceHolder(arrowParameterPlaceHolder);
            VisitedArrowParameterPlaceHolder?.Invoke(this, arrowParameterPlaceHolder);
        }

        public override void VisitObjectPattern(ObjectPattern objectPattern)
        {
            VisitingObjectPattern?.Invoke(this, objectPattern);
            base.VisitObjectPattern(objectPattern);
            VisitedObjectPattern?.Invoke(this, objectPattern);
        }

        public override void VisitSpreadElement(SpreadElement spreadElement)
        {
            VisitingSpreadElement?.Invoke(this, spreadElement);
            base.VisitSpreadElement(spreadElement);
            VisitedSpreadElement?.Invoke(this, spreadElement);
        }

        public override void VisitAssignmentPattern(AssignmentPattern assignmentPattern)
        {
            VisitingAssignmentPattern?.Invoke(this, assignmentPattern);
            base.VisitAssignmentPattern(assignmentPattern);
            VisitedAssignmentPattern?.Invoke(this, assignmentPattern);
        }

        public override void VisitArrayPattern(ArrayPattern arrayPattern)
        {
            VisitingArrayPattern?.Invoke(this, arrayPattern);
            base.VisitArrayPattern(arrayPattern);
            VisitedArrayPattern?.Invoke(this, arrayPattern);
        }

        public override void VisitVariableDeclarator(VariableDeclarator variableDeclarator)
        {
            VisitingVariableDeclarator?.Invoke(this, variableDeclarator);
            base.VisitVariableDeclarator(variableDeclarator);
            VisitedVariableDeclarator?.Invoke(this, variableDeclarator);
        }

        public override void VisitTemplateLiteral(TemplateLiteral templateLiteral)
        {
            VisitingTemplateLiteral?.Invoke(this, templateLiteral);
            base.VisitTemplateLiteral(templateLiteral);
            VisitedTemplateLiteral?.Invoke(this, templateLiteral);
        }

        public override void VisitTemplateElement(TemplateElement templateElement)
        {
            VisitingTemplateElement?.Invoke(this, templateElement);
            base.VisitTemplateElement(templateElement);
            VisitedTemplateElement?.Invoke(this, templateElement);
        }

        public override void VisitRestElement(RestElement restElement)
        {
            VisitingRestElement?.Invoke(this, restElement);
            base.VisitRestElement(restElement);
            VisitedRestElement?.Invoke(this, restElement);
        }

        public override void VisitProperty(Property property)
        {
            VisitingProperty?.Invoke(this, property);
            base.VisitProperty(property);
            VisitedProperty?.Invoke(this, property);
        }

        public override void VisitConditionalExpression(ConditionalExpression conditionalExpression)
        {
            VisitingConditionalExpression?.Invoke(this, conditionalExpression);
            base.VisitConditionalExpression(conditionalExpression);
            VisitedConditionalExpression?.Invoke(this, conditionalExpression);
        }

        public override void VisitCallExpression(CallExpression callExpression)
        {
            VisitingCallExpression?.Invoke(this, callExpression);
            base.VisitCallExpression(callExpression);
            VisitedCallExpression?.Invoke(this, callExpression);
        }

        public override void VisitBinaryExpression(BinaryExpression binaryExpression)
        {
            VisitingBinaryExpression?.Invoke(this, binaryExpression);
            base.VisitBinaryExpression(binaryExpression);
            VisitedBinaryExpression?.Invoke(this, binaryExpression);
        }

        public override void VisitArrayExpression(ArrayExpression arrayExpression)
        {
            VisitingArrayExpression?.Invoke(this, arrayExpression);
            base.VisitArrayExpression(arrayExpression);
            VisitedArrayExpression?.Invoke(this, arrayExpression);
        }

        public override void VisitAssignmentExpression(AssignmentExpression assignmentExpression)
        {
            VisitingAssignmentExpression?.Invoke(this, assignmentExpression);
            base.VisitAssignmentExpression(assignmentExpression);
            VisitedAssignmentExpression?.Invoke(this, assignmentExpression);
        }

        public override void VisitContinueStatement(ContinueStatement continueStatement)
        {
            VisitingContinueStatement?.Invoke(this, continueStatement);
            base.VisitContinueStatement(continueStatement);
            VisitedContinueStatement?.Invoke(this, continueStatement);
        }

        public override void VisitBreakStatement(BreakStatement breakStatement)
        {
            VisitingBreakStatement?.Invoke(this, breakStatement);
            base.VisitBreakStatement(breakStatement);
            VisitedBreakStatement?.Invoke(this, breakStatement);
        }

        public override void VisitBlockStatement(BlockStatement blockStatement)
        {
            VisitingBlockStatement?.Invoke(this, blockStatement);
            base.VisitBlockStatement(blockStatement);
            VisitedBlockStatement?.Invoke(this, blockStatement);
        }
    }
}
