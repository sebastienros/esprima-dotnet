namespace Esprima.Tests;

public class RangeTests
{
    [Theory]
    [InlineData(1, 1, "[1)")]
    [InlineData(0, 5, "[0..5)")]
    public void ToStringTest(int start, int end, string expected)
    {
        var range = new Range(start, end);
        Assert.Equal(expected, range.ToString());
    }
}
