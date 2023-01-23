using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Esprima.SourceGenerators.Helpers;

/// <summary>
/// Helpers from mostly based Andrew Lock's work: https://andrewlock.net/series/creating-a-source-generator/
/// </summary>
internal static class CodeAnalysisHelper
{
    // determine the namespace the class/enum/struct is declared in, if any
    public static string GetNamespace(this BaseTypeDeclarationSyntax syntax)
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

    public static Dictionary<string, string> GetAttributes(this AttributeSyntax syntax)
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

    private static readonly SymbolDisplayFormat s_fullyQualifiedFormatWithoutAlias = new(
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
        miscellaneousOptions:
            SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
            SymbolDisplayMiscellaneousOptions.UseSpecialTypes);

    public static string GetFullName(this ITypeSymbol type)
    {
        return type.ToDisplayString(s_fullyQualifiedFormatWithoutAlias);
    }

    private static readonly SymbolDisplayFormat s_fullyQualifiedFormatWithoutAliasWithNullability = new(
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
        miscellaneousOptions:
            SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
            SymbolDisplayMiscellaneousOptions.UseSpecialTypes |
            SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

    public static string GetFullNameWithNullability(this ITypeSymbol type)
    {
        return type.ToDisplayString(s_fullyQualifiedFormatWithoutAliasWithNullability);
    }


    public static IEnumerable<INamedTypeSymbol> GetBaseTypes(this ITypeSymbol type)
    {
        for (var current = type.BaseType; current is not null; current = current.BaseType)
        {
            yield return current;
        }
    }

    public static bool InheritsFrom(this ITypeSymbol type, ITypeSymbol baseType)
    {
        return type.GetBaseTypes().Any(type => SymbolEqualityComparer.Default.Equals(type, baseType));
    }

    public static bool InheritsFromOrIsSameAs(this ITypeSymbol type, ITypeSymbol baseType)
    {
        return SymbolEqualityComparer.Default.Equals(type, baseType) || type.InheritsFrom(baseType);
    }

    public static string MakeValidVariableName(string value)
    {
        return SyntaxFacts.GetKeywordKind(value) == SyntaxKind.None
            ? value
            : "@" + value;
    }
}
