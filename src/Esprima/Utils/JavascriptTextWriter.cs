using System.Runtime.CompilerServices;
using Esprima.Ast;

namespace Esprima.Utils;

public record class JavaScriptTextWriterOptions
{
    public static readonly JavaScriptTextWriterOptions Default = new();

    protected internal virtual JavaScriptTextWriter CreateWriter(TextWriter writer) => new JavaScriptTextWriter(writer, this);
}

/// <summary>
/// Base JavaScript text writer (code formatter) which uses the most compact possible (i.e. minimal) format.
/// </summary>
public partial class JavaScriptTextWriter
{
    private readonly TextWriter _writer;

    public JavaScriptTextWriter(TextWriter writer, JavaScriptTextWriterOptions options)
    {
        _writer = writer ?? throw new ArgumentNullException(nameof(writer));

        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        LastTokenType = TokenType.EOF;
        LastTriviaType = TriviaType.EndOfLine;
        CurrentLineIsEmptyOrWhiteSpace = true;
    }

    protected TokenType LastTokenType { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; [MethodImpl(MethodImplOptions.AggressiveInlining)] private set; }
    protected TokenFlags LastTokenFlags { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; [MethodImpl(MethodImplOptions.AggressiveInlining)] private set; }

    protected TriviaType LastTriviaType { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; [MethodImpl(MethodImplOptions.AggressiveInlining)] private set; }

    protected bool CurrentLineIsEmptyOrWhiteSpace { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; [MethodImpl(MethodImplOptions.AggressiveInlining)] private set; }
    protected bool PendingRequiredNewLine { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; [MethodImpl(MethodImplOptions.AggressiveInlining)] private set; }

