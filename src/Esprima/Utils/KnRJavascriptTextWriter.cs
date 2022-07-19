using System.Diagnostics;
using System.Runtime.CompilerServices;
using Esprima.Ast;

namespace Esprima.Utils;

/// <summary>
/// Javascript text writer (code formatter) which implements the most common <see href="https://en.wikipedia.org/wiki/Indentation_style#K&amp;R_style">K&amp;R style</see>.
/// </summary>
public class KnRJavascriptTextWriter : JavascriptTextWriter
{
    public new record class Options : JavascriptTextWriter.Options
    {
        public static new readonly Options Default = new();

        internal static Options GetDefaultFrom(JavascriptTextWriter.Options baseOptions) => Default;

        public string? Indent { get; init; }
        public bool UseEgyptianBraces { get; init; } = true;
        public bool KeepSingleStatementBodyInLine { get; init; }
        public bool KeepEmptyBlockBodyInLine { get; init; } = true;
        public int MultiLineArrayLiteralThreshold { get; init; } = 7;
        public int MultiLineObjectLiteralThreshold { get; init; } = 3;
    }

    private const int UseEgyptianBracesFlag = 1 << 0;
    private const int KeepSingleStatementBodyInLineFlag = 1 << 1;
    private const int KeepEmptyBlockBodyInLineFlag = 1 << 2;

    private readonly int _optionFlags;
    private readonly string _indent;
    private int _indentionLevel;

