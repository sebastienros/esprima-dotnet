using System;
using System.Runtime.CompilerServices;

namespace Esprima
{
    public static class ParserExtensions
    {
        private static readonly string[] charToString = new string[256];

        static ParserExtensions()
        {
            for (var i = 0; i < charToString.Length; ++i)
            {
                var c = (char) i;
                charToString[i] = c.ToString();
            }
        }

        public static string Slice(this string source, int start, int end)
        {
            var len = source.Length;
            var from = start < 0 ? Math.Max(len + start, 0) : Math.Min(start, len);
            var to = end < 0 ? Math.Max(len + end, 0) : Math.Min(end, len);
            var span = Math.Max(to - from, 0);

            // check some common cases
            if (span == 1)
            {
                return CharToString(source[from]);
            }
            if (span == 2)
            {
                if (source[from] == 'i')
                {
                    if (source[from + 1] == 'f')
                    {
                        return "if";
                    }
                    if (source[from + 1] == 'd')
                    {
                        return "id";
                    }
                    if (source[from + 1] == 'n')
                    {
                        return "in";
                    }
                }
                if (source[from] == '"' && source[from + 1] == '"')
                {
                    return "\"\"";
                }
                if (source[from] == '\'' && source[from + 1] == '\'')
                {
                    return "''";
                }
                if (source[from] == 'f' && source[from + 1] == 'n')
                {
                    return "fn";
                }
                if (source[from] == 'o' && source[from + 1] == 'n')
                {
                    return "on";
                }
                if (source[from] == 'd' && source[from + 1] == 'o')
                {
                    return "do";
                }
            }
            else if (span == 3)
            {
                if (source[from] == 'a')
                {
                    if (source[from + 1] == 'd' && source[from + 2] == 'd')
                    {
                        return "add";
                    }
                    if (source[from + 1] == 'r' && source[from + 2] == 'g')
                    {
                        return "arg";
                    }
                }
                if (source[from] == 'f' && source[from + 1] == 'o' && source[from + 2] == 'r')
                {
                    return "for";
                }
                if (source[from] == 'v' && source[from + 1] == 'a')
                {
                    if (source[from + 2] == 'l')
                    {
                        return "val";
                    }
                    if (source[from + 2] == 'r')
                    {
                        return "var";
                    }
                }
                if (source[from] == 'm')
                {
                    if (source[from + 1] == 'a')
                    {
                        if (source[from + 2] == 'p')
                        {
                            return "map";
                        }
                        if (source[from + 2] == 'x')
                        {
                            return "max";
                        }
                    }
                    if (source[from + 1] == 'i' && source[from + 2] == 'n')
                    {
                        return "min";
                    }
                }
                if (source[from] == 'n' && source[from + 1] == 'e' && source[from + 2] == 'w')
                {
                    return "new";
                }
                if (source[from] == 'k' && source[from + 1] == 'e' && source[from + 2] == 'y')
                {
                    return "key";
                }
                if (source[from] == 'l' && source[from + 1] == 'e' && source[from + 2] == 'n')
                {
                    return "len";
                }
                if (source[from] == 'o' && source[from + 1] == 'b' && source[from + 2] == 'j')
                {
                    return "obj";
                }
                if (source[from] == 'p' && source[from + 1] == 'o' && source[from + 2] == 'p')
                {
                    return "pop";
                }
                if (source[from] == 'u' && source[from + 1] == 'r' && source[from + 2] == 'l')
                {
                    return "url";
                }
                if (source[from] == 't' && source[from + 1] == 'r' && source[from + 2] == 'y')
                {
                    return "try";
                }
            }
            else if (span == 4)
            {
                if (source[from] == 't')
                {
                    if (source[from + 1] == 'e')
                    {
                        if (source[from + 2] == 'x' && source[from + 3] == 't')
                        {
                            return "text";
                        }
                        if (source[from + 2] == 's' && source[from + 3] == 't')
                        {
                            return "test";
                        }
                    }
                    if (source[from + 1] == 'r' && source[from + 2] == 'u' && source[from + 3] == 'e')
                    {
                        return "true";
                    }
                    if (source[from + 1] == 'h')
                    {
                        if (source[from + 2] == 'i' && source[from + 3] == 's')
                        {
                            return "this";
                        }
                        if (source[from + 2] == 'e' && source[from + 3] == 'n')
                        {
                            return "then";
                        }
                    }
                    if (source[from + 1] == 'y' && source[from + 2] == 'p' && source[from + 3] == 'e')
                    {
                        return "type";
                    }
                }
                if (source[from] == 'e')
                {
                    if (source[from + 1] == 'a' && source[from + 2] == 'c' && source[from + 3] == 'h')
                    {
                        return "each";
                    }

                    if (source[from + 1] == 'l')
                    {
                        if (source[from + 2] == 's' && source[from + 3] == 'e')
                        {
                            return "else";
                        }
                        if (source[from + 2] == 'e' && source[from + 3] == 'm')
                        {
                            return "elem";
                        }
                    }
                }
                if (source[from] == 'a')
                {
                    if (source[from + 1] == 'r' && source[from + 2] == 'g' && source[from + 3] == 's')
                    {
                        return "args";
                    }
                    if (source[from + 1] == 't' && source[from + 2] == 't' && source[from + 3] == 'r')
                    {
                        return "attr";
                    }
                }
                if (source[from] == 'f')
                {
                    if (source[from + 1] == 'i' && source[from + 2] == 'n' && source[from + 3] == 'd')
                    {
                        return "find";
                    }
                    if (source[from + 1] == 'r' && source[from + 2] == 'o' && source[from + 3] == 'm')
                    {
                        return "from";
                    }
                }
                if (source[from] == 'd')
                {
                    if (source[from + 1] == 'a' && source[from + 2] == 't' && source[from + 3] == 'a')
                    {
                        return "data";
                    }
                    if (source[from + 1] == 'o' && source[from + 2] == 'n' && source[from + 3] == 'e')
                    {
                        return "done";
                    }
                }
                if (source[from] == 'M' && source[from + 1] == 'a' && source[from + 2] == 't' && source[from + 3] == 'h')
                {
                    return "Math";
                }
                if (source[from] == 'p' && source[from + 1] == 'u' && source[from + 2] == 's' && source[from + 3] == 'h')
                {
                    return "push";
                }
                if (source[from] == 'k' && source[from + 1] == 'e' && source[from + 2] == 'y' && source[from + 3] == 's')
                {
                    return "keys";
                }
                if (source[from] == 'b' && source[from + 1] == 'i' && source[from + 2] == 'n' && source[from + 3] == 'd')
                {
                    return "bind";
                }
                if (source[from] == 'j' && source[from + 1] == 'o' && source[from + 2] == 'i' && source[from + 3] == 'n')
                {
                    return "join";
                }
                if (source[from] == 'c' && source[from + 1] == 'a')
                {
                    if (source[from + 2] == 's' && source[from + 3] == 'e')
                    {
                        return "case";
                    }
                    if (source[from + 2] == 'l' && source[from + 3] == 'l')
                    {
                        return "call";
                    }
                }
                if (source[from] == 'n')
                {
                    if (source[from + 1] == 'u' && source[from + 2] == 'l' && source[from + 3] == 'l')
                    {
                        return "null";
                    }
                    if (source[from + 1] == 'a' && source[from + 2] == 'm' && source[from + 3] == 'e')
                    {
                        return "name";
                    }
                    if (source[from + 1] == 'o' && source[from + 2] == 'd' && source[from + 3] == 'e')
                    {
                        return "node";
                    }
                }
                if (source[from] == 's')
                {
                    if (source[from + 1] == 'e' && source[from + 2] == 'l' && source[from + 3] == 'f')
                    {
                        return "self";
                    }
                    if (source[from + 1] == 'o' && source[from + 2] == 'r' && source[from + 3] == 't')
                    {
                        return "sort";
                    }
                }
            }
            else if (span == 5)
            {
                if (source[from] == 'a' && source[from + 1] == 'p' && source[from + 2] == 'p' && source[from + 3] == 'l' && source[from + 4] == 'y')
                {
                    return "apply";
                }
                if (source[from] == 'b' && source[from + 1] == 'r' && source[from + 2] == 'e' && source[from + 3] == 'a' && source[from + 4] == 'k')
                {
                    return "break";
                }
                if (source[from] == 'm' && source[from + 1] == 'a' && source[from + 2] == 't' && source[from + 3] == 'c' && source[from + 4] == 'h')
                {
                    return "match";
                }
                if (source[from] == 'f' && source[from + 1] == 'a' && source[from + 2] == 'l' && source[from + 3] == 's' && source[from + 4] == 'e')
                {
                    return "false";
                }
                if (source[from] == 'v' && source[from + 1] == 'a' && source[from + 2] == 'l' && source[from + 3] == 'u' && source[from + 4] == 'e')
                {
                    return "value";
                }
                if (source[from] == 'e' && source[from + 1] == 'v' && source[from + 2] == 'e')
                {
                    if (source[from + 3] == 'r' && source[from + 4] == 'y')
                    {
                        return "every";
                    }
                    if (source[from + 3] == 'n' && source[from + 4] == 't')
                    {
                        return "event";
                    }
                }
                if (source[from] == 'i' && source[from + 1] == 'n' && source[from + 2] == 'd' && source[from + 3] == 'e' && source[from + 4] == 'x')
                {
                    return "index";
                }
                if (source[from] == 'w' && source[from + 1] == 'h' && source[from + 2] == 'i' && source[from + 3] == 'l' && source[from + 4] == 'e')
                {
                    return "while";
                }
                if (source[from] == 't' && source[from + 1] == 'h' && source[from + 2] == 'r' && source[from + 3] == 'o' && source[from + 4] == 'w')
                {
                    return "throw";
                }
                if (source[from] == 'c' && source[from + 1] == 'a' && source[from + 2] == 't' && source[from + 3] == 'c' && source[from + 4] == 'h')
                {
                    return "catch";
                }
                if (source[from] == 's')
                {
                    if (source[from + 1] == 'l' && source[from + 2] == 'i' && source[from + 3] == 'c' && source[from + 4] == 'e')
                    {
                        return "slice";
                    }
                    if (source[from + 1] == 'p' && source[from + 2] == 'l' && source[from + 3] == 'i' && source[from + 4] == 't')
                    {
                        return "split";
                    }
                    if (source[from + 1] == 'h' && source[from + 2] == 'i' && source[from + 3] == 'f' && source[from + 4] == 't')
                    {
                        return "shift";
                    }
                }
                if (source[from] == 'A' && source[from + 1] == 'r' && source[from + 2] == 'r' && source[from + 3] == 'a' && source[from + 4] == 'y')
                {
                    return "Array";
                }
            }
            else if (span == 6)
            {
                if (source[from] == 'o' && source[from + 1] == 'b' && source[from + 2] == 'j' && source[from + 3] == 'e' && source[from + 4] == 'c' && source[from + 5] == 't')
                {
                    return "object";
                }
                if (source[from] == 'O' && source[from + 1] == 'b' && source[from + 2] == 'j' && source[from + 3] == 'e' && source[from + 4] == 'c' && source[from + 5] == 't')
                {
                    return "Object";
                }
                if (source[from] == 'c' && source[from + 1] == 'o' && source[from + 2] == 'n' && source[from + 3] == 'c' && source[from + 4] == 'a' && source[from + 5] == 't')
                {
                    return "concat";
                }
                if (source[from] == 'e' && source[from + 1] == 'x' && source[from + 2] == 't' && source[from + 3] == 'e' && source[from + 4] == 'n' && source[from + 5] == 'd')
                {
                    return "extend";
                }
                if (source[from] == 'f' && source[from + 1] == 'i' && source[from + 2] == 'l' && source[from + 3] == 't' && source[from + 4] == 'e' && source[from + 5] == 'r')
                {
                    return "filter";
                }
                if (source[from] == 'l' && source[from + 1] == 'e' && source[from + 2] == 'n' && source[from + 3] == 'g' && source[from + 4] == 't' && source[from + 5] == 'h')
                {
                    return "length";
                }
                if (source[from] == 'r' && source[from + 1] == 'e')
                {
                    if (source[from + 2] == 't' && source[from + 3] == 'u' && source[from + 4] == 'r' && source[from + 5] == 'n')
                    {
                        return "return";
                    }
                    if (source[from + 2] == 's' && source[from + 3] == 'u' && source[from + 4] == 'l' && source[from + 5] == 't')
                    {
                        return "result";
                    }
                }
                if (source[from] == 'j' && source[from + 1] == 'Q' && source[from + 2] == 'u' && source[from + 3] == 'e' && source[from + 4] == 'r' && source[from + 5] == 'y')
                {
                    return "jQuery";
                }
                if (source[from] == 'm' && source[from + 1] == 'o' && source[from + 2] == 'b' && source[from + 3] == 'i' && source[from + 4] == 'l' && source[from + 5] == 'e')
                {
                    return "mobile";
                }
                if (source[from] == 't' && source[from + 1] == 'y' && source[from + 2] == 'p' && source[from + 3] == 'e' && source[from + 4] == 'o' && source[from + 5] == 'f')
                {
                    return "typeof";
                }
                if (source[from] == 'w' && source[from + 1] == 'i' && source[from + 2] == 'n' && source[from + 3] == 'd' && source[from + 4] == 'o' && source[from + 5] == 'w')
                {
                    return "window";
                }
                if (source[from] == 's')
                {
                    if (source[from + 1] == 'u' && source[from + 2] == 'b' && source[from + 3] == 's' && source[from + 4] == 't' && source[from + 5] == 'r')
                    {
                        return "substr";
                    }
                    if (source[from + 1] == 'p' && source[from + 2] == 'l' && source[from + 3] == 'i' && source[from + 4] == 'c' && source[from + 5] == 'e')
                    {
                        return "splice";
                    }
                }
            }
            else if (span == 7)
            {
                if (source[from] == 'e' && source[from + 1] == 'l' && source[from + 2] == 'e' && source[from + 3] == 'm'
                    && source[from + 4] == 'e' && source[from + 5] == 'n' && source[from + 6] == 't')
                {
                    return "element";
                }
                if (source[from] == 'o' && source[from + 1] == 'p' && source[from + 2] == 't' && source[from + 3] == 'i'
                    && source[from + 4] == 'o' && source[from + 5] == 'n' && source[from + 6] == 's')
                {
                    return "options";
                }
                if (source[from] == 'r' && source[from + 1] == 'e')
                {
                    if (source[from + 2] == 'p' && source[from + 3] == 'l' && source[from + 4] == 'a' && source[from + 5] == 'c' && source[from + 6] == 'e')
                    {
                        return "replace";
                    }
                    if (source[from + 2] == 'v' && source[from + 3] == 'e' && source[from + 4] == 'r' && source[from + 5] == 's' && source[from + 6] == 'e')
                    {
                        return "reverse";
                    }
                }
                if (source[from] == 'f' && source[from + 1] == 'o' && source[from + 2] == 'r' && source[from + 3] == 'E'
                    && source[from + 4] == 'a' && source[from + 5] == 'c' && source[from + 6] == 'h')
                {
                    return "forEach";
                }
                if (source[from] == 'i' && source[from + 1] == 'n' && source[from + 2] == 'd' && source[from + 3] == 'e'
                    && source[from + 4] == 'x' && source[from + 5] == 'O' && source[from + 6] == 'f')
                {
                    return "indexOf";
                }
                if (source[from] == 'c' && source[from + 1] == 'o' && source[from + 2] == 'n' && source[from + 3] == 't'
                    && source[from + 4] == 'e' && source[from + 5] == 'x' && source[from + 6] == 't')
                {
                    return "context";
                }
                if (source[from] == 'i' && source[from + 1] == 's' && source[from + 2] == 'A' && source[from + 3] == 'r'
                    && source[from + 4] == 'r' && source[from + 5] == 'a' && source[from + 6] == 'y')
                {
                    return "isArray";
                }
                if (source[from] == 'u' && source[from + 1] == 'n' && source[from + 2] == 's' && source[from + 3] == 'h'
                    && source[from + 4] == 'i' && source[from + 5] == 'f' && source[from + 6] == 't')
                {
                    return "unshift";
                }
            }
            else if (span == 8)
            {
                if (source[from] == 'f' && source[from + 1] == 'u' && source[from + 2] == 'n' && source[from + 3] == 'c'
                    && source[from + 4] == 't' && source[from + 5] == 'i' && source[from + 6] == 'o' && source[from + 7] == 'n')
                {
                    return "function";
                }
                if (source[from] == 'd' && source[from + 1] == 'o' && source[from + 2] == 'c' && source[from + 3] == 'u'
                    && source[from + 4] == 'm' && source[from + 5] == 'e' && source[from + 6] == 'n' && source[from + 7] == 't')
                {
                    return "document";
                }
                if (source[from] == 't' && source[from + 1] == 'o' && source[from + 2] == 'S' && source[from + 3] == 't'
                    && source[from + 4] == 'r' && source[from + 5] == 'i' && source[from + 6] == 'n' && source[from + 7] == 'g')
                {
                    return "toString";
                }
                if (source[from] == 's' && source[from + 1] == 'e' && source[from + 2] == 'l' && source[from + 3] == 'e'
                    && source[from + 4] == 'c' && source[from + 5] == 't' && source[from + 6] == 'o' && source[from + 7] == 'r')
                {
                    return "selector";
                }
            }
            else if (span == 9)
            {
                if (source[from] == 'u' && source[from + 1] == 'n' && source[from + 2] == 'd' && source[from + 3] == 'e'
                    && source[from + 4] == 'f' && source[from + 5] == 'i' && source[from + 6] == 'n' && source[from + 7] == 'e' && source[from + 8] == 'd')
                {
                    return "undefined";
                }
                if (source[from] == 'p' && source[from + 1] == 'r' && source[from + 2] == 'o' && source[from + 3] == 't'
                    && source[from + 4] == 'o' && source[from + 5] == 't' && source[from + 6] == 'y' && source[from + 7] == 'p' && source[from + 8] == 'e')
                {
                    return "prototype";
                }
                if (source[from] == 's' && source[from + 1] == 'u' && source[from + 2] == 'b' && source[from + 3] == 's'
                    && source[from + 4] == 't' && source[from + 5] == 'r' && source[from + 6] == 'i' && source[from + 7] == 'n' && source[from + 8] == 'g')
                {
                    return "substring";
                }
            }
            else if (span == 10)
            {
                if (source[from] == 'i' && source[from + 1] == 's' && source[from + 2] == 'F' && source[from + 3] == 'u'
                    && source[from + 4] == 'n' && source[from + 5] == 'c' && source[from + 6] == 't' && source[from + 7] == 'i'
                    && source[from + 8] == 'o' && source[from + 9] == 'n')
                {
                    return "isFunction";
                }
            }
            else if (span == 11)
            {
                if (source[from] == 't' && source[from + 1] == 'o' && source[from + 2] == 'L' && source[from + 3] == 'o'
                    && source[from + 4] == 'w' && source[from + 5] == 'e' && source[from + 6] == 'r' && source[from + 7] == 'C'
                    && source[from + 8] == 'a' && source[from + 9] == 's' && source[from + 10] == 'e')
                {
                    return "toLowerCase";
                }
            }
            else if (span == 14)
            {
                if (source[from] == 'h' && source[from + 1] == 'a' && source[from + 2] == 's' && source[from + 3] == 'O'
                    && source[from + 4] == 'w' && source[from + 5] == 'n' && source[from + 6] == 'P' && source[from + 7] == 'r'
                    && source[from + 8] == 'o' && source[from + 9] == 'p' && source[from + 10] == 'e' && source[from + 11] == 'r'
                    && source[from + 12] == 't' && source[from + 13] == 'y')
                {
                    return "hasOwnProperty";
                }
            }

            var substring = source.Substring(from, span);
            return substring;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string CharToString(char c)
        {
            if (c >= 0 && c < charToString.Length)
            {
                return charToString[c];
            }
            return c.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static char CharCodeAt(this string source, int index)
        {
            if (index < 0 || index > source.Length - 1)
            {
                // char.MinValue is used as the null value
                return char.MinValue;
            }

            return source[index];
        }
    }
}