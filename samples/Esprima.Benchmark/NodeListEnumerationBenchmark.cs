using System;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;
using Esprima.Ast;

namespace Esprima.Benchmark
{
    [MemoryDiagnoser]
    [RankColumn]
    [CsvExporter]
    [MarkdownExporterAttribute.GitHub]
    public class NodeListEnumerationBenchmark
    {
        private const string FileName = "bundle";

        private NodeList<Node>[] _nodeLists;

        [Params(0, 1, 2)]
        public int NodeListIndex { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            var filePath = Path.Combine(AppContext.BaseDirectory, "3rdparty", $"{FileName}.js");
            var code = File.ReadAllText(filePath);
            var ast = new JavaScriptParser(code).ParseScript();

            _nodeLists = new NodeList<Node>[3];
            _nodeLists[0] = NodeList.Create(ast.DescendantNodesAndSelf());
            _nodeLists[1] = NodeList.Create(ast.DescendantNodesAndSelf().Take(1000));
            _nodeLists[2] = NodeList.Create(ast.DescendantNodesAndSelf().Take(10));
        }

        [Benchmark]
        public object For_DirectIndexing()
        {
            var nodeList = _nodeLists[NodeListIndex];

            var result = 0;
            for (var i = 0; i < nodeList.Count; i++)
            {
                result += (int) nodeList[i].Type;
            }
            return result;
        }

        [Benchmark]
        public object For_SpanIndexing()
        {
            var nodeList = _nodeLists[NodeListIndex].AsSpan();

            var result = 0;
            for (var i = 0; i < nodeList.Length; i++)
            {
                result += (int) nodeList[i].Type;
            }
            return result;
        }

        [Benchmark]
        public object ForEach_Span()
        {
            var nodeList = _nodeLists[NodeListIndex].AsSpan();

            var result = 0;
            foreach (var node in nodeList)
            {
                result += (int) node.Type;
            }
            return result;
        }
    }
}
