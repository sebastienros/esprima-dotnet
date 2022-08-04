using Esprima.Ast;

namespace Esprima.Utils;

public record class AstToJavascriptOptions
{
    public static readonly AstToJavascriptOptions Default = new();

    protected internal virtual AstToJavascriptConverter CreateConverter(JavascriptTextWriter writer) => new AstToJavascriptConverter(writer, this);
}

public static class AstToJavascript
{
    public static string ToJavascriptString(this Node node)
    {
        return ToJavascriptString(node, JavascriptTextWriterOptions.Default, AstToJavascriptOptions.Default);
    }

    public static string ToJavascriptString(this Node node, KnRJavascriptTextFormatterOptions formattingOptions)
    {
        return ToJavascriptString(node, formattingOptions, AstToJavascriptOptions.Default);
    }

    public static string ToJavascriptString(this Node node, bool format)
    {
        return ToJavascriptString(node, format ? KnRJavascriptTextFormatterOptions.Default : JavascriptTextWriterOptions.Default, AstToJavascriptOptions.Default);
    }

    public static string ToJavascriptString(this Node node, JavascriptTextWriterOptions writerOptions, AstToJavascriptOptions options)
    {
        using (var writer = new StringWriter())
        {
            WriteJavascript(node, writer, writerOptions, options);
            return writer.ToString();
        }
    }

    public static void WriteJavascript(this Node node, TextWriter writer)
    {
        WriteJavascript(node, writer, JavascriptTextWriterOptions.Default, AstToJavascriptOptions.Default);
    }

    public static void WriteJavascript(this Node node, TextWriter writer, KnRJavascriptTextFormatterOptions formattingOptions)
    {
        WriteJavascript(node, writer, formattingOptions, AstToJavascriptOptions.Default);
    }

    public static void WriteJavascript(this Node node, TextWriter writer, bool format)
    {
        WriteJavascript(node, writer, format ? KnRJavascriptTextFormatterOptions.Default : JavascriptTextWriterOptions.Default, AstToJavascriptOptions.Default);
    }

    public static void WriteJavascript(this Node node, TextWriter writer, JavascriptTextWriterOptions writerOptions, AstToJavascriptOptions options)
    {
        if (writerOptions is null)
        {
            throw new ArgumentNullException(nameof(writerOptions));
        }

        WriteJavascript(node, writerOptions.CreateWriter(writer), options);
    }

    public static void WriteJavascript(this Node node, JavascriptTextWriter writer, AstToJavascriptOptions options)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        options.CreateConverter(writer).Convert(node);
    }
}
