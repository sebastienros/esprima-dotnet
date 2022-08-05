using System.Runtime.CompilerServices;
using Esprima.Ast;

namespace Esprima.Utils;

public record class KnRJavascriptTextFormatterOptions : JavascriptTextFormatterOptions
{
    public static new readonly KnRJavascriptTextFormatterOptions Default = new();

    public KnRJavascriptTextFormatterOptions()
    {
        KeepEmptyBlockBodyInLine = true;
    }

    public bool UseEgyptianBraces { get; init; } = true;

    protected override JavascriptTextFormatter CreateFormatter(TextWriter writer) => new KnRJavascriptTextFormatter(writer, this);
}

/// <summary>
/// Javascript code formatter which implements the most common <see href="https://en.wikipedia.org/wiki/Indentation_style#K&amp;R_style">K&amp;R style</see>.
/// </summary>
public class KnRJavascriptTextFormatter : JavascriptTextFormatter
{
    public KnRJavascriptTextFormatter(TextWriter writer, KnRJavascriptTextFormatterOptions options) : base(writer, options)
    {
        UseEgyptianBraces = options.UseEgyptianBraces;
    }

    protected bool UseEgyptianBraces { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    protected override void WriteWhiteSpaceBetweenTokenAndKeyword(string value, TokenFlags flags, ref WriteContext context)
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

    protected virtual bool CanUseEgyptianBraces(ref WriteContext context)
    {
        return KeepEmptyBlockBodyInLine
            ? RetrieveStatementBodyFromContext(ref context) is BlockStatement blockStatement && blockStatement.Body.Count > 0
            : RetrieveStatementBodyFromContext(ref context).Type == Nodes.BlockStatement;
    }
}
