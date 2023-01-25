using System.Text;
using Esprima.SourceGenerators.Helpers;

namespace Esprima.SourceGenerators;

partial class VisitationBoilerplateGenerator
{
    private static void GenerateVisitableNodeClasses(StringBuilder sb, string? @namespace, IEnumerable<VisitableNodeInfo> nodeInfos, CancellationToken cancellationToken)
    {
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        if (@namespace is not null)
        {
            sb.AppendLine($"namespace {@namespace};");
            sb.AppendLine();
        }

        string? classSeparator = null;
        int indentionLevel = 0;
        foreach (var nodeInfo in nodeInfos.OrderBy(nodeInfo => nodeInfo.ClassName.TypeName))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (nodeInfo.GenerateNextChildNodeMethod || nodeInfo.GenerateAcceptMethod || nodeInfo.GenerateUpdateWithMethod)
            {
                AppendVisitableNodeClass(sb, nodeInfo, ref classSeparator, ref indentionLevel);
            }
        }
    }

    private static void AppendVisitableNodeClass(StringBuilder sb, VisitableNodeInfo nodeInfo, ref string? classSeparator, ref int indentionLevel)
    {
        sb.Append(classSeparator);
        classSeparator = Environment.NewLine;

        sb.AppendIndent(indentionLevel).AppendLine($"partial class {nodeInfo.ClassName.TypeName}");
        sb.AppendIndent(indentionLevel).AppendLine("{");
        indentionLevel++;

        string? methodSeparator = null;

        if (nodeInfo.GenerateNextChildNodeMethod)
        {
            sb.Append(methodSeparator);
            methodSeparator = Environment.NewLine;

            AppendVisitableNodeNextChildNodeMethod(sb, nodeInfo, ref indentionLevel);
        }

        if (nodeInfo.GenerateAcceptMethod)
        {
            sb.Append(methodSeparator);
            methodSeparator = Environment.NewLine;

            AppendVisitableNodeAcceptMethod(sb, nodeInfo, ref indentionLevel);
        }

        if (nodeInfo.GenerateUpdateWithMethod)
        {
            sb.Append(methodSeparator);

            AppendVisitableNodeUpdateWithMethod(sb, nodeInfo, ref indentionLevel);
        }

        indentionLevel--;
        sb.AppendIndent(indentionLevel).AppendLine("}");
    }

    private static void AppendVisitableNodeAcceptMethod(StringBuilder sb, VisitableNodeInfo nodeInfo, ref int indentionLevel)
    {
        var @sealed = nodeInfo.SealOverrideMethods ? "sealed " : null;

        sb.AppendIndent(indentionLevel).Append($"protected internal {@sealed}override object? Accept(");
        (nodeInfo.VisitorTypeName is not null ? sb.AppendTypeName(nodeInfo.VisitorTypeName) : sb.Append(AstVisitorCSharpTypeName))
            .AppendLine($" visitor) => visitor.Visit{nodeInfo.ClassName.TypeName}(this);");
    }

    private static void AppendVisitableNodeNextChildNodeMethod(StringBuilder sb, VisitableNodeInfo nodeInfo, ref int indentionLevel)
    {
        var @sealed = nodeInfo.SealOverrideMethods ? "sealed " : null;

        sb.AppendIndent(indentionLevel).Append($"internal {@sealed}override {NodeCSharpTypeName}? NextChildNode(ref {ChildNodesEnumeratorCSharpTypeName} enumerator) => ");
        if (nodeInfo.ChildPropertyInfos.Length > 0)
        {
            sb.Append("enumerator.");
            AppendChildNodesEnumeratorHelperMethodName(sb, nodeInfo.ChildPropertyInfos);
            sb.Append("(");

            string? paramSeparator = null;
            foreach (var propertyInfo in nodeInfo.ChildPropertyInfos)
            {
                sb.Append(paramSeparator);
                paramSeparator = ", ";

                sb.Append(propertyInfo.PropertyName);
            }

            sb.Append(")");
        }
        else
        {
            sb.Append("null");
        }
        sb.AppendLine(";");
    }

    private static void AppendVisitableNodeUpdateWithMethod(StringBuilder sb, VisitableNodeInfo nodeInfo, ref int indentionLevel)
    {
        sb.AppendIndent(indentionLevel).Append($"public {nodeInfo.ClassName.TypeName} UpdateWith(");

        string? paramSeparator = null;
        foreach (var propertyInfo in nodeInfo.ChildPropertyInfos)
        {
            sb.Append(paramSeparator);
            paramSeparator = ", ";

            if (propertyInfo.IsRefReadonly)
            {
                sb.Append("in ");
            }

            sb.AppendTypeName(propertyInfo.PropertyTypeName).Append(" ").Append(propertyInfo.VariableName);
        }

        sb.AppendLine(")");
        sb.AppendIndent(indentionLevel).AppendLine("{");
        indentionLevel++;

        sb.AppendIndent(indentionLevel).Append("if (");

        string? conditionSeparator = null;
        foreach (var propertyInfo in nodeInfo.ChildPropertyInfos)
        {
            sb.Append(conditionSeparator);
            conditionSeparator = " && ";

            if (propertyInfo.IsNodeList)
            {
                sb.Append(propertyInfo.VariableName).Append(".IsSameAs(").Append(propertyInfo.PropertyName).Append(")");
            }
            else
            {
                sb.Append("ReferenceEquals(").Append(propertyInfo.VariableName).Append(", ").Append(propertyInfo.PropertyName).Append(")");
            }
        }
        sb.AppendLine(")");

        sb.AppendIndent(indentionLevel).AppendLine("{");
        indentionLevel++;

        sb.AppendIndent(indentionLevel).AppendLine("return this;");

        indentionLevel--;
        sb.AppendIndent(indentionLevel).AppendLine("}");

        sb.AppendIndent(indentionLevel).AppendLine();

        sb.AppendIndent(indentionLevel).Append("return Rewrite(");

        paramSeparator = null;
        foreach (var propertyInfo in nodeInfo.ChildPropertyInfos)
        {
            sb.Append(paramSeparator);
            paramSeparator = ", ";

            sb.Append(propertyInfo.VariableName);
        }

        sb.AppendLine(");");

        indentionLevel--;
        sb.AppendIndent(indentionLevel).AppendLine("}");
    }
}
