using System.Globalization;
using System.Text;
using Esprima.SourceGenerators.Helpers;

namespace Esprima.SourceGenerators;

partial class VisitationBoilerplateGenerator
{
    private sealed class ChildNodesEnumeratorHelperMethodSignatureEqualityComparer : IEqualityComparer<IChildNodesEnumeratorHelperParamInfo[]>
    {
        public bool Equals(IChildNodesEnumeratorHelperParamInfo[] x, IChildNodesEnumeratorHelperParamInfo[] y)
        {
            if (x.Length != y.Length)
            {
                return false;
            }

            for (var i = 0; i < x.Length; i++)
            {
                var paramInfo1 = x[i];
                var paramInfo2 = y[i];

                if (paramInfo1.IsNodeList != paramInfo2.IsNodeList || paramInfo1.IsOptional != paramInfo2.IsOptional)
                {
                    return false;
                }
            }

            return true;
        }

        public int GetHashCode(IChildNodesEnumeratorHelperParamInfo[] obj)
        {
            var hashCode = 1327044938;
            foreach (var paramInfo in obj)
            {
                hashCode = hashCode * -1521134295 + paramInfo.IsOptional.GetHashCode();
                hashCode = hashCode * -1521134295 + paramInfo.IsNodeList.GetHashCode();
            }
            return hashCode;
        }
    }

    private static void GenerateChildNodesEnumeratorHelpers(StringBuilder sb, IEnumerable<VisitableNodeInfo> nodeInfos, CancellationToken cancellationToken)
    {
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine("namespace Esprima.Ast;");
        sb.AppendLine();

        var indentionLevel = 0;
        sb.AppendIndent(indentionLevel).AppendLine("partial struct ChildNodes");
        sb.AppendIndent(indentionLevel).AppendLine("{");
        indentionLevel++;

        sb.AppendIndent(indentionLevel).AppendLine("partial struct Enumerator");
        sb.AppendIndent(indentionLevel).AppendLine("{");
        indentionLevel++;

        var methodSignatures = nodeInfos
            .Where(nodeInfo => nodeInfo.GenerateNextChildNodeMethod && nodeInfo.ChildPropertyInfos.Length > 0)
            .Select(nodeInfo => nodeInfo.ChildPropertyInfos)
            .Distinct(new ChildNodesEnumeratorHelperMethodSignatureEqualityComparer())
            .OrderBy(signature => signature.Length)
            .ThenBy(signature => signature.Count(paramInfo => paramInfo.IsOptional))
            .ThenBy(signature => signature
                .Select((paramInfo, index) => (paramInfo, index))
                .Aggregate(0UL, (weight, item) => item.paramInfo.IsOptional ? weight | 1UL << item.index : weight))
            .ThenBy(signature => signature.Count(paramInfo => paramInfo.IsNodeList))
            .ThenBy(signature => signature
                .Select((paramInfo, index) => (paramInfo, index))
                .Aggregate(0UL, (weight, item) => item.paramInfo.IsNodeList ? weight | 1UL << item.index : weight));

        string? methodSeparator = null;
        foreach (var methodSignature in methodSignatures)
        {
            cancellationToken.ThrowIfCancellationRequested();

            sb.Append(methodSeparator);
            methodSeparator = Environment.NewLine;

            AppendChildNodesEnumeratorHelperMethod(sb, methodSignature, ref indentionLevel);
        }

        indentionLevel--;
        sb.AppendIndent(indentionLevel).AppendLine("}");

        indentionLevel--;
        sb.AppendIndent(indentionLevel).AppendLine("}");
    }

    private static void AppendChildNodesEnumeratorHelperMethod(StringBuilder sb, IChildNodesEnumeratorHelperParamInfo[] methodSignature, ref int indentionLevel)
    {
        sb.AppendIndent(indentionLevel).Append($"internal {NodeCSharpTypeName}? ");
        AppendChildNodesEnumeratorHelperMethodName(sb, methodSignature);

        var isGeneric = false;
        for (var i = 0; i < methodSignature.Length; i++)
        {
            var paramInfo = methodSignature[i];

            if (paramInfo.IsNodeList)
            {
                if (!isGeneric)
                {
                    isGeneric = true;
                    sb.Append("<");
                }
                else
                {
                    sb.Append(", ");
                }

                sb.Append("T").Append(i.ToString(CultureInfo.InvariantCulture));
            }
        }

        if (isGeneric)
        {
            sb.Append(">");
        }

        sb.Append("(");

        string? paramSeparator = null;
        for (var i = 0; i < methodSignature.Length; i++)
        {
            sb.Append(paramSeparator);
            paramSeparator = ", ";

            var paramInfo = methodSignature[i];
            var paramIndex = i.ToString(CultureInfo.InvariantCulture);

            if (paramInfo.IsNodeList)
            {
                sb.Append("in ").Append(NodeListOfTCSharpTypeName, 0, NodeListOfTCSharpTypeName.Length - 2);
                sb.Append("<T").Append(paramIndex);
                if (paramInfo.IsOptional)
                {
                    sb.Append("?");
                }
                sb.Append(">");
            }
            else
            {
                sb.Append(NodeCSharpTypeName);
                if (paramInfo.IsOptional)
                {
                    sb.Append("?");
                }
            }

            sb.Append(" arg").Append(paramIndex);
        }

        sb.Append(")");

        if (isGeneric)
        {
            indentionLevel++;
            for (var i = 0; i < methodSignature.Length; i++)
            {
                var paramInfo = methodSignature[i];

                if (paramInfo.IsNodeList)
                {
                    sb.AppendLine();
                    sb.AppendIndent(indentionLevel).Append("where T").Append(i.ToString(CultureInfo.InvariantCulture))
                        .Append(" : ").Append(NodeCSharpTypeName);
                }
            }
            indentionLevel--;
        }

        sb.AppendLine();

        sb.AppendIndent(indentionLevel).AppendLine("{");
        indentionLevel++;

        AppendChildNodesEnumeratorHelperMethodBody(sb, methodSignature, ref indentionLevel);

        indentionLevel--;
        sb.AppendIndent(indentionLevel).AppendLine("}");
    }

