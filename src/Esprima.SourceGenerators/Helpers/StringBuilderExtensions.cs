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
}
