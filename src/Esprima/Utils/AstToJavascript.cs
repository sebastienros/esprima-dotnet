using Esprima.Ast;

namespace Esprima.Utils;

public record class AstToJavaScriptOptions
{
    public static readonly AstToJavaScriptOptions Default = new();

    internal bool IgnoreExtensions { get; init; }

    protected internal virtual AstToJavaScriptConverter CreateConverter(JavaScriptTextWriter writer) => new AstToJavaScriptConverter(writer, this);
}

public static class AstToJavaScript
{
    public static string ToJavaScriptString(this Node node)
    {
        return ToJavaScriptString(node, JavaScriptTextWriterOptions.Default, AstToJavaScriptOptions.Default);
    }

    public static string ToJavaScriptString(this Node node, KnRJavaScriptTextFormatterOptions formattingOptions)
    {
        return ToJavaScriptString(node, formattingOptions, AstToJavaScriptOptions.Default);
    }

    public static string ToJavaScriptString(this Node node, bool format)
    {
        return ToJavaScriptString(node, format ? KnRJavaScriptTextFormatterOptions.Default : JavaScriptTextWriterOptions.Default, AstToJavaScriptOptions.Default);
    }

    public static string ToJavaScriptString(this Node node, JavaScriptTextWriterOptions writerOptions, AstToJavaScriptOptions options)
    {
        using (var writer = new StringWriter())
        {
            WriteJavaScript(node, writer, writerOptions, options);
            return writer.ToString();
        }
    }

    public static void WriteJavaScript(this Node node, TextWriter writer)
    {
        WriteJavaScript(node, writer, JavaScriptTextWriterOptions.Default, AstToJavaScriptOptions.Default);
    }

    public static void WriteJavaScript(this Node node, TextWriter writer, KnRJavaScriptTextFormatterOptions formattingOptions)
    {
        WriteJavaScript(node, writer, formattingOptions, AstToJavaScriptOptions.Default);
    }

    public static void WriteJavaScript(this Node node, TextWriter writer, bool format)
    {
        WriteJavaScript(node, writer, format ? KnRJavaScriptTextFormatterOptions.Default : JavaScriptTextWriterOptions.Default, AstToJavaScriptOptions.Default);
    }

    public static void WriteJavaScript(this Node node, TextWriter writer, JavaScriptTextWriterOptions writerOptions, AstToJavaScriptOptions options)
    {
        if (writerOptions is null)
        {
            throw new ArgumentNullException(nameof(writerOptions));
        }

        WriteJavaScript(node, writerOptions.CreateWriter(writer), options);
    }

    public static void WriteJavaScript(this Node node, JavaScriptTextWriter writer, AstToJavaScriptOptions options)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        options.CreateConverter(writer).Convert(node);
    }
}
