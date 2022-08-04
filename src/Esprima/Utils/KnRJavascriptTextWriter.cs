using System.Runtime.CompilerServices;
using Esprima.Ast;

namespace Esprima.Utils;

public record class KnRJavascriptTextWriterOptions : JavascriptTextWriterOptions
{
    public static new readonly KnRJavascriptTextWriterOptions Default = new();

    public string? Indent { get; init; }
    public bool UseEgyptianBraces { get; init; } = true;
    public bool KeepSingleStatementBodyInLine { get; init; }
    public bool KeepEmptyBlockBodyInLine { get; init; } = true;
    public int MultiLineArrayLiteralThreshold { get; init; } = 7;
    public int MultiLineObjectLiteralThreshold { get; init; } = 3;

    protected internal override JavascriptTextWriter CreateWriter(TextWriter writer) => new KnRJavascriptTextWriter(writer, this);
}

/// <summary>
/// Javascript text writer (code formatter) which implements the most common <see href="https://en.wikipedia.org/wiki/Indentation_style#K&amp;R_style">K&amp;R style</see>.
/// </summary>
public class KnRJavascriptTextWriter : JavascriptTextWriter
{
    private const int UseEgyptianBracesFlag = 1 << 0;
    private const int KeepSingleStatementBodyInLineFlag = 1 << 1;
    private const int KeepEmptyBlockBodyInLineFlag = 1 << 2;

    private readonly int _optionFlags;
    private readonly string _indent;
    private int _indentionLevel;

    public KnRJavascriptTextWriter(TextWriter writer, KnRJavascriptTextWriterOptions options) : base(writer, options)
    {
        if (!string.IsNullOrWhiteSpace(options.Indent))
        {
            throw new ArgumentException("Indent must be null or white-space.", nameof(options));
        }

        _indent = options.Indent ?? "  ";

        if (options.UseEgyptianBraces)
        {
            _optionFlags |= UseEgyptianBracesFlag;
        }

        if (options.KeepSingleStatementBodyInLine)
        {
            _optionFlags |= KeepSingleStatementBodyInLineFlag;
        }

        if (options.KeepEmptyBlockBodyInLine)
        {
            _optionFlags |= KeepEmptyBlockBodyInLineFlag;
        }

        MultiLineArrayLiteralThreshold = options.MultiLineArrayLiteralThreshold >= 0 ? options.MultiLineArrayLiteralThreshold : int.MaxValue;
        MultiLineObjectLiteralThreshold = options.MultiLineObjectLiteralThreshold >= 0 ? options.MultiLineObjectLiteralThreshold : int.MaxValue;
    }

