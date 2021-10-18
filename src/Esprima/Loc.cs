using System;
using System.Diagnostics;

namespace Esprima
{
    [DebuggerDisplay("{ToString()}")]
    public readonly struct Location : IEquatable<Location>
    {
        public Position Start { get; }
        public Position End { get; }
        public string? Source { get; }

        public Location(Position start, Position end) : this(start, end, null)
        {
        }

        public Location(in Position start, in Position end, string? source)
        {
#if LOCATION_ASSERTS
            if (start == default && end != default
                || end == default && start != default
                || end.Line < start.Line
                || start.Line > 0 && start.Line == end.Line && end.Column < start.Column)
            {
                EsprimaExceptionHelper.ThrowArgumentOutOfRangeException(nameof(end), end, Exception<ArgumentOutOfRangeException>.DefaultMessage);
            }
#endif

            Start = start;
            End = end;
            Source = source;
        }

        public Location WithPosition(in Position start, in Position end)
        {
            return new Location(start, end, Source);
        }

        public override bool Equals(object obj)
        {
            return obj is Location other && Equals(other);
        }

        public bool Equals(Location other)
        {
            return Start.Equals(other.Start)
                   && End.Equals(other.End)
                   && string.Equals(Source, other.Source);
        }

        public override int GetHashCode()
        {
            return unchecked((Start.GetHashCode() * 397)
                             ^ (End.GetHashCode() * 397)
                             ^ (Source?.GetHashCode() ?? 0));
        }

        public override string ToString()
        {
            return $"{Start}...{End}{(Source is string s ? ": " + s : null)}";
        }

        public static bool operator ==(in Location left, in Location right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(in Location left, in Location right)
        {
            return !left.Equals(right);
        }

        public void Deconstruct(out Position start, out Position end)
        {
            start = Start;
            end = End;
        }

        public void Deconstruct(out Position start, out Position end, out string? source)
        {
            start = Start;
            end = End;
            source = Source;
        }
    }
}
