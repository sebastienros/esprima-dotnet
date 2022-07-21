using Test262Harness;

namespace Esprima.Tests.Test262;

public abstract partial class Test262Test
{
    private JavaScriptParser BuildTestExecutor(Test262File file)
    {
        var options = new ParserOptions(file.FileName)
        {
            Tolerant = false
        };
        return new JavaScriptParser(file.Program, options);
    }

    private static void ExecuteTest(JavaScriptParser parser, Test262File file)
    {
        if (file.Type == ProgramType.Script)
        {
            parser.ParseScript(file.Strict);
        }
        else
        {
            parser.ParseModule();
        }
    }

    private partial bool ShouldThrow(Test262File testCase, bool strict)
    {
        return testCase.NegativeTestCase?.Type == ExpectedErrorType.SyntaxError || testCase.NegativeTestCase?.Phase == TestingPhase.Parse;
    }
}
