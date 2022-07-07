namespace Esprima.Utils;

partial class AstToJavascriptConverter
{
    [Flags]
    protected internal enum BinaryOperationFlags
    {
        None = 0,
        LeftOperandNeedsBrackets = 1 << 0,
        RightOperandNeedsBrackets = 1 << 1,
        BothOperandsNeedBrackets = LeftOperandNeedsBrackets | RightOperandNeedsBrackets
    }

    [Flags]
    protected internal enum StatementFlags
    {
        None = 0,

        NeedsSemicolon = JavascriptTextWriter.StatementFlags.NeedsSemicolon,
        MayOmitRightMostSemicolon = JavascriptTextWriter.StatementFlags.MayOmitRightMostSemicolon,
        IsRightMost = JavascriptTextWriter.StatementFlags.IsRightMost,
        IsStatementBody = JavascriptTextWriter.StatementFlags.IsStatementBody,

        NestedVariableDeclaration = 1 << 16,
    }

    [Flags]
    protected internal enum ExpressionFlags
    {
        None = 0,

        NeedsBrackets = JavascriptTextWriter.ExpressionFlags.NeedsBrackets,
        IsLeftMost = JavascriptTextWriter.ExpressionFlags.IsLeftMost,

        SpaceBeforeBracketsRecommended = JavascriptTextWriter.ExpressionFlags.SpaceBeforeBracketsRecommended,
        SpaceAfterBracketsRecommended = JavascriptTextWriter.ExpressionFlags.SpaceAfterBracketsRecommended,
        SpaceAroundBracketsRecommended = JavascriptTextWriter.ExpressionFlags.SpaceAroundBracketsRecommended,

        IsMethod = 1 << 16,

        InOperatorIsAmbiguousInDeclaration = 1 << 24, // automatically propagated to sub-expressions

        IsLeftMostInArrowFunctionBody = 1 << 25,  // automatically combined and propagated to sub-expressions
        IsInsideArrowFunctionBody = 1 << 26, // automatically propagated to sub-expressions

        // https://stackoverflow.com/a/17587899/8656352
        IsLeftMostInNewCallee = 1 << 27,  // automatically combined and propagated to sub-expressions
        IsInsideNewCallee = 1 << 28, // automatically propagated to sub-expressions

        IsLeftMostInLeftHandSideExpression = 1 << 29, // automatically combined and propagated to sub-expressions
        IsInsideLeftHandSideExpression = 1 << 30, // automatically propagated to sub-expressions

        IsInsideStatementExpression = 1 << 31, // automatically propagated to sub-expressions

        IsInPotentiallyAmbiguousContext = InOperatorIsAmbiguousInDeclaration | IsInsideArrowFunctionBody | IsInsideNewCallee | IsInsideLeftHandSideExpression | IsInsideStatementExpression,
    }
}
