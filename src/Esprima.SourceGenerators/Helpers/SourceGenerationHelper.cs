using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Esprima.SourceGenerators.Helpers;

/// <summary>
/// Helpers from mostly based Andrew Lock's work: https://andrewlock.net/series/creating-a-source-generator/
/// </summary>
internal static class SourceGenerationHelper
{
    public const string Attributes = @"
namespace Esprima;

[System.AttributeUsage(System.AttributeTargets.Method)]
internal class StringMatcherAttribute : System.Attribute
{
    public StringMatcherAttribute(params string[] targets)
    {
        Targets = targets;
    }

    public string[] Targets { get; }
}
";

    /// <summary>
    /// Builds optimized value lookup using known facts about keys.
    /// </summary>
    internal static string GenerateLookups(string[] alternatives, int indent, bool checkNull, bool returnString, bool sourceIsSpan)
    {
        var sb = new StringBuilder();

        var byLength = alternatives
            .Distinct()
            .ToLookup(x => x.Length)
            .OrderBy(x => x.Key)
            .Select(x => (Key: x.Key, x.OrderBy(x => x).ToArray()))
            .ToArray();

        var indentStr = new string(' ', indent);
        var baseIndent = byLength.Length > 1 ? "        " : "    ";

        if (checkNull)
        {
            sb.Append(indentStr).AppendLine("    if (input is null)");
            sb.Append(indentStr).AppendLine("    {");
            sb.Append(indentStr).Append("        return ").Append(returnString ? "null" : "false").AppendLine(";");
            sb.Append(indentStr).AppendLine("    }");
        }

        if (byLength.Length > 1)
        {
            sb.Append(indentStr).AppendLine("    switch (input.Length)");
            sb.Append(indentStr).AppendLine("    {");
        }

        foreach (var (length, group) in byLength)
        {
            if (byLength.Length > 1)
            {
                sb.Append(indentStr).Append("        case ").Append(length).AppendLine(":");
                sb.Append(indentStr).AppendLine("        {");
            }

            if (group.Length == 1)
            {
                var item = group[0];
                sb.Append(indentStr).Append(baseIndent).Append("    return ");
                StringEquality(sb, item, sourceIsSpan, startIndex: 0, discriminatorIndex: -1, charLookupGenerated: false);
                if (returnString)
                {
                    sb.Append(" ? ").Append("\"").Append(Escape(item)).Append("\"").Append(" : null");
                }
                sb.AppendLine(";");
            }
            else
            {
                var discriminatorIndex = FindDiscriminatorIndex(group, 0);

                if (discriminatorIndex == -1)
                {
                    // try next best effort
                    var startChars = group.Select(x => x[0]).Distinct();
                    var indentToUse = indentStr + baseIndent;
                    if (byLength.Length > 1)
                    {
                        indentToUse += indentStr;
                    }

                    for (var i = 1; i < length; ++i)
                    {
                        sb.Append(indentToUse).Append("var c").Append(i).Append(" = input[").Append(i).AppendLine("];");
                    }

                    foreach (var c in startChars)
                    {
                        sb.Append(indentToUse).Append("if (input[0] == '").Append(c).AppendLine("')");
                        sb.Append(indentToUse).AppendLine("{");
                        GenerateIfForStringContent(sb, group.Where(x => x[0] == c), indentToUse + indentStr, returnString, sourceIsSpan, startIndex: 1, charLookupGenerated: true);
                        sb.Append(indentToUse).AppendLine("}");
                    }

                    sb.Append(indentToUse).Append("return ").Append(returnString ? "null" : "false").AppendLine(";");

                    /* alternatives
                    if (group.Key > 4)
                    {
                        var indentToUse = byLength.Length > 1 ? indentStr + indentStr + baseIndent : indentStr + baseIndent;
                        // hash-based or equality-based then, let compiler generate decision tree
                        GenerateSwitchForStringContent(sb, group, indentToUse, returnString);
                    }
                    else
                    {
                        // smaller strings are fast to just brute-force check
                        var indentToUse = byLength.Length > 1 ? indentStr + indentStr + baseIndent : indentStr + baseIndent;
                        GenerateIfForStringContent(sb, group, indentToUse, returnString, sourceIsSpan);
                    }
                    */
                }
                else
                {
                    BuildDiscriminatorMatching(sb, group, discriminatorIndex, indent: baseIndent + indentStr, returnString, sourceIsSpan);
                }
            }

            if (byLength.Length > 1)
            {
                sb.Append(indentStr).AppendLine("        }");
            }
        }

        if (byLength.Length > 1)
        {
            sb.Append(indentStr).Append(baseIndent).AppendLine("default:");
            sb.Append(indentStr).Append(baseIndent).Append("   return ").Append(returnString ? "null" : "false").AppendLine(";");

            sb.Append(indentStr).AppendLine("    }");
        }

        return sb.ToString();
    }

