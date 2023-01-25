using System.Text;
using Microsoft.CodeAnalysis;

namespace Esprima.SourceGenerators.Helpers;

/// <summary>
/// Cacheable data structure which helps dealing with C# type names in source generators.
/// </summary>
internal record class CSharpTypeName(CSharpTypeBareName BareName)
{
    public static CSharpTypeName? From(ITypeSymbol type)
    {
        (int, bool)[] arrayDescriptors;
        if (type is IArrayTypeSymbol arrayType)
        {
            var arrayDescriptorList = new List<(int, bool)>(capacity: 1);
            do
            {
                arrayDescriptorList.Add((arrayType.Rank, arrayType.NullableAnnotation == NullableAnnotation.Annotated));
                type = arrayType.ElementType;
                arrayType = (type as IArrayTypeSymbol)!;
            }
            while (arrayType is not null);
            arrayDescriptors = arrayDescriptorList.ToArray();
        }
        else
        {
            arrayDescriptors = Array.Empty<(int, bool)>();
        }

        if (CSharpTypeBareName.From(type) is not { } bareName)
        {
            return null;
        }

        return new CSharpTypeName(bareName)
        {
            Namespace = type.TypeKind != TypeKind.TypeParameter ? type.ContainingNamespace?.ToString() : null,
            ArrayDescriptors = arrayDescriptors,
            TypeKind = type.TypeKind,
            IsNullable = type.IsValueType
                ? type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T
                : type.NullableAnnotation == NullableAnnotation.Annotated,
        };
    }

    public string TypeName => BareName.TypeName;

    public string? Namespace { get; init; }

    private StructuralEqualityWrapper<(int Rank, bool IsNullable)[]> _arrayDescriptors = Array.Empty<(int, bool)>();
    public (int Rank, bool IsNullable)[] ArrayDescriptors { get => _arrayDescriptors.Target; init => _arrayDescriptors = value; }

    public bool IsArray => ArrayDescriptors.Length > 0;

    public TypeKind TypeKind { get; init; }

    public bool IsNullable { get; init; }

    public CSharpTypeName ToNonGeneric()
    {
        return BareName.IsGeneric
            ? this with
            {
                BareName = BareName with { GenericArguments = Array.Empty<CSharpTypeName>() }
            }
            : this;
    }

    public void AppendTo(StringBuilder sb, Predicate<CSharpTypeName> includeNamespace)
    {
        var isNestedType = false;

        var bareName = BareName;

        if (TypeKind == TypeKind.Struct && IsNullable)
        {
            bareName = bareName.GenericArguments[0].BareName;
        }

        if (bareName.SpecialTypeName is null)
        {
            if (Namespace is not null && includeNamespace(this))
            {
                sb.Append(Namespace).Append('.');
            }

            if (bareName.IsNested)
            {
                var fragmentStack = new Stack<CSharpTypeBareName>(capacity: 2);
                fragmentStack.Push(bareName);

                bareName = bareName.Container!;
                do
                {
                    fragmentStack.Push(bareName);
                    bareName = bareName.Container;
                }
                while (bareName is not null);

                while (fragmentStack.Count > 0)
                {
                    bareName = fragmentStack.Pop();

                    if (!isNestedType)
                        isNestedType = true;
                    else
                        sb.Append('.');

                    bareName.AppendTo(sb, includeNamespace);
                }
            }
            else
            {
                bareName.AppendTo(sb, includeNamespace);
            }
        }
        else
        {
            sb.Append(bareName.SpecialTypeName);
        }

        if (IsNullable)
        {
            sb.Append('?');
        }

        if (IsArray)
        {
            var arrayDimensions = ArrayDescriptors;
            for (var i = 0; i < arrayDimensions.Length; i++)
            {
                sb.Append('[');
                var (rank, isNullable) = arrayDimensions[i];
                if (--rank > 0)
                    sb.Append(',', rank);
                sb.Append(']');
                if (isNullable)
                {
                    sb.Append('?');
                }
            }
        }
    }

