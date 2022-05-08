namespace Esprima;

internal static class Shims
{
#if !NETSTANDARD2_1_OR_GREATER
    public static System.Text.StringBuilder Append(this System.Text.StringBuilder sb, ReadOnlySpan<char> value)
    {
        return sb.Append(value.ToString());
    }
#endif
}