    private static void BuildDiscriminatorMatching(
        StringBuilder sb,
        string[] group,
        int discriminatorIndex,
        string indent,
        bool returnString,
        bool sourceIsSpan)
    {
        sb.Append(indent).Append("    return ").Append("input[").Append(discriminatorIndex).AppendLine("] switch");
        sb.Append(indent).AppendLine("    {");

        foreach (var item in group)
        {
            sb.Append(indent).Append("        '");
            var value = item[discriminatorIndex];
            if (value == '\'')
            {
                sb.Append("\\'");
            }
            else
            {
                sb.Append(value);
            }
            sb.Append("' => ");

            if (group.Length == 1)
            {
                sb.Append("true");
            }
            else
            {
                StringEquality(sb, item, sourceIsSpan, startIndex: 0, discriminatorIndex, charLookupGenerated: false);
                if (returnString)
                {
                    sb.Append(" ? ").Append("\"").Append(Escape(item)).Append("\"").Append(" : null");
                }
            }
            sb.AppendLine(",");
        }
        sb.Append(indent).Append("        _ => ").AppendLine(returnString ? "null" : "false");
        sb.Append(indent).AppendLine("    };");
    }

    private static void StringEquality(
        StringBuilder builder,
        string toCheck,
        bool sourceIsSpan,
        int startIndex,
        int discriminatorIndex,
        bool charLookupGenerated)
    {
        toCheck = toCheck.Replace("\"", "\"");
        if (!sourceIsSpan)
        {
            if (startIndex != 0)
            {
                throw new NotImplementedException("Non-zero start index for string not implemented");
            }

            builder.Append("input == \"");
            builder.Append(Escape(toCheck));
            builder.Append("\"");
        }
        else
        {
            // TODO with net7 should be faster to do the sequence equals always
            // if we ever add a target for net7/8 we should revisit this equality checking
            var lengthToCheck = toCheck.Length - startIndex;
            if (lengthToCheck < 10)
            {
                // check char by char
                var addAnd = false;
                for (var i = startIndex; i < toCheck.Length; ++i)
                {
                    if (i == discriminatorIndex)
                    {
                        // no need to check
                        continue;
                    }

                    if (addAnd)
                    {
                        builder.Append(" && ");
                    }

                    if (charLookupGenerated)
                    {
                        builder.Append("c").Append(i);
                    }
                    else
                    {
                        builder.Append("input[").Append(i).Append("]");
                    }

                    builder.Append(" == '").Append(toCheck[i]).Append("'");
                    addAnd = true;
                }
            }
            else
            {
                if (startIndex == 0)
                {
                    builder.Append("input");
                }
                else
                {
                    builder.Append("input.AsSpan(").Append(startIndex).Append(")");
                }
                builder.Append(".SequenceEqual(\"");
                builder.Append(Escape(toCheck.AsSpan(startIndex).ToString()));
                builder.Append("\".AsSpan())");
            }
        }
    }

