namespace CherryTypes.Tests;

[TestClass]
public static class Prepare
{
    [AssemblyInitialize]
    public static void Init(TestContext context)
    {
        TypeHelper.Init();
    }
}