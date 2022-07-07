using Esprima.Ast;

namespace Esprima.Utils;

public enum LocationMembersPlacement
{
    End,
    Start
}

public static class AstJson
{
    public record class Options
    {
        public static readonly Options Default = new();

        public bool IncludingLineColumn { get; init; }
        public bool IncludingRange { get; init; }
        public LocationMembersPlacement LocationMembersPlacement { get; init; }
        /// <summary>
        /// This switch is intended for enabling a compatibility mode for <see cref="AstToJsonConverter"/> to build a JSON output
        /// which matches the format of the test fixtures of the original Esprima project.
        /// </summary>
        internal bool TestCompatibilityMode { get; init; }
    }

    public static string ToJsonString(this Node node, AstToJsonConverter.Factory? converterFactory = null)
    {
        return ToJsonString(node, indent: null, converterFactory);
    }

    public static string ToJsonString(this Node node, string? indent, AstToJsonConverter.Factory? converterFactory = null)
    {
        return ToJsonString(node, Options.Default, indent, converterFactory);
    }

    public static string ToJsonString(this Node node, Options options, AstToJsonConverter.Factory? converterFactory = null)
    {
        return ToJsonString(node, options, indent: null, converterFactory);
    }

    public static string ToJsonString(this Node node, Options options, string? indent, AstToJsonConverter.Factory? converterFactory = null)
    {
        using (var writer = new StringWriter())
        {
            WriteJson(node, writer, options, indent, converterFactory);
            return writer.ToString();
        }
    }

    public static void WriteJson(this Node node, TextWriter writer, AstToJsonConverter.Factory? converterFactory = null)
    {
        WriteJson(node, writer, indent: null, converterFactory);
    }

    public static void WriteJson(this Node node, TextWriter writer, string? indent, AstToJsonConverter.Factory? converterFactory = null)
    {
        WriteJson(node, writer, Options.Default, indent, converterFactory);
    }

    public static void WriteJson(this Node node, TextWriter writer, Options options, AstToJsonConverter.Factory? converterFactory = null)
    {
        WriteJson(node, writer, options, indent: null, converterFactory);
    }

    public static void WriteJson(this Node node, TextWriter writer, Options options, string? indent, AstToJsonConverter.Factory? converterFactory = null)
    {
        WriteJson(node, new JsonTextWriter(writer, indent), options, converterFactory);
    }

    public static void WriteJson(this Node node, JsonWriter writer, Options options, AstToJsonConverter.Factory? converterFactory = null)
    {
        converterFactory ??= static (writer, options) => new AstToJsonConverter(writer, options);

        converterFactory(writer, options).Convert(node);
    }
}
