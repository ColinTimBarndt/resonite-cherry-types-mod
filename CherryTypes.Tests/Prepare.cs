namespace CherryTypes.Tests;

/// Contains the method to run before all tests.
[TestClass]
public static class Prepare
{
    /// Initializes the <see cref="TypeHelper" />'s type index.
    [AssemblyInitialize]
    public static void Init(TestContext context)
    {
        TypeHelper.Init();
    }
}