    private static void GenerateSwitchForStringContent(
        StringBuilder sb,
        IGrouping<int, string> group,
        string indent,
        bool returnString)
    {
        sb.Append(indent).AppendLine("switch (input)");
        sb.Append(indent).AppendLine("{");
        if (returnString)
        {
            foreach (var item in group)
            {
                sb.Append(indent).Append("    case \"").Append(Escape(item)).AppendLine("\":");
                sb.Append(indent).Append("        return \"").Append(Escape(item)).AppendLine("\";");
            }
        }
        else
        {
            foreach (var item in group)
            {
                sb.Append(indent).Append("    case \"").Append(Escape(item)).AppendLine("\":");
            }

            sb.Append(indent).AppendLine("        return true;");
        }

        sb.Append(indent).AppendLine("    default:");
        sb.Append(indent).Append("        return ").Append(returnString ? "null" : "false").AppendLine(";");
        sb.Append(indent).AppendLine("}");
    }

    private static void GenerateIfForStringContent(
        StringBuilder sb,
        IEnumerable<string> group,
        string indent,
        bool returnString,
        bool sourceIsSpan,
        int startIndex,
        bool charLookupGenerated)
    {
        foreach (var item in group)
        {
            sb.Append(indent).Append("if (");
            StringEquality(sb, item, sourceIsSpan, startIndex, discriminatorIndex: -1, charLookupGenerated);
            sb.AppendLine(")");
            sb.Append(indent).AppendLine("{");
            if (returnString)
            {
                sb.Append(indent).Append("    return \"").Append(Escape(item)).AppendLine("\";");
            }
            else
            {
                sb.Append(indent).AppendLine("    return false;");
            }
            sb.Append(indent).AppendLine("}");
        }

        sb.Append(indent).Append("return ").Append(returnString ? "null" : "false").AppendLine(";");
    }

    private static string Escape(string s)
    {
        return s.Replace("\"", "\\\"");
    }

    private static int FindDiscriminatorIndex(string[] grouping, int start)
    {
        var chars = new HashSet<char>();
        for (var i = start; i < grouping[0].Length; ++i)
        {
            chars.Clear();
            var allDifferent = true;
            foreach (var item in grouping)
            {
                allDifferent &= chars.Add(item[i]);
                if (!allDifferent)
                {
                    break;
                }
            }

            if (allDifferent)
            {
                return i;
            }
        }

        // not found
        return -1;
    }

    // determine the namespace the class/enum/struct is declared in, if any
    public static string GetNamespace(BaseTypeDeclarationSyntax syntax)
    {
        // If we don't have a namespace at all we'll return an empty string
        // This accounts for the "default namespace" case
        var nameSpace = string.Empty;

        // Get the containing syntax node for the type declaration
        // (could be a nested type, for example)
        var potentialNamespaceParent = syntax.Parent;

        // Keep moving "out" of nested classes etc until we get to a namespace
        // or until we run out of parents
        while (potentialNamespaceParent != null &&
               potentialNamespaceParent is not NamespaceDeclarationSyntax
               && potentialNamespaceParent is not FileScopedNamespaceDeclarationSyntax)
        {
            potentialNamespaceParent = potentialNamespaceParent.Parent;
        }

        // Build up the final namespace by looping until we no longer have a namespace declaration
        if (potentialNamespaceParent is BaseNamespaceDeclarationSyntax namespaceParent)
        {
            // We have a namespace. Use that as the type
            nameSpace = namespaceParent.Name.ToString();

            // Keep moving "out" of the namespace declarations until we
            // run out of nested namespace declarations
            while (true)
            {
                if (namespaceParent.Parent is not NamespaceDeclarationSyntax parent)
                {
                    break;
                }

                // Add the outer namespace as a prefix to the final namespace
                nameSpace = $"{namespaceParent.Name}.{nameSpace}";
                namespaceParent = parent;
            }
        }

        // return the final namespace
        return nameSpace;
    }

    public static Dictionary<string, string> GetAttributes(AttributeSyntax syntax)
    {
        var attributes = new Dictionary<string, string>();
        foreach (var item in syntax.ChildNodes())
        {
            if (item is AttributeArgumentListSyntax arguments)
            {
                foreach (var ag in arguments.Arguments)
                {
                    attributes.Add(ag.NameEquals!.Name.NormalizeWhitespace().ToFullString(), ag!.Expression.ChildTokens().FirstOrDefault().Value?.ToString() ?? "");
                }
            }
        }

        return attributes;
    }
}
