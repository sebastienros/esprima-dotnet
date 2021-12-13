using System;
using System.Text;

namespace Esprima;

internal static class Shims
{
#if NETFRAMEWORK || NETSTANDARD2_0
    public static StringBuilder Append(this StringBuilder sb, ReadOnlySpan<char> value)
    {
        return sb.Append(value.ToString());
    }
#endif
}
