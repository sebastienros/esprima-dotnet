using Microsoft.CodeAnalysis.CSharp;

namespace Esprima.SourceGenerators.Helpers;

internal static class CodeGenerationHelper
{
    public static string MakeValidVariableName(string value)
    {
        return SyntaxFacts.GetKeywordKind(value) == SyntaxKind.None
            ? value
            : "@" + value;
    }

    public static string ToCamelCase(string value)
    {
        return value.Length > 0
            ? char.ToLowerInvariant(value[0]) + value.Substring(1)
            : string.Empty;
    }
}
