using System.Text;

namespace Esprima.SourceGenerators.Helpers;

public static class StringBuilderExtensions
{
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
}
