using System.Diagnostics;
using System.Runtime.CompilerServices;
using Esprima.Ast;
using static Esprima.Utils.JavascriptTextWriter;

namespace Esprima.Utils;

partial class AstToJavascriptConverter
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private protected static StatementFlags StatementBodyFlags(bool isRightMost)
    {
        return StatementFlags.IsStatementBody | isRightMost.ToFlag(StatementFlags.IsRightMost);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private protected static TokenFlags StatementBodyFlagsToKeywordFlags(StatementFlags previousBodyFlags)
    {
        // Maps IsStatementBody to keyword flags.
        return (TokenFlags) (previousBodyFlags & StatementFlags.IsStatementBody);
    }

    protected StatementFlags PropagateStatementFlags(StatementFlags flags)
    {
        // Caller must not set NeedsSemicolon or MayOmitRightMostSemicolon.
        // NeedsSemicolon is set by the visitation handler of statement via the StatementNeedsSemicolon method,
        // MayOmitRightMostSemicolon is set by VisitStatementList.
        Debug.Assert((flags & (StatementFlags.NeedsSemicolon | StatementFlags.MayOmitRightMostSemicolon)) == 0);

        // Combines IsRightMost of parent and current statement to determine its effective value for the current statement list.
        flags &= ~StatementFlags.IsRightMost | _currentStatementFlags & StatementFlags.IsRightMost;

        // Propagates MayOmitRightMostSemicolon to current statement.
        flags |= _currentStatementFlags & StatementFlags.MayOmitRightMostSemicolon;

        return flags;
    }

    private protected static readonly Func<AstToJavascriptConverter, Statement, StatementFlags, StatementFlags> s_getCombinedStatementFlags = static (@this, statement, flags) =>
        @this.PropagateStatementFlags(flags);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void VisitStatement(Statement statement, StatementFlags flags)
    {
        VisitStatement(statement, flags, s_getCombinedStatementFlags);
    }

    protected void VisitStatement(Statement statement, StatementFlags flags, Func<AstToJavascriptConverter, Statement, StatementFlags, StatementFlags> getCombinedFlags)
    {
        var originalStatementFlags = _currentStatementFlags;
        _currentStatementFlags = getCombinedFlags(this, statement, flags);

        Writer.StartStatement((JavascriptTextWriter.StatementFlags) _currentStatementFlags, ref _writeContext);
        Visit(statement);
        Writer.EndStatement((JavascriptTextWriter.StatementFlags) _currentStatementFlags, ref _writeContext);

        _currentStatementFlags = originalStatementFlags;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void VisitStatementList(in NodeList<Statement> statementList)
    {
        VisitStatementList(in statementList, static (_, _, index, count) =>
            (index == count - 1).ToFlag(StatementFlags.IsRightMost | StatementFlags.MayOmitRightMostSemicolon));
    }

    protected void VisitStatementList(in NodeList<Statement> statementList, Func<AstToJavascriptConverter, Statement, int, int, StatementFlags> getCombinedItemFlags)
    {
        Writer.StartStatementList(statementList.Count, ref _writeContext);

        for (var i = 0; i < statementList.Count; i++)
        {
            VisitStatementListItem(statementList[i], i, statementList.Count, getCombinedItemFlags);
        }

        Writer.EndStatementList(statementList.Count, ref _writeContext);
    }

    protected void VisitStatementListItem(Statement statement, int index, int count, Func<AstToJavascriptConverter, Statement, int, int, StatementFlags> getCombinedFlags)
    {
        var originalStatementFlags = _currentStatementFlags;
        _currentStatementFlags = getCombinedFlags(this, statement, index, count);

        Writer.StartStatementListItem(index, count, (JavascriptTextWriter.StatementFlags) _currentStatementFlags, ref _writeContext);
        Visit(statement);
        Writer.EndStatementListItem(index, count, (JavascriptTextWriter.StatementFlags) _currentStatementFlags, ref _writeContext);

        _currentStatementFlags = originalStatementFlags;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void StatementNeedsSemicolon() => _currentStatementFlags |= StatementFlags.NeedsSemicolon;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private protected static ExpressionFlags RootExpressionFlags(bool needsBrackets)
    {
        return ExpressionFlags.IsLeftMost | needsBrackets.ToFlag(ExpressionFlags.NeedsBrackets);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private protected static ExpressionFlags LeftHandSideRootExpressionFlags(bool needsBrackets)
    {
        return ExpressionFlags.IsInsideLeftHandSideExpression | ExpressionFlags.IsLeftMostInLeftHandSideExpression | RootExpressionFlags(needsBrackets);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private protected static ExpressionFlags SubExpressionFlags(bool needsBrackets, bool isLeftMost)
    {
        return needsBrackets.ToFlag(ExpressionFlags.NeedsBrackets) | isLeftMost.ToFlag(ExpressionFlags.IsLeftMost);
    }

    protected ExpressionFlags PropagateExpressionFlags(ExpressionFlags flags)
    {
        const ExpressionFlags isLeftMostFlags =
            ExpressionFlags.IsLeftMost |
            ExpressionFlags.IsLeftMostInArrowFunctionBody |
            ExpressionFlags.IsLeftMostInNewCallee |
            ExpressionFlags.IsLeftMostInLeftHandSideExpression;

        // Combines IsLeftMost* flags of parent and current statement to determine their effective values for the current expression tree.
        if (_currentExpressionFlags.HasFlagFast(ExpressionFlags.NeedsBrackets) || !flags.HasFlagFast(ExpressionFlags.IsLeftMost))
        {
            flags &= ~isLeftMostFlags;
        }
        else
        {
            flags = flags & ~isLeftMostFlags | _currentExpressionFlags & isLeftMostFlags;
        }

        // Propagates IsInsideStatementExpression, IsInsideArrowFunctionBody and IsInsideLeftHandSideExpression to current expression.
        flags |= _currentExpressionFlags & ExpressionFlags.IsInPotentiallyAmbiguousContext;

        return flags;
    }

    protected ExpressionFlags DisambiguateExpression(Expression expression, ExpressionFlags flags)
    {
        if (flags.HasFlagFast(ExpressionFlags.NeedsBrackets))
        {
            return flags & ~ExpressionFlags.InOperatorIsAmbiguousInDeclaration;
        }

        // Puts the left-most expression in brackets if necessary (in cases where it would be interpreted differently without brackets).
        if ((flags & ExpressionFlags.IsInPotentiallyAmbiguousContext) != 0)
        {
            if (flags.HasFlagFast(ExpressionFlags.IsInsideStatementExpression | ExpressionFlags.IsLeftMost) && ExpressionIsAmbiguousAsStatementExpression(expression) ||
                flags.HasFlagFast(ExpressionFlags.IsInsideLeftHandSideExpression | ExpressionFlags.IsLeftMostInLeftHandSideExpression) && LeftHandSideExpressionIsParenthesized(expression) ||
                flags.HasFlagFast(ExpressionFlags.IsInsideArrowFunctionBody | ExpressionFlags.IsLeftMostInArrowFunctionBody) && ExpressionIsAmbiguousAsArrowFunctionBody(expression) ||
                flags.HasFlagFast(ExpressionFlags.IsInsideNewCallee | ExpressionFlags.IsLeftMostInNewCallee) && ExpressionIsAmbiguousAsNewCallee(expression))
            {
                return (flags | ExpressionFlags.NeedsBrackets) & ~ExpressionFlags.InOperatorIsAmbiguousInDeclaration;
            }
            // Edge case: for (var a = b = (c in d in e) in x);
            else if (flags.HasFlagFast(ExpressionFlags.InOperatorIsAmbiguousInDeclaration) && expression is BinaryExpression { Operator: BinaryOperator.In })
            {
                return (flags | ExpressionFlags.NeedsBrackets) & ~ExpressionFlags.InOperatorIsAmbiguousInDeclaration;
            }
        }

        return flags;
    }

    private protected static readonly Func<AstToJavascriptConverter, Expression, ExpressionFlags, ExpressionFlags> s_getCombinedRootExpressionFlags = static (@this, expression, flags) =>
        @this.DisambiguateExpression(expression, flags);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void VisitRootExpression(Expression expression, ExpressionFlags flags)
    {
        VisitExpression(expression, flags, s_getCombinedRootExpressionFlags);
    }

    private protected static readonly Func<AstToJavascriptConverter, Expression, ExpressionFlags, ExpressionFlags> s_getCombinedSubExpressionFlags = static (@this, expression, flags) =>
        @this.DisambiguateExpression(expression, @this.PropagateExpressionFlags(flags));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void VisitSubExpression(Expression expression, ExpressionFlags flags)
    {
        VisitExpression(expression, flags, s_getCombinedSubExpressionFlags);
    }

    protected void VisitExpression(Expression expression, ExpressionFlags flags, Func<AstToJavascriptConverter, Expression, ExpressionFlags, ExpressionFlags> getCombinedFlags)
    {
        var originalExpressionFlags = _currentExpressionFlags;
        _currentExpressionFlags = getCombinedFlags(this, expression, flags);

        Writer.StartExpression((JavascriptTextWriter.ExpressionFlags) _currentExpressionFlags, ref _writeContext);
        Visit(expression);
        Writer.EndExpression((JavascriptTextWriter.ExpressionFlags) _currentExpressionFlags, ref _writeContext);

        _currentExpressionFlags = originalExpressionFlags;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void VisitSubExpressionList(in NodeList<Expression> expressionList)
    {
        VisitExpressionList(in expressionList, static (@this, expression, index, _) =>
            s_getCombinedSubExpressionFlags(@this, expression, SubExpressionFlags(@this.ExpressionNeedsBracketsInList(expression), isLeftMost: false)));
    }

    protected void VisitExpressionList(in NodeList<Expression> expressionList, Func<AstToJavascriptConverter, Expression, int, int, ExpressionFlags> getCombinedItemFlags)
    {
        Writer.StartExpressionList(expressionList.Count, ref _writeContext);

        for (var i = 0; i < expressionList.Count; i++)
        {
            VisitExpressionListItem(expressionList[i], i, expressionList.Count, getCombinedItemFlags);
        }

        Writer.EndExpressionList(expressionList.Count, ref _writeContext);
    }

    protected void VisitExpressionListItem(Expression expression, int index, int count, Func<AstToJavascriptConverter, Expression, int, int, ExpressionFlags> getCombinedFlags)
    {
        var originalExpressionFlags = _currentExpressionFlags;
        _currentExpressionFlags = getCombinedFlags(this, expression, index, count);

        Writer.StartExpressionListItem(index, count, (JavascriptTextWriter.ExpressionFlags) _currentExpressionFlags, ref _writeContext);
        Visit(expression);
        Writer.EndExpressionListItem(index, count, (JavascriptTextWriter.ExpressionFlags) _currentExpressionFlags, ref _writeContext);

        _currentExpressionFlags = originalExpressionFlags;
    }

    private void VisitAssertions(in NodeList<ImportAttribute> assertions)
    {
        // https://github.com/tc39/proposal-import-assertions

        Writer.WriteKeyword("assert", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

        Writer.StartObject(assertions.Count, ref _writeContext);

        VisitAuxiliaryNodeList(in assertions, separator: ",");

        Writer.EndObject(assertions.Count, ref _writeContext);
    }

    private void VisitExportOrImportSpecifierIdentifier(Expression identifierExpression)
    {
        if (identifierExpression is Identifier { Name: "default" } identifier)
        {
            Writer.WriteKeyword("default", ref _writeContext);
        }
        else
        {
            VisitRootExpression(identifierExpression, RootExpressionFlags(needsBrackets: false));
        }
    }

    private void VisitPropertyKey(Expression key, bool computed, TokenFlags leadingBracketFlags = TokenFlags.None, TokenFlags trailingBracketFlags = TokenFlags.None)
    {
        if (computed)
        {
            Writer.WritePunctuator("[", TokenFlags.Leading | leadingBracketFlags, ref _writeContext);
            VisitRootExpression(key, RootExpressionFlags(needsBrackets: ExpressionNeedsBracketsInList(key)));
            Writer.WritePunctuator("]", TokenFlags.Trailing | trailingBracketFlags, ref _writeContext);
        }
        else if (key.Type == Nodes.Identifier)
        {
            VisitAuxiliaryNode(key);
        }
        else
        {
            VisitRootExpression(key, RootExpressionFlags(needsBrackets: false));
        }
    }

    protected virtual bool ExpressionIsAmbiguousAsStatementExpression(Expression expression)
    {
        switch (expression.Type)
        {
            case Nodes.ClassExpression:
            case Nodes.FunctionExpression:
            case Nodes.ObjectExpression:
            case Nodes.AssignmentExpression when expression.As<AssignmentExpression>() is { Left.Type: Nodes.ObjectPattern }:
            case Nodes.Identifier when Scanner.IsStrictModeReservedWord(expression.As<Identifier>().Name):
                return true;
        }

        return false;
    }

    protected virtual bool ExpressionIsAmbiguousAsArrowFunctionBody(Expression expression)
    {
        switch (expression.Type)
        {
            case Nodes.ObjectExpression:
            case Nodes.AssignmentExpression when expression.As<AssignmentExpression>() is { Left.Type: Nodes.ObjectPattern }:
                return true;
        }

        return false;
    }

    protected virtual bool ExpressionIsAmbiguousAsNewCallee(Expression expression)
    {
        switch (expression.Type)
        {
            case Nodes.CallExpression:
                return true;
        }

        return false;
    }

    protected virtual bool LeftHandSideExpressionIsParenthesized(Expression expression)
    {
        // https://tc39.es/ecma262/#sec-left-hand-side-expressions

        switch (expression.Type)
        {
            case Nodes.ArrowFunctionExpression:
            case Nodes.AssignmentExpression:
            case Nodes.AwaitExpression:
            case Nodes.BinaryExpression:
            case Nodes.LogicalExpression:
            case Nodes.ConditionalExpression:
            case Nodes.SequenceExpression:
            case Nodes.UnaryExpression:
            case Nodes.UpdateExpression:
            case Nodes.YieldExpression:
            case Nodes.ClassExpression when expression.As<ClassExpression>().Decorators.Count > 0:
                return true;
        }

        return false;
    }

    protected virtual bool ExpressionNeedsBracketsInList(Expression expression)
    {
        return expression.Type is
            Nodes.SequenceExpression;
    }

    protected virtual int GetOperatorPrecedence(Expression expression, out int associativity) =>
        expression.GetOperatorPrecedence(out associativity) is >= 0 and var result
        ? result
        : throw new NotImplementedException($"Operator precedence for expression of type {expression.GetType()} is not defined.");

    protected bool UnaryOperandNeedsBrackets(Expression operation, Expression operand) =>
         GetOperatorPrecedence(operation, out _) > GetOperatorPrecedence(operand, out _);

    protected BinaryOperationFlags BinaryOperandsNeedBrackets(Expression operation, Expression leftOperand, Expression rightOperand)
    {
        var operationPrecedence = GetOperatorPrecedence(operation, out var associativity);
        var leftOperandPrecedence = GetOperatorPrecedence(leftOperand, out _);
        var rightOperandPrecedence = GetOperatorPrecedence(rightOperand, out _);

        var result = BinaryOperationFlags.None;

        if (operationPrecedence > leftOperandPrecedence || operationPrecedence == leftOperandPrecedence && associativity > 0) // right-to-left associativity
        {
            result |= BinaryOperationFlags.LeftOperandNeedsBrackets;
        }

        if (operationPrecedence > rightOperandPrecedence || operationPrecedence == rightOperandPrecedence && associativity < 0) // left-to-right associativity
        {
            result |= BinaryOperationFlags.RightOperandNeedsBrackets;
        }

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void VisitAuxiliaryNode(Node node)
    {
        VisitAuxiliaryNode(node, static delegate { return null; });
    }

    protected void VisitAuxiliaryNode(Node node, Func<AstToJavascriptConverter, Node, object?> getNodeContext)
    {
        var originalAuxiliaryNodeContext = _currentAuxiliaryNodeContext;
        _currentAuxiliaryNodeContext = getNodeContext(this, node);

        Writer.StartAuxiliaryNode(_currentAuxiliaryNodeContext, ref _writeContext);
        Visit(node);
        Writer.EndAuxiliaryNode(_currentAuxiliaryNodeContext, ref _writeContext);

        _currentAuxiliaryNodeContext = originalAuxiliaryNodeContext;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void VisitAuxiliaryNodeList<TNode>(in NodeList<TNode> nodeList, string separator)
        where TNode : Node
    {
        VisitAuxiliaryNodeList(in nodeList, separator, static delegate { return null; });
    }

    protected void VisitAuxiliaryNodeList<TNode>(in NodeList<TNode> nodeList, string separator, Func<AstToJavascriptConverter, Node?, int, int, object?> getNodeContext)
        where TNode : Node
    {
        Writer.StartAuxiliaryNodeList<TNode>(nodeList.Count, ref _writeContext);

        for (var i = 0; i < nodeList.Count; i++)
        {
            VisitAuxiliaryNodeListItem(nodeList[i], i, nodeList.Count, separator, getNodeContext);
        }

        Writer.EndAuxiliaryNodeList<TNode>(nodeList.Count, ref _writeContext);
    }

    protected void VisitAuxiliaryNodeListItem<TNode>(TNode node, int index, int count, string separator, Func<AstToJavascriptConverter, Node?, int, int, object?> getNodeContext)
        where TNode : Node
    {
        var originalAuxiliaryNodeContext = _currentAuxiliaryNodeContext;
        _currentAuxiliaryNodeContext = getNodeContext(this, node, index, count);

        Writer.StartAuxiliaryNodeListItem<TNode>(index, count, separator, _currentAuxiliaryNodeContext, ref _writeContext);
        Visit(node);
        Writer.EndAuxiliaryNodeListItem<TNode>(index, count, separator, _currentAuxiliaryNodeContext, ref _writeContext);

        _currentAuxiliaryNodeContext = originalAuxiliaryNodeContext;
    }
}
