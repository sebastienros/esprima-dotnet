using System;
using System.Collections.Generic;

namespace Esprima
{
    public static class ParserExtensions
    {
        public static string Slice(this string source, int start, int end)
        {
            var len = source.Length;
            var from = start < 0 ? Math.Max(len + start, 0) : Math.Min(start, len);
            var to = end < 0 ? Math.Max(len + end, 0) : Math.Min(end, len);
            var span = Math.Max(to - from, 0);

            return source.Substring(from, span);
        }

        public static char CharCodeAt(this string source, int index)
        {
            if (index < 0 || index > source.Length - 1)
            {
                // char.MinValue is used as the null value
                return char.MinValue;
            }

            return source[index];
        }

        public static T Pop<T>(this List<T> list)
        {
            var lastIndex = list.Count - 1;
            var last = list[lastIndex];
            list.RemoveAt(lastIndex);
            return last;
        }

        public static void Push<T>(this List<T> list, T item)
        {
            list.Add(item);
        }
    }
}