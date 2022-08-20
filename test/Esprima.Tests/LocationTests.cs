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
        var start = Position.From(startLine, startColumn);
        var end = Position.From(endLine, endColumn);
        var (actualStart, actualEnd) = Location.From(in start, in end);
        Assert.Equal(start, actualStart);
        Assert.Equal(end, actualEnd);
    }

    [Theory]
    [InlineData(0, 0, 1, 0, "start")]
    [InlineData(1, 0, 0, 0, "end")]
    [InlineData(2, 0, 1, 0, "end")]
    [InlineData(1, 1, 1, 0, "end")]
    public void InvalidStartAndEnd(int startLine, int startColumn, int endLine, int endColumn, string paramName)
    {
        var start = Position.From(startLine, startColumn);
        var end = Position.From(endLine, endColumn);
        var e = Assert.Throws<ArgumentOutOfRangeException>(() =>
            Location.From(in start, in end));
        Assert.Equal(paramName, e.ParamName);
        Assert.Equal(paramName == "end" ? end : start, e.ActualValue);
    }

    [Theory]
    [InlineData(1, 2, 1, 2, null, "[1,2)")]
    [InlineData(1, 2, 1, 5, null, "[1,2..5)")]
    [InlineData(1, 0, 5, 0, null, "[1..5,0)")]
    [InlineData(1, 0, 5, 5, "foo.js", "[1,0..5,5): foo.js")]
    public void ToStringTest(int startLine, int startColumn, int endLine, int endColumn, string? source, string expected)
    {
        var start = Position.From(startLine, startColumn);
        var end = Position.From(endLine, endColumn);
        var location = Location.From(in start, in end, source);
        Assert.Equal(expected, location.ToString());
    }

    [Theory]
    [InlineData(1, 2, 1, 2, null)]
    [InlineData(1, 2, 1, 5, null)]
    [InlineData(1, 0, 5, 0, null)]
    [InlineData(1, 0, 5, 5, "foo.js")]
    public void ParseTest(int startLine, int startColumn, int endLine, int endColumn, string? source)
    {
        var start = Position.From(startLine, startColumn);
        var end = Position.From(endLine, endColumn);
        var location = Location.From(in start, in end, source);
        Assert.Equal(location, Location.Parse(location.ToString()));
    }

    [Theory]
    [InlineData("[1,2..3,)", typeof(FormatException))]
    [InlineData("[1,2..0,0)", typeof(ArgumentOutOfRangeException))]
    public void ParseTest_InvalidInput(string s, Type exceptionType)
    {
        Assert.Throws(exceptionType, () => Location.Parse(s));
    }
}
