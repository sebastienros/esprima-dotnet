using System.Runtime.CompilerServices;
using Esprima.Ast;

namespace Esprima.Utils;

public delegate object? NodePropertyValueAccessor(Node node);

public delegate ref readonly NodeList<T> NodePropertyListValueAccessor<T>(Node node) where T : Node?;

/// <summary>
/// Base Javascript text writer (code formatter) which uses the most compact possible (i.e. minimal) format.
/// </summary>
public partial class JavascriptTextWriter
{
    public record class Options
    {
        public static readonly Options Default = new();
    }

    public delegate JavascriptTextWriter Factory(TextWriter writer, Options options);

    private readonly TextWriter _writer;

    public JavascriptTextWriter(TextWriter writer, Options options)
    {
        _writer = writer ?? throw new ArgumentNullException(nameof(writer));

        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        LastTokenType = TokenType.EOF;
        WhiteSpaceWrittenSinceLastToken = true;
    }

    protected TokenType LastTokenType { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; [MethodImpl(MethodImplOptions.AggressiveInlining)] private set; }
    protected TokenFlags LastTokenFlags { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; [MethodImpl(MethodImplOptions.AggressiveInlining)] private set; }
    protected bool WhiteSpaceWrittenSinceLastToken { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; [MethodImpl(MethodImplOptions.AggressiveInlining)] private set; }

    #region White-space

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void WriteSpace()
    {
        WriteWhiteSpace(" ");
    }

    protected void WriteLine()
    {
        _writer.WriteLine();
        WhiteSpaceWrittenSinceLastToken = true;
    }

    protected void WriteWhiteSpace(string value)
    {
        _writer.Write(value);
        WhiteSpaceWrittenSinceLastToken = true;
    }

    #endregion

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void ForceRecommendedSpace()
    {
        LastTokenFlags |= TokenFlags.TrailingSpaceRecommended;
    }

    public virtual void WriteEpsilon(TokenFlags flags, in WriteContext context) { }

    #region Identifiers

    protected virtual void StartIdentifier(string value, TokenFlags flags, in WriteContext context)
    {
        switch (LastTokenType)
        {
            case TokenType.BigIntLiteral:
            case TokenType.BooleanLiteral:
            case TokenType.Identifier:
            case TokenType.Keyword:
            case TokenType.NullLiteral:
            case TokenType.NumericLiteral:
            case TokenType.RegularExpression:
                WriteSpace();
                break;
            case TokenType.EOF:
            case TokenType.Punctuator:
            case TokenType.StringLiteral:
            case TokenType.Template:
                break;
            default:
                throw new InvalidOperationException();
        }
    }

