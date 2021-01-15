using System.Collections.Generic;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;

namespace Esprima.Benchmark
{
    [MemoryDiagnoser]
    public class FileParsingBenchmark
    {
        private static readonly Dictionary<string, string> files = new Dictionary<string, string>
        {
            {"underscore-1.5.2", null},
            {"backbone-1.1.0", null},
            {"mootools-1.4.5", null},
            {"jquery-1.9.1", null},
            {"yui-3.12.0", null},
            {"jquery.mobile-1.4.2", null},
            {"angular-1.2.5", null}
        };

        private static readonly ParserOptions parserOptions = new ParserOptions()
        {
            Comment = true,
            Tokens = true 
        };

        [GlobalSetup]
        public void Setup()
        {
            foreach (var fileName in files.Keys.ToList())
            {
                files[fileName] = File.ReadAllText($"3rdparty/{fileName}.js");
            }
        }

        [ParamsSource(nameof(FileNames))]
        public string FileName { get; set; }

        public IEnumerable<string> FileNames()
        {
            foreach (var entry in files)
            {
                yield return entry.Key;
            }
        }

        [Benchmark]
        public void ParseProgram()
        {
            var parser = new JavaScriptParser(files[FileName], parserOptions);
            parser.ParseScript();
        }
    }
}