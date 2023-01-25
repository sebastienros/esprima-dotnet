using System.Text;
using Esprima.SourceGenerators.Helpers;
using Microsoft.CodeAnalysis;

namespace Esprima.SourceGenerators;

partial class VisitationBoilerplateGenerator
{
    private static void GenerateAstVisitorClass(StringBuilder sb, AstVisitorInfo astVisitorInfo, IEnumerable<VisitableNodeInfo> nodeInfos, CancellationToken cancellationToken)
    {
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        if (astVisitorInfo.ClassName.Namespace is not null)
        {
            sb.AppendLine($"namespace {astVisitorInfo.ClassName.Namespace};");
            sb.AppendLine();
        }

        var indentionLevel = 0;
        sb.AppendIndent(indentionLevel).Append("partial class ").AppendTypeBareName(astVisitorInfo.ClassName.BareName).AppendLine();
        sb.AppendIndent(indentionLevel).AppendLine("{");
        indentionLevel++;

        var visitMethodModifiers =
            astVisitorInfo.VisitorTypeName is { TypeKind: TypeKind.Interface } ? "public virtual" :
            astVisitorInfo is { VisitorTypeName: null, Kind: VisitorKind.Visitor } ? "protected internal virtual" :
            "protected internal override";

        var (visitMethodFilter, visitMethodBodyAppender) = astVisitorInfo.Kind switch
        {
            VisitorKind.Visitor =>
            (
                new Func<string, HashSet<string>, VisitableNodeInfo, AstVisitorInfo, bool>(static (visitMethodName, definedVisitMethods, _, _) =>
                    !definedVisitMethods.Contains(visitMethodName)),
                new AstVisitorVisitMethodBodyAppender(AppendAstVisitorVisitMethodBody)
            ),
            VisitorKind.Rewriter =>
            (
                static (visitMethodName, definedVisitMethods, nodeInfo, astVisitorInfo) =>
                    (astVisitorInfo.BaseVisitorFieldName is not null || nodeInfo.ChildPropertyInfos.Length > 0)
                    && !definedVisitMethods.Contains(visitMethodName),
                AppendAstRewriterVisitMethodBody
            ),
            _ => throw new InvalidOperationException()
        };

        var definedVisitMethods = new HashSet<string>(astVisitorInfo.DefinedVisitMethods);

        string? methodSeparator = null;
        foreach (var nodeInfo in nodeInfos.OrderBy(nodeInfo => nodeInfo.ClassName.TypeName))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var visitMethodName = VisitMethodNamePrefix + nodeInfo.ClassName.TypeName;

            if (visitMethodFilter(visitMethodName, definedVisitMethods, nodeInfo, astVisitorInfo))
            {
                sb.Append(methodSeparator);
                methodSeparator = Environment.NewLine;

                AppendAstVisitorVisitMethod(sb, visitMethodName, visitMethodModifiers, visitMethodBodyAppender,
                    nodeInfo, astVisitorInfo, ref indentionLevel);
            }
        }