    public override string ToString()
    {
        return new StringBuilder().AppendTypeName(this).ToString();
    }
}

internal record class CSharpTypeBareName(string TypeName)
{
    private static bool HasSpecialTypeName(INamedTypeSymbol namedType)
    {
        return namedType.SpecialType switch
        {
            SpecialType.None => false,
            SpecialType.System_Object or
            SpecialType.System_Void or
            SpecialType.System_Boolean or
            SpecialType.System_Char or
            SpecialType.System_SByte or
            SpecialType.System_Byte or
            SpecialType.System_Int16 or
            SpecialType.System_UInt16 or
            SpecialType.System_Int32 or
            SpecialType.System_UInt32 or
            SpecialType.System_Int64 or
            SpecialType.System_UInt64 or
            SpecialType.System_Decimal or
            SpecialType.System_Single or
            SpecialType.System_Double or
            SpecialType.System_String =>
                true,
            SpecialType.System_IntPtr or
            SpecialType.System_UIntPtr when namedType.IsNativeIntegerType =>
                true,
            _ =>
                false
        };
    }

    public static CSharpTypeBareName? From(ITypeSymbol type)
    {
        switch (type.TypeKind)
        {
            case TypeKind.Class:
            case TypeKind.Delegate:
            case TypeKind.Enum:
            case TypeKind.Interface:
            case TypeKind.Struct:
                var namedType = (INamedTypeSymbol) type;

                CSharpTypeBareName? container;
                if (type.ContainingType is null)
                {
                    container = null;
                }
                else if ((container = From(type.ContainingType)) is null)
                {
                    return null;
                }

                CSharpTypeName[] genericArguments;
                if (namedType.IsGenericType)
                {
                    genericArguments = new CSharpTypeName[namedType.TypeArguments.Length];
                    for (int i = 0, n = namedType.TypeArguments.Length; i < n; i++)
                    {
                        if (CSharpTypeName.From(namedType.TypeArguments[i]) is not { } genericArgument)
                        {
                            return null;
                        }
                        genericArguments[i] = genericArgument;
                    }
                }
                else
                {
                    genericArguments = Array.Empty<CSharpTypeName>();
                }

                return new CSharpTypeBareName(type.Name)
                {
                    Container = container,
                    GenericArguments = genericArguments,
                    SpecialTypeName = HasSpecialTypeName(namedType)
                        ? namedType.ToDisplayString(NullableFlowState.NotNull, SymbolDisplayFormat.MinimallyQualifiedFormat)
                        : null,
                };
            case TypeKind.TypeParameter:
                return new CSharpTypeBareName(type.Name);
            default:
                return null;
        }
    }

    public string? SpecialTypeName { get; init; }

    public CSharpTypeBareName? Container { get; init; }

    public bool IsNested => Container is not null;

    private StructuralEqualityWrapper<CSharpTypeName[]> _genericArguments = Array.Empty<CSharpTypeName>();
    public CSharpTypeName[] GenericArguments { get => _genericArguments.Target; init => _genericArguments = value; }

    public bool IsGeneric => GenericArguments.Length > 0;

    public void AppendTo(StringBuilder sb, Predicate<CSharpTypeName> includeNamespace)
    {
        sb.Append(TypeName);

        if (IsGeneric)
        {
            sb.Append('<');
            var genericArguments = GenericArguments;
            genericArguments[0]?.AppendTo(sb, includeNamespace);
            for (var i = 1; i < genericArguments.Length; i++)
            {
                sb.Append(',');
                if (genericArguments[i] is { } genericArgument)
                {
                    sb.Append(' ');
                    genericArgument.AppendTo(sb, includeNamespace);
                }
            }
            sb.Append('>');
        }
    }

    public override string ToString()
    {
        return new StringBuilder().AppendTypeBareName(this).ToString();
    }
}
