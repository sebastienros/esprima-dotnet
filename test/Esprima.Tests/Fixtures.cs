using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Esprima.Ast;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Xunit;

namespace Esprima.Test
{
    public class Fixtures
    {
        public string ParseAndFormat(string source, ParserOptions options)
        {
            var parser = new JavaScriptParser(source, options);

            var program = parser.ParseProgram();

            var settings = new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
            };

            settings.Converters.Add(new StringEnumConverter { CamelCaseText = true });

            return JsonConvert.SerializeObject(program, Formatting.Indented, settings);
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
        public void ExecuteTestCase(string jsFilePath)
        {
            var options = new ParserOptions
            {
                Range = true,
                Loc = true,
                Tokens = true,
                SourceType = SourceType.Script
            };

            string treeFilePath, failureFilePath, moduleFilePath;
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

            // Convert to LF to match the number of chars the parser finds
            var script = File.ReadAllText(jsFilePath);
            script = script.Replace(Environment.NewLine, "\n");

            if (jsFilePath.EndsWith(".source.js"))
            {
                var parser = new JavaScriptParser(script);
                var program = parser.ParseProgram();
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

            options.SourceType = isModule
                ? SourceType.Module
                : SourceType.Script;

            if (File.Exists(moduleFilePath))
            {
                options.SourceType = SourceType.Module;
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

            invalid |=
                filename.Contains("error") ||
                filename.Contains("invalid");

            if (!invalid)
            {
                options.Tolerant = true;

                var actual = ParseAndFormat(script, options);
                Assert.True(CompareTrees(actual, expected), jsFilePath);
            }
            else
            {
                options.Tolerant = false;

                // TODO: check the accuracy of the message and of the location
                Assert.Throws<Error>(() => ParseAndFormat(script, options));
            }
        }

        public static IEnumerable<object[]> SourceFiles(string relativePath)
        {
            var assemblyPath = new Uri(typeof(Fixtures).GetTypeInfo().Assembly.CodeBase).LocalPath;
            var assemblyDirectory = new FileInfo(assemblyPath).Directory;

            var root = assemblyDirectory.Parent.Parent.Parent.FullName;
            var fixturesPath = Path.Combine(root, relativePath);

            var files = Directory.GetFiles(fixturesPath, "*.js", SearchOption.AllDirectories);

            return files
                .Select(x => new object[] { x })
                .ToList();
        }

        [Fact]
        public void CommentsAreParsed()
        {
            int count = 0;
            Action<INode> action = node => count++;
            var parser = new JavaScriptParser("// this is a comment", new ParserOptions(), action);
            parser.ParseProgram();

            Assert.Equal(1, count);
        }
    }
}
