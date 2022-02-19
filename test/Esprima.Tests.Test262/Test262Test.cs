using Test262Harness;

namespace Esprima.Tests.Test262;

public abstract partial class Test262Test
{
    private JavaScriptParser BuildTestExecutor(Test262File file)
    {
        return new JavaScriptParser(file.Program, new ParserOptions(file.FileName));
    }

    private static void ExecuteTest(JavaScriptParser parser, Test262File file)
    {
        parser.ParseScript(file.Strict);
    }
}