    public void WriteIdentifier(string value, TokenFlags flags, in WriteContext context)
    {
        StartIdentifier(value, flags, in context);
        _writer.Write(value);
        WhiteSpaceWrittenSinceLastToken = false;
        EndIdentifier(value, flags, in context);

        LastTokenType = TokenType.Identifier;
        LastTokenFlags = flags;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteIdentifier(string value, in WriteContext context)
    {
        WriteIdentifier(value, TokenFlags.None, in context);
    }

    protected virtual void EndIdentifier(string value, TokenFlags flags, in WriteContext context) { }

    #endregion

    #region Keywords

    protected virtual void StartKeyword(string value, TokenFlags flags, in WriteContext context)
    {
        switch (LastTokenType)
        {
            case TokenType.BigIntLiteral:
            case TokenType.BooleanLiteral:
            case TokenType.Identifier:
            case TokenType.Keyword:
            case TokenType.NullLiteral:
            case TokenType.NumericLiteral:
            case TokenType.RegularExpression:
                WriteSpace();
                break;
            case TokenType.EOF:
            case TokenType.Punctuator:
            case TokenType.StringLiteral:
            case TokenType.Template:
                break;
            default:
                throw new InvalidOperationException();
        }
    }

    public void WriteKeyword(string value, TokenFlags flags, in WriteContext context)
    {
        StartKeyword(value, flags, in context);
        _writer.Write(value);
        WhiteSpaceWrittenSinceLastToken = false;
        EndKeyword(value, flags, in context);

        LastTokenType = TokenType.Keyword;
        LastTokenFlags = flags;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteKeyword(string value, in WriteContext context)
    {
        WriteKeyword(value, TokenFlags.None, in context);
    }

    protected virtual void EndKeyword(string value, TokenFlags flags, in WriteContext context) { }

    #endregion

    #region Literals

    protected virtual void StartLiteral(string value, TokenType type, TokenFlags flags, in WriteContext context)
    {
        switch (LastTokenType)
        {
            case TokenType.BigIntLiteral:
            case TokenType.BooleanLiteral:
            case TokenType.Identifier:
            case TokenType.Keyword:
            case TokenType.NullLiteral:
            case TokenType.NumericLiteral:
            case TokenType.RegularExpression:
                if (type is not (TokenType.StringLiteral or TokenType.RegularExpression))
                {
                    WriteSpace();
                }
                break;
            case TokenType.EOF:
            case TokenType.Punctuator:
            case TokenType.StringLiteral:
            case TokenType.Template:
                break;
            default:
                throw new InvalidOperationException();
        }
    }

    public void WriteLiteral(string value, TokenType type, TokenFlags flags, in WriteContext context)
    {
        StartLiteral(value, type, flags, in context);
        _writer.Write(value);
        WhiteSpaceWrittenSinceLastToken = false;
        EndLiteral(value, type, flags, in context);

        LastTokenType = type;
        LastTokenFlags = flags;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteLiteral(string value, TokenType tokenType, in WriteContext context)
    {
        WriteLiteral(value, tokenType, TokenFlags.None, in context);
    }

    protected virtual void EndLiteral(string value, TokenType type, TokenFlags flags, in WriteContext context) { }

    #endregion

    #region Punctuators

    protected virtual void StartPunctuator(string value, TokenFlags flags, in WriteContext context) { }

    public void WritePunctuator(string value, TokenFlags flags, in WriteContext context)
    {
        StartPunctuator(value, flags, in context);
        _writer.Write(value);
        WhiteSpaceWrittenSinceLastToken = false;
        EndPunctuator(value, flags, in context);

        LastTokenType = TokenType.Punctuator;
        LastTokenFlags = flags;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WritePunctuator(string value, in WriteContext context)
    {
        WritePunctuator(value, TokenFlags.None, in context);
    }

    protected virtual void EndPunctuator(string value, TokenFlags flags, in WriteContext context) { }

    #endregion

    #region Arrays

    public virtual void StartArray(int elementCount, in WriteContext context)
    {
        WritePunctuator("[", TokenFlags.Leading, in context);
    }

    public virtual void EndArray(int elementCount, in WriteContext context)
    {
        WritePunctuator("]", TokenFlags.Trailing, in context);
    }

    #endregion

    #region Objects

    public virtual void StartObject(int propertyCount, in WriteContext context)
    {
        WritePunctuator("{", TokenFlags.Leading | TokenFlags.TrailingSpaceRecommended, in context);
    }

    public virtual void EndObject(int propertyCount, in WriteContext context)
    {
        WritePunctuator("}", TokenFlags.Trailing | TokenFlags.LeadingSpaceRecommended, in context);
    }

    #endregion

    #region Blocks

    public virtual void StartBlock(int statementCount, in WriteContext context)
    {
        WritePunctuator("{", TokenFlags.Leading | TokenFlags.SurroundingSpaceRecommended, in context);
    }

    public virtual void EndBlock(int statementCount, in WriteContext context)
    {
        WritePunctuator("}", TokenFlags.Trailing | TokenFlags.LeadingSpaceRecommended, in context);
    }

    #endregion

    #region Statements

    public virtual void StartStatement(StatementFlags flags, in WriteContext context) { }

    public virtual void EndStatement(StatementFlags flags, in WriteContext context)
    {
        // Writes statement terminator unless it can be omitted.
        if (flags.HasFlagFast(StatementFlags.NeedsSemicolon) && !flags.HasFlagFast(StatementFlags.MayOmitRightMostSemicolon | StatementFlags.IsRightMost))
        {
            WritePunctuator(";", TokenFlags.Trailing | TokenFlags.TrailingSpaceRecommended, in context);
        }
    }

    public virtual void StartStatementList(int count, in WriteContext context) { }

    public virtual void StartStatementListItem(int index, int count, StatementFlags flags, in WriteContext context) { }

    public virtual void EndStatementListItem(int index, int count, StatementFlags flags, in WriteContext context)
    {
        // Writes statement terminator unless it can be omitted.
        if (flags.HasFlagFast(StatementFlags.NeedsSemicolon) && !flags.HasFlagFast(StatementFlags.MayOmitRightMostSemicolon | StatementFlags.IsRightMost))
        {
            WritePunctuator(";", TokenFlags.Trailing | TokenFlags.TrailingSpaceRecommended, in context);
        }
    }

    public virtual void EndStatementList(int count, in WriteContext context) { }

    #endregion

    #region Expressions

    public virtual void StartExpression(ExpressionFlags flags, in WriteContext context)
    {
        if (flags.HasFlagFast(ExpressionFlags.NeedsBrackets))
        {
            WritePunctuator("(", TokenFlags.Leading | flags.HasFlagFast(ExpressionFlags.SpaceAroundBracketsRecommended).ToFlag(TokenFlags.LeadingSpaceRecommended), in context);
        }
    }

    public virtual void EndExpression(ExpressionFlags flags, in WriteContext context)
    {
        if (flags.HasFlagFast(ExpressionFlags.NeedsBrackets))
        {
            WritePunctuator(")", TokenFlags.Trailing | flags.HasFlagFast(ExpressionFlags.SpaceAroundBracketsRecommended).ToFlag(TokenFlags.TrailingSpaceRecommended), in context);
        }
    }

    public virtual void StartExpressionList(int count, in WriteContext context) { }

    public virtual void StartExpressionListItem(int index, int count, ExpressionFlags flags, in WriteContext context)
    {
        if (flags.HasFlagFast(ExpressionFlags.NeedsBrackets))
        {
            WritePunctuator("(", TokenFlags.Leading, in context);
        }
    }

    public virtual void EndExpressionListItem(int index, int count, ExpressionFlags flags, in WriteContext context)
    {
        if (flags.HasFlagFast(ExpressionFlags.NeedsBrackets))
        {
            WritePunctuator(")", TokenFlags.Trailing, in context);
        }

        if (index < count - 1)
        {
            WritePunctuator(",", TokenFlags.InBetween | TokenFlags.TrailingSpaceRecommended, in context);
        }
    }

    public virtual void EndExpressionList(int count, in WriteContext context) { }

    #endregion

    #region Auxiliary nodes

    public virtual void StartAuxiliaryNode(object? nodeContext, in WriteContext context) { }

    public virtual void EndAuxiliaryNode(object? nodeContext, in WriteContext context) { }

    public virtual void StartAuxiliaryNodeList<T>(int count, in WriteContext context) where T : Node? { }

    public virtual void StartAuxiliaryNodeListItem<T>(int index, int count, string separator, object? nodeContext, in WriteContext context) where T : Node? { }

    public virtual void EndAuxiliaryNodeListItem<T>(int index, int count, string separator, object? nodeContext, in WriteContext context) where T : Node?
    {
        if (separator.Length > 0 && index < count - 1)
        {
            WritePunctuator(separator, TokenFlags.InBetween | TokenFlags.TrailingSpaceRecommended, in context);
        }
    }

    public virtual void EndAuxiliaryNodeList<T>(int count, in WriteContext context) where T : Node? { }

    #endregion
}