        indentionLevel--;
        sb.AppendIndent(indentionLevel).AppendLine("}");
    }

    private static void AppendAstVisitorVisitMethod(StringBuilder sb, string methodName, string methodModifiers, AstVisitorVisitMethodBodyAppender bodyAppender,
        VisitableNodeInfo nodeInfo, AstVisitorInfo astVisitorInfo, ref int indentionLevel)
    {
        sb.AppendIndent(indentionLevel).Append($"{methodModifiers} object? {methodName}(")
            .AppendTypeName(nodeInfo.ClassName).Append(" ").Append(nodeInfo.VariableName).AppendLine(")");

        sb.AppendIndent(indentionLevel).AppendLine("{");
        indentionLevel++;

        bodyAppender(sb, methodName, nodeInfo, astVisitorInfo, ref indentionLevel);

        indentionLevel--;
        sb.AppendIndent(indentionLevel).AppendLine("}");
    }

    private delegate void AstVisitorVisitMethodBodyAppender(StringBuilder sb, string methodName, VisitableNodeInfo nodeInfo, AstVisitorInfo astVisitorInfo, ref int indentionLevel);

    private static void AppendAstVisitorVisitMethodBody(StringBuilder sb, string methodName, VisitableNodeInfo nodeInfo, AstVisitorInfo astVisitorInfo, ref int indentionLevel)
    {
        var thisVisitorFieldAccess = astVisitorInfo.TargetVisitorFieldName is not null ? astVisitorInfo.TargetVisitorFieldName + "." : null;

        foreach (var propertyInfo in nodeInfo.ChildPropertyInfos)
        {
            if (propertyInfo.IsNodeList)
            {
                sb.AppendIndent(indentionLevel).Append("ref readonly var ").Append(propertyInfo.VariableName).Append(" = ref ")
                    .Append(nodeInfo.VariableName).Append(".").Append(propertyInfo.PropertyName).AppendLine(";");

                sb.AppendIndent(indentionLevel).Append("for (var i = 0; i < ").Append(propertyInfo.VariableName).AppendLine(".Count; i++)");
                sb.AppendIndent(indentionLevel).AppendLine("{");
                indentionLevel++;

                string itemExpression;
                if (propertyInfo.IsOptional)
                {
                    itemExpression = propertyInfo.VariableName + "Item";
                    sb.AppendIndent(indentionLevel).Append("var ").Append(itemExpression).Append(" = ").Append(propertyInfo.VariableName).AppendLine("[i];");
                    sb.AppendIndent(indentionLevel).Append("if (").Append(itemExpression).AppendLine(" is not null)");
                    sb.AppendIndent(indentionLevel).AppendLine("{");
                    indentionLevel++;
                }
                else
                {
                    itemExpression = propertyInfo.VariableName + "[i]";
                }

                sb.AppendIndent(indentionLevel).Append(thisVisitorFieldAccess).Append("Visit(").Append(itemExpression).AppendLine(");");

                if (propertyInfo.IsOptional)
                {
                    indentionLevel--;
                    sb.AppendIndent(indentionLevel).AppendLine("}");
                }

                indentionLevel--;
                sb.AppendIndent(indentionLevel).AppendLine("}");
            }
            else
            {
                if (propertyInfo.IsOptional)
                {
                    sb.AppendIndent(indentionLevel).Append("if (").Append(nodeInfo.VariableName).Append(".").Append(propertyInfo.PropertyName).AppendLine(" is not null)");
                    sb.AppendIndent(indentionLevel).AppendLine("{");
                    indentionLevel++;
                }

                sb.AppendIndent(indentionLevel).Append(thisVisitorFieldAccess).Append("Visit(").Append(nodeInfo.VariableName).Append(".").Append(propertyInfo.PropertyName).AppendLine(");");

                if (propertyInfo.IsOptional)
                {
                    indentionLevel--;
                    sb.AppendIndent(indentionLevel).AppendLine("}");
                }
            }

            sb.AppendLine();
        }

        sb.AppendIndent(indentionLevel).Append("return ").Append(nodeInfo.VariableName).AppendLine(";");
    }

    private static void AppendAstRewriterVisitMethodBody(StringBuilder sb, string methodName, VisitableNodeInfo nodeInfo, AstVisitorInfo astVisitorInfo, ref int indentionLevel)
    {
        var thisVisitorFieldAccess = astVisitorInfo.TargetVisitorFieldName is not null ? astVisitorInfo.TargetVisitorFieldName + "." : null;
        var baseVisitorFieldAccess = astVisitorInfo.BaseVisitorFieldName is not null ? astVisitorInfo.BaseVisitorFieldName + "." : null;

        if (nodeInfo.ChildPropertyInfos.Length > 0)
        {
            foreach (var propertyInfo in nodeInfo.ChildPropertyInfos)
            {
                if (propertyInfo.IsNodeList)
                {
                    sb.AppendIndent(indentionLevel).Append(thisVisitorFieldAccess).Append("VisitAndConvert(")
                        .Append(nodeInfo.VariableName).Append(".").Append(propertyInfo.PropertyName)
                        .Append(", out var ").Append(propertyInfo.VariableName);

                    if (propertyInfo.IsOptional)
                    {
                        sb.Append(", allowNullElement: true");
                    }
                    sb.AppendLine(");");
                }
                else
                {
                    sb.AppendIndent(indentionLevel).Append("var ").Append(propertyInfo.VariableName).Append(" = ")
                        .Append(thisVisitorFieldAccess).Append("VisitAndConvert(")
                        .Append(nodeInfo.VariableName).Append(".").Append(propertyInfo.PropertyName);

                    if (propertyInfo.IsOptional)
                    {
                        sb.Append(", allowNull: true");
                    }
                    sb.AppendLine(");");
                }

                sb.AppendLine();
            }

            sb.AppendIndent(indentionLevel).Append("return ").Append(nodeInfo.VariableName).Append(".UpdateWith(");

            string? argumentSeparator = null;
            foreach (var propertyInfo in nodeInfo.ChildPropertyInfos)
            {
                sb.Append(argumentSeparator);
                argumentSeparator = ", ";

                sb.Append(propertyInfo.VariableName);
            }

            sb.AppendLine(");");
        }
        else
        {
            sb.AppendIndent(indentionLevel).Append("return ").Append(baseVisitorFieldAccess).Append(methodName).Append("(")
                .Append(nodeInfo.VariableName).AppendLine(");");
        }
    }

    private static void GenerateAstVisitorInterface(StringBuilder sb, AstVisitorInfo astVisitorInfo, IEnumerable<VisitableNodeInfo> nodeInfos, CancellationToken cancellationToken)
    {
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        if (astVisitorInfo.VisitorTypeName!.Namespace is not null)
        {
            sb.AppendLine($"namespace {astVisitorInfo.VisitorTypeName.Namespace};");
            sb.AppendLine();
        }

        var indentionLevel = 0;
        sb.AppendIndent(indentionLevel).Append("partial interface ").AppendTypeBareName(astVisitorInfo.VisitorTypeName.BareName).AppendLine();
        sb.AppendIndent(indentionLevel).AppendLine("{");
        indentionLevel++;

        string? methodSeparator = null;
        foreach (var nodeInfo in nodeInfos.OrderBy(nodeInfo => nodeInfo.ClassName.TypeName))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var visitMethodName = VisitMethodNamePrefix + nodeInfo.ClassName.TypeName;

            sb.Append(methodSeparator);
            methodSeparator = Environment.NewLine;

            AppendAstVisitorInterfaceVisitMethod(sb, visitMethodName, nodeInfo, ref indentionLevel);
        }

        indentionLevel--;
        sb.AppendIndent(indentionLevel).AppendLine("}");
    }

    private static void AppendAstVisitorInterfaceVisitMethod(StringBuilder sb, string methodName, VisitableNodeInfo nodeInfo, ref int indentionLevel)
    {
        sb.AppendIndent(indentionLevel).Append($"object? {methodName}(")
            .AppendTypeName(nodeInfo.ClassName).Append(" ").Append(nodeInfo.VariableName).AppendLine(");");
    }
}
