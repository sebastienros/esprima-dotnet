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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void ForceRecommendedSpace()
    {
        LastTokenFlags |= TokenFlags.TrailingSpaceRecommended;
    }

    public virtual void WriteEpsilon(TokenFlags flags, ref WriteContext context) { }

    protected virtual void StartIdentifier(string value, TokenFlags flags, ref WriteContext context)
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

    public void WriteIdentifier(string value, TokenFlags flags, ref WriteContext context)
    {
        StartIdentifier(value, flags, ref context);
        _writer.Write(value);
        WhiteSpaceWrittenSinceLastToken = false;
        EndIdentifier(value, flags, ref context);

        LastTokenType = TokenType.Identifier;
        LastTokenFlags = flags;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteIdentifier(string value, ref WriteContext context)
    {
        WriteIdentifier(value, TokenFlags.None, ref context);
    }

    protected virtual void EndIdentifier(string value, TokenFlags flags, ref WriteContext context) { }

    protected virtual void StartKeyword(string value, TokenFlags flags, ref WriteContext context)
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

    public void WriteKeyword(string value, TokenFlags flags, ref WriteContext context)
    {
        StartKeyword(value, flags, ref context);
        _writer.Write(value);
        WhiteSpaceWrittenSinceLastToken = false;
        EndKeyword(value, flags, ref context);

        LastTokenType = TokenType.Keyword;
        LastTokenFlags = flags;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteKeyword(string value, ref WriteContext context)
    {
        WriteKeyword(value, TokenFlags.None, ref context);
    }

    protected virtual void EndKeyword(string value, TokenFlags flags, ref WriteContext context) { }

    protected virtual void StartLiteral(string value, TokenType type, TokenFlags flags, ref WriteContext context)
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

    public void WriteLiteral(string value, TokenType type, TokenFlags flags, ref WriteContext context)
    {
        StartLiteral(value, type, flags, ref context);
        _writer.Write(value);
        WhiteSpaceWrittenSinceLastToken = false;
        EndLiteral(value, type, flags, ref context);

        LastTokenType = type;
        LastTokenFlags = flags;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteLiteral(string value, TokenType tokenType, ref WriteContext context)
    {
        WriteLiteral(value, tokenType, TokenFlags.None, ref context);
    }

    protected virtual void EndLiteral(string value, TokenType type, TokenFlags flags, ref WriteContext context) { }

    protected virtual void StartPunctuator(string value, TokenFlags flags, ref WriteContext context) { }

    public void WritePunctuator(string value, TokenFlags flags, ref WriteContext context)
    {
        StartPunctuator(value, flags, ref context);
        _writer.Write(value);
        WhiteSpaceWrittenSinceLastToken = false;
        EndPunctuator(value, flags, ref context);

        LastTokenType = TokenType.Punctuator;
        LastTokenFlags = flags;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WritePunctuator(string value, ref WriteContext context)
    {
        WritePunctuator(value, TokenFlags.None, ref context);
    }

    protected virtual void EndPunctuator(string value, TokenFlags flags, ref WriteContext context) { }

    public virtual void StartArray(int elementCount, ref WriteContext context)
    {
        WritePunctuator("[", TokenFlags.Leading, ref context);
    }

    public virtual void EndArray(int elementCount, ref WriteContext context)
    {
        WritePunctuator("]", TokenFlags.Trailing, ref context);
    }

    public virtual void StartObject(int propertyCount, ref WriteContext context)
    {
        WritePunctuator("{", TokenFlags.Leading | TokenFlags.TrailingSpaceRecommended, ref context);
    }

    public virtual void EndObject(int propertyCount, ref WriteContext context)
    {
        WritePunctuator("}", TokenFlags.Trailing | TokenFlags.LeadingSpaceRecommended, ref context);
    }

    public virtual void StartBlock(int statementCount, ref WriteContext context)
    {
        WritePunctuator("{", TokenFlags.Leading | TokenFlags.SurroundingSpaceRecommended, ref context);
    }

    public virtual void EndBlock(int statementCount, ref WriteContext context)
    {
        WritePunctuator("}", TokenFlags.Trailing | TokenFlags.LeadingSpaceRecommended, ref context);
    }

    public virtual void StartStatement(StatementFlags flags, ref WriteContext context) { }

    public virtual void EndStatement(StatementFlags flags, ref WriteContext context)
    {
        // Writes statement terminator unless it can be omitted.
        if (flags.HasFlagFast(StatementFlags.NeedsSemicolon) && !flags.HasFlagFast(StatementFlags.MayOmitRightMostSemicolon | StatementFlags.IsRightMost))
        {
            WritePunctuator(";", TokenFlags.Trailing | TokenFlags.TrailingSpaceRecommended, ref context);
        }
    }

    public virtual void StartStatementList(int count, ref WriteContext context) { }

    public virtual void StartStatementListItem(int index, int count, StatementFlags flags, ref WriteContext context) { }

    public virtual void EndStatementListItem(int index, int count, StatementFlags flags, ref WriteContext context)
    {
        // Writes statement terminator unless it can be omitted.
        if (flags.HasFlagFast(StatementFlags.NeedsSemicolon) && !flags.HasFlagFast(StatementFlags.MayOmitRightMostSemicolon | StatementFlags.IsRightMost))
        {
            WritePunctuator(";", TokenFlags.Trailing | TokenFlags.TrailingSpaceRecommended, ref context);
        }
    }

    public virtual void EndStatementList(int count, ref WriteContext context) { }

    public virtual void StartExpression(ExpressionFlags flags, ref WriteContext context)
    {
        if (flags.HasFlagFast(ExpressionFlags.NeedsBrackets))
        {
            WritePunctuator("(", TokenFlags.Leading | flags.HasFlagFast(ExpressionFlags.SpaceAroundBracketsRecommended).ToFlag(TokenFlags.LeadingSpaceRecommended), ref context);
        }
    }

    public virtual void EndExpression(ExpressionFlags flags, ref WriteContext context)
    {
        if (flags.HasFlagFast(ExpressionFlags.NeedsBrackets))
        {
            WritePunctuator(")", TokenFlags.Trailing | flags.HasFlagFast(ExpressionFlags.SpaceAroundBracketsRecommended).ToFlag(TokenFlags.TrailingSpaceRecommended), ref context);
        }
    }

    public virtual void StartExpressionList(int count, ref WriteContext context) { }

    public virtual void StartExpressionListItem(int index, int count, ExpressionFlags flags, ref WriteContext context)
    {
        if (flags.HasFlagFast(ExpressionFlags.NeedsBrackets))
        {
            WritePunctuator("(", TokenFlags.Leading, ref context);
        }
    }

    public virtual void EndExpressionListItem(int index, int count, ExpressionFlags flags, ref WriteContext context)
    {
        if (flags.HasFlagFast(ExpressionFlags.NeedsBrackets))
        {
            WritePunctuator(")", TokenFlags.Trailing, ref context);
        }

        if (index < count - 1)
        {
            WritePunctuator(",", TokenFlags.InBetween | TokenFlags.TrailingSpaceRecommended, ref context);
        }
    }

    public virtual void EndExpressionList(int count, ref WriteContext context) { }

    public virtual void StartAuxiliaryNode(object? nodeContext, ref WriteContext context) { }

    public virtual void EndAuxiliaryNode(object? nodeContext, ref WriteContext context) { }

    public virtual void StartAuxiliaryNodeList<T>(int count, ref WriteContext context) where T : Node? { }

    public virtual void StartAuxiliaryNodeListItem<T>(int index, int count, string separator, object? nodeContext, ref WriteContext context) where T : Node? { }

    public virtual void EndAuxiliaryNodeListItem<T>(int index, int count, string separator, object? nodeContext, ref WriteContext context) where T : Node?
    {
        if (separator.Length > 0 && index < count - 1)
        {
            WritePunctuator(separator, TokenFlags.InBetween | TokenFlags.TrailingSpaceRecommended, ref context);
        }
    }

    public virtual void EndAuxiliaryNodeList<T>(int count, ref WriteContext context) where T : Node? { }
}
