﻿namespace Esprima.Ast
{
    using System;
    using System.Globalization;

    public readonly struct Range : IEquatable<Range>
    {
        public readonly int Start;
        public readonly int End;

        public Range(int start, int end)
        {
            Start = start >= 0 ? start : throw new ArgumentOutOfRangeException(nameof(start));
            End = end > start ? end : throw new ArgumentOutOfRangeException(nameof(end));
        }

        public bool Equals(Range other) =>
            Start == other.Start && End == other.End;

        public override bool Equals(object obj) =>
            obj is Range other && Equals(other);

        public override int GetHashCode() =>
            unchecked((Start * 397) ^ End);

        public override string ToString() =>
            string.Format(CultureInfo.InvariantCulture, "{0}...{1}", Start, End);

        public static bool operator ==(Range left, Range right) => left.Equals(right);
        public static bool operator !=(Range left, Range right) => !left.Equals(right);
    }
}