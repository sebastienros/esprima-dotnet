using Esprima.Ast;

namespace Esprima.Utils;

public static class AstToJavascript
{
    public record class Options
    {
        public static readonly Options Default = new();
        internal static readonly Options DefaultWithBeautify = Default with { Beautify = true };

        public bool Beautify { get; init; }
        public string? Indent { get; init; }
    }

    public static string ToJavascriptString(this Node node, AstToJavascriptConverter.Factory? converterFactory = null)
    {
        return ToJavascriptString(node, Options.Default, converterFactory);
    }

    public static string ToJavascriptString(this Node node, bool beautify, AstToJavascriptConverter.Factory? converterFactory = null)
    {
        return ToJavascriptString(node, beautify ? Options.DefaultWithBeautify : Options.Default, converterFactory);
    }

    public static string ToJavascriptString(this Node node, Options options, AstToJavascriptConverter.Factory? converterFactory = null)
    {
        using (var writer = new StringWriter())
        {
            WriteJavascript(node, writer, options, converterFactory);
            return writer.ToString();
        }
    }

    public static void WriteJavascript(this Node node, TextWriter writer, AstToJavascriptConverter.Factory? converterFactory = null)
    {
        WriteJavascript(node, writer, Options.Default, converterFactory);
    }

    public static void WriteJavascript(this Node node, TextWriter writer, bool beautify, AstToJavascriptConverter.Factory? converterFactory = null)
    {
        WriteJavascript(node, writer, beautify ? Options.DefaultWithBeautify : Options.Default, converterFactory);
    }

    public static void WriteJavascript(this Node node, TextWriter writer, Options options, AstToJavascriptConverter.Factory? converterFactory = null)
    {
        converterFactory ??= (writer, options) => new AstToJavascriptConverter(writer, options);

        converterFactory(writer, options).Convert(node);
    }
}
