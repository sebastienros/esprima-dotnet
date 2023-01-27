using System.IO;
using System.Runtime.CompilerServices;
using Esprima.SourceGenerators;
using Microsoft.CodeAnalysis;
using static Esprima.Tests.SourceGenerators.VerifyHelper;

namespace Esprima.Tests.SourceGenerators;

public class VisitationBoilerplateGeneratorTests : SourceGeneratorTest
{
    [Fact]
    public Task VisitationBoilerplateGeneration()
    {
        var sourceFiles = Directory.EnumerateFiles(ToEsprimaSourcePath("Ast"), "*.cs", SearchOption.AllDirectories)
            .Concat(new[]
            {
                ToEsprimaSourcePath("Utils/AstVisitor.cs"),
                ToEsprimaSourcePath("Utils/AstRewriter.cs"),
                ToEsprimaSourcePath("Utils/Jsx/IJsxAstVisitor.cs"),
                ToEsprimaSourcePath("Utils/Jsx/JsxAstVisitor.cs"),
                ToEsprimaSourcePath("Utils/Jsx/JsxAstRewriter.cs"),
            })
            .Select(File.ReadAllText)
            .ToArray();

        var references =
            new[]
            {
                typeof(object).Assembly,
            }
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
            .ToArray();

        return Verify<VisitationBoilerplateGenerator>(sourceFiles, references);
    }
}
