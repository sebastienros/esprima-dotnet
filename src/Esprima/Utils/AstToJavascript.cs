using Esprima.Ast;

namespace Esprima.Utils;

public static class AstToJavascript
{
    public record class Options
    {
        public static readonly Options Default = new();
    }

    public static string ToJavascriptString(this Node node, AstToJavascriptConverter.Factory? converterFactory = null)
    {
        JavascriptTextWriter.Factory writerFactory = static (writer, formattingOptions) => new JavascriptTextWriter(writer, formattingOptions);
        return ToJavascriptString(node, writerFactory, JavascriptTextWriter.Options.Default, Options.Default, converterFactory);
    }

    public static string ToJavascriptString(this Node node, KnRJavascriptTextWriter.Options formattingOptions, AstToJavascriptConverter.Factory? converterFactory = null)
    {
        JavascriptTextWriter.Factory writerFactory = static (writer, formattingOptions) => new KnRJavascriptTextWriter(writer, formattingOptions);
        return ToJavascriptString(node, writerFactory, formattingOptions, Options.Default, converterFactory);
    }

    public static string ToJavascriptString(this Node node, bool beautify, AstToJavascriptConverter.Factory? converterFactory = null)
    {
        if (beautify)
        {
            return ToJavascriptString(node, KnRJavascriptTextWriter.Options.Default, converterFactory);
        }
        else
        {
            return ToJavascriptString(node, converterFactory);
        }
    }

    public static string ToJavascriptString(this Node node, JavascriptTextWriter.Factory writerFactory, JavascriptTextWriter.Options formattingOptions, Options options, AstToJavascriptConverter.Factory? converterFactory = null)
    {
        if (writerFactory is null)
        {
            throw new ArgumentNullException(nameof(writerFactory));
        }

        using (var writer = new StringWriter())
        {
            WriteJavascript(node, writerFactory(writer, formattingOptions), options, converterFactory);
            return writer.ToString();
        }
    }

    public static void WriteJavascript(this Node node, TextWriter writer, AstToJavascriptConverter.Factory? converterFactory = null)
    {
        WriteJavascript(node, new JavascriptTextWriter(writer, JavascriptTextWriter.Options.Default), Options.Default, converterFactory);
    }

    public static void WriteJavascript(this Node node, TextWriter writer, KnRJavascriptTextWriter.Options formattingOptions, AstToJavascriptConverter.Factory? converterFactory = null)
    {
        WriteJavascript(node, new KnRJavascriptTextWriter(writer, formattingOptions), Options.Default, converterFactory);
    }

    public static void WriteJavascript(this Node node, TextWriter writer, bool beautify, AstToJavascriptConverter.Factory? converterFactory = null)
    {
        if (beautify)
        {
            WriteJavascript(node, writer, KnRJavascriptTextWriter.Options.Default, converterFactory);
        }
        else
        {
            WriteJavascript(node, writer, converterFactory);
        }
    }

    public static void WriteJavascript(this Node node, JavascriptTextWriter writer, Options options, AstToJavascriptConverter.Factory? converterFactory = null)
    {
        converterFactory ??= static (writer, options) => new AstToJavascriptConverter(writer, options);

        converterFactory(writer, options).Convert(node);
    }
}