    protected virtual void OnTriviaWritten(TriviaType type, TriviaFlags flags)
    {
        LastTriviaType = type;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void WriteSpace()
    {
        WriteWhiteSpace(" ");
    }

    protected void WriteWhiteSpace(string value)
    {
        _writer.Write(value);
        OnTriviaWritten(TriviaType.WhiteSpace, TriviaFlags.None);
    }

    protected void WriteEndOfLine()
    {
        _writer.WriteLine();
        OnTriviaWritten(TriviaType.EndOfLine, TriviaFlags.None);
        CurrentLineIsEmptyOrWhiteSpace = true;
        PendingRequiredNewLine = false;
    }

    protected virtual void WriteLine()
    {
        WriteEndOfLine();
    }

    protected virtual void WriteLineCommentCore(TextWriter writer, ReadOnlySpan<char> line, TriviaFlags flags)
    {
        writer.Write("//");
        writer.Write(line);
    }

    public void WriteLineComment(string line, TriviaFlags flags)
    {
        WriteLineComment(line.AsSpan(), flags);
    }

    public void WriteLineComment(ReadOnlySpan<char> line, TriviaFlags flags)
    {
        if (PendingRequiredNewLine || !CurrentLineIsEmptyOrWhiteSpace && flags.HasFlagFast(TriviaFlags.LeadingNewLineRequired))
        {
            WriteLine();
        }

        WriteLineCommentCore(_writer, line, flags);
        OnTriviaWritten(TriviaType.LineComment, flags);
        CurrentLineIsEmptyOrWhiteSpace = false;
        PendingRequiredNewLine = true; // New line after line comments is always required.
    }

    protected virtual void WriteBlockCommentLine(TextWriter writer, ReadOnlySpan<char> line, bool isFirst)
    {
        writer.Write(line);
    }

    protected virtual void WriteBlockCommentCore(TextWriter writer, IEnumerable<string> lines, TriviaFlags flags)
    {
        writer.Write("/*");
        using (var enumerator = lines.GetEnumerator())
        {
            if (enumerator.MoveNext())
            {
                WriteBlockCommentLine(writer, enumerator.Current.AsSpan(), isFirst: true);

                while (enumerator.MoveNext())
                {
                    writer.WriteLine();
                    WriteBlockCommentLine(writer, enumerator.Current.AsSpan(), isFirst: false);
                }
            }
        }
        writer.Write("*/");
    }

    public void WriteBlockComment(IEnumerable<string> lines, TriviaFlags flags)
    {
        if (PendingRequiredNewLine || !CurrentLineIsEmptyOrWhiteSpace && flags.HasFlagFast(TriviaFlags.LeadingNewLineRequired))
        {
            WriteLine();
        }

        WriteBlockCommentCore(_writer, lines, flags);
        OnTriviaWritten(TriviaType.BlockComment, flags);
        CurrentLineIsEmptyOrWhiteSpace = false;
        PendingRequiredNewLine = flags.HasFlagFast(TriviaFlags.TrailingNewLineRequired);
    }

    protected virtual void OnTokenWritten(TokenType type, TokenFlags flags)
    {
        LastTokenType = type;
        LastTokenFlags = flags;

        LastTriviaType = TriviaType.None;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SpaceRecommendedAfterLastToken()
    {
        LastTokenFlags |= TokenFlags.TrailingSpaceRecommended;
    }

    protected void WriteRequiredSpaceBetweenTokenAndIdentifier()
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

    protected virtual void StartIdentifier(ReadOnlySpan<char> value, TokenFlags flags, ref WriteContext context)
    {
        if (LastTriviaType == TriviaType.None)
        {
            WriteRequiredSpaceBetweenTokenAndIdentifier();
        }
    }

    public void WriteIdentifier(ReadOnlySpan<char> value, TokenFlags flags, ref WriteContext context)
    {
        if (PendingRequiredNewLine)
        {
            WriteLine();
        }

        StartIdentifier(value, flags, ref context);
        _writer.Write(value);
        EndIdentifier(value, flags, ref context);

        OnTokenWritten(TokenType.Identifier, flags);
        CurrentLineIsEmptyOrWhiteSpace = false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteIdentifier(ReadOnlySpan<char> value, ref WriteContext context)
    {
        WriteIdentifier(value, TokenFlags.None, ref context);
    }

    protected virtual void EndIdentifier(ReadOnlySpan<char> value, TokenFlags flags, ref WriteContext context) { }

    protected void WriteRequiredSpaceBetweenTokenAndKeyword()
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

    protected virtual void StartKeyword(ReadOnlySpan<char> value, TokenFlags flags, ref WriteContext context)
    {
        if (LastTriviaType == TriviaType.None)
        {
            WriteRequiredSpaceBetweenTokenAndKeyword();
        }
    }

    public void WriteKeyword(string value, TokenFlags flags, ref WriteContext context)
    {
        WriteKeyword(value.AsSpan(), flags, ref context);
    }

    public void WriteKeyword(ReadOnlySpan<char> value, TokenFlags flags, ref WriteContext context)
    {
        if (PendingRequiredNewLine)
        {
            WriteLine();
        }

        StartKeyword(value, flags, ref context);
        _writer.Write(value);
        EndKeyword(value, flags, ref context);

        OnTokenWritten(TokenType.Keyword, flags);
        CurrentLineIsEmptyOrWhiteSpace = false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteKeyword(string value, ref WriteContext context)
    {
        WriteKeyword(value.AsSpan(), TokenFlags.None, ref context);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteKeyword(ReadOnlySpan<char> value, ref WriteContext context)
    {
        WriteKeyword(value, TokenFlags.None, ref context);
    }

    protected virtual void EndKeyword(ReadOnlySpan<char> value, TokenFlags flags, ref WriteContext context) { }

    protected void WriteRequiredSpaceBetweenTokenAndLiteral(TokenType type)
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

    protected virtual void StartLiteral(ReadOnlySpan<char> value, TokenType type, TokenFlags flags, ref WriteContext context)
    {
        if (LastTriviaType == TriviaType.None)
        {
            WriteRequiredSpaceBetweenTokenAndLiteral(type);
        }
    }

    public void WriteLiteral(ReadOnlySpan<char> value, TokenType type, TokenFlags flags, ref WriteContext context)
    {
        if (PendingRequiredNewLine)
        {
            WriteLine();
        }

        StartLiteral(value, type, flags, ref context);
        _writer.Write(value);
        EndLiteral(value, type, flags, ref context);

        OnTokenWritten(type, flags);
        CurrentLineIsEmptyOrWhiteSpace = false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteLiteral(ReadOnlySpan<char> value, TokenType tokenType, ref WriteContext context)
    {
        WriteLiteral(value, tokenType, TokenFlags.None, ref context);
    }

    protected virtual void EndLiteral(ReadOnlySpan<char> value, TokenType type, TokenFlags flags, ref WriteContext context)
    {
    }

    protected virtual void StartPunctuator(string value, TokenFlags flags, ref WriteContext context)
    {
    }

    public void WritePunctuator(string value, TokenFlags flags, ref WriteContext context)
    {
        if (PendingRequiredNewLine)
        {
            WriteLine();
        }

        StartPunctuator(value, flags, ref context);
        _writer.Write(value);
        EndPunctuator(value, flags, ref context);

        OnTokenWritten(TokenType.Punctuator, flags);
        CurrentLineIsEmptyOrWhiteSpace = false;
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

    public virtual void Finish()
    {
        OnTokenWritten(TokenType.EOF, TokenFlags.None);
    }
}
