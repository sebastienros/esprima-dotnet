using System.Runtime.CompilerServices;
using Esprima.Ast;
using static Esprima.Utils.JavascriptTextWriter;

namespace Esprima.Utils;

public partial class AstToJavascriptConverter : AstVisitor
{
    // Notes for maintainers:
    // Don't visit nodes directly (by calling Visit) unless it's necessary for some special reason (but in that case you'll need to setup the context of the visitation manually!)
    // For examples of special reason, see VisitArrayExpression, VisitObjectExpression, VisitImport, etc. In usual cases just use the following predefined visitation helper methods:
    // * Visit statements using VisitStatement / VisitStatementList.
    // * Visit expressions using VisitRootExpression and sub-expressions (expressions inside another expression) using VisitSubExpression / VisitSubExpressionList.
    // * Visit identifiers using VisitAuxiliaryNode when they are binding identifiers (declarations) and visit them using VisitRootExpression when they are identifier references (actual expressions).
    // * Visit any other nodes using VisitAuxiliaryNode / VisitAuxiliaryNodeList.

    private static readonly object s_lastSwitchCaseFlag = new();
    private static readonly object s_forLoopInitDeclarationFlag = new();
    private static readonly object s_bindingPatternAllowsExpressionsFlag = new(); // automatically propagated to sub-patterns

    private readonly bool _ignoreExtensions;

    private WriteContext _writeContext;
    private StatementFlags _currentStatementFlags;
    private ExpressionFlags _currentExpressionFlags;
    private object? _currentAuxiliaryNodeContext;

    public AstToJavascriptConverter(JavascriptTextWriter writer, AstToJavascriptOptions options)
    {
        Writer = writer ?? throw new ArgumentNullException(nameof(writer));

        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        _ignoreExtensions = options.IgnoreExtensions;
    }

