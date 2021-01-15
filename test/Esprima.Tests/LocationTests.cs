using Xunit;

namespace Esprima.Tests
{
    using System;

    public class LocationTests
    {
        [Theory]
        [InlineData(0, 0, 0, 0)]
        [InlineData(1, 0, 1, 0)]
        [InlineData(1, 0, 2, 0)]
        [InlineData(1, 4, 2, 0)]
        [InlineData(1, 4, 2, 5)]
        public void Construction(int startLine, int startColumn, int endLine, int endColumn)
        {
            var start = new Position(startLine, startColumn);
            var end = new Position(endLine, endColumn);
            var (actualStart, actualEnd) = new Location(start, end);
            Assert.Equal(start, actualStart);
            Assert.Equal(end, actualEnd);
        }

#if LOCATION_ASSERTS
        [Theory]
        [InlineData(0, 0, 1, 0)]
        [InlineData(1, 0, 0, 0)]
        [InlineData(2, 0, 1, 0)]
        [InlineData(1, 1, 1, 0)]
        public void InvalidStartAndEnd(int startLine, int startColumn, int endLine, int endColumn)
        {
            var start = new Position(startLine, startColumn);
            var end = new Position(endLine, endColumn);
            var e = Assert.Throws<ArgumentOutOfRangeException>(() =>
                        new Location(start, end));
            Assert.Equal("end", e.ParamName);
            Assert.Equal(end, e.ActualValue);
        }
#endif        
    }
}