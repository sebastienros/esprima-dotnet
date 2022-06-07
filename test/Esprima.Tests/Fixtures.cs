using System.Reflection;
using Esprima.Ast;
using Esprima.Utils;
using Esprima.Utils.Jsx;
using Newtonsoft.Json.Linq;

namespace Esprima.Test
{
    public class Fixtures
    {
        // Do manually set it to true to update local test files with the current results.
        // Only use this when the test is deemed wrong.
        const bool WriteBackExpectedTree = false;

        [Fact]
        public void HoistingScopeShouldWork()
        {
            var parser = new JavaScriptParser(@"
                function p() {}
                var x;");
            var program = parser.ParseScript();
        }

        private static string ParseAndFormat(SourceType sourceType, string source, ParserOptions options, Func<string, ParserOptions, JavaScriptParser> parserFactory,
            AstJson.IConverter converter)
        {
            var parser = parserFactory(source, options);
            var program = sourceType == SourceType.Script ? (Program) parser.ParseScript() : parser.ParseModule();
            const string indent = "  ";
            return program.ToJsonString(
                AstJson.Options.Default
                    .WithIncludingLineColumn(true)
                    .WithIncludingRange(true)
                    .WithTestCompatibilityMode(true),
                indent,
                converter
            );
        }

        private static bool CompareTreesInternal(string actual, string expected)
        {
            var actualJObject = JObject.Parse(actual);
            var expectedJObject = JObject.Parse(expected);

            // Don't compare the tokens array as it's not in the generated AST
            expectedJObject.Remove("tokens");
            expectedJObject.Remove("comments");
            expectedJObject.Remove("errors");

            return JToken.DeepEquals(actualJObject, expectedJObject);
        }

        private static void CompareTrees(string actual, string expected, string path)
        {
            var actualJObject = JObject.Parse(actual);
            var expectedJObject = JObject.Parse(expected);

            // Don't compare the tokens array as it's not in the generated AST
            expectedJObject.Remove("tokens");
            expectedJObject.Remove("comments");
            expectedJObject.Remove("errors");

            var areEqual = JToken.DeepEquals(actualJObject, expectedJObject);
            if (!areEqual)
            {
                var actualString = actualJObject.ToString();
                var expectedString = expectedJObject.ToString();
                Assert.Equal(expectedString, actualString);
            }
        }

        [Theory]
        [MemberData(nameof(SourceFiles), "Fixtures")]
        public void ExecuteTestCase(string fixture)
        {
            var (options, parserFactory, converter) = fixture.StartsWith("JSX") ?
                (new JsxParserOptions(),
                    (src, opts) => new JsxParser(src, (JsxParserOptions) opts),
                    JsxAstToJsonConverter.Default) :
                (new ParserOptions(),
                    new Func<string, ParserOptions, JavaScriptParser>((src, opts) => new JavaScriptParser(src, opts)),
                    AstToJsonConverter.Default);

            options.Tokens = true;

            string treeFilePath, failureFilePath, moduleFilePath;
            var jsFilePath = Path.Combine(GetFixturesPath(), "Fixtures", fixture);
            var jsFileDirectoryName = Path.GetDirectoryName(jsFilePath)!;
            if (jsFilePath.EndsWith(".source.js"))
            {
                treeFilePath = Path.Combine(jsFileDirectoryName, Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(jsFilePath))) + ".tree.json";
                failureFilePath = Path.Combine(jsFileDirectoryName, Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(jsFilePath))) + ".failure.json";
                moduleFilePath = Path.Combine(jsFileDirectoryName, Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(jsFilePath))) + ".module.json";
            }
            else
            {
                treeFilePath = Path.Combine(jsFileDirectoryName, Path.GetFileNameWithoutExtension(jsFilePath)) + ".tree.json";
                failureFilePath = Path.Combine(jsFileDirectoryName, Path.GetFileNameWithoutExtension(jsFilePath)) + ".failure.json";
                moduleFilePath = Path.Combine(jsFileDirectoryName, Path.GetFileNameWithoutExtension(jsFilePath)) + ".module.json";
            }

