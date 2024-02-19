namespace Esprima.Tests;

public class ParserOptionsTests
{
    [Fact]
    public void CopyCtorShouldCreateDeepClone()
    {
        var options1 = new ParserOptions { Tolerant = false };
        var options2 = options1 with { Tolerant = true };

        Assert.NotSame(options1.GetScannerOptions(), options2.GetScannerOptions());
        Assert.True(options2.Tolerant);
        Assert.False(options1.Tolerant);
    }

    [Fact]
    public void EqualsShouldCheckStructuralEquality()
    {
        var options1 = new ParserOptions { Tolerant = false };
        var options2 = options1 with { Tolerant = false };

        Assert.NotSame(options1.GetScannerOptions(), options2.GetScannerOptions());
        Assert.Equal(options1.GetScannerOptions(), options2.GetScannerOptions());
        Assert.Equal(options1, options2);
    }
}
