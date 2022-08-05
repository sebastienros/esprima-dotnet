using System.Runtime.CompilerServices;

namespace Esprima.Utils;

internal static class EnumHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TEnum ToFlag<TEnum>(this bool value, TEnum flag) where TEnum : struct, Enum =>
        value.ToFlag(flag, default);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TEnum ToFlag<TEnum>(this bool value, TEnum flag, TEnum fallbackFlag) where TEnum : struct, Enum =>
        value ? flag : fallbackFlag;

    // Enum.HasFlag is slow (at least, on older runtimes). However, a non-allocating, generic solution would require System.Runtime.CompilerServices.Unsafe:
    // https://github.com/dotnet/csharplang/discussions/1993#discussioncomment-104840
    // In case System.Runtime.CompilerServices.Unsafe becomes available, these methods should be replaced with a generic implementation.

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasFlagFast(this AstToJavascriptConverter.BinaryOperationFlags flags, AstToJavascriptConverter.BinaryOperationFlags flag) => (flags & flag) == flag;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasFlagFast(this AstToJavascriptConverter.StatementFlags flags, AstToJavascriptConverter.StatementFlags flag) => (flags & flag) == flag;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasFlagFast(this AstToJavascriptConverter.ExpressionFlags flags, AstToJavascriptConverter.ExpressionFlags flag) => (flags & flag) == flag;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasFlagFast(this JavascriptTextWriter.TriviaType flags, JavascriptTextWriter.TriviaType flag) => (flags & flag) == flag;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasFlagFast(this JavascriptTextWriter.TriviaFlags flags, JavascriptTextWriter.TriviaFlags flag) => (flags & flag) == flag;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasFlagFast(this JavascriptTextWriter.TokenFlags flags, JavascriptTextWriter.TokenFlags flag) => (flags & flag) == flag;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasFlagFast(this JavascriptTextWriter.StatementFlags flags, JavascriptTextWriter.StatementFlags flag) => (flags & flag) == flag;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasFlagFast(this JavascriptTextWriter.ExpressionFlags flags, JavascriptTextWriter.ExpressionFlags flag) => (flags & flag) == flag;
}
