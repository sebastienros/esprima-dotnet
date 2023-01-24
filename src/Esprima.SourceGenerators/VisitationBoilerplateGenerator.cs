using System.Collections.Immutable;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Text;
using Esprima.SourceGenerators.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Esprima.SourceGenerators;

// Spec for incremental generators: https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.md
// How to implement:
// * https://andrewlock.net/exploring-dotnet-6-part-9-source-generator-updates-incremental-generators/
// * https://www.thinktecture.com/en/net/roslyn-source-generators-performance/
// How to debug: https://stackoverflow.com/a/71314452/8656352

/// <summary>
/// Generates a bunch of boilerplate code of the visitation/enumeration logic based on
/// the annotations of the AST nodes (VisitableNodeAttribute):
/// <list type="bullet">
///     <item>NextChildNode methods of annotated AST nodes</item>
///     <item>Accept methods of annotated AST nodes</item>
///     <item>UpdateWith methods of annotated AST nodes</item>
/// </list>
/// </summary>
[Generator]
public class VisitationBoilerplateGenerator : IIncrementalGenerator
{
    public const string VisitableNodeAttributeSourceText =
@"#nullable enable

namespace Esprima.Ast;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
internal sealed class VisitableNodeAttribute : Attribute
{
    public Type? VisitorType { get; set; }
    public string[]? ChildProperties { get; set; }
    public bool SealOverrideMethods { get; set; }
}
";

    private const string NodeTypeName = "Esprima.Ast.Node";
    private const string NodeCSharpTypeName = "Esprima.Ast.Node";

    private const string NodeListOfTTypeName = "Esprima.Ast.NodeList`1";
    private const string NodeListOfTCSharpTypeName = "Esprima.Ast.NodeList<>";

    private const string ChildNodesEnumeratorTypeName = "Esprima.Ast.ChildNodes+Enumerator";
    private const string ChildNodesEnumeratorCSharpTypeName = "Esprima.Ast.ChildNodes.Enumerator";

    private const string AstVisitorTypeName = "Esprima.Utils.AstVisitor";
    private const string AstVisitorCSharpTypeName = "Esprima.Utils.AstVisitor";

    private static readonly IReadOnlyDictionary<string, string> s_wellKnownTypeNames = new Dictionary<string, string>
    {
        [NodeTypeName] = NodeCSharpTypeName,
        [NodeListOfTTypeName] = NodeListOfTCSharpTypeName,
        [ChildNodesEnumeratorTypeName] = ChildNodesEnumeratorCSharpTypeName,
        [AstVisitorTypeName] = AstVisitorCSharpTypeName,
    };

