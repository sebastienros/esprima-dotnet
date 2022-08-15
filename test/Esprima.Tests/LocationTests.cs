namespace Esprima.Tests;

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
        var (actualStart, actualEnd) = new Location(in start, in end);
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
        var e = Assert.Throws<System.ArgumentOutOfRangeException>(() =>
            new Location(in start, in end));
        Assert.Equal("end", e.ParamName);
        Assert.Equal(end, e.ActualValue);
    }
#endif

    [Theory]
    [InlineData(1, 2, 1, 2, null, "[1,2)")]
    [InlineData(1, 2, 1, 5, null, "[1,2..5)")]
    [InlineData(1, 0, 5, 0, null, "[1..5,0)")]
    [InlineData(1, 0, 5, 5, "foo.js", "[1,0..5,5): foo.js")]
    public void ToStringTest(int startLine, int startColumn, int endLine, int endColumn, string? source, string expected)
    {
        var start = new Position(startLine, startColumn);
        var end = new Position(endLine, endColumn);
        var location = new Location(in start, in end, source);
        Assert.Equal(expected, location.ToString());
    }
}
