using Esprima.SourceGenerators;
using static Esprima.Tests.SourceGenerators.VerifyHelper;

namespace Esprima.Tests.SourceGenerators;

public class StringMatcherGeneratorTests : SourceGeneratorTest
{
    [Fact]
    public Task ScannerStringMatchingGeneration()
    {
        return Verify<StringMatcherGenerator>(
            LoadEsprimaSourceFile("JavaScriptParser.cs"),
            LoadEsprimaSourceFile("Scanner.cs"),
            LoadEsprimaSourceFile("ParserExtensions.cs")
        );
    }
}
