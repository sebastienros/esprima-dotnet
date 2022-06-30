using System.Collections.Immutable;
using System.Globalization;
using System.Text;
using Esprima.SourceGenerators.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Esprima.SourceGenerators;

// Spec for incremental generators: https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.md
// How to implement:
// * https://andrewlock.net/exploring-dotnet-6-part-9-source-generator-updates-incremental-generators/
// * https://www.thinktecture.com/en/net/roslyn-source-generators-performance/
// How to debug: https://stackoverflow.com/a/71314452/8656352
[Generator]
public class ChildNodesEnumerationHelpersGenerator : IIncrementalGenerator
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

    // IIncrementalGenerator has an Initialize method that is called by the host exactly once,
    // regardless of the number of further compilations that may occur.
    // For instance a host with multiple loaded projects may share the same generator instance across multiple projects,
    // and will only call Initialize a single time for the lifetime of the host.
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var compilationDiagnostics = context.CompilationProvider
            .Select(GetCompilationDiagnostics);

        IncrementalValuesProvider<HelperMethodInfo> helperMethodInfos = context.SyntaxProvider
            .CreateSyntaxProvider(IsSyntaxTargetForGeneration, GetSemanticTargetForGeneration)
            .Where(item => item is not null)!;

        context.RegisterSourceOutput(compilationDiagnostics.Combine(helperMethodInfos.Collect()), (context, source) => Execute(context, source.Left, source.Right));
    }

    private static StructuralEqualityWrapper<Diagnostic[]> GetCompilationDiagnostics(Compilation compilation, CancellationToken cancellationToken)
    {
        return new[] { NodeTypeName, NodeListTypeName, ChildNodesEnumeratorTypeName }
            .Select(typeName => (typeName, type: compilation.GetTypeByMetadataName(typeName)))
            .Where(item => item.type is null)
            .Select(item => Diagnostic.Create(s_typeNotFound, Location.None, item.typeName))
            .ToArray();
    }

    private static bool IsSyntaxTargetForGeneration(SyntaxNode node, CancellationToken cancellationToken)
    {
        return
            node is MethodDeclarationSyntax methodDeclarationSyntax &&
            methodDeclarationSyntax.Body is null &&
            methodDeclarationSyntax.Identifier.ValueText.StartsWith("MoveNext", StringComparison.Ordinal);
    }

    private sealed record class HelperMethodParamInfo(string ParamName, bool IsList, bool IsOptional);

    private sealed record class HelperMethodInfo(string MethodDeclaration, StructuralEqualityWrapper<HelperMethodParamInfo[]> ParamInfos);

    private static HelperMethodInfo? GetSemanticTargetForGeneration(GeneratorSyntaxContext context, CancellationToken cancellationToken)
    {
        INamedTypeSymbol? nodeType, nodeListType, childNodesEnumeratorType;

        var methodDeclarationSyntax = (MethodDeclarationSyntax) context.Node;
        var method = context.SemanticModel.GetDeclaredSymbol(methodDeclarationSyntax, cancellationToken);

        if (method is null ||
            !method.IsPartialDefinition ||
            method.DeclaringSyntaxReferences.Length != 1 ||
            (childNodesEnumeratorType = context.SemanticModel.Compilation.GetTypeByMetadataName(ChildNodesEnumeratorTypeName)) is null ||
            !SymbolEqualityComparer.Default.Equals(method.ContainingType, childNodesEnumeratorType) ||
            (nodeType = context.SemanticModel.Compilation.GetTypeByMetadataName(NodeTypeName)) is null ||
            (nodeListType = context.SemanticModel.Compilation.GetTypeByMetadataName(NodeListTypeName)?.ConstructUnboundGenericType()) is null ||
            !IsValidReturnType(method, nodeType) ||
            method.IsGenericMethod && !method.TypeParameters.All(param => IsValidGenericTypeParam(param, nodeType)))
        {
            return null;
        }

        var paramInfos = method.Parameters
            .Select(param => GetMethodParamInfo(param, nodeType, nodeListType))
            .TakeWhile(paramInfo => paramInfo is not null)
            .ToArray();

        if (paramInfos.Length < method.Parameters.Length)
        {
            return null;
        }

        return new HelperMethodInfo(GetMethodDeclaration(method), paramInfos!);

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

        static HelperMethodParamInfo? GetMethodParamInfo(IParameterSymbol param, INamedTypeSymbol nodeType, INamedTypeSymbol nodeListType)
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

    private static void Execute(SourceProductionContext context, StructuralEqualityWrapper<Diagnostic[]> compilationDiagnostics, ImmutableArray<HelperMethodInfo> helperMethodInfos)
    {
        if (compilationDiagnostics.Target.Length > 0)
        {
            foreach (var diagnostic in compilationDiagnostics.Target)
            {
                context.ReportDiagnostic(diagnostic);
            }

            return;
        }

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
        foreach (var helperMethodInfo in helperMethodInfos)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            sourceBuilder.Append(separator);
            separator = Environment.NewLine;

            GenerateHelperMethodImpl(helperMethodInfo, sourceBuilder);
        }

        sourceBuilder
            .AppendLine("    }")
            .AppendLine("}");


        context.AddSource($"ChildNodes.Helpers.g.cs", sourceBuilder.ToString());
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
