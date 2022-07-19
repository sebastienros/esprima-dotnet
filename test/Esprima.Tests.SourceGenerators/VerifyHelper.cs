using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Esprima.Tests.SourceGenerators;

public static class VerifyHelper
{
    public static Task Verify<T>(params string[] sources) where T : class, IIncrementalGenerator, new()
    {
        var syntaxTrees = sources.Select(x => CSharpSyntaxTree.ParseText(x));

        var compilation = CSharpCompilation.Create(
            assemblyName: "Tests",
            syntaxTrees);

        var generator = new T();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        driver = driver.RunGenerators(compilation);

        return Verifier
            .Verify(driver)
            .UseDirectory("Snapshots");
    }
}
