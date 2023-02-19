using System.Runtime.CompilerServices;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Body) }, SealOverrideMethods = true)]
public abstract partial class Program : Node, ISyntaxTreeRoot
{
    private readonly NodeList<Statement> _body;

    protected Program(in NodeList<Statement> body) : base(Nodes.Program)
    {
        _body = body;
    }

    public ref readonly NodeList<Statement> Body { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _body; }

    public abstract SourceType SourceType { get; }
    public abstract bool Strict { get; }

    /// <summary>
    /// Gets or sets the list of tokens associated with the AST represented by this node.
    /// This property is automatically set by <see cref="JavaScriptParser.ParseScript"/> and <see cref="JavaScriptParser.ParseModule"/> when <see cref="ParserOptions.Tokens"/> is set to <see langword="true"/>.
    /// </summary>
    /// <remarks>
    /// The operation is not guaranteed to be thread-safe. In case concurrent access or update is possible, the necessary synchronization is caller's responsibility.
    /// </remarks>
    public IReadOnlyList<SyntaxToken>? Tokens
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (IReadOnlyList<SyntaxToken>?) GetDynamicPropertyValue(TokensPropertyIndex);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => SetDynamicPropertyValue(TokensPropertyIndex, value);
    }

    /// <summary>
    /// Gets or sets the list of comments associated with the AST represented by this node.
    /// This property is automatically set by <see cref="JavaScriptParser.ParseScript"/> and <see cref="JavaScriptParser.ParseModule"/> when <see cref="ParserOptions.Comments"/> is set to <see langword="true"/>.
    /// </summary>
    /// <remarks>
    /// The operation is not guaranteed to be thread-safe. In case concurrent access or update is possible, the necessary synchronization is caller's responsibility.
    /// </remarks>
    public IReadOnlyList<SyntaxComment>? Comments
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (IReadOnlyList<SyntaxComment>?) GetDynamicPropertyValue(CommentsPropertyIndex);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => SetDynamicPropertyValue(CommentsPropertyIndex, value);
    }

    protected abstract Program Rewrite(in NodeList<Statement> body);
}
