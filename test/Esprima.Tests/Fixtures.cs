using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Esprima.Ast;
using Esprima.Utils;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Esprima.Test
{
    public class Fixtures
    {
        [Fact]
        public void HoistingScopeShouldWork()
        {
            var parser = new JavaScriptParser(@"
                function p() {}
                var x;");
            var program = parser.ParseScript();
        }

        private static string ParseAndFormat(SourceType sourceType, string source, ParserOptions options)
        {
            var parser = new JavaScriptParser(source, options);
            var program = sourceType == SourceType.Script ?  (Program) parser.ParseScript() : parser.ParseModule();
            const string indent = "  ";
            return program.ToJsonString(
                AstJson.Options.Default
                    .WithIncludingLineColumn(true)
                    .WithIncludingRange(true),
                indent
            );
        }

        public bool CompareTrees(string actual, string expected)
        {
            var actualJObject = JObject.Parse(actual);
            var expectedJObject = JObject.Parse(expected);

            // Don't compare the tokens array as it's not in the generated AST
            expectedJObject.Remove("tokens");
            expectedJObject.Remove("comments");
            expectedJObject.Remove("errors");

            return JToken.DeepEquals(actualJObject, expectedJObject);
        }

        [Theory]
        [MemberData(nameof(SourceFiles), "Fixtures")]
        public void ExecuteTestCase(string fixture)
        {
            var options = new ParserOptions
            {
                Tokens = true
            };

            string treeFilePath, failureFilePath, moduleFilePath;
            var jsFilePath = Path.Combine(GetFixturesPath(), "Fixtures", fixture);
            if (jsFilePath.EndsWith(".source.js"))
            {
                treeFilePath = Path.Combine(Path.GetDirectoryName(jsFilePath), Path.GetFileNameWithoutExtension((Path.GetFileNameWithoutExtension(jsFilePath)))) + ".tree.json";
                failureFilePath = Path.Combine(Path.GetDirectoryName(jsFilePath), Path.GetFileNameWithoutExtension((Path.GetFileNameWithoutExtension(jsFilePath)))) + ".failure.json";
                moduleFilePath = Path.Combine(Path.GetDirectoryName(jsFilePath), Path.GetFileNameWithoutExtension((Path.GetFileNameWithoutExtension(jsFilePath)))) + ".module.json";
            }
            else
            {
                treeFilePath = Path.Combine(Path.GetDirectoryName(jsFilePath), Path.GetFileNameWithoutExtension(jsFilePath)) + ".tree.json";
                failureFilePath = Path.Combine(Path.GetDirectoryName(jsFilePath), Path.GetFileNameWithoutExtension(jsFilePath)) + ".failure.json";
                moduleFilePath = Path.Combine(Path.GetDirectoryName(jsFilePath), Path.GetFileNameWithoutExtension(jsFilePath)) + ".module.json";
            }

            // Convert to LF to match the number of chars the parser finds, but some tests expect to check Windows
            // TODO should be based on .gitattributes
            var script = File.ReadAllText(jsFilePath);
            if (!jsFilePath.EndsWith("primary\\literal\\string\\migrated_0017.js")
                && !jsFilePath.EndsWith("expression\\binary\\multiline_string_literal.js"))
            {
                script = script.Replace(Environment.NewLine, "\n");
            }

            if (jsFilePath.EndsWith(".source.js"))
            {
                var parser = new JavaScriptParser(script);
                var program = parser.ParseScript();
                var source = program.Body.First().As<VariableDeclaration>().Declarations.First().As<VariableDeclarator>().Init.As<Literal>().StringValue;
                script = source;
            }

            string expected = "";
            bool invalid = false;

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

            if (File.Exists(moduleFilePath))
            {
                sourceType = SourceType.Module;
                expected = File.ReadAllText(moduleFilePath);
            }
            else if(File.Exists(treeFilePath))
            {
                expected = File.ReadAllText(treeFilePath);
            }
            else if (File.Exists(failureFilePath))
            {
                invalid = true;
                expected = File.ReadAllText(failureFilePath);
            }
            else
            {
                // cannot compare
                return;
            }

            invalid |=
                filename.Contains("error") ||
                (filename.Contains("invalid") && !filename.Contains("invalid-yield-object-")) ;

            if (!invalid)
            {
                options.Tolerant = true;

                var actual = ParseAndFormat(sourceType, script, options);
                Assert.True(CompareTrees(actual, expected), jsFilePath);
            }
            else
            {
                options.Tolerant = false;

                // TODO: check the accuracy of the message and of the location
                Assert.Throws<ParserException>(() => ParseAndFormat(sourceType, script, options));
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

        private static string GetFixturesPath()
        {
            var assemblyPath = new Uri(typeof(Fixtures).GetTypeInfo().Assembly.CodeBase).LocalPath;
            var assemblyDirectory = new FileInfo(assemblyPath).Directory;

            var root = assemblyDirectory.Parent.Parent.Parent.FullName;
            return root;
        }

        [Fact]
        public void CommentsAreParsed()
        {
            int count = 0;
            Action<Node> action = node => count++;
            var parser = new JavaScriptParser("// this is a comment", new ParserOptions(), action);
            parser.ParseScript();

            Assert.Equal(1, count);
        }
    }
}
