using System.Runtime.CompilerServices;

namespace Esprima.Tests.SourceGenerators;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        VerifySourceGenerators.Initialize();
    }
}