    public JavascriptTextWriter Writer { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    protected ref WriteContext WriteContext { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _writeContext; }

    protected Node? ParentNode { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _writeContext.ParentNode; }

    public void Convert(Node node)
    {
        _writeContext = default;
        _currentStatementFlags = StatementFlags.None;
        _currentExpressionFlags = ExpressionFlags.None;
        _currentAuxiliaryNodeContext = null;

        Visit(node ?? throw new ArgumentNullException(nameof(node)));

        Writer.Finish();
    }

    public override object? Visit(Node node)
    {
        var originalWriteContext = _writeContext;
        _writeContext = new WriteContext(originalWriteContext.Node, node);

        var result = base.Visit(node);

        _writeContext = originalWriteContext;

        return result;
    }

    protected internal override object? VisitArrayExpression(ArrayExpression arrayExpression)
    {
        _writeContext.SetNodeProperty(nameof(arrayExpression.Elements), static node => ref node.As<ArrayExpression>().Elements);

        Writer.StartArray(arrayExpression.Elements.Count, ref _writeContext);

        // Elements need special care because it may contain null values denoting omitted elements.

        Writer.StartExpressionList(arrayExpression.Elements.Count, ref _writeContext);

        for (var i = 0; i < arrayExpression.Elements.Count; i++)
        {
            var element = arrayExpression.Elements[i];

            if (element is not null)
            {
                VisitExpressionListItem(element, i, arrayExpression.Elements.Count, static (@this, expression, index, _) =>
                    s_getCombinedSubExpressionFlags(@this, expression, SubExpressionFlags(@this.ExpressionNeedsBracketsInList(expression), isLeftMost: false)));
            }
            else
            {
                var originalExpressionFlags = _currentExpressionFlags;
                _currentExpressionFlags = PropagateExpressionFlags(SubExpressionFlags(needsBrackets: false, isLeftMost: false));

                Writer.StartExpressionListItem(i, arrayExpression.Elements.Count, (JavascriptTextWriter.ExpressionFlags) _currentExpressionFlags, ref _writeContext);
                Writer.EndExpressionListItem(i, arrayExpression.Elements.Count, (JavascriptTextWriter.ExpressionFlags) _currentExpressionFlags, ref _writeContext);

                _currentExpressionFlags = originalExpressionFlags;
            }
        }

        Writer.EndExpressionList(arrayExpression.Elements.Count, ref _writeContext);

        Writer.EndArray(arrayExpression.Elements.Count, ref _writeContext);

        return arrayExpression;
    }

    protected internal override object? VisitArrayPattern(ArrayPattern arrayPattern)
    {
        _writeContext.SetNodeProperty(nameof(arrayPattern.Elements), static node => ref node.As<ArrayPattern>().Elements);

        Writer.StartArray(arrayPattern.Elements.Count, ref _writeContext);

        // Elements need special care because it may contain null values denoting omitted elements.

        Writer.StartAuxiliaryNodeList<Node?>(arrayPattern.Elements.Count, ref _writeContext);

        for (var i = 0; i < arrayPattern.Elements.Count; i++)
        {
            var element = arrayPattern.Elements[i];

            if (element is not null)
            {
                if (_currentAuxiliaryNodeContext != s_bindingPatternAllowsExpressionsFlag)
                {
                    VisitAuxiliaryNodeListItem(element, i, arrayPattern.Elements.Count, separator: ",", static delegate { return null; });
                }
                else if (element is not Expression expression)
                {
                    VisitAuxiliaryNodeListItem(element, i, arrayPattern.Elements.Count, separator: ",", static delegate { return s_bindingPatternAllowsExpressionsFlag; }); // propagate flag to sub-patterns
                }
                else
                {
                    var originalAuxiliaryNodeContext = _currentAuxiliaryNodeContext;
                    _currentAuxiliaryNodeContext = null;

                    Writer.StartAuxiliaryNodeListItem<Node?>(i, arrayPattern.Elements.Count, separator: ",", _currentAuxiliaryNodeContext, ref _writeContext);
                    VisitRootExpression(expression, RootExpressionFlags(needsBrackets: ExpressionNeedsBracketsInList(expression)));
                    Writer.EndAuxiliaryNodeListItem<Node?>(i, arrayPattern.Elements.Count, separator: ",", _currentAuxiliaryNodeContext, ref _writeContext);

                    _currentAuxiliaryNodeContext = originalAuxiliaryNodeContext;
                }
            }
            else
            {
                var originalAuxiliaryNodeContext = _currentAuxiliaryNodeContext;
                _currentAuxiliaryNodeContext = null;

                Writer.StartAuxiliaryNodeListItem<Node?>(i, arrayPattern.Elements.Count, separator: ",", _currentAuxiliaryNodeContext, ref _writeContext);
                Writer.EndAuxiliaryNodeListItem<Node?>(i, arrayPattern.Elements.Count, separator: ",", _currentAuxiliaryNodeContext, ref _writeContext);

                _currentAuxiliaryNodeContext = originalAuxiliaryNodeContext;
            }
        }

        Writer.EndAuxiliaryNodeList<Node?>(arrayPattern.Elements.Count, ref _writeContext);

        Writer.EndArray(arrayPattern.Elements.Count, ref _writeContext);

        return arrayPattern;
    }

    protected internal override object? VisitArrowFunctionExpression(ArrowFunctionExpression arrowFunctionExpression)
    {
        if (arrowFunctionExpression.Async)
        {
            _writeContext.SetNodeProperty(nameof(arrowFunctionExpression.Async), static node => node.As<ArrowFunctionExpression>().Async);
            Writer.WriteKeyword("async", TokenFlags.TrailingSpaceRecommended, ref _writeContext);
        }

        _writeContext.SetNodeProperty(nameof(arrowFunctionExpression.Params), static node => ref node.As<ArrowFunctionExpression>().Params);

        if (arrowFunctionExpression.Params.Count == 1 && arrowFunctionExpression.Params[0].Type == Nodes.Identifier)
        {
            VisitAuxiliaryNodeList(in arrowFunctionExpression.Params, separator: ",");
        }
        else
        {
            Writer.WritePunctuator("(", TokenFlags.Leading, ref _writeContext);
            VisitAuxiliaryNodeList(in arrowFunctionExpression.Params, separator: ",");
            Writer.WritePunctuator(")", TokenFlags.Trailing, ref _writeContext);
        }

        _writeContext.ClearNodeProperty();
        Writer.WritePunctuator("=>", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

        _writeContext.SetNodeProperty(nameof(arrowFunctionExpression.Body), static node => node.As<ArrowFunctionExpression>().Body);
        if (arrowFunctionExpression.Body is BlockStatement bodyBlockStatement)
        {
            VisitStatement(bodyBlockStatement, StatementFlags.IsRightMost);
        }
        else
        {
            var bodyExpression = arrowFunctionExpression.Body.As<Expression>();
            var bodyNeedsBrackets = UnaryOperandNeedsBrackets(arrowFunctionExpression, bodyExpression);
            VisitExpression(bodyExpression, SubExpressionFlags(bodyNeedsBrackets, isLeftMost: false), static (@this, expression, flags) =>
                @this.DisambiguateExpression(expression, ExpressionFlags.IsInsideArrowFunctionBody | ExpressionFlags.IsLeftMostInArrowFunctionBody | @this.PropagateExpressionFlags(flags)));
        }

        return arrowFunctionExpression;
    }

    protected internal override object? VisitAssignmentExpression(AssignmentExpression assignmentExpression)
    {
        _writeContext.SetNodeProperty(nameof(assignmentExpression.Left), static node => node.As<AssignmentExpression>().Left);
        if (assignmentExpression.Left is Expression leftExpression)
        {
            VisitSubExpression(leftExpression, SubExpressionFlags(needsBrackets: false, isLeftMost: true));
        }
        else
        {
            VisitAuxiliaryNode(assignmentExpression.Left, static delegate { return s_bindingPatternAllowsExpressionsFlag; });
        }

        var op = AssignmentExpression.GetAssignmentOperatorToken(assignmentExpression.Operator);

        _writeContext.SetNodeProperty(nameof(assignmentExpression.Operator), static node => node.As<AssignmentExpression>().Operator);
        Writer.WritePunctuator(op, TokenFlags.InBetween | TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

        // AssignmentExpression is not a real binary operation because its left side is not an expression. 
        var rightNeedsBrackets = GetOperatorPrecedence(assignmentExpression, out _) > GetOperatorPrecedence(assignmentExpression.Right, out _);

        _writeContext.SetNodeProperty(nameof(assignmentExpression.Right), static node => node.As<AssignmentExpression>().Right);
        VisitSubExpression(assignmentExpression.Right, SubExpressionFlags(rightNeedsBrackets, isLeftMost: false));

        return assignmentExpression;
    }

    protected internal override object? VisitAssignmentPattern(AssignmentPattern assignmentPattern)
    {
        _writeContext.SetNodeProperty(nameof(assignmentPattern.Left), static node => node.As<AssignmentPattern>().Left);
        if (_currentAuxiliaryNodeContext != s_bindingPatternAllowsExpressionsFlag)
        {
            VisitAuxiliaryNode(assignmentPattern.Left);
        }
        else if (assignmentPattern.Left is not Expression leftExpression)
        {
            VisitAuxiliaryNode(assignmentPattern.Left, static delegate { return s_bindingPatternAllowsExpressionsFlag; }); // propagate flag to sub-patterns
        }
        else
        {
            VisitRootExpression(leftExpression, RootExpressionFlags(needsBrackets: ExpressionNeedsBracketsInList(leftExpression)));
        }

        _writeContext.ClearNodeProperty();
        Writer.WritePunctuator("=", TokenFlags.InBetween | TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

        _writeContext.SetNodeProperty(nameof(assignmentPattern.Right), static node => node.As<AssignmentPattern>().Right);
        VisitRootExpression(assignmentPattern.Right, RootExpressionFlags(needsBrackets: false));

        return assignmentPattern;
    }

    protected internal override object? VisitAwaitExpression(AwaitExpression awaitExpression)
    {
        Writer.WriteKeyword("await", TokenFlags.TrailingSpaceRecommended, ref _writeContext);

        var argumentNeedsBrackets = UnaryOperandNeedsBrackets(awaitExpression, awaitExpression.Argument);

        _writeContext.SetNodeProperty(nameof(awaitExpression.Argument), static node => node.As<AwaitExpression>().Argument);
        VisitSubExpression(awaitExpression.Argument, SubExpressionFlags(argumentNeedsBrackets, isLeftMost: false));

        return awaitExpression;
    }

    protected internal override object? VisitBinaryExpression(BinaryExpression binaryExpression)
    {
        var operationFlags = BinaryOperandsNeedBrackets(binaryExpression, binaryExpression.Left, binaryExpression.Right);

        // The operand of unary operators cannot be an exponentiation without grouping.
        // E.g. -1 ** 2 is syntactically unambiguous but the language requires (-1) ** 2 instead.
        if (!operationFlags.HasFlagFast(BinaryOperationFlags.LeftOperandNeedsBrackets) &&
            binaryExpression.Operator == BinaryOperator.Exponentiation &&
            binaryExpression.Left is UnaryExpression leftUnaryExpression)
        {
            operationFlags |= BinaryOperationFlags.LeftOperandNeedsBrackets;
        }

        _writeContext.SetNodeProperty(nameof(binaryExpression.Left), static node => node.As<BinaryExpression>().Left);
        VisitSubExpression(binaryExpression.Left, SubExpressionFlags(operationFlags.HasFlagFast(BinaryOperationFlags.LeftOperandNeedsBrackets), isLeftMost: true));

        var op = BinaryExpression.GetBinaryOperatorToken(binaryExpression.Operator);

        _writeContext.SetNodeProperty(nameof(binaryExpression.Operator), static node => node.As<BinaryExpression>().Operator);
        if (char.IsLetter(op[0]))
        {
            Writer.WriteKeyword(op, TokenFlags.SurroundingSpaceRecommended, ref _writeContext);
        }
        else
        {
            Writer.WritePunctuator(op, TokenFlags.InBetween | TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

            // Cases like 1 + (+x) must be disambiguated with brackets.
            if (!operationFlags.HasFlagFast(BinaryOperationFlags.RightOperandNeedsBrackets) &&
                binaryExpression.Right is UnaryExpression rightUnaryExpression &&
                rightUnaryExpression.Prefix &&
                op[op.Length - 1] is '+' or '-' &&
                op[op.Length - 1] == UnaryExpression.GetUnaryOperatorToken(rightUnaryExpression.Operator)[0])
            {
                operationFlags |= BinaryOperationFlags.RightOperandNeedsBrackets;
            }
        }

        _writeContext.SetNodeProperty(nameof(binaryExpression.Right), static node => node.As<BinaryExpression>().Right);
        VisitSubExpression(binaryExpression.Right, SubExpressionFlags(operationFlags.HasFlagFast(BinaryOperationFlags.RightOperandNeedsBrackets), isLeftMost: false));

        return binaryExpression;
    }

    protected internal override object? VisitBlockStatement(BlockStatement blockStatement)
    {
        _writeContext.SetNodeProperty(nameof(blockStatement.Body), static node => ref node.As<BlockStatement>().Body);
        Writer.StartBlock(blockStatement.Body.Count, ref _writeContext);

        VisitStatementList(in blockStatement.Body);

        Writer.EndBlock(blockStatement.Body.Count, ref _writeContext);

        return blockStatement;
    }

    protected internal override object? VisitBreakStatement(BreakStatement breakStatement)
    {
        Writer.WriteKeyword("break", TokenFlags.LeadingSpaceRecommended, ref _writeContext);

        if (breakStatement.Label is not null)
        {
            _writeContext.SetNodeProperty(nameof(breakStatement.Label), static node => node.As<BreakStatement>().Label);
            VisitRootExpression(breakStatement.Label, RootExpressionFlags(needsBrackets: false));
        }

        StatementNeedsSemicolon();

        return breakStatement;
    }

    protected internal override object? VisitCallExpression(CallExpression callExpression)
    {
        var calleeNeedsBrackets = UnaryOperandNeedsBrackets(callExpression, callExpression.Callee);

        _writeContext.SetNodeProperty(nameof(callExpression.Callee), static node => node.As<CallExpression>().Callee);
        VisitSubExpression(callExpression.Callee, SubExpressionFlags(calleeNeedsBrackets, isLeftMost: true));

        if (callExpression.Optional)
        {
            _writeContext.ClearNodeProperty();
            Writer.WritePunctuator("?.", TokenFlags.InBetween, ref _writeContext);
        }

        _writeContext.SetNodeProperty(nameof(callExpression.Arguments), static node => ref node.As<CallExpression>().Arguments);
        Writer.WritePunctuator("(", TokenFlags.Leading, ref _writeContext);
        VisitSubExpressionList(in callExpression.Arguments);
        Writer.WritePunctuator(")", TokenFlags.Trailing, ref _writeContext);

        return callExpression;
    }

    protected internal override object? VisitCatchClause(CatchClause catchClause)
    {
        if (catchClause.Param is not null)
        {
            _writeContext.SetNodeProperty(nameof(catchClause.Param), static node => node.As<CatchClause>().Param);
            Writer.WritePunctuator("(", TokenFlags.Leading | TokenFlags.LeadingSpaceRecommended, ref _writeContext);
            VisitAuxiliaryNode(catchClause.Param);
            Writer.WritePunctuator(")", TokenFlags.Trailing | TokenFlags.TrailingSpaceRecommended, ref _writeContext);
        }

        _writeContext.SetNodeProperty(nameof(catchClause.Body), static node => node.As<CatchClause>().Body);
        VisitStatement(catchClause.Body, StatementBodyFlags(isRightMost: ParentNode!.As<TryStatement>().Finalizer is null));

        return catchClause;
    }

    protected internal override object? VisitChainExpression(ChainExpression chainExpression)
    {
        _writeContext.SetNodeProperty(nameof(chainExpression.Expression), static node => node.As<ChainExpression>().Expression);
        VisitSubExpression(chainExpression.Expression, SubExpressionFlags(needsBrackets: false, isLeftMost: true));

        return chainExpression;
    }

    protected internal override object? VisitClassBody(ClassBody classBody)
    {
        _writeContext.SetNodeProperty(nameof(classBody.Body), static node => ref node.As<ClassBody>().Body);
        Writer.StartBlock(classBody.Body.Count, ref _writeContext);

        VisitAuxiliaryNodeList(in classBody.Body, separator: string.Empty);

        Writer.EndBlock(classBody.Body.Count, ref _writeContext);

        return classBody;
    }

    protected internal override object? VisitClassDeclaration(ClassDeclaration classDeclaration)
    {
        if (classDeclaration.Decorators.Count > 0)
        {
            _writeContext.SetNodeProperty(nameof(classDeclaration.Decorators), static node => ref node.As<ClassDeclaration>().Decorators);
            VisitAuxiliaryNodeList(classDeclaration.Decorators, separator: string.Empty);

            _writeContext.ClearNodeProperty();
        }

        Writer.WriteKeyword("class", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

        if (classDeclaration.Id is not null)
        {
            _writeContext.SetNodeProperty(nameof(classDeclaration.Id), static node => node.As<ClassDeclaration>().Id);
            VisitAuxiliaryNode(classDeclaration.Id);
        }

        if (classDeclaration.SuperClass is not null)
        {
            _writeContext.ClearNodeProperty();
            Writer.WriteKeyword("extends", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

            _writeContext.SetNodeProperty(nameof(classDeclaration.SuperClass), static node => node.As<ClassDeclaration>().SuperClass);
            VisitRootExpression(classDeclaration.SuperClass, LeftHandSideRootExpressionFlags(needsBrackets: false));
        }

        _writeContext.SetNodeProperty(nameof(classDeclaration.Body), static node => node.As<ClassDeclaration>().Body);
        VisitAuxiliaryNode(classDeclaration.Body);

        return classDeclaration;
    }

    protected internal override object? VisitClassExpression(ClassExpression classExpression)
    {
        if (classExpression.Decorators.Count > 0)
        {
            _writeContext.SetNodeProperty(nameof(classExpression.Decorators), static node => ref node.As<ClassExpression>().Decorators);
            VisitAuxiliaryNodeList(classExpression.Decorators, separator: string.Empty);

            _writeContext.ClearNodeProperty();
        }

        Writer.WriteKeyword("class", TokenFlags.TrailingSpaceRecommended, ref _writeContext);

        if (classExpression.Id is not null)
        {
            _writeContext.SetNodeProperty(nameof(classExpression.Id), static node => node.As<ClassExpression>().Id);
            VisitAuxiliaryNode(classExpression.Id);
        }

        if (classExpression.SuperClass is not null)
        {
            _writeContext.ClearNodeProperty();
            Writer.WriteKeyword("extends", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

            _writeContext.SetNodeProperty(nameof(classExpression.SuperClass), static node => node.As<ClassExpression>().SuperClass);
            VisitRootExpression(classExpression.SuperClass, LeftHandSideRootExpressionFlags(needsBrackets: false));
        }

        _writeContext.SetNodeProperty(nameof(classExpression.Body), static node => node.As<ClassExpression>().Body);
        VisitAuxiliaryNode(classExpression.Body);

        return classExpression;
    }

    protected internal override object? VisitConditionalExpression(ConditionalExpression conditionalExpression)
    {
        // Test expressions with the same precendence as ternary operator (such as nested conditional expression, assignment, yield, etc.) also needs brackets.
        var operandNeedsBrackets = GetOperatorPrecedence(conditionalExpression, out _) >= GetOperatorPrecedence(conditionalExpression.Test, out _);

        _writeContext.SetNodeProperty(nameof(conditionalExpression.Test), static node => node.As<ConditionalExpression>().Test);
        VisitSubExpression(conditionalExpression.Test, SubExpressionFlags(operandNeedsBrackets, isLeftMost: true));

        // Consequent expressions with the same precendence as ternary operator are unambiguous without brackets.
        operandNeedsBrackets = GetOperatorPrecedence(conditionalExpression, out _) > GetOperatorPrecedence(conditionalExpression.Consequent, out _);

        _writeContext.SetNodeProperty(nameof(conditionalExpression.Consequent), static node => node.As<ConditionalExpression>().Consequent);
        Writer.WritePunctuator("?", TokenFlags.Leading | TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

        VisitExpression(conditionalExpression.Consequent, SubExpressionFlags(operandNeedsBrackets, isLeftMost: false), static (@this, expression, flags) =>
            // Edge case: 'in' operators in for...in loop declarations are not ambigous when they are in the consequent part of the conditional expression.
            @this.DisambiguateExpression(expression, ~ExpressionFlags.InOperatorIsAmbiguousInDeclaration & @this.PropagateExpressionFlags(flags)));

        // Alternate expressions with the same precendence as ternary operator are unambiguous without brackets, even conditional expressions because of right-to-left associativity.
        operandNeedsBrackets = GetOperatorPrecedence(conditionalExpression, out _) > GetOperatorPrecedence(conditionalExpression.Alternate, out _);

        _writeContext.SetNodeProperty(nameof(conditionalExpression.Alternate), static node => node.As<ConditionalExpression>().Alternate);
        Writer.WritePunctuator(":", TokenFlags.Leading | TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

        VisitSubExpression(conditionalExpression.Alternate, SubExpressionFlags(operandNeedsBrackets, isLeftMost: false));

        return conditionalExpression;
    }

    protected internal override object? VisitContinueStatement(ContinueStatement continueStatement)
    {
        Writer.WriteKeyword("continue", TokenFlags.LeadingSpaceRecommended, ref _writeContext);

        if (continueStatement.Label is not null)
        {
            _writeContext.SetNodeProperty(nameof(continueStatement.Label), static node => node.As<ContinueStatement>().Label);
            VisitRootExpression(continueStatement.Label, RootExpressionFlags(needsBrackets: false));
        }

        StatementNeedsSemicolon();

        return continueStatement;
    }

    protected internal override object? VisitDebuggerStatement(DebuggerStatement debuggerStatement)
    {
        Writer.WriteKeyword("debugger", TokenFlags.LeadingSpaceRecommended, ref _writeContext);

        StatementNeedsSemicolon();

        return debuggerStatement;
    }

    protected internal override object? VisitDecorator(Decorator decorator)
    {
        // https://github.com/tc39/proposal-decorators

        Writer.WritePunctuator("@", TokenFlags.Leading | (ParentNode is not Expression).ToFlag(TokenFlags.LeadingSpaceRecommended), ref _writeContext);

        _writeContext.SetNodeProperty(nameof(decorator.Expression), static node => node.As<Decorator>().Expression);
        VisitRootExpression(decorator.Expression, LeftHandSideRootExpressionFlags(needsBrackets: false));

        Writer.SpaceRecommendedAfterLastToken();

        return decorator;
    }

    protected internal override object? VisitDoWhileStatement(DoWhileStatement doWhileStatement)
    {
        Writer.WriteKeyword("do", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

        _writeContext.SetNodeProperty(nameof(doWhileStatement.Body), static node => node.As<DoWhileStatement>().Body);
        StatementFlags bodyFlags;
        VisitStatement(doWhileStatement.Body, bodyFlags = StatementBodyFlags(isRightMost: false));

        _writeContext.ClearNodeProperty();
        Writer.WriteKeyword("while", TokenFlags.SurroundingSpaceRecommended | StatementBodyFlagsToKeywordFlags(bodyFlags), ref _writeContext);

        _writeContext.SetNodeProperty(nameof(doWhileStatement.Test), static node => node.As<DoWhileStatement>().Test);
        VisitRootExpression(doWhileStatement.Test, ExpressionFlags.SpaceBeforeBracketsRecommended | RootExpressionFlags(needsBrackets: true));

        return doWhileStatement;
    }

    protected internal override object? VisitEmptyStatement(EmptyStatement emptyStatement)
    {
        Writer.WritePunctuator(";", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

        return emptyStatement;
    }

    protected internal override object? VisitExportAllDeclaration(ExportAllDeclaration exportAllDeclaration)
    {
        Writer.WriteKeyword("export", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);
        Writer.WritePunctuator("*", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

        if (exportAllDeclaration.Exported is not null)
        {
            _writeContext.SetNodeProperty(nameof(exportAllDeclaration.Exported), static node => node.As<ExportAllDeclaration>().Exported);
            Writer.WriteKeyword("as", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

            VisitExportOrImportSpecifierIdentifier(exportAllDeclaration.Exported);
        }

        _writeContext.ClearNodeProperty();
        Writer.WriteKeyword("from", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

        _writeContext.SetNodeProperty(nameof(exportAllDeclaration.Source), static node => node.As<ExportAllDeclaration>().Source);
        VisitRootExpression(exportAllDeclaration.Source, RootExpressionFlags(needsBrackets: false));

        if (exportAllDeclaration.Assertions.Count > 0)
        {
            _writeContext.SetNodeProperty(nameof(exportAllDeclaration.Assertions), static node => ref node.As<ExportAllDeclaration>().Assertions);
            VisitAssertions(in exportAllDeclaration.Assertions);
        }

        StatementNeedsSemicolon();

        return exportAllDeclaration;
    }

    protected internal override object? VisitExportDefaultDeclaration(ExportDefaultDeclaration exportDefaultDeclaration)
    {
        Writer.WriteKeyword("export", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);
        Writer.WriteKeyword("default", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

        _writeContext.SetNodeProperty(nameof(exportDefaultDeclaration.Declaration), static node => node.As<ExportDefaultDeclaration>().Declaration);
        if (exportDefaultDeclaration.Declaration is Declaration declaration)
        {
            VisitStatement(declaration, StatementFlags.IsRightMost);
        }
        else
        {
            VisitRootExpression(exportDefaultDeclaration.Declaration.As<Expression>(), ExpressionFlags.IsInsideStatementExpression | RootExpressionFlags(needsBrackets: false));

            StatementNeedsSemicolon();
        }

        return exportDefaultDeclaration;
    }

    protected internal override object? VisitExportNamedDeclaration(ExportNamedDeclaration exportNamedDeclaration)
    {
        Writer.WriteKeyword("export", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

        if (exportNamedDeclaration.Declaration is not null)
        {
            _writeContext.SetNodeProperty(nameof(exportNamedDeclaration.Declaration), static node => node.As<ExportNamedDeclaration>().Declaration);
            VisitStatement(exportNamedDeclaration.Declaration.As<Declaration>(), StatementFlags.IsRightMost);
        }
        else
        {
            _writeContext.SetNodeProperty(nameof(exportNamedDeclaration.Specifiers), static node => ref node.As<ExportNamedDeclaration>().Specifiers);
            Writer.WritePunctuator("{", TokenFlags.Leading | TokenFlags.SurroundingSpaceRecommended, ref _writeContext);
            VisitAuxiliaryNodeList(in exportNamedDeclaration.Specifiers, separator: ",");
            Writer.WritePunctuator("}", TokenFlags.Trailing | TokenFlags.LeadingSpaceRecommended, ref _writeContext);

            if (exportNamedDeclaration.Source is not null)
            {
                _writeContext.ClearNodeProperty();
                Writer.WriteKeyword("from", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

                _writeContext.SetNodeProperty(nameof(exportNamedDeclaration.Source), static node => node.As<ExportNamedDeclaration>().Source);
                VisitRootExpression(exportNamedDeclaration.Source, RootExpressionFlags(needsBrackets: false));

                if (exportNamedDeclaration.Assertions.Count > 0)
                {
                    _writeContext.SetNodeProperty(nameof(exportNamedDeclaration.Assertions), static node => ref node.As<ExportNamedDeclaration>().Assertions);
                    VisitAssertions(in exportNamedDeclaration.Assertions);
                }
            }

            StatementNeedsSemicolon();
        }

        return exportNamedDeclaration;
    }

    protected internal override object? VisitExportSpecifier(ExportSpecifier exportSpecifier)
    {
        _writeContext.SetNodeProperty(nameof(exportSpecifier.Local), static node => node.As<ExportSpecifier>().Local);
        VisitExportOrImportSpecifierIdentifier(exportSpecifier.Local);

        if (exportSpecifier.Local != exportSpecifier.Exported)
        {
            _writeContext.ClearNodeProperty();
            Writer.WriteKeyword("as", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

            _writeContext.SetNodeProperty(nameof(exportSpecifier.Exported), static node => node.As<ExportSpecifier>().Exported);
            VisitExportOrImportSpecifierIdentifier(exportSpecifier.Exported);
        }

        return exportSpecifier;
    }

    protected internal override object? VisitExpressionStatement(ExpressionStatement expressionStatement)
    {
        Writer.SpaceRecommendedAfterLastToken();

        _writeContext.SetNodeProperty(nameof(expressionStatement.Expression), static node => node.As<ExpressionStatement>().Expression);
        VisitRootExpression(expressionStatement.Expression, ExpressionFlags.IsInsideStatementExpression | RootExpressionFlags(needsBrackets: false));

        StatementNeedsSemicolon();

        return expressionStatement;
    }

    protected internal override object? VisitExtension(Node node)
    {
        if (_ignoreExtensions)
        {
            Writer.WriteBlockComment(new[] { $" Unsupported node type ({node.GetType()}). " }, TriviaFlags.None);
            return node;
        }
        else
        {
            return base.VisitExtension(node);
        }
    }

    protected internal override object? VisitForInStatement(ForInStatement forInStatement)
    {
        Writer.WriteKeyword("for", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

        Writer.WritePunctuator("(", TokenFlags.Leading | TokenFlags.LeadingSpaceRecommended, ref _writeContext);

        _writeContext.SetNodeProperty(nameof(forInStatement.Left), static node => node.As<ForInStatement>().Left);

        if (forInStatement.Left is VariableDeclaration variableDeclaration)
        {
            VisitStatement(variableDeclaration, StatementFlags.NestedVariableDeclaration);
        }
        else if (forInStatement.Left is Expression leftExpression)
        {
            VisitRootExpression(leftExpression, RootExpressionFlags(needsBrackets: false));
        }
        else
        {
            VisitAuxiliaryNode(forInStatement.Left, static delegate { return s_bindingPatternAllowsExpressionsFlag; });
        }

        _writeContext.ClearNodeProperty();
        Writer.WriteKeyword("in", TokenFlags.InBetween | TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

        _writeContext.SetNodeProperty(nameof(forInStatement.Right), static node => node.As<ForInStatement>().Right);
        VisitRootExpression(forInStatement.Right, RootExpressionFlags(needsBrackets: false));

        _writeContext.ClearNodeProperty();
        Writer.WritePunctuator(")", TokenFlags.Trailing | TokenFlags.TrailingSpaceRecommended, ref _writeContext);

        _writeContext.SetNodeProperty(nameof(forInStatement.Body), static node => node.As<ForInStatement>().Body);
        VisitStatement(forInStatement.Body, StatementBodyFlags(isRightMost: true));

        return forInStatement;
    }

    protected internal override object? VisitForOfStatement(ForOfStatement forOfStatement)
    {
        Writer.WriteKeyword("for", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

        if (forOfStatement.Await)
        {
            _writeContext.SetNodeProperty(nameof(forOfStatement.Await), static node => node.As<ForOfStatement>().Await);
            Writer.WriteKeyword("await", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);
        }

        Writer.WritePunctuator("(", TokenFlags.Leading | TokenFlags.LeadingSpaceRecommended, ref _writeContext);

        _writeContext.SetNodeProperty(nameof(forOfStatement.Left), static node => node.As<ForOfStatement>().Left);

        if (forOfStatement.Left is VariableDeclaration variableDeclaration)
        {
            VisitStatement(variableDeclaration, StatementFlags.NestedVariableDeclaration);
        }
        else if (forOfStatement.Left is Expression leftExpression)
        {
            VisitRootExpression(leftExpression, RootExpressionFlags(needsBrackets: false));
        }
        else
        {
            VisitAuxiliaryNode(forOfStatement.Left, static delegate { return s_bindingPatternAllowsExpressionsFlag; });
        }

        _writeContext.ClearNodeProperty();
        Writer.WriteKeyword("of", TokenFlags.InBetween | TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

        _writeContext.SetNodeProperty(nameof(forOfStatement.Right), static node => node.As<ForOfStatement>().Right);
        VisitRootExpression(forOfStatement.Right, RootExpressionFlags(needsBrackets: ExpressionNeedsBracketsInList(forOfStatement.Right)));

        _writeContext.ClearNodeProperty();
        Writer.WritePunctuator(")", TokenFlags.Trailing | TokenFlags.TrailingSpaceRecommended, ref _writeContext);

        _writeContext.SetNodeProperty(nameof(forOfStatement.Body), static node => node.As<ForOfStatement>().Body);
        VisitStatement(forOfStatement.Body, StatementBodyFlags(isRightMost: true));

        return forOfStatement;
    }

    protected internal override object? VisitForStatement(ForStatement forStatement)
    {
        Writer.WriteKeyword("for", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

        Writer.WritePunctuator("(", TokenFlags.Leading | TokenFlags.LeadingSpaceRecommended, ref _writeContext);

        _writeContext.SetNodeProperty(nameof(forStatement.Init), static node => node.As<ForStatement>().Init);

        if (forStatement.Init is not null)
        {
            if (forStatement.Init is VariableDeclaration variableDeclaration)
            {
                VisitStatement(variableDeclaration, StatementFlags.NestedVariableDeclaration);
            }
            else
            {
                VisitRootExpression(forStatement.Init.As<Expression>(), RootExpressionFlags(needsBrackets: false));
            }
        }

        Writer.WritePunctuator(";", TokenFlags.Trailing | TokenFlags.TrailingSpaceRecommended, ref _writeContext);

        _writeContext.SetNodeProperty(nameof(forStatement.Test), static node => node.As<ForStatement>().Test);

        if (forStatement.Test is not null)
        {
            VisitRootExpression(forStatement.Test, RootExpressionFlags(needsBrackets: false));
        }

        Writer.WritePunctuator(";", TokenFlags.Trailing | TokenFlags.TrailingSpaceRecommended, ref _writeContext);

        if (forStatement.Update is not null)
        {
            _writeContext.SetNodeProperty(nameof(forStatement.Update), static node => node.As<ForStatement>().Update);

            VisitRootExpression(forStatement.Update, RootExpressionFlags(needsBrackets: false));
        }

        _writeContext.ClearNodeProperty();
        Writer.WritePunctuator(")", TokenFlags.Trailing | TokenFlags.TrailingSpaceRecommended, ref _writeContext);

        _writeContext.SetNodeProperty(nameof(forStatement.Body), static node => node.As<ForStatement>().Body);
        VisitStatement(forStatement.Body, StatementBodyFlags(isRightMost: true));

        return forStatement;
    }

    protected internal override object? VisitFunctionDeclaration(FunctionDeclaration functionDeclaration)
    {
        if (functionDeclaration.Async)
        {
            _writeContext.SetNodeProperty(nameof(functionDeclaration.Async), static node => node.As<FunctionDeclaration>().Async);
            Writer.WriteKeyword("async", TokenFlags.LeadingSpaceRecommended, ref _writeContext);

            _writeContext.ClearNodeProperty();
        }

        Writer.WriteKeyword("function", TokenFlags.LeadingSpaceRecommended, ref _writeContext);

        if (functionDeclaration.Generator)
        {
            _writeContext.SetNodeProperty(nameof(functionDeclaration.Generator), static node => node.As<FunctionDeclaration>().Generator);
            Writer.WritePunctuator("*", (functionDeclaration.Id is not null).ToFlag(TokenFlags.TrailingSpaceRecommended), ref _writeContext);
        }

        if (functionDeclaration.Id is not null)
        {
            _writeContext.SetNodeProperty(nameof(functionDeclaration.Id), static node => node.As<FunctionDeclaration>().Id);
            VisitAuxiliaryNode(functionDeclaration.Id);
        }

        _writeContext.SetNodeProperty(nameof(functionDeclaration.Params), static node => ref node.As<FunctionDeclaration>().Params);
        Writer.WritePunctuator("(", TokenFlags.Leading, ref _writeContext);
        VisitAuxiliaryNodeList(in functionDeclaration.Params, separator: ",");
        Writer.WritePunctuator(")", TokenFlags.Trailing, ref _writeContext);

        _writeContext.SetNodeProperty(nameof(functionDeclaration.Body), static node => node.As<FunctionDeclaration>().Body);
        VisitStatement(functionDeclaration.Body, StatementBodyFlags(isRightMost: true));

        return functionDeclaration;
    }

    protected internal override object? VisitFunctionExpression(FunctionExpression functionExpression)
    {
        if (!_currentExpressionFlags.HasFlagFast(ExpressionFlags.IsMethod))
        {
            if (functionExpression.Async)
            {
                _writeContext.SetNodeProperty(nameof(functionExpression.Async), static node => node.As<FunctionExpression>().Async);
                Writer.WriteKeyword("async", ref _writeContext);

                _writeContext.ClearNodeProperty();
            }

            Writer.WriteKeyword("function", ref _writeContext);

            if (functionExpression.Generator)
            {
                _writeContext.SetNodeProperty(nameof(functionExpression.Generator), static node => node.As<FunctionExpression>().Generator);
                Writer.WritePunctuator("*", (functionExpression.Id is not null).ToFlag(TokenFlags.TrailingSpaceRecommended), ref _writeContext);
            }

            if (functionExpression.Id is not null)
            {
                _writeContext.SetNodeProperty(nameof(functionExpression.Id), static node => node.As<FunctionExpression>().Id);
                VisitAuxiliaryNode(functionExpression.Id);
            }
        }
        else
        {
            var keyIsFirstToken = true;

            if (functionExpression.Async)
            {
                _writeContext.SetNodeProperty(nameof(functionExpression.Async), static node => node.As<FunctionExpression>().Async);
                Writer.WriteKeyword("async", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

                keyIsFirstToken = false;
            }

            if (functionExpression.Generator)
            {
                _writeContext.SetNodeProperty(nameof(functionExpression.Generator), static node => node.As<FunctionExpression>().Generator);
                Writer.WritePunctuator("*", TokenFlags.LeadingSpaceRecommended, ref _writeContext);

                keyIsFirstToken = false;
            }

            _writeContext.SetNodeProperty(nameof(functionExpression.Id), static node => node.As<FunctionExpression>().Id);
            var property = (IProperty) ParentNode!;
            if (property.Kind != PropertyKind.Constructor || property.Key.Type == Nodes.Literal)
            {
                if (keyIsFirstToken && !property.Computed)
                {
                    Writer.SpaceRecommendedAfterLastToken();
                }

                VisitPropertyKey(property.Key, property.Computed, leadingBracketFlags: keyIsFirstToken.ToFlag(TokenFlags.LeadingSpaceRecommended));
            }
            else
            {
                Writer.WriteKeyword("constructor", TokenFlags.LeadingSpaceRecommended, ref _writeContext);
            }
        }

        _writeContext.SetNodeProperty(nameof(functionExpression.Params), static node => ref node.As<FunctionExpression>().Params);
        Writer.WritePunctuator("(", TokenFlags.Leading, ref _writeContext);
        VisitAuxiliaryNodeList(in functionExpression.Params, separator: ",");
        Writer.WritePunctuator(")", TokenFlags.Trailing, ref _writeContext);

        _writeContext.SetNodeProperty(nameof(functionExpression.Body), static node => node.As<FunctionExpression>().Body);
        VisitStatement(functionExpression.Body, StatementBodyFlags(isRightMost: true));

        return functionExpression;
    }

    protected internal override object? VisitIdentifier(Identifier identifier)
    {
        _writeContext.SetNodeProperty(nameof(identifier.Name), static node => node.As<Identifier>().Name);
        Writer.WriteIdentifier(identifier.Name, ref _writeContext);

        return identifier;
    }

    protected internal override object? VisitIfStatement(IfStatement ifStatement)
    {
        Writer.WriteKeyword("if", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

        _writeContext.SetNodeProperty(nameof(ifStatement.Test), static node => node.As<IfStatement>().Test);
        VisitRootExpression(ifStatement.Test, ExpressionFlags.SpaceAroundBracketsRecommended | RootExpressionFlags(needsBrackets: true));

        _writeContext.SetNodeProperty(nameof(ifStatement.Consequent), static node => node.As<IfStatement>().Consequent);
        StatementFlags bodyFlags;
        VisitStatement(ifStatement.Consequent, bodyFlags = StatementBodyFlags(isRightMost: ifStatement.Alternate is null));

        if (ifStatement.Alternate is not null)
        {
            _writeContext.ClearNodeProperty();
            Writer.WriteKeyword("else", TokenFlags.SurroundingSpaceRecommended | StatementBodyFlagsToKeywordFlags(bodyFlags), ref _writeContext);

            _writeContext.SetNodeProperty(nameof(ifStatement.Alternate), static node => node.As<IfStatement>().Alternate);
            VisitStatement(ifStatement.Alternate, StatementBodyFlags(isRightMost: true));
        }

        return ifStatement;
    }

    protected internal override object? VisitImport(Import import)
    {
        Writer.WriteKeyword("import", ref _writeContext);

        Writer.WritePunctuator("(", TokenFlags.Leading, ref _writeContext);

        // Import arguments need special care because of the unusual model (separate expressions instead of an expression list).

        var paramCount = import.Attributes is null ? 1 : 2;
        Writer.StartExpressionList(paramCount, ref _writeContext);

        _writeContext.SetNodeProperty(nameof(Import.Source), static node => node.As<Import>().Source);
        VisitExpressionListItem(import.Source, 0, paramCount, static (@this, expression, _, _) =>
            s_getCombinedSubExpressionFlags(@this, expression, SubExpressionFlags(@this.ExpressionNeedsBracketsInList(expression), isLeftMost: false)));

        if (import.Attributes is not null)
        {
            // https://github.com/tc39/proposal-import-assertions

            _writeContext.SetNodeProperty(nameof(Import.Attributes), static node => node.As<Import>().Attributes);
            VisitExpressionListItem(import.Attributes, 1, paramCount, static (@this, expression, _, _) =>
                s_getCombinedSubExpressionFlags(@this, expression, SubExpressionFlags(@this.ExpressionNeedsBracketsInList(expression), isLeftMost: false)));
        }

        Writer.EndExpressionList(paramCount, ref _writeContext);

        _writeContext.ClearNodeProperty();
        Writer.WritePunctuator(")", TokenFlags.Trailing, ref _writeContext);

        return import;
    }

    protected internal override object? VisitImportAttribute(ImportAttribute importAttribute)
    {
        // https://github.com/tc39/proposal-import-assertions

        _writeContext.SetNodeProperty(nameof(importAttribute.Key), static node => node.As<ImportAttribute>().Key);
        VisitPropertyKey(importAttribute.Key, computed: false);
        Writer.WritePunctuator(":", TokenFlags.Trailing | TokenFlags.TrailingSpaceRecommended, ref _writeContext);

        _writeContext.SetNodeProperty(nameof(importAttribute.Value), static node => node.As<ImportAttribute>().Value);

        VisitRootExpression(importAttribute.Value, RootExpressionFlags(needsBrackets: false));

        return importAttribute;
    }

    protected internal override object? VisitImportDeclaration(ImportDeclaration importDeclaration)
    {
        Writer.WriteKeyword("import", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

        // Specifiers need special care because of the unusual syntax.

        _writeContext.SetNodeProperty(nameof(importDeclaration.Specifiers), static node => ref node.As<ImportDeclaration>().Specifiers);
        Writer.StartAuxiliaryNodeList<ImportDeclarationSpecifier>(importDeclaration.Specifiers.Count, ref _writeContext);

        if (importDeclaration.Specifiers.Count == 0)
        {
            Writer.EndAuxiliaryNodeList<ImportDeclarationSpecifier>(count: 0, ref _writeContext);

            goto WriteSource;
        }

        var index = 0;
        Func<AstToJavascriptConverter, Node?, int, int, object?> getNodeContext = static delegate { return null; };

        if (importDeclaration.Specifiers[index].Type == Nodes.ImportDefaultSpecifier)
        {
            VisitAuxiliaryNodeListItem(importDeclaration.Specifiers[index], index, importDeclaration.Specifiers.Count, ",", getNodeContext);

            if (++index >= importDeclaration.Specifiers.Count)
            {
                goto EndSpecifiers;
            }
        }

        if (importDeclaration.Specifiers[index].Type == Nodes.ImportNamespaceSpecifier)
        {
            VisitAuxiliaryNodeListItem(importDeclaration.Specifiers[index], index, importDeclaration.Specifiers.Count, ",", getNodeContext);

            if (++index >= importDeclaration.Specifiers.Count)
            {
                goto EndSpecifiers;
            }
        }

        Writer.WritePunctuator("{", TokenFlags.Leading | TokenFlags.TrailingSpaceRecommended, ref _writeContext);

        for (; index < importDeclaration.Specifiers.Count; index++)
        {
            VisitAuxiliaryNodeListItem(importDeclaration.Specifiers[index], index, importDeclaration.Specifiers.Count, ",", getNodeContext);
        }

        Writer.WritePunctuator("}", TokenFlags.Trailing | TokenFlags.LeadingSpaceRecommended, ref _writeContext);

EndSpecifiers:
        Writer.EndAuxiliaryNodeList<ImportDeclarationSpecifier>(importDeclaration.Specifiers.Count, ref _writeContext);

        _writeContext.ClearNodeProperty();
        Writer.WriteKeyword("from", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

WriteSource:
        _writeContext.SetNodeProperty(nameof(importDeclaration.Source), static node => node.As<ImportDeclaration>().Source);
        VisitRootExpression(importDeclaration.Source, RootExpressionFlags(needsBrackets: false));

        if (importDeclaration.Assertions.Count > 0)
        {
            _writeContext.SetNodeProperty(nameof(importDeclaration.Assertions), static node => ref node.As<ImportDeclaration>().Assertions);
            VisitAssertions(in importDeclaration.Assertions);
        }

        StatementNeedsSemicolon();

        return importDeclaration;
    }

    protected internal override object? VisitImportDefaultSpecifier(ImportDefaultSpecifier importDefaultSpecifier)
    {
        _writeContext.SetNodeProperty(nameof(importDefaultSpecifier.Local), static node => node.As<ImportDefaultSpecifier>().Local);
        VisitAuxiliaryNode(importDefaultSpecifier.Local);

        return importDefaultSpecifier;
    }

    protected internal override object? VisitImportNamespaceSpecifier(ImportNamespaceSpecifier importNamespaceSpecifier)
    {
        Writer.WritePunctuator("*", TokenFlags.TrailingSpaceRecommended, ref _writeContext);

        Writer.WriteKeyword("as", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

        _writeContext.SetNodeProperty(nameof(importNamespaceSpecifier.Local), static node => node.As<ImportNamespaceSpecifier>().Local);
        VisitAuxiliaryNode(importNamespaceSpecifier.Local);

        return importNamespaceSpecifier;
    }

    protected internal override object? VisitImportSpecifier(ImportSpecifier importSpecifier)
    {
        if (importSpecifier.Imported != importSpecifier.Local)
        {
            _writeContext.SetNodeProperty(nameof(importSpecifier.Imported), static node => node.As<ImportSpecifier>().Imported);
            VisitExportOrImportSpecifierIdentifier(importSpecifier.Imported);

            _writeContext.ClearNodeProperty();
            Writer.WriteKeyword("as", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);
        }

        _writeContext.SetNodeProperty(nameof(importSpecifier.Local), static node => node.As<ImportSpecifier>().Local);
        VisitAuxiliaryNode(importSpecifier.Local);

        return importSpecifier;
    }

    protected internal override object? VisitLabeledStatement(LabeledStatement labeledStatement)
    {
        Writer.SpaceRecommendedAfterLastToken();

        _writeContext.SetNodeProperty(nameof(labeledStatement.Label), static node => node.As<LabeledStatement>().Label);
        VisitAuxiliaryNode(labeledStatement.Label);

        Writer.WritePunctuator(":", TokenFlags.Trailing | TokenFlags.TrailingSpaceRecommended, ref _writeContext);

        _writeContext.SetNodeProperty(nameof(labeledStatement.Body), static node => node.As<LabeledStatement>().Body);
        VisitStatement(labeledStatement.Body, StatementFlags.IsRightMost);

        return labeledStatement;
    }

    protected internal override object? VisitLiteral(Literal literal)
    {
        _writeContext.SetNodeProperty(nameof(literal.Raw), static node => node.As<Literal>().Raw);
        Writer.WriteLiteral(literal.Raw, literal.TokenType, ref _writeContext);

        return literal;
    }

    protected internal override object? VisitMemberExpression(MemberExpression memberExpression)
    {
        var operationFlags = BinaryOperandsNeedBrackets(memberExpression, memberExpression.Object, memberExpression.Property);

        // Cases like 1.toString() must be disambiguated with brackets.
        if (!operationFlags.HasFlagFast(BinaryOperationFlags.LeftOperandNeedsBrackets) &&
            memberExpression is { Computed: false, Optional: false, Object: Literal objectLiteral } &&
            objectLiteral.TokenType == TokenType.NumericLiteral &&
            objectLiteral.Raw.IndexOf('.') < 0)
        {
            operationFlags |= BinaryOperationFlags.LeftOperandNeedsBrackets;
        }

        _writeContext.SetNodeProperty(nameof(memberExpression.Object), static node => node.As<MemberExpression>().Object);
        VisitSubExpression(memberExpression.Object, SubExpressionFlags(operationFlags.HasFlagFast(BinaryOperationFlags.LeftOperandNeedsBrackets), isLeftMost: true));

        if (memberExpression.Computed)
        {
            if (memberExpression.Optional)
            {
                _writeContext.ClearNodeProperty();
                Writer.WritePunctuator("?.", TokenFlags.InBetween, ref _writeContext);
            }

            _writeContext.SetNodeProperty(nameof(memberExpression.Property), static node => node.As<MemberExpression>().Property);
            Writer.WritePunctuator("[", TokenFlags.Leading, ref _writeContext);
            VisitSubExpression(memberExpression.Property, SubExpressionFlags(needsBrackets: false, isLeftMost: false));
            Writer.WritePunctuator("]", TokenFlags.Trailing, ref _writeContext);
        }
        else
        {
            _writeContext.ClearNodeProperty();
            Writer.WritePunctuator(memberExpression.Optional ? "?." : ".", TokenFlags.InBetween, ref _writeContext);

            _writeContext.SetNodeProperty(nameof(memberExpression.Property), static node => node.As<MemberExpression>().Property);
            VisitSubExpression(memberExpression.Property, SubExpressionFlags(needsBrackets: false, isLeftMost: false));
        }

        return memberExpression;
    }

    protected internal override object? VisitMetaProperty(MetaProperty metaProperty)
    {
        _writeContext.SetNodeProperty(nameof(metaProperty.Meta), static node => node.As<MetaProperty>().Meta);
        Writer.WriteKeyword(metaProperty.Meta.Name, ref _writeContext);

        _writeContext.ClearNodeProperty();
        Writer.WritePunctuator(".", TokenFlags.InBetween, ref _writeContext);

        _writeContext.SetNodeProperty(nameof(metaProperty.Property), static node => node.As<MetaProperty>().Property);
        VisitSubExpression(metaProperty.Property, SubExpressionFlags(needsBrackets: false, isLeftMost: false));

        return metaProperty;
    }

    protected internal override object? VisitMethodDefinition(MethodDefinition methodDefinition)
    {
        if (methodDefinition.Decorators.Count > 0)
        {
            _writeContext.SetNodeProperty(nameof(methodDefinition.Decorators), static node => ref node.As<MethodDefinition>().Decorators);
            VisitAuxiliaryNodeList(methodDefinition.Decorators, separator: string.Empty);

            _writeContext.ClearNodeProperty();
        }

        if (methodDefinition.Static)
        {
            _writeContext.SetNodeProperty(nameof(methodDefinition.Static), static node => node.As<MethodDefinition>().Static);
            Writer.WriteKeyword("static", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);
        }

        switch (methodDefinition.Kind)
        {
            case PropertyKind.Get:
                _writeContext.SetNodeProperty(nameof(methodDefinition.Kind), static node => node.As<MethodDefinition>().Kind);
                Writer.WriteKeyword("get", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);
                break;
            case PropertyKind.Set:
                _writeContext.SetNodeProperty(nameof(methodDefinition.Kind), static node => node.As<MethodDefinition>().Kind);
                Writer.WriteKeyword("set", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);
                break;
        }

        _writeContext.SetNodeProperty(nameof(methodDefinition.Value), static node => node.As<MethodDefinition>().Value);
        VisitRootExpression(methodDefinition.Value, ExpressionFlags.IsMethod | RootExpressionFlags(needsBrackets: false));

        return methodDefinition;
    }

    protected internal override object? VisitNewExpression(NewExpression newExpression)
    {
        Writer.WriteKeyword("new", TokenFlags.TrailingSpaceRecommended, ref _writeContext);

        var calleeNeedsBrackets = UnaryOperandNeedsBrackets(newExpression, newExpression.Callee);

        _writeContext.SetNodeProperty(nameof(newExpression.Callee), static node => node.As<NewExpression>().Callee);
        VisitExpression(newExpression.Callee, SubExpressionFlags(calleeNeedsBrackets, isLeftMost: false), static (@this, expression, flags) =>
            @this.DisambiguateExpression(expression, ExpressionFlags.IsInsideNewCallee | ExpressionFlags.IsLeftMostInNewCallee | @this.PropagateExpressionFlags(flags)));

        if (newExpression.Arguments.Count > 0)
        {
            _writeContext.SetNodeProperty(nameof(newExpression.Arguments), static node => ref node.As<NewExpression>().Arguments);
            Writer.WritePunctuator("(", TokenFlags.Leading, ref _writeContext);
            VisitSubExpressionList(in newExpression.Arguments);
            Writer.WritePunctuator(")", TokenFlags.Trailing, ref _writeContext);
        }

        return newExpression;
    }

    protected internal override object? VisitObjectExpression(ObjectExpression objectExpression)
    {
        _writeContext.SetNodeProperty(nameof(objectExpression.Properties), static node => ref node.As<ObjectExpression>().Properties);

        Writer.StartObject(objectExpression.Properties.Count, ref _writeContext);

        // Properties need special care because it may contain spread elements, which are actual expressions (as opposed to normal properties).

        Writer.StartAuxiliaryNodeList<Node>(objectExpression.Properties.Count, ref _writeContext);

        for (var i = 0; i < objectExpression.Properties.Count; i++)
        {
            var property = objectExpression.Properties[i];
            if (property is SpreadElement spreadElement)
            {
                var originalAuxiliaryNodeContext = _currentAuxiliaryNodeContext;
                _currentAuxiliaryNodeContext = null;

                Writer.StartAuxiliaryNodeListItem<Node>(i, objectExpression.Properties.Count, separator: ",", _currentAuxiliaryNodeContext, ref _writeContext);
                VisitRootExpression(spreadElement, RootExpressionFlags(needsBrackets: ExpressionNeedsBracketsInList(spreadElement)));
                Writer.EndAuxiliaryNodeListItem<Node>(i, objectExpression.Properties.Count, separator: ",", _currentAuxiliaryNodeContext, ref _writeContext);

                _currentAuxiliaryNodeContext = originalAuxiliaryNodeContext;
            }
            else
            {
                VisitAuxiliaryNodeListItem(property, i, objectExpression.Properties.Count, separator: ",", static delegate { return null; });
            }
        }

        Writer.EndAuxiliaryNodeList<Node>(objectExpression.Properties.Count, ref _writeContext);

        Writer.EndObject(objectExpression.Properties.Count, ref _writeContext);

        return objectExpression;
    }

    protected internal override object? VisitObjectPattern(ObjectPattern objectPattern)
    {
        _writeContext.SetNodeProperty(nameof(objectPattern.Properties), static node => ref node.As<ObjectPattern>().Properties);

        Writer.StartObject(objectPattern.Properties.Count, ref _writeContext);

        VisitAuxiliaryNodeList(in objectPattern.Properties, separator: ",", static (@this, _, _, _) =>
            @this._currentAuxiliaryNodeContext == s_bindingPatternAllowsExpressionsFlag
                ? s_bindingPatternAllowsExpressionsFlag // propagate flag to sub-patterns
                : null);

        Writer.EndObject(objectPattern.Properties.Count, ref _writeContext);

        return objectPattern;
    }

    protected internal override object? VisitPrivateIdentifier(PrivateIdentifier privateIdentifier)
    {
        _writeContext.SetNodeProperty(nameof(privateIdentifier.Name), static node => node.As<PrivateIdentifier>().Name);
        Writer.WritePunctuator("#", TokenFlags.Leading, ref _writeContext);
        Writer.WriteIdentifier(privateIdentifier.Name, ref _writeContext);

        return privateIdentifier;
    }

    protected internal override object? VisitProgram(Program program)
    {
        _writeContext.SetNodeProperty(nameof(program.Body), static node => ref node.As<Program>().Body);
        VisitStatementList(in program.Body);

        return program;
    }

    protected internal override object? VisitProperty(Property property)
    {
        bool isMethod;

        switch (property.Kind)
        {
            case PropertyKind.Get:
                _writeContext.SetNodeProperty(nameof(property.Kind), static node => node.As<Property>().Kind);
                Writer.WriteKeyword("get", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

                isMethod = true;
                break;
            case PropertyKind.Set:
                _writeContext.SetNodeProperty(nameof(property.Kind), static node => node.As<Property>().Kind);
                Writer.WriteKeyword("set", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

                isMethod = true;
                break;
            case PropertyKind.Init when property.Method:
                isMethod = true;
                break;
            default:
                if (!property.Shorthand)
                {
                    _writeContext.SetNodeProperty(nameof(property.Key), static node => node.As<Property>().Key);
                    VisitPropertyKey(property.Key, property.Computed, leadingBracketFlags: TokenFlags.LeadingSpaceRecommended);
                    Writer.WritePunctuator(":", TokenFlags.Trailing | TokenFlags.TrailingSpaceRecommended, ref _writeContext);
                }

                isMethod = false;
                break;
        }

        _writeContext.SetNodeProperty(nameof(property.Value), static node => node.As<Property>().Value);

        Expression? valueExpression;
        if (ParentNode is { Type: Nodes.ObjectPattern })
        {
            if (_currentAuxiliaryNodeContext != s_bindingPatternAllowsExpressionsFlag)
            {
                VisitAuxiliaryNode(property.Value);
            }
            else if ((valueExpression = property.Value as Expression) is null)
            {
                VisitAuxiliaryNode(property.Value, static delegate { return s_bindingPatternAllowsExpressionsFlag; }); // propagate flag to sub-patterns
            }
            else
            {
                VisitRootExpression(valueExpression, RootExpressionFlags(needsBrackets: ExpressionNeedsBracketsInList(valueExpression)));
            }
        }
        else
        {
            valueExpression = property.Value.As<Expression>();
            VisitRootExpression(valueExpression, isMethod.ToFlag(ExpressionFlags.IsMethod) | RootExpressionFlags(needsBrackets: ExpressionNeedsBracketsInList(valueExpression)));
        }

        return property;
    }

    protected internal override object? VisitPropertyDefinition(PropertyDefinition propertyDefinition)
    {
        if (propertyDefinition.Decorators.Count > 0)
        {
            _writeContext.SetNodeProperty(nameof(propertyDefinition.Decorators), static node => ref node.As<PropertyDefinition>().Decorators);
            VisitAuxiliaryNodeList(propertyDefinition.Decorators, separator: string.Empty);

            _writeContext.ClearNodeProperty();
        }

        if (propertyDefinition.Static)
        {
            _writeContext.SetNodeProperty(nameof(propertyDefinition.Static), static node => node.As<PropertyDefinition>().Static);
            Writer.WriteKeyword("static", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);
        }
        else
        {
            Writer.SpaceRecommendedAfterLastToken();
        }

        _writeContext.SetNodeProperty(nameof(propertyDefinition.Key), static node => node.As<PropertyDefinition>().Key);
        VisitPropertyKey(propertyDefinition.Key, propertyDefinition.Computed, leadingBracketFlags: TokenFlags.LeadingSpaceRecommended);

        if (propertyDefinition.Value is not null)
        {
            _writeContext.ClearNodeProperty();
            Writer.WritePunctuator("=", TokenFlags.InBetween | TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

            _writeContext.SetNodeProperty(nameof(propertyDefinition.Value), static node => node.As<PropertyDefinition>().Value);
            VisitRootExpression(propertyDefinition.Value, RootExpressionFlags(needsBrackets: ExpressionNeedsBracketsInList(propertyDefinition.Value)));
        }

        Writer.WritePunctuator(";", TokenFlags.Trailing | TokenFlags.TrailingSpaceRecommended, ref _writeContext);

        return propertyDefinition;
    }

    protected internal override object? VisitRestElement(RestElement restElement)
    {
        _writeContext.SetNodeProperty(nameof(restElement.Argument), static node => node.As<RestElement>().Argument);
        Writer.WritePunctuator("...", TokenFlags.Leading, ref _writeContext);

        if (_currentAuxiliaryNodeContext != s_bindingPatternAllowsExpressionsFlag)
        {
            VisitAuxiliaryNode(restElement.Argument);
        }
        else if (restElement.Argument is not Expression argumentExpression)
        {
            VisitAuxiliaryNode(restElement.Argument, static delegate { return s_bindingPatternAllowsExpressionsFlag; }); // propagate flag to sub-patterns
        }
        else
        {
            VisitRootExpression(argumentExpression, RootExpressionFlags(needsBrackets: ExpressionNeedsBracketsInList(argumentExpression)));
        }

        return restElement;
    }

    protected internal override object? VisitReturnStatement(ReturnStatement returnStatement)
    {
        Writer.WriteKeyword("return", (returnStatement.Argument is not null).ToFlag(TokenFlags.SurroundingSpaceRecommended, TokenFlags.LeadingSpaceRecommended), ref _writeContext);

        if (returnStatement.Argument is not null)
        {
            _writeContext.SetNodeProperty(nameof(returnStatement.Argument), static node => node.As<ReturnStatement>().Argument);
            VisitRootExpression(returnStatement.Argument, RootExpressionFlags(needsBrackets: false));
        }

        StatementNeedsSemicolon();

        return returnStatement;
    }

    protected internal override object? VisitSequenceExpression(SequenceExpression sequenceExpression)
    {
        _writeContext.SetNodeProperty(nameof(sequenceExpression.Expressions), static node => ref node.As<SequenceExpression>().Expressions);

        VisitExpressionList(in sequenceExpression.Expressions, static (@this, expression, index, _) =>
            s_getCombinedSubExpressionFlags(@this, expression, SubExpressionFlags(@this.ExpressionNeedsBracketsInList(expression), isLeftMost: index == 0)));

        return sequenceExpression;
    }

    protected internal override object? VisitSpreadElement(SpreadElement spreadElement)
    {
        var argumentNeedsBrackets = UnaryOperandNeedsBrackets(spreadElement, spreadElement.Argument);

        _writeContext.SetNodeProperty(nameof(spreadElement.Argument), static node => node.As<SpreadElement>().Argument);
        Writer.WritePunctuator("...", TokenFlags.Leading, ref _writeContext);

        VisitSubExpression(spreadElement.Argument, SubExpressionFlags(argumentNeedsBrackets, isLeftMost: false));

        return spreadElement;
    }

    protected internal override object? VisitStaticBlock(StaticBlock staticBlock)
    {
        Writer.WriteKeyword("static", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

        _writeContext.SetNodeProperty(nameof(staticBlock.Body), static node => ref node.As<StaticBlock>().Body);
        Writer.StartBlock(staticBlock.Body.Count, ref _writeContext);

        VisitStatementList(in staticBlock.Body);

        Writer.EndBlock(staticBlock.Body.Count, ref _writeContext);

        return staticBlock;
    }

    protected internal override object? VisitSuper(Super super)
    {
        Writer.WriteKeyword("super", ref _writeContext);

        return super;
    }

    protected internal override object? VisitSwitchCase(SwitchCase switchCase)
    {
        if (switchCase.Test is not null)
        {
            Writer.WriteKeyword("case", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

            _writeContext.SetNodeProperty(nameof(switchCase.Test), static node => node.As<SwitchCase>().Test);
            VisitRootExpression(switchCase.Test, RootExpressionFlags(needsBrackets: false));

            _writeContext.ClearNodeProperty();
        }
        else
        {
            Writer.WriteKeyword("default", TokenFlags.LeadingSpaceRecommended, ref _writeContext);
        }

        Writer.WritePunctuator(":", TokenFlags.Trailing | TokenFlags.TrailingSpaceRecommended, ref _writeContext);

        _writeContext.SetNodeProperty(nameof(switchCase.Consequent), static node => ref node.As<SwitchCase>().Consequent);

        if (_currentAuxiliaryNodeContext == s_lastSwitchCaseFlag)
        {
            // If this is the last case, then the right-most semicolon can be omitted.
            VisitStatementList(in switchCase.Consequent);
        }
        else
        {
            // If this isn't the last case, then the right-most semicolon must not be omitted!
            VisitStatementList(in switchCase.Consequent, static delegate { return StatementFlags.None; });
        }

        return switchCase;
    }

    protected internal override object? VisitSwitchStatement(SwitchStatement switchStatement)
    {
        Writer.WriteKeyword("switch", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

        _writeContext.SetNodeProperty(nameof(switchStatement.Discriminant), static node => node.As<SwitchStatement>().Discriminant);
        VisitRootExpression(switchStatement.Discriminant, ExpressionFlags.SpaceAroundBracketsRecommended | RootExpressionFlags(needsBrackets: true));

        _writeContext.SetNodeProperty(nameof(switchStatement.Cases), static node => ref node.As<SwitchStatement>().Cases);
        Writer.StartBlock(switchStatement.Cases.Count, ref _writeContext);

        // Passes contextual information about whether it's the last one in the statement or not to each SwitchCase.
        VisitAuxiliaryNodeList(in switchStatement.Cases, separator: string.Empty, static (_, _, index, count) =>
            index == count - 1 ? s_lastSwitchCaseFlag : null);

        Writer.EndBlock(switchStatement.Cases.Count, ref _writeContext);

        return switchStatement;
    }

    protected internal override object? VisitTaggedTemplateExpression(TaggedTemplateExpression taggedTemplateExpression)
    {
        _writeContext.SetNodeProperty(nameof(taggedTemplateExpression.Tag), static node => node.As<TaggedTemplateExpression>().Tag);
        VisitExpression(taggedTemplateExpression.Tag, SubExpressionFlags(needsBrackets: false, isLeftMost: true), static (@this, expression, flags) =>
            @this.DisambiguateExpression(expression, ExpressionFlags.IsInsideLeftHandSideExpression | ExpressionFlags.IsLeftMostInLeftHandSideExpression | @this.PropagateExpressionFlags(flags)));

        _writeContext.SetNodeProperty(nameof(taggedTemplateExpression.Quasi), static node => node.As<TaggedTemplateExpression>().Quasi);
        VisitSubExpression(taggedTemplateExpression.Quasi, SubExpressionFlags(needsBrackets: false, isLeftMost: false));

        return taggedTemplateExpression;
    }

    protected internal override object? VisitTemplateElement(TemplateElement templateElement)
    {
        _writeContext.SetNodeProperty(nameof(templateElement.Value), static node => node.As<TemplateElement>().Value);
        Writer.WriteLiteral(templateElement.Value.Raw, TokenType.Template, ref _writeContext);

        return templateElement;
    }

    protected internal override object? VisitTemplateLiteral(TemplateLiteral templateLiteral)
    {
        Writer.WritePunctuator("`", TokenFlags.Leading, ref _writeContext);

        TemplateElement quasi;
        for (var i = 0; !(quasi = templateLiteral.Quasis[i]).Tail; i++)
        {
            _writeContext.SetNodeProperty(nameof(templateLiteral.Quasis), static node => ref node.As<TemplateLiteral>().Quasis);
            VisitAuxiliaryNode(quasi);

            _writeContext.SetNodeProperty(nameof(templateLiteral.Expressions), static node => ref node.As<TemplateLiteral>().Expressions);
            Writer.WritePunctuator("${", TokenFlags.Leading, ref _writeContext);
            VisitRootExpression(templateLiteral.Expressions[i], RootExpressionFlags(needsBrackets: false));
            Writer.WritePunctuator("}", TokenFlags.Trailing, ref _writeContext);
        }

        _writeContext.SetNodeProperty(nameof(templateLiteral.Quasis), static node => ref node.As<TemplateLiteral>().Quasis);
        VisitAuxiliaryNode(quasi);

        Writer.WritePunctuator("`", TokenFlags.Trailing, ref _writeContext);

        return templateLiteral;
    }

    protected internal override object? VisitThisExpression(ThisExpression thisExpression)
    {
        Writer.WriteKeyword("this", ref _writeContext);

        return thisExpression;
    }

    protected internal override object? VisitThrowStatement(ThrowStatement throwStatement)
    {
        Writer.WriteKeyword("throw", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

        _writeContext.SetNodeProperty(nameof(throwStatement.Argument), static node => node.As<ThrowStatement>().Argument);
        VisitRootExpression(throwStatement.Argument, RootExpressionFlags(needsBrackets: false));

        StatementNeedsSemicolon();

        return throwStatement;
    }

    protected internal override object? VisitTryStatement(TryStatement tryStatement)
    {
        Writer.WriteKeyword("try", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

        _writeContext.SetNodeProperty(nameof(tryStatement.Block), static node => node.As<TryStatement>().Block);
        StatementFlags bodyFlags;
        VisitStatement(tryStatement.Block, bodyFlags = StatementBodyFlags(isRightMost: false));

        if (tryStatement.Handler is not null)
        {
            _writeContext.ClearNodeProperty();
            Writer.WriteKeyword("catch", TokenFlags.SurroundingSpaceRecommended | StatementBodyFlagsToKeywordFlags(bodyFlags), ref _writeContext);

            _writeContext.SetNodeProperty(nameof(tryStatement.Handler), static node => node.As<TryStatement>().Handler);
            VisitAuxiliaryNode(tryStatement.Handler);
            bodyFlags = StatementBodyFlags(isRightMost: tryStatement.Finalizer is null);
        }

        if (tryStatement.Finalizer is not null)
        {
            _writeContext.ClearNodeProperty();
            Writer.WriteKeyword("finally", TokenFlags.SurroundingSpaceRecommended | StatementBodyFlagsToKeywordFlags(bodyFlags), ref _writeContext);

            _writeContext.SetNodeProperty(nameof(tryStatement.Finalizer), static node => node.As<TryStatement>().Finalizer);
            VisitStatement(tryStatement.Finalizer, StatementBodyFlags(isRightMost: true));
        }

        return tryStatement;
    }

    protected internal override object? VisitUnaryExpression(UnaryExpression unaryExpression)
    {
        var argumentNeedsBrackets = UnaryOperandNeedsBrackets(unaryExpression, unaryExpression.Argument);
        var op = UnaryExpression.GetUnaryOperatorToken(unaryExpression.Operator);

        if (unaryExpression.Prefix)
        {
            _writeContext.SetNodeProperty(nameof(unaryExpression.Operator), static node => node.As<UnaryExpression>().Operator);
            if (char.IsLetter(op[0]))
            {
                Writer.WriteKeyword(op, TokenFlags.TrailingSpaceRecommended, ref _writeContext);
            }
            else
            {
                Writer.WritePunctuator(op, TokenFlags.Leading, ref _writeContext);

                // Cases like +(+x) or +(++x) must be disambiguated with brackets.
                if (!argumentNeedsBrackets &&
                    unaryExpression.Argument is UnaryExpression argumentUnaryExpression &&
                    argumentUnaryExpression.Prefix &&
                    op[op.Length - 1] is '+' or '-' &&
                    op[op.Length - 1] == UnaryExpression.GetUnaryOperatorToken(argumentUnaryExpression.Operator)[0])
                {
                    argumentNeedsBrackets = true;
                }
            }

            _writeContext.SetNodeProperty(nameof(unaryExpression.Argument), static node => node.As<UnaryExpression>().Argument);
            VisitSubExpression(unaryExpression.Argument, SubExpressionFlags(argumentNeedsBrackets, isLeftMost: false));
        }
        else
        {
            _writeContext.SetNodeProperty(nameof(unaryExpression.Argument), static node => node.As<UnaryExpression>().Argument);
            VisitSubExpression(unaryExpression.Argument, SubExpressionFlags(argumentNeedsBrackets, isLeftMost: true));

            _writeContext.SetNodeProperty(nameof(unaryExpression.Operator), static node => node.As<UnaryExpression>().Operator);
            Writer.WritePunctuator(op, TokenFlags.Trailing, ref _writeContext);
        }

        return unaryExpression;
    }

    protected internal override object? VisitVariableDeclaration(VariableDeclaration variableDeclaration)
    {
        _writeContext.SetNodeProperty(nameof(variableDeclaration.Kind), static node => node.As<VariableDeclaration>().Kind);
        Writer.WriteKeyword(VariableDeclaration.GetVariableDeclarationKindToken(variableDeclaration.Kind),
            _currentStatementFlags.HasFlagFast(StatementFlags.NestedVariableDeclaration).ToFlag(TokenFlags.TrailingSpaceRecommended, TokenFlags.SurroundingSpaceRecommended), ref _writeContext);

        _writeContext.SetNodeProperty(nameof(variableDeclaration.Declarations), static node => ref node.As<VariableDeclaration>().Declarations);

        if (!_currentStatementFlags.HasFlagFast(StatementFlags.NestedVariableDeclaration))
        {
            VisitAuxiliaryNodeList(in variableDeclaration.Declarations, separator: ",");

            StatementNeedsSemicolon();
        }
        else if (ParentNode is not { Type: Nodes.ForStatement or Nodes.ForInStatement })
        {
            VisitAuxiliaryNodeList(in variableDeclaration.Declarations, separator: ",");
        }
        else
        {
            VisitAuxiliaryNodeList(in variableDeclaration.Declarations, separator: ",", static delegate { return s_forLoopInitDeclarationFlag; });
        }

        return variableDeclaration;
    }

    protected internal override object? VisitVariableDeclarator(VariableDeclarator variableDeclarator)
    {
        _writeContext.SetNodeProperty(nameof(variableDeclarator.Id), static node => node.As<VariableDeclarator>().Id);
        VisitAuxiliaryNode(variableDeclarator.Id);

        if (variableDeclarator.Init is not null)
        {
            _writeContext.ClearNodeProperty();
            Writer.WritePunctuator("=", TokenFlags.InBetween | TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

            _writeContext.SetNodeProperty(nameof(variableDeclarator.Init), static node => node.As<VariableDeclarator>().Init);

            if (_currentAuxiliaryNodeContext != s_forLoopInitDeclarationFlag)
            {
                VisitRootExpression(variableDeclarator.Init, RootExpressionFlags(needsBrackets: ExpressionNeedsBracketsInList(variableDeclarator.Init)));
            }
            else
            {
                VisitRootExpression(variableDeclarator.Init, ExpressionFlags.InOperatorIsAmbiguousInDeclaration | RootExpressionFlags(needsBrackets: ExpressionNeedsBracketsInList(variableDeclarator.Init)));
            }
        }

        return variableDeclarator;
    }

    protected internal override object? VisitWhileStatement(WhileStatement whileStatement)
    {
        Writer.WriteKeyword("while", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

        _writeContext.SetNodeProperty(nameof(whileStatement.Test), static node => node.As<WhileStatement>().Test);
        VisitRootExpression(whileStatement.Test, ExpressionFlags.SpaceAroundBracketsRecommended | RootExpressionFlags(needsBrackets: true));

        _writeContext.SetNodeProperty(nameof(whileStatement.Body), static node => node.As<WhileStatement>().Body);
        VisitStatement(whileStatement.Body, StatementBodyFlags(isRightMost: true));

        return whileStatement;
    }

    protected internal override object? VisitWithStatement(WithStatement withStatement)
    {
        Writer.WriteKeyword("with", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

        _writeContext.SetNodeProperty(nameof(withStatement.Object), static node => node.As<WithStatement>().Object);
        VisitRootExpression(withStatement.Object, ExpressionFlags.SpaceAroundBracketsRecommended | RootExpressionFlags(needsBrackets: true));

        _writeContext.SetNodeProperty(nameof(withStatement.Body), static node => node.As<WithStatement>().Body);
        VisitStatement(withStatement.Body, StatementBodyFlags(isRightMost: true));

        return withStatement;
    }

    protected internal override object? VisitYieldExpression(YieldExpression yieldExpression)
    {
        Writer.WriteKeyword("yield", (!yieldExpression.Delegate && yieldExpression.Argument is not null).ToFlag(TokenFlags.TrailingSpaceRecommended), ref _writeContext);

        if (yieldExpression.Delegate)
        {
            _writeContext.SetNodeProperty(nameof(yieldExpression.Delegate), static node => node.As<YieldExpression>().Delegate);
            Writer.WritePunctuator("*", (yieldExpression.Argument is not null).ToFlag(TokenFlags.TrailingSpaceRecommended), ref _writeContext);
        }

        if (yieldExpression.Argument is not null)
        {
            var argumentNeedsBrackets = UnaryOperandNeedsBrackets(yieldExpression, yieldExpression.Argument);

            _writeContext.SetNodeProperty(nameof(yieldExpression.Argument), static node => node.As<YieldExpression>().Argument);
            VisitSubExpression(yieldExpression.Argument, SubExpressionFlags(argumentNeedsBrackets, isLeftMost: false));
        }

        return yieldExpression;
    }
}
