using System.Globalization;
using System.Text;
using Esprima.SourceGenerators.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Esprima.SourceGenerators;

// How to debug: https://stackoverflow.com/a/71314452/8656352
[Generator]
public class ChildNodesEnumerationHelpersGenerator : ISourceGenerator
{
    private const string NodeTypeName = "Esprima.Ast.Node";
    private const string NodeListTypeName = "Esprima.Ast.NodeList`1";
    private const string ChildNodesEnumeratorTypeName = "Esprima.Ast.ChildNodes+Enumerator";

    private static readonly DiagnosticDescriptor s_typeNotFound = new DiagnosticDescriptor(
        id: "ESP0001",
        title: $"Type not found",
        messageFormat: "Type '{0}' does not exist in the compilation",
        category: "Design",
        DiagnosticSeverity.Error, isEnabledByDefault: true);

    public void Initialize(GeneratorInitializationContext context) { }

    public void Execute(GeneratorExecutionContext context)
    {
        var nodeType = GetRequiredType(NodeTypeName, context);
        var nodeListType = GetRequiredType(NodeListTypeName, context);
        var childNodesEnumeratorType = GetRequiredType(ChildNodesEnumeratorTypeName, context);

        if (nodeType is null || nodeListType is null || childNodesEnumeratorType is null)
        {
            return;
        }

        nodeListType = nodeListType.ConstructUnboundGenericType();

        var helperMethods = GetHelperMethodsToGenerate(childNodesEnumeratorType, nodeType, nodeListType, context.Compilation);

        var sourceBuilder = new StringBuilder();

        sourceBuilder
            .AppendLine("#nullable enable")
            .AppendLine()
            .AppendLine("namespace Esprima.Ast;")
            .AppendLine()
            .AppendLine("public readonly partial struct ChildNodes : IEnumerable<Node>")
            .AppendLine("{")
            .AppendLine("    public partial struct Enumerator : IEnumerator<Node>")
            .AppendLine("    {");

        string? separator = null;
        foreach (var method in helperMethods)
        {
            sourceBuilder.Append(separator);
            separator = Environment.NewLine;

            GenerateHelperMethodImpl(method, sourceBuilder);
        }

        sourceBuilder
            .AppendLine("    }")
            .AppendLine("}");


        context.AddSource($"{childNodesEnumeratorType.ContainingType.Name}.Helpers.g.cs", sourceBuilder.ToString());
    }

    private static INamedTypeSymbol? GetRequiredType(string typeName, GeneratorExecutionContext context)
    {
        var type = context.Compilation.Assembly.GetTypeByMetadataName(typeName);
        if (type is null)
        {
            context.ReportDiagnostic(Diagnostic.Create(s_typeNotFound, Location.None, typeName));
        }

        return type;
    }

    private sealed record class HelperMethodParamInfo(string ParamName, bool IsList, bool IsOptional);

    private sealed record class HelperMethodInfo(string MethodDeclaration, StructuralEqualityWrapper<HelperMethodParamInfo[]> ParamInfos);

