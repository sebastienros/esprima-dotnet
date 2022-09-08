using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast;

public abstract class Program : Node, ISyntaxTreeRoot
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
        get => (IReadOnlyList<SyntaxToken>?) AdditionalData[s_tokensAdditionalDataKey];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => AdditionalData[s_tokensAdditionalDataKey] = value;
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
        get => (IReadOnlyList<SyntaxComment>?) AdditionalData[s_commentsAdditionalDataKey];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => AdditionalData[s_commentsAdditionalDataKey] = value;
    }

    internal sealed override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Body);

    protected internal sealed override object? Accept(AstVisitor visitor) => visitor.VisitProgram(this);

    protected abstract Program Rewrite(in NodeList<Statement> body);

    public Program UpdateWith(in NodeList<Statement> body)
    {
        if (NodeList.AreSame(body, Body))
        {
            return this;
        }

        return Rewrite(body);
    }
}
