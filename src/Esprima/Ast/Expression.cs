using System.Runtime.CompilerServices;

namespace Esprima.Ast;

/// <summary>
/// A JavaScript expression. 
/// </summary>
public abstract class Expression : StatementListItem, ISyntaxTreeRoot
{
    protected Expression(Nodes type) : base(type)
    {
    }

    /// <summary>
    /// Gets or sets the list of tokens associated with the AST represented by this node.
    /// This property is automatically set by <see cref="JavaScriptParser.ParseExpression(string)"/> when <see cref="ParserOptions.Tokens"/> is set to <see langword="true"/>.
    /// </summary>
    /// <remarks>
    /// The operation is not guaranteed to be thread-safe. In case concurrent access or update is possible, the necessary synchronization is caller's responsibility.
    /// </remarks>
    public IReadOnlyList<SyntaxToken>? Tokens
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (IReadOnlyList<SyntaxToken>?) GetAdditionalData(s_tokensAdditionalDataKey);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => SetAdditionalData(s_tokensAdditionalDataKey, value);
    }

    /// <summary>
    /// Gets or sets the list of comments associated with the AST represented by this node.
    /// This property is automatically set by <see cref="JavaScriptParser.ParseExpression(string)"/> when <see cref="ParserOptions.Comments"/> is set to <see langword="true"/>.
    /// </summary>
    /// <remarks>
    /// The operation is not guaranteed to be thread-safe. In case concurrent access or update is possible, the necessary synchronization is caller's responsibility.
    /// </remarks>
    public IReadOnlyList<SyntaxComment>? Comments
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (IReadOnlyList<SyntaxComment>?) GetAdditionalData(s_commentsAdditionalDataKey);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => SetAdditionalData(s_commentsAdditionalDataKey, value);
    }
}
