using System.Diagnostics;
using static Esprima.EsprimaExceptionHelper;

namespace Esprima
{
    using System;

    [DebuggerDisplay("{ToString()}")]
    public readonly struct Location : IEquatable<Location>
    {
        public Position Start  { get; }
        public Position End    { get; }
        public string?   Source { get; }

        public Location(Position start, Position end) : this(start, end, null)
        {
        }

        public Location(Position start, Position end, string? source)
        {
            Start  = start;

            End    = start == default && end != default
                     || end == default && start != default
                     || end.Line < start.Line
                     || start.Line > 0 && start.Line == end.Line && end.Column < start.Column
                   ? ThrowArgumentOutOfRangeException<Position>(nameof(end), end, Exception<ArgumentOutOfRangeException>.DefaultMessage)
                   : end;
            Source = source;
        }

        public Location WithPosition(Position start, Position end) =>
            new Location(start, end, Source);

        public override bool Equals(object obj) =>
            obj is Location other && Equals(other);

        public bool Equals(Location other) =>
            Start.Equals(other.Start)
            && End.Equals(other.End)
            && string.Equals(Source, other.Source);

        public override int GetHashCode() =>
            unchecked(  (Start.GetHashCode() * 397)
                      ^ (End.GetHashCode() * 397)
                      ^ (Source?.GetHashCode() ?? 0));

        public override string ToString() =>
            $"{Start}...{End}{(Source is string s ? ": " + s : null)}";

        public static bool operator ==(Location left, Location right) => left.Equals(right);
        public static bool operator !=(Location left, Location right) => !left.Equals(right);

        public void Deconstruct(out Position start, out Position end)
        {
            start = Start;
            end   = End;
        }

        public void Deconstruct(out Position start, out Position end, out string? source)
        {
            start  = Start;
            end    = End;
            source = Source;
        }
    }
}
