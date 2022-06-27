using System.Reflection;
using Esprima.Ast;
using Esprima.Utils;
using Esprima.Utils.Jsx;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Esprima.Test
{
    public class Fixtures
    {
        // Do manually set it to true to update local test files with the current results.
        // Only use this when the test is deemed wrong.
        private const bool WriteBackExpectedTree = false;

        private const string FixturesDirName = "Fixtures";

        private static Lazy<Dictionary<string, FixtureMetadata>> Metadata { get; } = new(() => FixtureMetadata.ReadMetadata());

        [Fact]
        public void HoistingScopeShouldWork()
        {
            var parser = new JavaScriptParser(@"
                function p() {}
                var x;");
            var program = parser.ParseScript();
        }

        private static string ParseAndFormat(SourceType sourceType, string source,
            ParserOptions parserOptions, Func<string, ParserOptions, JavaScriptParser> parserFactory,
            AstJson.IConverter converter, AstJson.Options conversionOptions)
        {
            var parser = parserFactory(source, parserOptions);
            var program = sourceType == SourceType.Script ? (Program) parser.ParseScript() : parser.ParseModule();

            return program.ToJsonString(conversionOptions, indent: "  ", converter);
        }

        private static bool CompareTreesInternal(JObject actualJObject, JObject expectedJObject, FixtureMetadata metadata)
        {
            // Don't compare the tokens array as it's not in the generated AST
            expectedJObject.Remove("tokens");
            expectedJObject.Remove("comments");
            expectedJObject.Remove("errors");

            // Don't compare location sources as it's not in the generated AST
            if (metadata.IncludesLocationSource)
            {
                IEnumerable<JObject> locObjects = expectedJObject.Descendants()
                    .OfType<JProperty>()
                    .Where(prop => prop.Name == "loc")
                    .Select(prop => prop.Value as JObject)
                    .Where(obj => obj != null)!;

                foreach (var obj in locObjects)
                {
                    obj.Remove("source");
                }
            }

            return JToken.DeepEquals(actualJObject, expectedJObject);
        }

        private static bool CompareTrees(string actual, string expected, FixtureMetadata metadata)
        {
            var actualJObject = JObject.Parse(actual);
            var expectedJObject = JObject.Parse(expected);

            return CompareTreesInternal(actualJObject, expectedJObject, metadata);
        }

        private static void CompareTreesAndAssert(string actual, string expected, FixtureMetadata metadata)
        {
            var actualJObject = JObject.Parse(actual);
            var expectedJObject = JObject.Parse(expected);

            var areEqual = CompareTreesInternal(actualJObject, expectedJObject, metadata);
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
            var (parserOptions, parserFactory, converter) = fixture.StartsWith("JSX")
                ? (new JsxParserOptions(),
                    (src, opts) => new JsxParser(src, (JsxParserOptions) opts),
                    JsxAstToJsonConverter.Default)
                : (new ParserOptions(),
                    new Func<string, ParserOptions, JavaScriptParser>((src, opts) => new JavaScriptParser(src, opts)),
                    AstToJsonConverter.Default);

            parserOptions.Tokens = true;

            string treeFilePath, failureFilePath, moduleFilePath;
            var jsFilePath = Path.Combine(GetFixturesPath(), FixturesDirName, fixture);
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

            if (!Metadata.Value.TryGetValue(jsFilePath, out var metadata))
            {
                metadata = FixtureMetadata.Default;
            }

            parserOptions.AdaptRegexp = !metadata.IgnoresRegex;

#pragma warning disable 162
            if (File.Exists(moduleFilePath))
            {
                sourceType = SourceType.Module;
                expected = File.ReadAllText(moduleFilePath);
                if (WriteBackExpectedTree && !metadata.ConversionOptions.TestCompatibilityMode)
                {
                    var actual = ParseAndFormat(sourceType, script, parserOptions, parserFactory, converter, metadata.ConversionOptions);
                    if (!CompareTrees(actual, expected, metadata))
                        File.WriteAllText(moduleFilePath, actual);
                }
            }
            else if (File.Exists(treeFilePath))
            {
                expected = File.ReadAllText(treeFilePath);
                if (WriteBackExpectedTree && !metadata.ConversionOptions.TestCompatibilityMode)
                {
                    var actual = ParseAndFormat(sourceType, script, parserOptions, parserFactory, converter, metadata.ConversionOptions);
                    if (!CompareTrees(actual, expected, metadata))
                        File.WriteAllText(treeFilePath, actual);
                }
            }
            else if (File.Exists(failureFilePath))
            {
                invalid = true;
                expected = File.ReadAllText(failureFilePath);
                if (WriteBackExpectedTree && !metadata.ConversionOptions.TestCompatibilityMode)
                {
                    var actual = ParseAndFormat(sourceType, script, parserOptions, parserFactory, converter, metadata.ConversionOptions);
                    if (!CompareTrees(actual, expected, metadata))
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
                parserOptions.Tolerant = true;

                var actual = ParseAndFormat(sourceType, script, parserOptions, parserFactory, converter, metadata.ConversionOptions);
                CompareTreesAndAssert(actual, expected, metadata);
            }
            else
            {
                parserOptions.Tolerant = false;

                // TODO: check the accuracy of the message and of the location
                Assert.Throws<ParserException>(() => ParseAndFormat(sourceType, script, parserOptions, parserFactory, converter, metadata.ConversionOptions));
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

        private sealed class ParentNodeChecker : AstVisitor
        {
            public void Check(Node node)
            {
                Assert.Null(node.Data);

                base.Visit(node);
            }

            public override object? Visit(Node node)
            {
                var parent = (Node?) node.Data;
                Assert.NotNull(parent);
                Assert.Contains(node, parent!.ChildNodes);

                return base.Visit(node);
            }
        }

        [Fact]
        public void NodeDataCanBeSetToParentNode()
        {
            Action<Node> action = node =>
            {
                foreach (var child in node.ChildNodes)
                {
                    if (child is not null)
                    {
                        child.Data = node;
                    }
                }
            };

            var parser = new JavaScriptParser("function add(a, b) { return a + b; }", new ParserOptions { OnNodeCreated = action });
            var script = parser.ParseScript();

            new ParentNodeChecker().Check(script);
        }

        [Fact]
        public void CommentsAreParsed()
        {
            var count = 0;
            Action<Node> action = node => count++;
            var parser = new JavaScriptParser("// this is a comment", new ParserOptions { OnNodeCreated = action });
            parser.ParseScript();

            Assert.Equal(1, count);
        }

        private sealed class FixtureMetadata
        {
            public static readonly FixtureMetadata Default = new FixtureMetadata(
                AstJson.Options.Default
                    .WithIncludingLineColumn(true)
                    .WithIncludingRange(true),
                includesLocationSource: false,
                ignoresRegex: false);

            private sealed class Group
            {
                public HashSet<string> Flags { get; } = new();
                public HashSet<string> Files { get; } = new();
            }

            public static Dictionary<string, FixtureMetadata> ReadMetadata()
            {
                var fixturesDirPath = Path.Combine(GetFixturesPath(), FixturesDirName);
                var compatListFilePath = Path.Combine(fixturesDirPath, "fixtures-metadata.json");

                var baseUri = new Uri(fixturesDirPath + "/");

                Group[]? groups;
                using (var reader = new StreamReader(compatListFilePath))
                    groups = (Group[]?) JsonSerializer.CreateDefault().Deserialize(reader, typeof(Group[]));

                return (groups ?? Array.Empty<Group>())
                    .SelectMany(group =>
                    {
                        var metadata = CreateFrom(group.Flags);

                        return group.Files.Select(file =>
                        (
                            filePath: new Uri(baseUri, file).LocalPath,
                            metadata
                        ));
                    })
                    .ToDictionary(item => item.filePath, item => item.metadata);
            }

            private static FixtureMetadata CreateFrom(HashSet<string> flags)
            {
                var conversionOptions = AstJson.Options.Default;

                if (flags.Contains("IncludesLocation"))
                    conversionOptions = conversionOptions.WithIncludingLineColumn(true);

                if (flags.Contains("IncludesRange"))
                    conversionOptions = conversionOptions.WithIncludingRange(true);

                if (flags.Contains("BorrowedFixture"))
                    conversionOptions = conversionOptions.WithTestCompatibilityMode(true);

                var includesLocationSource = flags.Contains("IncludesLocationSource");
                var ignoresRegex = flags.Contains("IgnoresRegex");

                return new FixtureMetadata(conversionOptions, includesLocationSource, ignoresRegex);
            }

            private FixtureMetadata(AstJson.Options conversionOptions, bool includesLocationSource, bool ignoresRegex)
            {
                ConversionOptions = conversionOptions;
                IncludesLocationSource = includesLocationSource;
                IgnoresRegex = ignoresRegex;
            }

            public AstJson.Options ConversionOptions { get; }
            public bool IncludesLocationSource { get; }
            public bool IgnoresRegex { get; }
        }
    }
}
