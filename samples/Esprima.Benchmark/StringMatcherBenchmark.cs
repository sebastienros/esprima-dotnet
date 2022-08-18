using System;
using System.Collections.Generic;
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
        var parser = new JavaScriptParser(code, new ParserOptions { Tokens = true });
        parser.ParseScript();
        _tokens = parser.Tokens.ToArray();
    }

    [Benchmark]
    public int CountKeywords()
    {
        int count = 0;
        for (int i = 0; i < _tokens.Length; i++)
        {
            if (Scanner.IsKeyword(_tokens[i].Value))
            {
                count++;
            }
        }
        return count;
    }
}