    protected bool UseEgyptianBraces { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => (_optionFlags & UseEgyptianBracesFlag) != 0; }
    protected bool KeepSingleStatementBodyInLine { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => (_optionFlags & KeepSingleStatementBodyInLineFlag) != 0; }
    protected bool KeepEmptyBlockBodyInLine { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => (_optionFlags & KeepEmptyBlockBodyInLineFlag) != 0; }
    protected int MultiLineArrayLiteralThreshold { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    protected int MultiLineObjectLiteralThreshold { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    protected void IncreaseIndent()
    {
        _indentionLevel++;
    }

    protected void DecreaseIndent()
    {
        _indentionLevel--;
    }

    protected void WriteIndent()
    {
        for (var n = _indentionLevel; n > 0; n--)
        {
            WriteWhiteSpace(_indent);
        }
    }

    protected override void WriteLine()
    {
        WriteEndOfLine();
        WriteIndent();
    }

    protected override void WriteLineCommentCore(TextWriter writer, string line, TriviaFlags flags)
    {
        if (!LastTriviaType.HasFlag(WhiteSpaceTriviaFlag))
        {
            WriteSpace();
        }

        base.WriteLineCommentCore(writer, line, flags);
    }

    protected override void WriteBlockCommentLine(TextWriter writer, string line, bool isFirst)
    {
        if (!isFirst)
        {
            for (var n = _indentionLevel; n > 0; n--)
            {
                writer.Write(_indent);
            }
        }

        base.WriteBlockCommentLine(writer, line, isFirst);
    }

    protected override void WriteBlockCommentCore(TextWriter writer, IEnumerable<string> lines, TriviaFlags flags)
    {
        if (!LastTriviaType.HasFlag(WhiteSpaceTriviaFlag))
        {
            WriteSpace();
        }

        base.WriteBlockCommentCore(writer, lines, flags);
    }

    protected override void StartIdentifier(string value, TokenFlags flags, ref WriteContext context)
    {
        if (LastTriviaType == TriviaType.None)
        {
            if (flags.HasFlagFast(TokenFlags.LeadingSpaceRecommended) || LastTokenFlags.HasFlagFast(TokenFlags.TrailingSpaceRecommended))
            {
                WriteSpace();
            }
            else
            {
                WriteRequiredSpaceBetweenTokenAndIdentifier();
            }
        }
        else if (!LastTriviaType.HasFlag(WhiteSpaceTriviaFlag))
        {
            WriteSpace();
        }
    }

    protected override void StartKeyword(string value, TokenFlags flags, ref WriteContext context)
    {
        if (LastTriviaType == TriviaType.None)
        {
            if (flags.HasFlagFast(TokenFlags.FollowsStatementBody))
            {
                if (UseEgyptianBraces && CanUseEgyptianBraces(ref context))
                {
                    WriteSpace();
                }
                else
                {
                    WriteLine();
                }
            }
            else if (flags.HasFlagFast(TokenFlags.LeadingSpaceRecommended) || LastTokenFlags.HasFlagFast(TokenFlags.TrailingSpaceRecommended))
            {
                WriteSpace();
            }
            else
            {
                WriteRequiredSpaceBetweenTokenAndKeyword();
            }
        }
        else if (!LastTriviaType.HasFlag(WhiteSpaceTriviaFlag))
        {
            WriteSpace();
        }
    }

    protected override void StartLiteral(string value, TokenType type, TokenFlags flags, ref WriteContext context)
    {
        if (LastTriviaType == TriviaType.None)
        {
            if (flags.HasFlagFast(TokenFlags.LeadingSpaceRecommended) || LastTokenFlags.HasFlagFast(TokenFlags.TrailingSpaceRecommended))
            {
                WriteSpace();
            }
            else
            {
                WriteRequiredSpaceBetweenTokenAndLiteral(type);
            }
        }
        else if (!LastTriviaType.HasFlag(WhiteSpaceTriviaFlag))
        {
            WriteSpace();
        }
    }

    protected override void StartPunctuator(string value, TokenFlags flags, ref WriteContext context)
    {
        if (LastTriviaType == TriviaType.None)
        {
            if (flags.HasFlagFast(TokenFlags.LeadingSpaceRecommended) || LastTokenFlags.HasFlagFast(TokenFlags.TrailingSpaceRecommended))
            {
                WriteSpace();
            }
        }
        else if (!LastTriviaType.HasFlag(WhiteSpaceTriviaFlag))
        {
            WriteSpace();
        }
    }

    public override void StartArray(int elementCount, ref WriteContext context)
    {
        base.StartArray(elementCount, ref context);

        if (!CanKeepArrayInLine(elementCount, ref context))
        {
            WriteEndOfLine();
            IncreaseIndent();
        }
    }

    public override void EndArray(int elementCount, ref WriteContext context)
    {
        if (!CanKeepArrayInLine(elementCount, ref context))
        {
            DecreaseIndent();
            WriteIndent();
        }

        base.EndArray(elementCount, ref context);
    }

    protected virtual bool CanKeepArrayInLine(int elementCount, ref WriteContext context)
    {
        return context.Node.Type != Nodes.ArrayExpression || elementCount < MultiLineArrayLiteralThreshold;
    }

    public override void StartObject(int propertyCount, ref WriteContext context)
    {
        base.StartObject(propertyCount, ref context);

        if (!CanKeepObjectInLine(propertyCount, ref context))
        {
            WriteEndOfLine();
            IncreaseIndent();
        }
    }

    public override void EndObject(int propertyCount, ref WriteContext context)
    {
        if (!CanKeepObjectInLine(propertyCount, ref context))
        {
            DecreaseIndent();
            WriteIndent();
        }

        base.EndObject(propertyCount, ref context);
    }

    protected virtual bool CanKeepObjectInLine(int propertyCount, ref WriteContext context)
    {
        return context.Node.Type != Nodes.ObjectExpression || propertyCount < MultiLineObjectLiteralThreshold;
    }

    public override void StartBlock(int statementCount, ref WriteContext context)
    {
        base.StartBlock(statementCount, ref context);

        if (!CanKeepBlockInLine(statementCount, ref context))
        {
            WriteEndOfLine();
            IncreaseIndent();
        }
    }

    public override void EndBlock(int statementCount, ref WriteContext context)
    {
        if (!CanKeepBlockInLine(statementCount, ref context))
        {
            DecreaseIndent();
            WriteIndent();
        }

        base.EndBlock(statementCount, ref context);
    }

    protected virtual bool CanKeepBlockInLine(int statementCount, ref WriteContext context)
    {
        return statementCount == 0 && KeepEmptyBlockBodyInLine;
    }

    protected void StoreStatementBodyIntoContext(Statement statement, ref WriteContext context)
    {
        context._additionalDataContainer.InternalData = statement;
    }

    protected Statement RetrieveStatementBodyFromContext(ref WriteContext context)
    {
        return (Statement) (context._additionalDataContainer.InternalData ?? throw new InvalidOperationException());
    }

    public override void StartStatement(StatementFlags flags, ref WriteContext context)
    {
        if (flags.HasFlagFast(StatementFlags.IsStatementBody))
        {
            var statement = context.GetNodePropertyValue<Statement>();
            StoreStatementBodyIntoContext(statement, ref context);

            // Is single statement body?
            if (statement.Type != Nodes.BlockStatement)
            {
                if (CanKeepSingleStatementBodyInLine(statement, flags, ref context))
                {
                    WriteSpace();
                }
                else
                {
                    WriteEndOfLine();
                    IncreaseIndent();
                    WriteIndent();
                }
            }
        }
    }

    public override void EndStatement(StatementFlags flags, ref WriteContext context)
    {
        if (flags.HasFlagFast(StatementFlags.IsStatementBody))
        {
            var statement = RetrieveStatementBodyFromContext(ref context);

            // Is single statement body?
            if (statement.Type != Nodes.BlockStatement)
            {
                if (!CanKeepSingleStatementBodyInLine(statement, flags, ref context))
                {
                    DecreaseIndent();
                }
            }
        }

        if (flags.HasFlagFast(StatementFlags.NeedsSemicolon) || ShouldTerminateStatementAnyway(context.GetNodePropertyValue<Statement>(), flags, ref context))
        {
            WritePunctuator(";", TokenFlags.Trailing | TokenFlags.TrailingSpaceRecommended, ref context);
        }
    }

    public override void StartStatementList(int count, ref WriteContext context)
    {
        if (context.Node.Type == Nodes.SwitchCase)
        {
            if (count == 1 && context.GetNodePropertyListValue<Statement>()[0].Type == Nodes.BlockStatement)
            {
                WriteSpace();
            }
            else
            {
                WriteEndOfLine();
                IncreaseIndent();
            }
        }
    }

    public override void StartStatementListItem(int index, int count, StatementFlags flags, ref WriteContext context)
    {
        if (context.Node.Type == Nodes.SwitchCase)
        {
            if (index == 0 && count == 1 && context.GetNodePropertyListValue<Statement>()[0].Type == Nodes.BlockStatement)
            {
                return;
            }
        }

        WriteIndent();
    }

    public override void EndStatementListItem(int index, int count, StatementFlags flags, ref WriteContext context)
    {
        if (flags.HasFlagFast(StatementFlags.NeedsSemicolon) || ShouldTerminateStatementAnyway(context.GetNodePropertyListValue<Statement>()[index], flags, ref context))
        {
            WritePunctuator(";", TokenFlags.Trailing | TokenFlags.TrailingSpaceRecommended, ref context);
        }

        WriteEndOfLine();
    }

    public override void EndStatementList(int count, ref WriteContext context)
    {
        if (context.Node.Type == Nodes.SwitchCase)
        {
            if (!(count == 1 && context.GetNodePropertyListValue<Statement>()[0].Type == Nodes.BlockStatement))
            {
                DecreaseIndent();
            }
        }
    }

    protected virtual bool CanUseEgyptianBraces(ref WriteContext context)
    {
        return KeepEmptyBlockBodyInLine
            ? RetrieveStatementBodyFromContext(ref context) is BlockStatement blockStatement && blockStatement.Body.Count > 0
            : RetrieveStatementBodyFromContext(ref context).Type == Nodes.BlockStatement;
    }

    protected virtual bool CanKeepSingleStatementBodyInLine(Statement statement, StatementFlags flags, ref WriteContext context)
    {
        return statement.Type switch
        {
            // Statements
            Nodes.BreakStatement or
            Nodes.ContinueStatement or
            Nodes.DebuggerStatement or
            Nodes.EmptyStatement or
            Nodes.ExpressionStatement or
            Nodes.ReturnStatement or
            Nodes.ThrowStatement =>
                KeepSingleStatementBodyInLine,

            Nodes.BlockStatement or
            Nodes.DoWhileStatement or
            Nodes.ForInStatement or
            Nodes.ForOfStatement or
            Nodes.ForStatement or
            Nodes.LabeledStatement or
            Nodes.SwitchStatement or
            Nodes.TryStatement or
            Nodes.WhileStatement or
            Nodes.WithStatement =>
                false,

            Nodes.IfStatement =>
                context is { Node: IfStatement, NodePropertyName: nameof(IfStatement.Alternate) },

            // Declarations
            Nodes.FunctionDeclaration or
            Nodes.VariableDeclaration =>
                KeepSingleStatementBodyInLine,

            Nodes.ClassDeclaration or
            Nodes.ImportDeclaration or
            Nodes.ExportAllDeclaration or
            Nodes.ExportDefaultDeclaration or
            Nodes.ExportNamedDeclaration =>
                throw new ArgumentException($"Operation is not defined for nodes of type {statement.Type}.", nameof(statement)),

            // Extensions
            _ => false,
        };
    }

    protected virtual bool ShouldTerminateStatementAnyway(Statement statement, StatementFlags flags, ref WriteContext context)
    {
        return statement.Type switch
        {
            Nodes.DoWhileStatement => true,
            _ => false
        };
    }

    public override void StartExpressionListItem(int index, int count, ExpressionFlags flags, ref WriteContext context)
    {
        if (context.Node.Type == Nodes.ArrayExpression && count >= MultiLineArrayLiteralThreshold)
        {
            WriteIndent();
        }

        base.StartExpressionListItem(index, count, flags, ref context);
    }

    public override void EndExpressionListItem(int index, int count, ExpressionFlags flags, ref WriteContext context)
    {
        base.EndExpressionListItem(index, count, flags, ref context);

        if (context.Node.Type == Nodes.ArrayExpression && count >= MultiLineArrayLiteralThreshold)
        {
            WriteEndOfLine();
        }
    }

    public override void StartAuxiliaryNodeListItem<T>(int index, int count, string separator, object? nodeContext, ref WriteContext context)
    {
        if (typeof(T) == typeof(SwitchCase) ||
            context.Node.Type == Nodes.ClassBody ||
            context.Node.Type == Nodes.ObjectExpression && count >= MultiLineObjectLiteralThreshold)
        {
            WriteIndent();
        }
    }

    public override void EndAuxiliaryNodeListItem<T>(int index, int count, string separator, object? nodeContext, ref WriteContext context)
    {
        base.EndAuxiliaryNodeListItem<T>(index, count, separator, nodeContext, ref context);

        if (context.Node.Type is Nodes.ClassBody ||
            context.Node.Type == Nodes.ObjectExpression && count >= MultiLineObjectLiteralThreshold)
        {
            WriteEndOfLine();
        }
        else if (typeof(T) == typeof(Decorator))
        {
            WriteLine();
        }
    }

    public override void Finish()
    {
        if (LastTriviaType != TriviaType.EndOfLine)
        {
            WriteEndOfLine();
        }

        base.Finish();
    }
}
