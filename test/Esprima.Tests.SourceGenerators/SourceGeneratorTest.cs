namespace Esprima.Tests.SourceGenerators;

[UsesVerify]
public class SourceGeneratorTest
{
    protected static string LoadEsprimaSourceFile(string file)
    {
        return File.ReadAllText(ToEsprimaSourcePath(file));
    }

    private static string ToEsprimaSourcePath(string path)
    {
        return Path.Combine("../../../../../src/Esprima/", path);
    }
}
