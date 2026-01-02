using System.Reflection;

namespace CherryTypes.Tests;

[TestClass]
public static class Prepare
{
    [AssemblyInitialize]
    public static void Init(TestContext context)
    {
        // Preload assemblies such that they are in the lookup table.
        Assembly.Load("FrooxEngine");
        Assembly.Load("Elements.Core");
        TypeHelper.Init();
    }
}