    public KnRJavascriptTextWriter(TextWriter writer, JavascriptTextWriter.Options options) : base(writer, options)
    {
        var extendedOptions = options as Options ?? Options.GetDefaultFrom(options);

        if (!string.IsNullOrWhiteSpace(extendedOptions.Indent))
        {
            throw new ArgumentException("Indent must be null or white-space.", nameof(extendedOptions));
        }

        _indent = extendedOptions.Indent ?? "  ";

        if (extendedOptions.UseEgyptianBraces)
        {
            _optionFlags |= UseEgyptianBracesFlag;
        }

        if (extendedOptions.KeepSingleStatementBodyInLine)
        {
            _optionFlags |= KeepSingleStatementBodyInLineFlag;
        }

        if (extendedOptions.KeepEmptyBlockBodyInLine)
        {
            _optionFlags |= KeepEmptyBlockBodyInLineFlag;
        }

        MultiLineArrayLiteralThreshold = extendedOptions.MultiLineArrayLiteralThreshold >= 0 ? extendedOptions.MultiLineArrayLiteralThreshold : int.MaxValue;
        MultiLineObjectLiteralThreshold = extendedOptions.MultiLineObjectLiteralThreshold >= 0 ? extendedOptions.MultiLineObjectLiteralThreshold : int.MaxValue;
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

    public override void WriteEpsilon(TokenFlags flags, ref WriteContext context)
    {
        if (WhiteSpaceWrittenSinceLastToken)
        {
            return;
        }

        if ((flags & (TokenFlags.LeadingSpaceRecommended | TokenFlags.TrailingSpaceRecommended)) != 0)
        {
            ForceRecommendedSpace();
        }
    }

    protected override void StartKeyword(string value, TokenFlags flags, ref WriteContext context)
    {
        if (WhiteSpaceWrittenSinceLastToken)
        {
            return;
        }

        if (flags.HasFlagFast(TokenFlags.FollowsStatementBody))
        {
            if (UseEgyptianBraces && CanUseEgyptianBraces(ref context))
            {
                WriteSpace();
            }
            else
            {
                WriteLine();
                WriteIndent();
            }
        }
        else if (flags.HasFlagFast(TokenFlags.LeadingSpaceRecommended) || LastTokenFlags.HasFlagFast(TokenFlags.TrailingSpaceRecommended))
        {
            WriteSpace();
        }
        else
        {
            base.StartKeyword(value, flags, ref context);
        }
    }

    protected override void StartIdentifier(string value, TokenFlags flags, ref WriteContext context)
    {
        if (WhiteSpaceWrittenSinceLastToken)
        {
            return;
        }

        if (flags.HasFlagFast(TokenFlags.LeadingSpaceRecommended) || LastTokenFlags.HasFlagFast(TokenFlags.TrailingSpaceRecommended))
        {
            WriteSpace();
        }
        else
        {
            base.StartIdentifier(value, flags, ref context);
        }
    }

    protected override void StartLiteral(string value, TokenType type, TokenFlags flags, ref WriteContext context)
    {
        if (WhiteSpaceWrittenSinceLastToken)
        {
            return;
        }

        if (flags.HasFlagFast(TokenFlags.LeadingSpaceRecommended) || LastTokenFlags.HasFlagFast(TokenFlags.TrailingSpaceRecommended))
        {
            WriteSpace();
        }
        else
        {
            base.StartLiteral(value, type, flags, ref context);
        }
    }

    protected override void StartPunctuator(string value, TokenFlags flags, ref WriteContext context)
    {
        if (WhiteSpaceWrittenSinceLastToken)
        {
            return;
        }

        if (flags.HasFlagFast(TokenFlags.LeadingSpaceRecommended) || LastTokenFlags.HasFlagFast(TokenFlags.TrailingSpaceRecommended))
        {
            WriteSpace();
        }
        else
        {
            base.StartPunctuator(value, flags, ref context);
        }
    }

    public override void StartArray(int elementCount, ref WriteContext context)
    {
        base.StartArray(elementCount, ref context);

        if (context.Node.Type == Nodes.ArrayExpression && elementCount >= MultiLineArrayLiteralThreshold)
        {
            WriteLine();
            IncreaseIndent();
        }
    }

    public override void EndArray(int elementCount, ref WriteContext context)
    {
        if (context.Node.Type == Nodes.ArrayExpression && elementCount >= MultiLineArrayLiteralThreshold)
        {
            DecreaseIndent();
            WriteIndent();
        }

        base.EndArray(elementCount, ref context);
    }

    public override void StartObject(int propertyCount, ref WriteContext context)
    {
        base.StartObject(propertyCount, ref context);

        if (context.Node.Type == Nodes.ObjectExpression && propertyCount >= MultiLineObjectLiteralThreshold)
        {
            WriteLine();
            IncreaseIndent();
        }
    }

    public override void EndObject(int propertyCount, ref WriteContext context)
    {
        if (context.Node.Type == Nodes.ObjectExpression && propertyCount >= MultiLineObjectLiteralThreshold)
        {
            DecreaseIndent();
            WriteIndent();
        }

        base.EndObject(propertyCount, ref context);
    }

    public override void StartBlock(int statementCount, ref WriteContext context)
    {
        base.StartBlock(statementCount, ref context);

        if (statementCount > 0 || !KeepEmptyBlockBodyInLine)
        {
            WriteLine();
            IncreaseIndent();
        }
    }

    public override void EndBlock(int statementCount, ref WriteContext context)
    {
        if (statementCount > 0 || !KeepEmptyBlockBodyInLine)
        {
            DecreaseIndent();
            WriteIndent();
        }

        base.EndBlock(statementCount, ref context);
    }

    protected virtual void StoreStatementBodyIntoContext(Statement statement, ref WriteContext context)
    {
        context.Data = statement;
    }

    protected virtual Statement RetrieveStatementBodyFromContext(ref WriteContext context)
    {
        return (Statement) (context.Data ?? throw new InvalidOperationException());
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
                if (CanInlineSingleStatementBody(statement, flags, ref context))
                {
                    WriteSpace();
                }
                else
                {
                    WriteLine();
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
                if (!CanInlineSingleStatementBody(statement, flags, ref context))
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
                WriteLine();
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

        WriteLine();
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

    protected virtual bool CanInlineSingleStatementBody(Statement statement, StatementFlags flags, ref WriteContext context)
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
            WriteLine();
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
            WriteLine();
        }
        else if (typeof(T) == typeof(Decorator))
        {
            WriteLine();
            WriteIndent();
        }
    }
}
