namespace Esprima.Utils;

partial class AstToJavaScriptConverter
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

        NeedsSemicolon = JavaScriptTextWriter.StatementFlags.NeedsSemicolon,
        MayOmitRightMostSemicolon = JavaScriptTextWriter.StatementFlags.MayOmitRightMostSemicolon,
        IsRightMost = JavaScriptTextWriter.StatementFlags.IsRightMost,
        IsStatementBody = JavaScriptTextWriter.StatementFlags.IsStatementBody,

        NestedVariableDeclaration = 1 << 16,
    }

    [Flags]
    protected internal enum ExpressionFlags
    {
        None = 0,

        NeedsBrackets = JavaScriptTextWriter.ExpressionFlags.NeedsBrackets,
        IsLeftMost = JavaScriptTextWriter.ExpressionFlags.IsLeftMost,

        SpaceBeforeBracketsRecommended = JavaScriptTextWriter.ExpressionFlags.SpaceBeforeBracketsRecommended,
        SpaceAfterBracketsRecommended = JavaScriptTextWriter.ExpressionFlags.SpaceAfterBracketsRecommended,
        SpaceAroundBracketsRecommended = JavaScriptTextWriter.ExpressionFlags.SpaceAroundBracketsRecommended,

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
