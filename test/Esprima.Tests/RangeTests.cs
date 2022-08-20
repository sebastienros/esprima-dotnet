namespace Esprima.Tests;

public class RangeTests
{
    [Theory]
    [InlineData(1, 1, "[1)")]
    [InlineData(0, 5, "[0..5)")]
    public void ToStringTest(int start, int end, string expected)
    {
        var range = Range.From(start, end);
        Assert.Equal(expected, range.ToString());
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(0, 5)]
    public void ParseTest(int start, int end)
    {
        var range = Range.From(start, end);
        Assert.Equal(range, Range.Parse(range.ToString()));
    }

    [Theory]
    [InlineData("[1,2 x)", typeof(FormatException))]
    [InlineData("[1..0)", typeof(ArgumentOutOfRangeException))]
    public void ParseTest_InvalidInput(string s, Type exceptionType)
    {
        Assert.Throws(exceptionType, () => Range.Parse(s));
    }
}
