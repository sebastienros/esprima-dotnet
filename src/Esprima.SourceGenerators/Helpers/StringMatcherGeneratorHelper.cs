using System.Text;

namespace Esprima.SourceGenerators.Helpers;

internal static class StringMatcherGeneratorHelper
{
    public const string Attribute =
@"#nullable enable

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
    internal static string GenerateLookups(string[] alternatives, string indent, int indentionLevel, bool checkNull, bool returnString, bool sourceIsSpan)
    {
        var sb = new StringBuilder();

        var byLength = alternatives
            .Distinct()
            .ToLookup(x => x.Length)
            .OrderBy(x => x.Key)
            .Select(x => (Key: x.Key, x.OrderBy(x => x).ToArray()))
            .ToArray();

        if (checkNull)
        {
            sb.AppendIndent(indent, indentionLevel).AppendLine("if (input is null)");
            sb.AppendIndent(indent, indentionLevel).AppendLine("{");

            indentionLevel++;
            sb.AppendIndent(indent, indentionLevel).Append("return ").Append(returnString ? "null" : "false").AppendLine(";");

            indentionLevel--;
            sb.AppendIndent(indent, indentionLevel).AppendLine("}");
        }

        if (byLength.Length > 1)
        {
            sb.AppendIndent(indent, indentionLevel).AppendLine("switch (input.Length)");
            sb.AppendIndent(indent, indentionLevel).AppendLine("{");
            indentionLevel++;
        }

        foreach (var (length, group) in byLength)
        {
            if (byLength.Length > 1)
            {
                sb.AppendIndent(indent, indentionLevel).Append("case ").Append(length).AppendLine(":");
                sb.AppendIndent(indent, indentionLevel).AppendLine("{");
                indentionLevel++;
            }

            if (group.Length == 1)
            {
                var item = group[0];
                sb.AppendIndent(indent, indentionLevel).Append("return ");
                StringEquality(sb, item, sourceIsSpan, startIndex: 0, discriminatorIndex: -1, charLookupGenerated: false);
                if (returnString)
                {
                    sb.Append(" ? ").Append("\"").AppendEscaped(item).Append("\"").Append(" : null");
                }
                sb.AppendLine(";");
            }
            else
            {
                var discriminatorIndex = FindDiscriminatorIndex(group, 0);

                if (discriminatorIndex == -1)
                {
                    // try next best effort
                    sb.AppendIndent(indent, indentionLevel).Append("return ");
                    GenerateSwitchForStringContent(sb, discriminatorIndex: 0, length, group, indent, indentionLevel, returnString, sourceIsSpan);
                    sb.AppendLine(";");
                }
                else
                {
                    BuildDiscriminatorMatching(sb, group, discriminatorIndex, indent, indentionLevel, returnString, sourceIsSpan);
                }
            }

            if (byLength.Length > 1)
            {
                indentionLevel--;
                sb.AppendIndent(indent, indentionLevel).AppendLine("}");
            }
        }

        if (byLength.Length > 1)
        {
            sb.AppendIndent(indent, indentionLevel).AppendLine("default:");
            indentionLevel++;

            sb.AppendIndent(indent, indentionLevel).Append("return ").Append(returnString ? "null" : "false").AppendLine(";");
            indentionLevel--;

            indentionLevel--;
            sb.AppendIndent(indent, indentionLevel).AppendLine("}");
        }

        return sb.ToString();
    }

    private static void BuildDiscriminatorMatching(
        StringBuilder sb,
        string[] group,
        int discriminatorIndex,
        string indent,
        int indentionLevel,
        bool returnString,
        bool sourceIsSpan)
    {
        sb.AppendIndent(indent, indentionLevel).Append("return ").Append("input[").Append(discriminatorIndex).AppendLine("] switch");
        sb.AppendIndent(indent, indentionLevel).AppendLine("{");
        indentionLevel++;

        foreach (var item in group)
        {
            sb.AppendIndent(indent, indentionLevel).Append("'").AppendEscaped(item[discriminatorIndex]).Append("' => ");

            if (group.Length == 1)
            {
                sb.Append("true");
            }
            else
            {
                StringEquality(sb, item, sourceIsSpan, startIndex: 0, discriminatorIndex, charLookupGenerated: false);
                if (returnString)
                {
                    sb.Append(" ? ").Append("\"").AppendEscaped(item).Append("\"").Append(" : null");
                }
            }
            sb.AppendLine(",");
        }
        sb.AppendIndent(indent, indentionLevel).Append("_ => ").AppendLine(returnString ? "null" : "false");

        indentionLevel--;
        sb.AppendIndent(indent, indentionLevel).AppendLine("};");
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
            builder.AppendEscaped(toCheck);
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

                    builder.Append(" == '").AppendEscaped(toCheck[i]).Append("'");
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
                builder.AppendEscaped(toCheck.AsSpan(startIndex).ToString());
                builder.Append("\".AsSpan())");
            }
        }
    }

    private static void GenerateSwitchForStringContent(
        StringBuilder sb,
        int discriminatorIndex,
        int length,
        string[] group,
        string indent,
        int indentionLevel,
        bool returnString,
        bool sourceIsSpan)
    {
        var subGroups = group
            .GroupBy(x => x[discriminatorIndex])
            .OrderBy(x => x.Key)
            .Select(x => (x.Key, x.OrderBy(x => x).ToArray()));

        sb.Append("input[").Append(discriminatorIndex).AppendLine("] switch");
        sb.AppendIndent(indent, indentionLevel).AppendLine("{");
        indentionLevel++;

        foreach (var (discriminator, subGroup) in subGroups)
        {
            sb.AppendIndent(indent, indentionLevel).Append("'").AppendEscaped(discriminator).Append("' => ");

            if (subGroup.Length == 1 ||
                discriminatorIndex == length - 1) // Guard against duplicate strings.
            {
                var item = subGroup.First();
                if (discriminatorIndex < length - 1)
                {
                    StringEquality(sb, item, sourceIsSpan, discriminatorIndex + 1, discriminatorIndex: discriminatorIndex, charLookupGenerated: false);
                    if (returnString)
                    {
                        sb.Append(" ? ").Append("\"").AppendEscaped(item).Append("\"").Append(" : null");
                    }
                }
                else if (returnString)
                {
                    sb.Append("\"").AppendEscaped(item).Append("\"");
                }
                else
                {
                    sb.Append("true");
                }
            }
            else
            {
                GenerateSwitchForStringContent(sb, discriminatorIndex + 1, length, subGroup, indent, indentionLevel, returnString, sourceIsSpan);
            }

            sb.AppendLine(",");
        }

        sb.AppendIndent(indent, indentionLevel).Append("_ => ").AppendLine(returnString ? "null" : "false");

        indentionLevel--;
        sb.AppendIndent(indent, indentionLevel).Append("}");
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
}
