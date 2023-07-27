using System.Globalization;
using System.Runtime.CompilerServices;

namespace Esprima;

internal static class Shims
{
#if !NETSTANDARD2_1_OR_GREATER
    public static System.Text.StringBuilder Append(this System.Text.StringBuilder sb, ReadOnlySpan<char> value)
    {
        return sb.Append(value.ToString());
    }

    public static bool TryAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
        where TKey : notnull
    {
        if (!dictionary.ContainsKey(key))
        {
            dictionary.Add(key, value);
            return true;
        }

        return false;
    }
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UnicodeCategory GetUnicodeCategory(int codePoint)
    {
#if NETSTANDARD2_1_OR_GREATER
        return CharUnicodeInfo.GetUnicodeCategory(codePoint);
#else
        return codePoint <= char.MaxValue
            ? CharUnicodeInfo.GetUnicodeCategory((char) codePoint)
            : CharUnicodeInfo.GetUnicodeCategory(char.ConvertFromUtf32(codePoint), 0);
#endif
    }
}

