using System.Text;

namespace Esprima.SourceGenerators.Helpers;

internal static class StringBuilderExtensions
{
    public static StringBuilder AppendIndent(this StringBuilder sb, int indentionLevel)
    {
        return sb.AppendIndent("    ", indentionLevel);
    }

    public static StringBuilder AppendIndent(this StringBuilder sb, string indent, int indentionLevel)
    {
        for (; indentionLevel > 0; indentionLevel--)
        {
            sb.Append(indent);
        }

        return sb;
    }

    public static StringBuilder AppendEscaped(this StringBuilder sb, char c)
    {
        return c == '\'' ? sb.Append("\\'") : sb.Append(c);
    }

    public static StringBuilder AppendEscaped(this StringBuilder sb, string s)
    {
        return sb.Append(s.Replace("\"", "\\\""));
    }

    public static StringBuilder AppendTypeName(this StringBuilder sb, CSharpTypeName typeName, Predicate<CSharpTypeName>? includeNamespace = null)
    {
        typeName.AppendTo(sb, includeNamespace ?? (static _ => true));
        return sb;
    }

    public static StringBuilder AppendTypeBareName(this StringBuilder sb, CSharpTypeBareName typeBareName, Predicate<CSharpTypeName>? includeNamespace = null)
    {
        typeBareName.AppendTo(sb, includeNamespace ?? (static _ => true));
        return sb;
    }
}
