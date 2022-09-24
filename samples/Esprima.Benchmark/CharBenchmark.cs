#if NET6_0_OR_GREATER

using BenchmarkDotNet.Attributes;
using Esprima.Tests;

namespace Esprima.Benchmark;

[MemoryDiagnoser]
public class CharBenchmark
{
    private static char[] _asciiRange;
    private static char[] _fullRange;

    static CharBenchmark()
    {
        _asciiRange = Enumerable.Range(char.MinValue, 127).Select(x => (char) x).ToArray();
        _fullRange = Enumerable.Range(char.MinValue, char.MaxValue).Select(x => (char) x).ToArray();
    }

    [Params(false, true)]
    public bool FullRange { get; set; }

    [Benchmark]
    public void IsHexDigit_Old()
    {
        var source = FullRange ? _fullRange : _asciiRange;
        foreach (var i in source)
        {
            CharMaskGeneratorTest.IsHexDigit(i);
        }
    }

    [Benchmark]
    public void IsHexDigit_New()
    {
        var source = FullRange ? _fullRange : _asciiRange;
        foreach (var i in source)
        {
            Character.IsHexDigit(i);
        }
    }

    [Benchmark]
    public void IsDecimalDigit_Old()
    {
        var source = FullRange ? _fullRange : _asciiRange;
        foreach (var i in source)
        {
            CharMaskGeneratorTest.IsDecimalDigit(i);
        }
    }

    [Benchmark]
    public void IsDecimalDigit_New()
    {
        var source = FullRange ? _fullRange : _asciiRange;
        foreach (var i in source)
        {
            Character.IsDecimalDigit(i);
        }
    }

    [Benchmark]
    public void IsWhiteSpace_Old()
    {
        var source = FullRange ? _fullRange : _asciiRange;
        foreach (var i in source)
        {
            CharMaskGeneratorTest.IsWhiteSpace(i);
        }
    }

    [Benchmark]
    public void IsWhiteSpace_New()
    {
        var source = FullRange ? _fullRange : _asciiRange;
        foreach (var i in source)
        {
            Character.IsWhiteSpace(i);
        }
    }

    [Benchmark]
    public void IsIdentifierStart_Old()
    {
        var source = FullRange ? _fullRange : _asciiRange;
        foreach (var i in source)
        {
            CharMaskGeneratorTest.IsIdentifierStart(i);
        }
    }

    [Benchmark]
    public void IsIdentifierStart_New()
    {
        var source = FullRange ? _fullRange : _asciiRange;
        foreach (var i in source)
        {
            Character.IsIdentifierStart(i);
        }
    }

    [Benchmark]
    public void IsIdentifierPart_Old()
    {
        var source = FullRange ? _fullRange : _asciiRange;
        foreach (var i in source)
        {
            CharMaskGeneratorTest.IsIdentifierPart(i);
        }
    }

    [Benchmark]
    public void IsIdentifierPart_New()
    {
        var source = FullRange ? _fullRange : _asciiRange;
        foreach (var i in source)
        {
            Character.IsIdentifierPart(i);
        }
    }
}

#endif

