using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Esprima.Tests.SourceGenerators;

public static class VerifyHelper
{
    // https://github.com/dotnet/sdk/blob/v6.0.405/src/Tasks/Microsoft.NET.Build.Tasks/targets/Microsoft.NET.Sdk.CSharp.props#L27
    private static readonly string s_implicitUsingsSourceText =
@"global using global::System;
global using global::System.Collections.Generic;
global using global::System.IO;
global using global::System.Linq;
global using global::System.Net.Http;
global using global::System.Threading;
global using global::System.Threading.Tasks;
";

    public static Task Verify<T>(params string[] sources) where T : class, IIncrementalGenerator, new()
    {
        return Verify<T>(sources, references: null);
    }

    public static Task Verify<T>(string[] sources, MetadataReference[]? references) where T : class, IIncrementalGenerator, new()
    {
        var syntaxTrees = sources
            .Prepend(s_implicitUsingsSourceText)
            .Select(x => CSharpSyntaxTree.ParseText(x));

        Compilation compilation = CSharpCompilation.Create(
            assemblyName: "Tests",
            syntaxTrees,
            references);

        var generator = new T();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);

        return Verifier
            .Verify(driver.GetRunResult())
            .UseDirectory("Snapshots");
    }
}