    private static HelperMethodInfo[] GetHelperMethodsToGenerate(INamedTypeSymbol childNodesEnumeratorType, INamedTypeSymbol nodeType, INamedTypeSymbol nodeListType,
        Compilation compilation)
    {
        return childNodesEnumeratorType.GetMembers()
            .OfType<IMethodSymbol>()
            .Select(method => GetMethodInfo(method, nodeType, nodeListType, compilation))
            .Where(methodInfo => methodInfo is not null)
            .ToArray()!;

        static HelperMethodInfo? GetMethodInfo(IMethodSymbol method, INamedTypeSymbol nodeType, INamedTypeSymbol nodeListType, Compilation compilation)
        {
            if (!method.IsPartialDefinition ||
                method.DeclaringSyntaxReferences.Length != 1 ||
                !method.Name.StartsWith("MoveNext") ||
                !IsValidReturnType(method, nodeType) ||
                method.IsGenericMethod && !method.TypeParameters.All(param => IsValidGenericTypeParam(param, nodeType)))
            {
                return null;
            }

            var paramInfos = method.Parameters
                .Select(param => GetMethodParamInfo(param, nodeType, nodeListType, compilation))
                .TakeWhile(paramInfo => paramInfo is not null)
                .ToArray();

            if (paramInfos.Length < method.Parameters.Length)
            {
                return null;
            }

            return new HelperMethodInfo(GetMethodDeclaration(method), paramInfos!);
        }

        static bool IsValidReturnType(IMethodSymbol method, INamedTypeSymbol nodeType)
        {
            return
                method.RefKind == RefKind.None &&
                method.ReturnNullableAnnotation == NullableAnnotation.Annotated &&
                SymbolEqualityComparer.Default.Equals(method.ReturnType, nodeType);
        }

        static bool IsValidGenericTypeParam(ITypeParameterSymbol typeParam, INamedTypeSymbol nodeType)
        {
            return typeParam.ConstraintTypes.Any(constraintType =>
                constraintType.NullableAnnotation == NullableAnnotation.NotAnnotated &&
                SymbolEqualityComparer.Default.Equals(constraintType, nodeType));
        }

        static HelperMethodParamInfo? GetMethodParamInfo(IParameterSymbol param, INamedTypeSymbol nodeType, INamedTypeSymbol nodeListType, Compilation compilation)
        {
            // param type must be one of the following: Node, Node?, in NodeList<T>, in NodeList<T?> (where T : Node)

            if (param.RefKind == RefKind.In &&
                param.Type is INamedTypeSymbol paramType &&
                paramType.IsGenericType &&
                SymbolEqualityComparer.Default.Equals(paramType.ConstructUnboundGenericType(), nodeListType))
            {
                return new HelperMethodParamInfo(GetParamName(param), IsList: true, IsOptional: paramType.TypeArguments[0].NullableAnnotation == NullableAnnotation.Annotated);
            }
            else if (param.RefKind == RefKind.None &&
                SymbolEqualityComparer.Default.Equals(param.Type, nodeType))
            {
                return new HelperMethodParamInfo(GetParamName(param), IsList: false, IsOptional: param.Type.NullableAnnotation == NullableAnnotation.Annotated);
            }

            return null;
        }


        static string GetParamName(IParameterSymbol param)
        {
            var paramSyntax = (ParameterSyntax) param.DeclaringSyntaxReferences[0].GetSyntax();
            return paramSyntax.Identifier.ToString();
        }

        static string GetMethodDeclaration(IMethodSymbol method)
        {
            var methodDeclarationSyntax = (MethodDeclarationSyntax) method.DeclaringSyntaxReferences[0].GetSyntax();

            // remove trailing semicolon

            methodDeclarationSyntax = SyntaxFactory.MethodDeclaration(
                methodDeclarationSyntax.AttributeLists,
                methodDeclarationSyntax.Modifiers,
                methodDeclarationSyntax.ReturnType,
                methodDeclarationSyntax.ExplicitInterfaceSpecifier,
                methodDeclarationSyntax.Identifier,
                methodDeclarationSyntax.TypeParameterList,
                methodDeclarationSyntax.ParameterList,
                methodDeclarationSyntax.ConstraintClauses,
                body: null,
                expressionBody: null);

            return methodDeclarationSyntax.ToString();
        }
    }

    private static void GenerateHelperMethodImpl(HelperMethodInfo methodInfo, StringBuilder sourceBuilder)
    {
        sourceBuilder
            .AppendLine($"        {methodInfo.MethodDeclaration}")
            .AppendLine("        {");

        sourceBuilder
            .AppendLine("            switch (_propertyIndex)")
            .AppendLine("            {");

        var itemVariable = "Node? item";
        var paramInfos = methodInfo.ParamInfos.Target;
        for (int i = 0, n = paramInfos.Length; i < n; i++)
        {
            var paramInfo = paramInfos[i];
            var paramName = paramInfo.ParamName;

            sourceBuilder
                .AppendLine($"                case {i.ToString(CultureInfo.InvariantCulture)}:");

            if (paramInfo.IsList)
            {
                sourceBuilder
                    .AppendLine($"                    if (_listIndex >= {paramName}.Count)")
                    .AppendLine("                    {")
                    .AppendLine("                        _listIndex = 0;")
                    .AppendLine("                        _propertyIndex++;")
                    .AppendLine($"                        goto {GetJumpLabel(i + 1, n)};")
                    .AppendLine("                    }")
                    .AppendLine()
                    .AppendLine($"                    {itemVariable} = {paramName}[_listIndex++];")
                    .AppendLine();

                itemVariable = "item";

                if (paramInfo.IsOptional)
                {
                    sourceBuilder
                        .AppendLine($"                    if ({itemVariable} is null)")
                        .AppendLine("                    {")
                        .AppendLine($"                        goto {GetJumpLabel(i, n)};")
                        .AppendLine("                    }")
                        .AppendLine();
                }

                sourceBuilder
                    .AppendLine($"                    return item;");
            }
            else
            {
                sourceBuilder
                    .AppendLine("                    _propertyIndex++;")
                    .AppendLine();

                if (paramInfo.IsOptional)
                {
                    sourceBuilder
                        .AppendLine($"                    if ({paramName} is null)")
                        .AppendLine("                    {")
                        .AppendLine($"                        goto {GetJumpLabel(i + 1, n)};")
                        .AppendLine("                    }")
                        .AppendLine();
                }

                sourceBuilder
                    .AppendLine($"                    return {paramName};");
            }
        }

        sourceBuilder
            .AppendLine("                default:")
            .AppendLine("                    return null;")
            .AppendLine("            }")
            .AppendLine("        }");

        static string GetJumpLabel(int targetParamIndex, int paramCount)
        {
            return targetParamIndex >= paramCount ? "default" : $"case {targetParamIndex.ToString(CultureInfo.InvariantCulture)}";
        }
    }
}
