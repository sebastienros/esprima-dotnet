namespace Esprima.Tests;

public class PositionTests
{
    [Theory]
    [InlineData(0, 0, "0,0")]
    [InlineData(1, 2, "1,2")]
    public void ToStringTest(int line, int column, string expected)
    {
        var position = Position.From(line, column);
        Assert.Equal(expected, position.ToString());
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 2)]
    public void ParseTest(int line, int column)
    {
        var position = Position.From(line, column);
        Assert.Equal(position, Position.Parse(position.ToString()));
    }

    [Theory]
    [InlineData("1,x", typeof(FormatException))]
    [InlineData("0,1", typeof(ArgumentOutOfRangeException))]
    public void ParseTest_InvalidInput(string s, Type exceptionType)
    {
        Assert.Throws(exceptionType, () => Position.Parse(s));
    }
}