    // IIncrementalGenerator has an Initialize method that is called by the host exactly once,
    // regardless of the number of further compilations that may occur.
    // For instance a host with multiple loaded projects may share the same generator instance across multiple projects,
    // and will only call Initialize a single time for the lifetime of the host.
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            "VisitableNodeAttribute.g.cs", SourceText.From(VisitableNodeAttributeSourceText, Encoding.UTF8)));

        var compilationDiagnostics = context.CompilationProvider
            .Select(GetCompilationDiagnostics);

        IncrementalValuesProvider<VisitableNodeInfo> visitableNodeClassInfos = context.SyntaxProvider
            .CreateSyntaxProvider(IsSyntaxTargetForGeneration, GetSemanticTargetForGeneration)
            .Where(item => item is not null)!;

        context.RegisterSourceOutput(compilationDiagnostics.Combine(visitableNodeClassInfos.Collect()), (context, source) => Execute(context, source.Left, source.Right));
    }

    private static StructuralEqualityWrapper<Diagnostic[]> GetCompilationDiagnostics(Compilation compilation, CancellationToken cancellationToken)
    {
        return s_wellKnownTypeNames
            .Select(kvp => (type: compilation.GetTypeByMetadataName(kvp.Key), csharpTypeName: kvp.Value))
            .Where(item => item.type is null)
            .Select(item => Diagnostic.Create(Diagnostics.TypeNotFoundError, Location.None, item.csharpTypeName))
            .ToArray();
    }

    private static bool IsSyntaxTargetForGeneration(SyntaxNode node, CancellationToken cancellationToken)
    {
        return node is ClassDeclarationSyntax classDeclarationSyntax
            && classDeclarationSyntax.AttributeLists.Count > 0;
    }

    private static VisitableNodeInfo? GetSemanticTargetForGeneration(GeneratorSyntaxContext context, CancellationToken cancellationToken)
    {
        // 1. Discover classes annotated with the expected attribute

        var classDeclarationSyntax = (ClassDeclarationSyntax) context.Node;
        var classType = context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax, cancellationToken);

        INamedTypeSymbol? nodeType, nodeListType, childNodesEnumeratorType, astVisitorType;
        AttributeData? attribute;

        if (classType is null
            || (attribute = classType.GetAttributes().FirstOrDefault())?.AttributeClass?.Name != "VisitableNodeAttribute"
            || attribute.AttributeClass.ContainingNamespace.ToString() != "Esprima.Ast"
            // Class may be split into multiple files but should be analyzed only once.
            || classDeclarationSyntax != classType.DeclaringSyntaxReferences.First().GetSyntax(cancellationToken)
            || (nodeType = context.SemanticModel.Compilation.GetTypeByMetadataName(NodeTypeName)) is null
            || (nodeListType = context.SemanticModel.Compilation.GetTypeByMetadataName(NodeListOfTTypeName)?.ConstructUnboundGenericType()) is null
            || (childNodesEnumeratorType = context.SemanticModel.Compilation.GetTypeByMetadataName(ChildNodesEnumeratorTypeName)) is null
            || (astVisitorType = context.SemanticModel.Compilation.GetTypeByMetadataName(AstVisitorTypeName)) is null)
        {
            return null;
        }

        var className = classType.Name;
        var classNamespace = classType.ContainingNamespace?.ToString();

        if (// Class must be non-generic.
            classType.IsGenericType
            // Class must be partial.
            || !classDeclarationSyntax.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.PartialKeyword))
            // Class must inherit from Node.
            || !classType.InheritsFrom(nodeType))
        {
            var location = attribute.ApplicationSyntaxReference?.GetSyntax(cancellationToken).GetLocation() ?? Location.None;
            var diagnostic = Diagnostic.Create(Diagnostics.InvalidVisitableNodeAttributeUsageWarning, location,
                classType.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
                nodeType.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));
            return new VisitableNodeInfo(className, classNamespace) { Diagnostics = new[] { diagnostic } };
        }

        // 2. Extract information from the attribute

        var namedArgument = attribute.NamedArguments.FirstOrDefault(kvp => kvp.Key == "VisitorType");
        var customVisitorType = namedArgument.Key is not null ? (ITypeSymbol?) namedArgument.Value.Value : null;

        namedArgument = attribute.NamedArguments.FirstOrDefault(kvp => kvp.Key == "ChildProperties");
        var childPropertyNames = namedArgument.Key is not null ? namedArgument.Value.Values.Select(value => (string) value.Value!).ToArray() : Array.Empty<string>();

        namedArgument = attribute.NamedArguments.FirstOrDefault(kvp => kvp.Key == "SealOverrideMethods");
        var sealOverrideMethods = namedArgument.Key is not null ? (bool) namedArgument.Value.Value! : false;

        // 3. Collect information about child properties

        var (childProperties, childPropertyInfos) = childPropertyNames.Length > 0
            ? (new IPropertySymbol[childPropertyNames.Length], new VisitableNodeChildPropertyInfo[childPropertyNames.Length])
            : (Array.Empty<IPropertySymbol>(), Array.Empty<VisitableNodeChildPropertyInfo>());
        List<Diagnostic>? diagnostics = null;

        ISymbol? member;
        for (var i = 0; i < childPropertyNames.Length; i++)
        {
            var propertyName = childPropertyNames[i];
            member = classType.GetBaseTypes()
                .Prepend(classType)
                .SelectMany(type => type.GetMembers(propertyName))
                .FirstOrDefault();

            if (member is null
                || member is not IPropertySymbol property)
            {
                var diagnostic = Diagnostic.Create(Diagnostics.PropertyNotFoundError, classDeclarationSyntax.GetLocation(),
                    classType.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
                    propertyName);
                (diagnostics ??= new List<Diagnostic>(capacity: 1)).Add(diagnostic);
                continue;
            }

            static ITypeSymbol? GetChildNodeType(IPropertySymbol property, INamedTypeSymbol nodeType, INamedTypeSymbol nodeListType, out bool isNodeList)
            {
                if (property.Type is INamedTypeSymbol { IsGenericType: true } namedType
                    && SymbolEqualityComparer.Default.Equals(namedType.ConstructUnboundGenericType(), nodeListType))
                {
                    isNodeList = true;
                    return property.ReturnsByRefReadonly ? namedType.TypeArguments[0] : null;
                }

                isNodeList = false;
                return property.RefKind == RefKind.None && property.Type.InheritsFromOrIsSameAs(nodeType) ? property.Type : null;
            }

            ITypeSymbol? childNodeType;
            if (property.GetMethod is null
                || (childNodeType = GetChildNodeType(property, nodeType, nodeListType, out var isNodeList)) is null)
            {
                var location = property.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(cancellationToken).GetLocation() ?? Location.None;
                var diagnostic = Diagnostic.Create(Diagnostics.InvalidVisitableNodeChildNodePropertyError, location,
                    classType.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
                    propertyName,
                    nodeType.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
                    nodeListType.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));
                (diagnostics ??= new List<Diagnostic>(capacity: 1)).Add(diagnostic);
                continue;
            }

            childPropertyInfos[i] = new VisitableNodeChildPropertyInfo(property.Name, propertyTypeFullName: property.Type.GetFullNameWithNullability())
            {
                IsOptional = childNodeType.NullableAnnotation == NullableAnnotation.Annotated,
                IsNodeList = isNodeList,
                IsRefReadonly = property.ReturnsByRefReadonly
            };
            childProperties[i] = property;
        }

        if (diagnostics is not null)
        {
            return new VisitableNodeInfo(className, classNamespace) { Diagnostics = diagnostics.ToArray() };
        }

        // 4. Determine which methods to generate

        // NOTE: SymbolEqualityComparer.Default.Equals doesn't care whether the compared types are by-ref or not.

        var generateNextChildNodeMethod = !classType.GetMembers("NextChildNode")
            .OfType<IMethodSymbol>()
            .Where(method => method.Parameters.Length == 1
                && method.Parameters[0] is { RefKind: RefKind.Ref } param
                && SymbolEqualityComparer.Default.Equals(param.Type, childNodesEnumeratorType))
            .Any();

        var visitorType = customVisitorType ?? astVisitorType;
        var generateAcceptMethod = !classType.GetMembers("Accept")
            .OfType<IMethodSymbol>()
            .Where(method => method.Parameters.Length == 1
                && method.Parameters[0] is { RefKind: RefKind.None } param
                && SymbolEqualityComparer.Default.Equals(param.Type, visitorType))
            .Any();

        static bool IsMatchingUpdateWithMethodSignature(IMethodSymbol method, IPropertySymbol[] childProperties)
        {
            return method.Parameters.Length == childProperties.Length
                && method.Parameters
                    .Zip(childProperties, (param, property) =>
                        param.RefKind == (property.ReturnsByRefReadonly ? RefKind.In : RefKind.None)
                        && SymbolEqualityComparer.Default.Equals(param.Type, property.Type))
                    .All(isMatchingParam => isMatchingParam);
        }

        var generateUpdateWithMethod = childProperties.Length > 0
            && !classType.GetMembers("UpdateWith")
                .OfType<IMethodSymbol>()
                .Where(method => IsMatchingUpdateWithMethodSignature(method, childProperties))
                .Any();

        return new VisitableNodeInfo(className, classNamespace)
        {
            VisitorTypeFullName = customVisitorType?.GetFullName(),
            ChildPropertyInfos = childPropertyInfos,
            SealOverrideMethods = sealOverrideMethods,
            GenerateNextChildNodeMethod = generateNextChildNodeMethod,
            GenerateAcceptMethod = generateAcceptMethod,
            GenerateUpdateWithMethod = generateUpdateWithMethod,
        };
    }

    private static void Execute(SourceProductionContext context, StructuralEqualityWrapper<Diagnostic[]> compilationDiagnostics, ImmutableArray<VisitableNodeInfo> visitableNodeInfos)
    {
        var diagnostics = compilationDiagnostics.Target
            .Concat(visitableNodeInfos.SelectMany(nodeInfo => nodeInfo.Diagnostics))
            .ToArray();

        if (diagnostics.Length > 0)
        {
            var hasError = false;

            foreach (var diagnostic in diagnostics)
            {
                context.ReportDiagnostic(diagnostic);
                hasError = hasError || diagnostic.Severity >= DiagnosticSeverity.Error;
            }

            if (hasError)
            {
                return;
            }
        }

        var nodeGroupsByNamespace = visitableNodeInfos
            .Where(nodeInfo => !nodeInfo.Diagnostics.Any(diagnostic => diagnostic.Severity >= DiagnosticSeverity.Warning))
            .GroupBy(nodeInfo => nodeInfo.ClassNamespace);

        var sb = new StringBuilder();

        foreach (var nodesByNamespace in nodeGroupsByNamespace)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var @namespace = nodesByNamespace.Key;

            GenerateVisitableNodeClasses(sb, @namespace, nodesByNamespace, context.CancellationToken);

            context.AddSource($"{@namespace ?? "[global]"}.VisitableNodes.g.cs", sb.ToString());

            sb.Clear();
        }

        GenerateChildNodeEnumeratorHelpers(sb, visitableNodeInfos, context.CancellationToken);

        context.AddSource($"ChildNodes.Helpers.g.cs", sb.ToString());
    }

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
        foreach (var nodeInfo in nodeInfos.OrderBy(nodeInfo => nodeInfo.ClassName))
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

        sb.AppendIndent(indentionLevel).AppendLine($"partial class {nodeInfo.ClassName}");
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
        var visitorTypeFullName = nodeInfo.VisitorTypeFullName ?? AstVisitorCSharpTypeName;
        var nodeName = nodeInfo.ClassName;

        sb.AppendLine($"    protected internal {@sealed}override object? Accept({visitorTypeFullName} visitor) => visitor.Visit{nodeName}(this);");
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
        sb.AppendIndent(indentionLevel).Append($"public {nodeInfo.ClassName} UpdateWith(");

        string? paramSeparator = null;
        foreach (var propertyInfo in nodeInfo.ChildPropertyInfos)
        {
            sb.Append(paramSeparator);
            paramSeparator = ", ";

            if (propertyInfo.IsRefReadonly)
            {
                sb.Append("in ");
            }

            sb.Append(propertyInfo.PropertyTypeFullName).Append(" ").Append(propertyInfo.VariableName);
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

    private sealed class ChildNodeEnumeratorHelperMethodSignatureEqualityComparer : IEqualityComparer<IChildNodeEnumeratorHelperParamInfo[]>
    {
        public bool Equals(IChildNodeEnumeratorHelperParamInfo[] x, IChildNodeEnumeratorHelperParamInfo[] y)
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

        public int GetHashCode(IChildNodeEnumeratorHelperParamInfo[] obj)
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

    private static void GenerateChildNodeEnumeratorHelpers(StringBuilder sb, IEnumerable<VisitableNodeInfo> nodeInfos, CancellationToken cancellationToken)
    {
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine("namespace Esprima.Ast;");
        sb.AppendLine();

        var indentationLevel = 0;
        sb.AppendIndent(indentationLevel).AppendLine("partial struct ChildNodes");
        sb.AppendIndent(indentationLevel).AppendLine("{");
        indentationLevel++;

        sb.AppendIndent(indentationLevel).AppendLine("partial struct Enumerator");
        sb.AppendIndent(indentationLevel).AppendLine("{");
        indentationLevel++;

        var methodSignatures = nodeInfos
            .Where(nodeInfo => nodeInfo.GenerateNextChildNodeMethod && nodeInfo.ChildPropertyInfos.Length > 0)
            .Select(nodeInfo => nodeInfo.ChildPropertyInfos)
            .Distinct(new ChildNodeEnumeratorHelperMethodSignatureEqualityComparer())
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

            AppendChildNodesEnumeratorHelperMethod(sb, methodSignature, ref indentationLevel);
        }

        indentationLevel--;
        sb.AppendIndent(indentationLevel).AppendLine("}");

        indentationLevel--;
        sb.AppendIndent(indentationLevel).AppendLine("}");
    }


    private static void AppendChildNodesEnumeratorHelperMethod(StringBuilder sb, IChildNodeEnumeratorHelperParamInfo[] methodSignature, ref int indentionLevel)
    {
        // internal partial Node? MoveNext(Node arg0)
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

    private static void AppendChildNodesEnumeratorHelperMethodName(StringBuilder sb, IChildNodeEnumeratorHelperParamInfo[] methodSignature)
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

    private static void AppendChildNodesEnumeratorHelperMethodBody(StringBuilder sb, IChildNodeEnumeratorHelperParamInfo[] methodSignature, ref int indentionLevel)
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

internal sealed record class VisitableNodeInfo
{
    public VisitableNodeInfo(string className, string? classNamespace)
    {
        ClassName = className;
        ClassNamespace = classNamespace;
    }

    public string ClassName { get; }
    public string? ClassNamespace { get; }

    public string? VisitorTypeFullName { get; init; }

    private StructuralEqualityWrapper<VisitableNodeChildPropertyInfo[]> _childProperties = Array.Empty<VisitableNodeChildPropertyInfo>();
    public VisitableNodeChildPropertyInfo[] ChildPropertyInfos { get => _childProperties.Target; init => _childProperties = value; }

    public bool SealOverrideMethods { get; init; }

    public bool GenerateNextChildNodeMethod { get; init; }
    public bool GenerateAcceptMethod { get; init; }
    public bool GenerateUpdateWithMethod { get; init; }

    private StructuralEqualityWrapper<Diagnostic[]> _diagnostics = Array.Empty<Diagnostic>();
    public Diagnostic[] Diagnostics { get => _diagnostics.Target; init => _diagnostics = value; }
}

internal sealed record class VisitableNodeChildPropertyInfo : IChildNodeEnumeratorHelperParamInfo
{
    public VisitableNodeChildPropertyInfo(string propertyName, string propertyTypeFullName)
    {
        PropertyName = propertyName;
        PropertyTypeFullName = propertyTypeFullName;
        VariableName = CodeAnalysisHelper.MakeValidVariableName(PropertyName.Length > 0
            ? char.ToLowerInvariant(PropertyName[0]) + PropertyName.Substring(1)
            : string.Empty);
    }

    public string PropertyName { get; }

    public string PropertyTypeFullName { get; }

    public bool IsOptional { get; init; }

    public string VariableName { get; }

    public bool IsNodeList { get; init; }

    public bool IsRefReadonly { get; init; }
}

internal interface IChildNodeEnumeratorHelperParamInfo
{
    bool IsOptional { get; }
    bool IsNodeList { get; }
}
