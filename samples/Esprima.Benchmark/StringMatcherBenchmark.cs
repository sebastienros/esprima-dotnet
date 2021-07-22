using System;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;
using Esprima.Ast;

namespace Esprima.Benchmark;

[MemoryDiagnoser]
public class StringMatcherBenchmark
{
    private SyntaxToken[] _tokens;

    [GlobalSetup]
    public void Setup()
    {
        var filePath = Path.Combine(AppContext.BaseDirectory, "3rdparty", "bundle.js");
        var code = File.ReadAllText(filePath);
        var parser = new JavaScriptParser(new ParserOptions { Tokens = true });
        parser.ParseScript(code);
        _tokens = parser.Tokens.ToArray();
    }

    [Benchmark]
    public int IsKeyword()
    {
        int count = 0;
        foreach (var token in _tokens)
        {
            if (Scanner.IsKeyword(token.Value))
            {
                count++;
            }
        }
        return count;
    }

    [Benchmark]
    public int TryGetInternedString()
    {
        int count = 0;
        foreach (var token in _tokens)
        {
            if (ParserExtensions.TryGetInternedString(token.Value.AsSpan()) is not null)
            {
                count++;
            }
        }
        return count;
    }
}
