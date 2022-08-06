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
    public static bool HasFlagFast(this AstToJavaScriptConverter.BinaryOperationFlags flags, AstToJavaScriptConverter.BinaryOperationFlags flag) => (flags & flag) == flag;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasFlagFast(this AstToJavaScriptConverter.StatementFlags flags, AstToJavaScriptConverter.StatementFlags flag) => (flags & flag) == flag;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasFlagFast(this AstToJavaScriptConverter.ExpressionFlags flags, AstToJavaScriptConverter.ExpressionFlags flag) => (flags & flag) == flag;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasFlagFast(this JavaScriptTextWriter.TriviaType flags, JavaScriptTextWriter.TriviaType flag) => (flags & flag) == flag;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasFlagFast(this JavaScriptTextWriter.TriviaFlags flags, JavaScriptTextWriter.TriviaFlags flag) => (flags & flag) == flag;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasFlagFast(this JavaScriptTextWriter.TokenFlags flags, JavaScriptTextWriter.TokenFlags flag) => (flags & flag) == flag;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasFlagFast(this JavaScriptTextWriter.StatementFlags flags, JavaScriptTextWriter.StatementFlags flag) => (flags & flag) == flag;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasFlagFast(this JavaScriptTextWriter.ExpressionFlags flags, JavaScriptTextWriter.ExpressionFlags flag) => (flags & flag) == flag;
}
