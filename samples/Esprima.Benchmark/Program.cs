using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Esprima.Benchmark
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var scripts =new [] {
                "underscore-1.5.2",
                "backbone-1.1.0",
                "mootools-1.4.5",
                "jquery-1.9.1",
                "yui-3.12.0",
                "jquery.mobile-1.4.2",
                "angular-1.2.5"
            };

            foreach(var script in scripts)
            {
                Bench(script);
            }
        }

        public static void Bench(string script)
        {
            var repeat = 20;
            var exclude = 3;

            var filename = $"../../test/Esprima.Tests/Fixtures/3rdparty/{script}.js";
            var content = File.ReadAllText(filename);

            var sw = new Stopwatch();
            var results = new List<long>();

            for(var i=0; i<repeat; i++)
            {
                var parser = new JavaScriptParser(content);
                sw.Restart();

                parser.ParseProgram();
                results.Add(sw.ElapsedMilliseconds);
            }

            var average = results
                .OrderBy(x => x)
                .Skip(exclude)
                .Take(repeat - exclude * 2)
                .Average();

            Console.WriteLine("{0} ({2}KB): {1} ms", script, Math.Round(average, 1), content.Length / 1024);
        }

    }
}
