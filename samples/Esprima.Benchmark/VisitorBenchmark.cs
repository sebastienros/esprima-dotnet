using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Esprima.Utils;

namespace Esprima.Benchmark
{
    [MemoryDiagnoser]
    [RankColumn]
    [CsvExporter]
    [MarkdownExporterAttribute.GitHub]
    public class VisitorBenchmark
    {
        private const string FileName = "bundle";

        private Ast.Program _ast;
        private AstVisitor _visitor;
        private AstRewriter _rewriter;

        [GlobalSetup]
        public void Setup()
        {
            var filePath = Path.Combine(AppContext.BaseDirectory, "3rdparty", $"{FileName}.js");
            var code = File.ReadAllText(filePath);
            _ast = new JavaScriptParser(code).ParseScript();

            _visitor = new AstVisitor();
            _rewriter = new AstRewriter();
        }

        [Benchmark]
        public object Visit()
        {
            return _visitor.Visit(_ast);
        }

        [Benchmark]
        public object Rewrite()
        {
            return _rewriter.Visit(_ast);
        }
    }
}
