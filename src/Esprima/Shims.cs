namespace Esprima;

internal static class Shims
{
#if !NETSTANDARD2_1_OR_GREATER
    public static System.Text.StringBuilder Append(this System.Text.StringBuilder sb, ReadOnlySpan<char> value)
    {
        return sb.Append(value.ToString());
    }

    public static void Write(this TextWriter writer, ReadOnlySpan<char> value)
    {
        writer.Write(value.ToString());
    }
#endif
}
