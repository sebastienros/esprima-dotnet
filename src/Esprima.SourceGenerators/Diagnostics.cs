﻿using Microsoft.CodeAnalysis;

namespace Esprima.SourceGenerators;

internal static class Diagnostics
{
    public static readonly DiagnosticDescriptor TypeNotFoundError = new(
        id: "ESP0001",
        title: $"Type not found",
        messageFormat: "Type '{0}' does not exist in the compilation",
        category: "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor PropertyNotFoundError = new(
        id: "ESP0002",
        title: $"Property not found",
        messageFormat: "Type '{0}' does not contain a property named '{1}'.",
        category: "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidVisitableNodeAttributeUsageWarning = new(
        id: "ESP0003",
        title: $"Invalid 'VisitableNodeAttribute' usage",
        messageFormat: "Class '{0}' must be non-generic, partial and must inherit from '{1}' when it is annotated with 'Esprima.Ast.VisitableNodeAttribute'.",
        category: "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidVisitableNodeChildNodePropertyError = new(
        id: "ESP0004",
        title: $"Invalid child node property",
        messageFormat: "Property '{1}' of class '{0}' must be readable and must have a return type which is either a type inheriting from '{2}' or type '{3}' returned by readonly reference.",
        category: "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);
}
