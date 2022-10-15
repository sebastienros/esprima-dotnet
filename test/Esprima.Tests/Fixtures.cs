using System.Reflection;
using DiffEngine;
using Esprima.Ast;
using Esprima.Utils;
using Esprima.Utils.Jsx;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Esprima.Test;

public class Fixtures
{
    // Do manually set it to true to update local test files with the current results.
    // Only use this when the test is deemed wrong.
    private const bool WriteBackExpectedTree = false;

    internal const string FixturesDirName = "Fixtures";

    private static Lazy<Dictionary<string, FixtureMetadata>> Metadata { get; } = new(() => FixtureMetadata.ReadMetadata());

    private static string ParseAndFormat(SourceType sourceType, string source,
        ParserOptions parserOptions, Func<ParserOptions, JavaScriptParser> parserFactory,
        AstToJsonOptions conversionOptions)
    {
        var parser = parserFactory(parserOptions);
        var program = sourceType == SourceType.Script ? (Program) parser.ParseScript(source) : parser.ParseModule(source);

        return program.ToJsonString(conversionOptions, indent: "  ");
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

            var file1 = Path.GetTempFileName() + ".json";
            var file2 = Path.GetTempFileName() + ".json";
            File.WriteAllText(file1, expectedString);
            File.WriteAllText(file2, actualString);
            DiffRunner.Launch(file1, file2);

            Assert.Equal(expectedString, actualString);
        }
    }

    [Theory]
    [MemberData(nameof(SourceFiles), "Fixtures")]
    public void ExecuteTestCase(string fixture)
    {
        static T CreateParserOptions<T>(bool tolerant, bool adaptRegex) where T : ParserOptions, new() =>
            new T { Tokens = true, Tolerant = tolerant, AdaptRegexp = adaptRegex };

        var (parserOptionsFactory, parserFactory, conversionDefaultOptions) = fixture.StartsWith("JSX", StringComparison.Ordinal)
            ? (CreateParserOptions<JsxParserOptions>,
                opts => new JsxParser((JsxParserOptions) opts),
                JsxAstToJsonOptions.Default)
            : (new Func<bool, bool, ParserOptions>(CreateParserOptions<ParserOptions>),
                new Func<ParserOptions, JavaScriptParser>(opts => new JavaScriptParser(opts)),
                AstToJsonOptions.Default);

        string treeFilePath, failureFilePath, moduleFilePath;
        var jsFilePath = Path.Combine(GetFixturesPath(), FixturesDirName, fixture);
        var jsFileDirectoryName = Path.GetDirectoryName(jsFilePath)!;
        if (jsFilePath.EndsWith(".source.js", StringComparison.Ordinal))
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
        if (jsFilePath.EndsWith(".source.js", StringComparison.Ordinal))
        {
            var parser = new JavaScriptParser();
            var program = parser.ParseScript(script);
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

        var parserOptions = parserOptionsFactory(false, !metadata.IgnoresRegex);

        var conversionOptions = metadata.CreateConversionOptions(conversionDefaultOptions);

#pragma warning disable 162
        if (File.Exists(moduleFilePath))
        {
            sourceType = SourceType.Module;
            expected = File.ReadAllText(moduleFilePath);
            if (WriteBackExpectedTree && conversionOptions.TestCompatibilityMode == AstToJsonTestCompatibilityMode.None)
            {
                var actual = ParseAndFormat(sourceType, script, parserOptions, parserFactory, conversionOptions);
                if (!CompareTrees(actual, expected, metadata))
                    File.WriteAllText(moduleFilePath, actual);
            }
        }
        else if (File.Exists(treeFilePath))
        {
            expected = File.ReadAllText(treeFilePath);
            if (WriteBackExpectedTree && conversionOptions.TestCompatibilityMode == AstToJsonTestCompatibilityMode.None)
            {
                var actual = ParseAndFormat(sourceType, script, parserOptions, parserFactory, conversionOptions);
                if (!CompareTrees(actual, expected, metadata))
                    File.WriteAllText(treeFilePath, actual);
            }
        }
        else if (File.Exists(failureFilePath))
        {
            invalid = true;
            expected = File.ReadAllText(failureFilePath);
            if (WriteBackExpectedTree && conversionOptions.TestCompatibilityMode == AstToJsonTestCompatibilityMode.None)
            {
                var actual = ParseAndFormat(sourceType, script, parserOptions, parserFactory, conversionOptions);
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
            parserOptions = parserOptionsFactory(true, parserOptions.AdaptRegexp);

            var actual = ParseAndFormat(sourceType, script, parserOptions, parserFactory, conversionOptions);
            CompareTreesAndAssert(actual, expected, metadata);
        }
        else
        {
            parserOptions = parserOptionsFactory(false, parserOptions.AdaptRegexp);

            // TODO: check the accuracy of the message and of the location
            Assert.Throws<ParserException>(() => ParseAndFormat(sourceType, script, parserOptions, parserFactory, conversionOptions));
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

    private sealed class FixtureMetadata
    {
        public static readonly FixtureMetadata Default = new FixtureMetadata(
            testCompatibilityMode: AstToJsonTestCompatibilityMode.None,
            includesLocation: true,
            includesRange: true,
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
            return new FixtureMetadata(
                testCompatibilityMode: flags.Contains("EsprimaOrgFixture") ? AstToJsonTestCompatibilityMode.EsprimaOrg : AstToJsonTestCompatibilityMode.None,
                includesLocation: flags.Contains("IncludesLocation"),
                includesRange: flags.Contains("IncludesRange"),
                includesLocationSource: flags.Contains("IncludesLocationSource"),
                ignoresRegex: flags.Contains("IgnoresRegex"));
        }

        private FixtureMetadata(AstToJsonTestCompatibilityMode testCompatibilityMode, bool includesLocation, bool includesRange, bool includesLocationSource, bool ignoresRegex)
        {
            TestCompatibilityMode = testCompatibilityMode;
            IncludesLocation = includesLocation;
            IncludesRange = includesRange;
            IncludesLocationSource = includesLocationSource;
            IgnoresRegex = ignoresRegex;
        }

        public AstToJsonTestCompatibilityMode TestCompatibilityMode { get; }
        public bool IncludesLocation { get; }
        public bool IncludesRange { get; }
        public bool IncludesLocationSource { get; }
        public bool IgnoresRegex { get; }

        public AstToJsonOptions CreateConversionOptions(AstToJsonOptions defaultOptions) => defaultOptions with
        {
            TestCompatibilityMode = TestCompatibilityMode,
            IncludeLineColumn = IncludesLocation,
            IncludeRange = IncludesRange,
        };
    }
}
