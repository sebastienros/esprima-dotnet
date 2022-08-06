using Esprima.Ast;

namespace Esprima.Utils;

partial class JavaScriptTextWriter
{
    private protected const TriviaType WhiteSpaceTriviaFlag = (TriviaType) (1 << 1);
    private protected const TriviaType CommentTriviaFlag = (TriviaType) (1 << 2);

    protected internal enum TriviaType
    {
        None = 0,

        WhiteSpace = WhiteSpaceTriviaFlag | 0,
        EndOfLine = WhiteSpaceTriviaFlag | 1,

        LineComment = CommentTriviaFlag | 0,
        BlockComment = CommentTriviaFlag | 1,
    }

    [Flags]
    public enum TriviaFlags
    {
        None = 0,

        // Whitespace hints for non-whitespace trivia (i.e. comments)

        /// <summary>
        /// A leading new line is required for the current trivia (i.e. it must start in a new line).
        /// </summary>
        LeadingNewLineRequired = 1 << 0,
        /// <summary>
        /// A trailing new line is required for the current trivia (i.e. it must be followed by a new line).
        /// </summary>
        TrailingNewLineRequired = 1 << 1,
        /// <summary>
        /// Surrounding new lines are required for the current trivia.
        /// </summary>
        SurroundingNewLineRequired = LeadingNewLineRequired | TrailingNewLineRequired,
    }

    [Flags]
    public enum TokenFlags
    {
        None = 0,

        // Position hints for punctuators (exclusive, i.e at most one of these flags should be set)

        /// <summary>
        /// The punctuator precedes the related token(s).
        /// </summary>
        Leading = 1 << 0,
        /// <summary>
        /// The punctuator is somewhere in the middle of the related token(s). 
        /// </summary>
        InBetween = 1 << 1,
        /// <summary>
        /// The punctuator follows the related token(s). 
        /// </summary>
        Trailing = 1 << 2,

        // Whitespace hints for keywords

        /// <summary>
        /// The keyword follows the body of a statement and precedes another body of the same statement (e.g. the else branch of an <see cref="IfStatement"/>).
        /// </summary>
        FollowsStatementBody = StatementFlags.IsStatementBody,

        // General whitespace hints

        /// <summary>
        /// A leading space is recommended for the current token (unless other white-space precedes it).
        /// </summary>
        /// <remarks>
        /// May or may not be respected. (It is decided by the actual <see cref="JavaScriptTextWriter"/> implementation.)
        /// </remarks>
        LeadingSpaceRecommended = 1 << 14,
        /// <summary>
        /// A trailing space is recommended for the current token (unless other white-space follows it).
        /// </summary>
        /// <remarks>
        /// May or may not be respected. (It is decided by the actual <see cref="JavaScriptTextWriter"/> implementation.)
        /// </remarks>
        TrailingSpaceRecommended = 1 << 15,

        /// <summary>
        /// Surrounding spaces are recommended for the current token (unless other white-spaces surround it).
        /// </summary>
        /// <remarks>
        /// May or may not be respected. (It is decided by the actual <see cref="JavaScriptTextWriter"/> implementation.)
        /// </remarks>
        SurroundingSpaceRecommended = LeadingSpaceRecommended | TrailingSpaceRecommended,
    }

    [Flags]
    public enum StatementFlags
    {
        // Notes for maintainers:
        // Don't use the high-order word as it's reserved for internal use (see AstToJavaScriptConverter.StatementFlags)

        None = 0,
        /// <summary>
        /// The statement must be terminated with a semicolon.
        /// </summary>
        NeedsSemicolon = 1 << 0,
        /// <summary>
        /// If <see cref="NeedsSemicolon"/> is set, determines if the semicolon can be omitted when the statement comes last in the current block (see <seealso cref="IsRightMost"/>).
        /// </summary>
        /// <remarks>
        /// Automatically propagated to child statements, should be set directly only for statement list items.
        /// Whether the semicolon is omitted or not is decided by the actual <see cref="JavaScriptTextWriter"/> implementation.
        /// </remarks>
        MayOmitRightMostSemicolon = 1 << 1,
        /// <summary>
        /// The statement comes last in the current statement list (more precisely, it is the right-most part in the textual representation of the current statement list).
        /// </summary>
        /// <remarks>
        /// In the the visitation handlers of <see cref="AstToJavaScriptConverter"/> the flag is interpreted differently: it indicates that the statement comes last in the parent statement.
        /// (Upon visiting a statement, this flag of the parent and child statement gets combined to determine its effective value for the current statement list.)
        /// </remarks>
        IsRightMost = 1 << 2,
        /// <summary>
        /// The statement represents the body of another statement (e.g. the if branch of an <see cref="IfStatement"/>).
        /// </summary>
        IsStatementBody = 1 << 3,
    }

    [Flags]
    public enum ExpressionFlags
    {
        // Notes for maintainers:
        // Don't use the high-order word as it's reserved for internal use (see AstToJavaScriptConverter.ExpressionFlags)

        None = 0,
        /// <summary>
        /// The expression must be wrapped in brackets.
        /// </summary>
        NeedsBrackets = 1 << 0,
        /// <summary>
        /// The expression comes first in the current expression tree, more precisely, it is the left-most part in the textual representation of the currently visited expression tree (incl. brackets).
        /// </summary>
        /// <remarks>
        /// In the the visitation handlers of <see cref="AstToJavaScriptConverter"/> the flag is interpreted differently: it indicates that the expression comes first in the parent expression.
        /// (Upon visiting an expression, this flag of the parent and child expression gets combined to determine its effective value for the expression tree.)
        /// </remarks>
        IsLeftMost = 1 << 1,

        // White-space hints

        SpaceBeforeBracketsRecommended = 1 << 14,
        SpaceAfterBracketsRecommended = 1 << 15,
        SpaceAroundBracketsRecommended = SpaceBeforeBracketsRecommended | SpaceAfterBracketsRecommended,
    }
}
