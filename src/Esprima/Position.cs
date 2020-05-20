using static Esprima.EsprimaExceptionHelper;

namespace Esprima
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Represents a source position as line number and column offset, where
    /// the first line is 1 and first column is 0.
    /// </summary>
    /// <remarks>
    /// A position where <see cref="Line"/> and <see cref="Column"/> are zero
    /// is an allowed (and the default) value but considered an invalid
    /// position.
    /// </remarks>

    public readonly struct Position : IEquatable<Position>
    {
        public int Line   { get; }
        public int Column { get; }

        public Position(int line, int column)
        {
            Line = line >= 0 ? line
                 : ThrowArgumentOutOfRangeException<int>(nameof(line), line, Exception<ArgumentOutOfRangeException>.DefaultMessage);

            Column = line > 0 && column >= 0
                     || line == 0 && column == 0 // if line is 0 then column MUST BE 0!
                   ? column
                   : ThrowArgumentOutOfRangeException<int>(nameof(column), column, Exception<ArgumentOutOfRangeException>.DefaultMessage);
        }

        public override bool Equals(object obj) =>
            obj is Position other && Equals(other);

        public bool Equals(Position other) =>
            Line == other.Line && Column == other.Column;

        public override int GetHashCode() =>
            unchecked((Line * 397) ^ Column);

        public override string ToString()
            => Line.ToString(CultureInfo.InvariantCulture)
             + ","
             + Column.ToString(CultureInfo.InvariantCulture);

        public static bool operator ==(Position left, Position right) => left.Equals(right);
        public static bool operator !=(Position left, Position right) => !left.Equals(right);

        public void Deconstruct(out int line, out int column)
        {
            line = Line;
            column = Column;
        }
    }
}