            var script = File.ReadAllText(jsFilePath);
            if (jsFilePath.EndsWith(".source.js"))
            {
                var parser = new JavaScriptParser(script);
                var program = parser.ParseScript();
                var source = program.Body.First().As<VariableDeclaration>().Declarations.First().As<VariableDeclarator>().Init!.As<Literal>().StringValue!;
                script = source;
            }

            var expected = "";
            var invalid = false;

            var filename = Path.GetFileNameWithoutExtension(jsFilePath);

            var isModule =
                filename.Contains("module") ||
                filename.Contains("export") ||
                filename.Contains("import");

            if (!filename.Contains(".module"))
            {
                isModule &= !jsFilePath.Contains("dynamic-import") && !jsFilePath.Contains("script");
            }

            var sourceType = isModule
                ? SourceType.Module
                : SourceType.Script;

#pragma warning disable 162
            if (File.Exists(moduleFilePath))
            {
                sourceType = SourceType.Module;
                expected = File.ReadAllText(moduleFilePath);
                if (WriteBackExpectedTree)
                {
                    var actual = ParseAndFormat(sourceType, script, options, parserFactory, converter);
                    if (!CompareTreesInternal(actual, expected))
                        File.WriteAllText(moduleFilePath, actual);
                }
            }
            else if (File.Exists(treeFilePath))
            {
                expected = File.ReadAllText(treeFilePath);
                if (WriteBackExpectedTree)

                {
                    var actual = ParseAndFormat(sourceType, script, options, parserFactory, converter);
                    if (!CompareTreesInternal(actual, expected))
                        File.WriteAllText(treeFilePath, actual);
                }
            }
            else if (File.Exists(failureFilePath))
            {
                invalid = true;
                expected = File.ReadAllText(failureFilePath);
                if (WriteBackExpectedTree)
                {
                    var actual = ParseAndFormat(sourceType, script, options, parserFactory, converter);
                    if (!CompareTreesInternal(actual, expected))
                        File.WriteAllText(failureFilePath, actual);
                }
#pragma warning restore 162
            }
            else
            {
                // cannot compare
                return;
            }

            invalid |=
                filename.Contains("error") ||
                filename.Contains("invalid") && (!filename.Contains("invalid-yield-object-") && !filename.Contains("attribute-invalid-entity"));

            if (!invalid)
            {
                options.Tolerant = true;

                var actual = ParseAndFormat(sourceType, script, options, parserFactory, converter);
                CompareTrees(actual, expected, jsFilePath);
            }
            else
            {
                options.Tolerant = false;

                // TODO: check the accuracy of the message and of the location
                Assert.Throws<ParserException>(() => ParseAndFormat(sourceType, script, options, parserFactory, converter));
            }
        }

        public static IEnumerable<object[]> SourceFiles(string relativePath)
        {
            var fixturesPath = Path.Combine(GetFixturesPath(), relativePath);

            var files = Directory.GetFiles(fixturesPath, "*.js", SearchOption.AllDirectories);

            return files
                .Select(x => new object[] { x.Substring(fixturesPath.Length + 1) })
                .ToList();
        }

        internal static string GetFixturesPath()
        {
#if NETFRAMEWORK
            var assemblyPath = new Uri(typeof(Fixtures).GetTypeInfo().Assembly.CodeBase).LocalPath;
            var assemblyDirectory = new FileInfo(assemblyPath).Directory;
#else
            var assemblyPath = typeof(Fixtures).GetTypeInfo().Assembly.Location;
            var assemblyDirectory = new FileInfo(assemblyPath).Directory;
#endif
            var root = assemblyDirectory?.Parent?.Parent?.Parent?.FullName;
            return root ?? "";
        }

        [Fact]
        public void CommentsAreParsed()
        {
            var count = 0;
            Action<Node> action = node => count++;
            var parser = new JavaScriptParser("// this is a comment", new ParserOptions(), action);
            parser.ParseScript();

            Assert.Equal(1, count);
        }
    }
}
