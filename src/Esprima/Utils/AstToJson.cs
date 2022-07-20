using Esprima.Ast;

namespace Esprima.Utils;

public enum LocationMembersPlacement
{
    End,
    Start
}

internal enum AstToJsonTestCompatibilityMode
{
    None,
    EsprimaOrg,
}

public record class AstToJsonOptions
{
    public static readonly AstToJsonOptions Default = new();

    public bool IncludingLineColumn { get; init; }
    public bool IncludingRange { get; init; }
    public LocationMembersPlacement LocationMembersPlacement { get; init; }
    /// <summary>
    /// This switch is intended for enabling a compatibility mode for <see cref="AstToJsonConverter"/> to build a JSON output
    /// which matches the format of the test fixtures of the original Esprima project.
    /// </summary>
    internal AstToJsonTestCompatibilityMode TestCompatibilityMode { get; init; }

    protected internal virtual AstToJsonConverter CreateConverter(JsonWriter writer) => new AstToJsonConverter(writer, this);
}

public static class AstToJson
{
    public static string ToJsonString(this Node node)
    {
        return ToJsonString(node, indent: null);
    }

    public static string ToJsonString(this Node node, string? indent)
    {
        return ToJsonString(node, AstToJsonOptions.Default, indent);
    }

    public static string ToJsonString(this Node node, AstToJsonOptions options)
    {
        return ToJsonString(node, options, indent: null);
    }

    public static string ToJsonString(this Node node, AstToJsonOptions options, string? indent)
    {
        using (var writer = new StringWriter())
        {
            WriteJson(node, writer, options, indent);
            return writer.ToString();
        }
    }

    public static void WriteJson(this Node node, TextWriter writer)
    {
        WriteJson(node, writer, indent: null);
    }

    public static void WriteJson(this Node node, TextWriter writer, string? indent)
    {
        WriteJson(node, writer, AstToJsonOptions.Default, indent);
    }

    public static void WriteJson(this Node node, TextWriter writer, AstToJsonOptions options)
    {
        WriteJson(node, writer, options, indent: null);
    }

    public static void WriteJson(this Node node, TextWriter writer, AstToJsonOptions options, string? indent)
    {
        WriteJson(node, new JsonTextWriter(writer, indent), options);
    }

    public static void WriteJson(this Node node, JsonWriter writer, AstToJsonOptions options)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        options.CreateConverter(writer).Convert(node);
    }
}
