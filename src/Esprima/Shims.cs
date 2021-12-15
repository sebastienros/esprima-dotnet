using System;
using System.Text;

namespace Esprima;

internal static class Shims
{
#if !NETSTANDARD2_1_OR_GREATER
    public static StringBuilder Append(this StringBuilder sb, ReadOnlySpan<char> value)
    {
        return sb.Append(value.ToString());
    }
#endif
}
