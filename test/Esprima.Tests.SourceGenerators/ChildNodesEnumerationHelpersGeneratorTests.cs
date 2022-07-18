using Esprima.SourceGenerators;
using static Esprima.Tests.SourceGenerators.VerifyHelper;

namespace Esprima.Tests.SourceGenerators;

public class ChildNodesEnumerationHelpersGeneratorTests : SourceGeneratorTest
{
    [Fact]
    public Task ChildNodesGeneration()
    {
        return Verify<ChildNodesEnumerationHelpersGenerator>(
            LoadEsprimaSourceFile("Ast/ChildNodes.Helpers.cs"),
            LoadEsprimaSourceFile("Ast/Node.cs"),
            LoadEsprimaSourceFile("Ast/NodeList.cs")
        );
    }
}