    private static void AppendChildNodesEnumeratorHelperMethodName(StringBuilder sb, IChildNodesEnumeratorHelperParamInfo[] methodSignature)
    {
        // We can't use a single overloaded method name as NRT annotations are not part of the method signature.
        // Thus, to disambiguate method resolution, we encode nullability of the parameters into the method name as follows:
        // * In case of a single parameter: 'MoveNext' when parameter/element type is not nullable,
        //   otherwise 'MoveNextNullable'.
        // * In case of multiple parameters: 'MoveNext' when all parameter/element types are not nullable,
        //   otherwise 'MoveNextNullableAt{NULLABLE_PARAM_INDICES_SEPARATED_BY_UNDERSCORE}'.

        sb.Append("MoveNext");
        if (methodSignature.Length == 1)
        {
            if (methodSignature[0].IsOptional)
            {
                sb.Append("Nullable");
            }
        }
        else
        {
            var prefix = "NullableAt";
            for (var i = 0; i < methodSignature.Length; i++)
            {
                var propertyInfo = methodSignature[i];

                if (propertyInfo.IsOptional)
                {
                    sb.Append(prefix);
                    prefix = "_";

                    sb.Append(i.ToString(CultureInfo.InvariantCulture));
                }
            }
        }
    }

    private static void AppendChildNodesEnumeratorHelperMethodBody(StringBuilder sb, IChildNodesEnumeratorHelperParamInfo[] methodSignature, ref int indentionLevel)
    {
        sb.AppendIndent(indentionLevel).AppendLine("switch (_propertyIndex)");
        sb.AppendIndent(indentionLevel).AppendLine("{");
        indentionLevel++;

        var itemVariable = NodeTypeName + "? item";
        for (int i = 0, n = methodSignature.Length; i < n; i++)
        {
            var paramInfo = methodSignature[i];
            var paramIndex = i.ToString(CultureInfo.InvariantCulture);
            var paramName = "arg" + paramIndex;

            sb.AppendIndent(indentionLevel).AppendLine($"case {paramIndex}:");
            indentionLevel++;

            if (paramInfo.IsNodeList)
            {
                sb.AppendIndent(indentionLevel).AppendLine($"if (_listIndex >= {paramName}.Count)");
                sb.AppendIndent(indentionLevel).AppendLine("{");
                indentionLevel++;

                sb.AppendIndent(indentionLevel).AppendLine("_listIndex = 0;");
                sb.AppendIndent(indentionLevel).AppendLine("_propertyIndex++;");
                sb.AppendIndent(indentionLevel).AppendLine($"goto {GetJumpLabel(i + 1, n)};");

                indentionLevel--;
                sb.AppendIndent(indentionLevel).AppendLine("}");
                sb.AppendIndent(indentionLevel).AppendLine();
                sb.AppendIndent(indentionLevel).AppendLine($"{itemVariable} = {paramName}[_listIndex++];");
                sb.AppendIndent(indentionLevel).AppendLine();

                itemVariable = "item";

                if (paramInfo.IsOptional)
                {
                    sb.AppendIndent(indentionLevel).AppendLine($"if ({itemVariable} is null)");
                    sb.AppendIndent(indentionLevel).AppendLine("{");
                    indentionLevel++;

                    sb.AppendIndent(indentionLevel).AppendLine($"goto {GetJumpLabel(i, n)};");

                    indentionLevel--;
                    sb.AppendIndent(indentionLevel).AppendLine("}");
                    sb.AppendIndent(indentionLevel).AppendLine();
                }

                sb.AppendIndent(indentionLevel).AppendLine($"return {itemVariable};");
            }
            else
            {
                sb.AppendIndent(indentionLevel).AppendLine("_propertyIndex++;");
                sb.AppendIndent(indentionLevel).AppendLine();

                if (paramInfo.IsOptional)
                {
                    sb.AppendIndent(indentionLevel).AppendLine($"if ({paramName} is null)");
                    sb.AppendIndent(indentionLevel).AppendLine("{");
                    indentionLevel++;

                    sb.AppendIndent(indentionLevel).AppendLine($"goto {GetJumpLabel(i + 1, n)};");

                    indentionLevel--;
                    sb.AppendIndent(indentionLevel).AppendLine("}");
                    sb.AppendIndent(indentionLevel).AppendLine();
                }

                sb.AppendIndent(indentionLevel).AppendLine($"return {paramName};");
            }

            indentionLevel--;
        }

        sb.AppendIndent(indentionLevel).AppendLine("default:");
        indentionLevel++;

        sb.AppendIndent(indentionLevel).AppendLine("return null;");
        indentionLevel--;

        indentionLevel--;
        sb.AppendIndent(indentionLevel).AppendLine("}");

        static string GetJumpLabel(int targetParamIndex, int paramCount)
        {
            return targetParamIndex >= paramCount ? "default" : $"case {targetParamIndex.ToString(CultureInfo.InvariantCulture)}";
        }
    }
}
