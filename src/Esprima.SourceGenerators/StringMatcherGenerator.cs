using System.Collections.Immutable;
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
[Generator]
public class StringMatcherGenerator : IIncrementalGenerator
{
    // IIncrementalGenerator has an Initialize method that is called by the host exactly once,
    // regardless of the number of further compilations that may occur.
    // For instance a host with multiple loaded projects may share the same generator instance across multiple projects,
    // and will only call Initialize a single time for the lifetime of the host.
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            "Attributes.g.cs", SourceText.From(SourceGenerationHelper.Attributes, Encoding.UTF8)));

        IncrementalValuesProvider<StringMatcherMethod> helperMethodInfos = context.SyntaxProvider
            .CreateSyntaxProvider(IsSyntaxTargetForGeneration, GetSemanticTargetForGeneration)
            .Where(item => item is not null)!;

        context.RegisterSourceOutput(helperMethodInfos.Collect(), Execute);
    }

    private static bool IsSyntaxTargetForGeneration(SyntaxNode node, CancellationToken cancellationToken)
    {
        return
            node is MethodDeclarationSyntax methodDeclarationSyntax &&
            methodDeclarationSyntax.Body is null;
    }

    sealed record StringMatcherMethod(string ContainingType, string Modifiers, string Name, string[] Alternatives);

    private static StringMatcherMethod? GetSemanticTargetForGeneration(GeneratorSyntaxContext context, CancellationToken cancellationToken)
    {
        var methodDeclarationSyntax = (MethodDeclarationSyntax) context.Node;
        var method = context.SemanticModel.GetDeclaredSymbol(methodDeclarationSyntax, cancellationToken);

        if (method is null ||
            !method.IsPartialDefinition ||
            method.DeclaringSyntaxReferences.Length != 1 ||
            method.GetAttributes().FirstOrDefault()?.AttributeClass?.Name != "StringMatcherAttribute")
        {
            return null;
        }

        var attribute = method.GetAttributes()[0];
        var attributeSyntax = (AttributeSyntax) attribute.ApplicationSyntaxReference!.GetSyntax();


        var targets = new List<string>();
        var namedAttributes = new Dictionary<string, string>();
        foreach (var item in attributeSyntax.ChildNodes())
        {
            if (item is AttributeArgumentListSyntax arguments)
            {
                foreach (var ag in arguments.Arguments)
                {
                    if (ag.NameEquals is null)
                    {
                        targets.Add(ag.Expression.ToString().Trim('"'));
                    }
                    else
                    {
                        namedAttributes.Add(ag.NameEquals.Name.NormalizeWhitespace().ToFullString(), ag.Expression.ChildTokens().FirstOrDefault().Value?.ToString() ?? "");
                    }
                }
            }
        }

        return new StringMatcherMethod(method.ContainingType.Name, methodDeclarationSyntax.Modifiers.ToString(), method.Name, targets.ToArray());
    }

    private static void Execute(SourceProductionContext context, ImmutableArray<StringMatcherMethod> methods)
    {
        var sourceBuilder = new StringBuilder();

        sourceBuilder
            .AppendLine("#nullable enable")
            .AppendLine()
            .AppendLine("namespace Esprima;")
            .AppendLine();

        var indent = new string(' ', 4);

        foreach (var typeGrouping in methods.GroupBy(x => x.ContainingType))
        {
            sourceBuilder.Append("public partial class ").AppendLine(typeGrouping.Key)
                .AppendLine("{");

            foreach (var method in methods)
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                sourceBuilder.Append(indent);
                sourceBuilder.Append(method.Modifiers).Append(" bool ").Append(method.Name).AppendLine("(string? input)");
                sourceBuilder.Append(indent).AppendLine("{");
                sourceBuilder.Append(indent).Append(indent).AppendLine("if (input is null)");
                sourceBuilder.Append(indent).Append(indent).AppendLine("{");
                sourceBuilder.Append(indent).Append(indent).Append(indent).AppendLine("return false;");
                sourceBuilder.Append(indent).Append(indent).AppendLine("}");
                sourceBuilder.Append(indent).Append(indent).Append(SourceGenerationHelper.GenerateLookups(method.Alternatives, 8));
                sourceBuilder.Append(indent).AppendLine("}");
                sourceBuilder.AppendLine();
            }

            sourceBuilder.AppendLine("}");
            sourceBuilder.AppendLine();
        }

        context.AddSource("StringMatchers.g.cs", sourceBuilder.ToString());
    }
}
