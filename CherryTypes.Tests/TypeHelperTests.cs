namespace CherryTypes.Tests;

/// Tests for <see cref="TypeHelper" />.
[TestClass]
public sealed class TypeHelperTests
{
    /// Can system types be converted to strings?
    [TestMethod]
    public void StringifyBasicSystemType()
    {
        Assert.AreEqual(
            "System.String",
            TypeHelper.Stringify(typeof(string))
        );
        Assert.AreEqual(
            "System.Int32",
            TypeHelper.Stringify(typeof(int))
        );
    }

    /// Can system types be parsed?
    [TestMethod]
    public void ParseBasicSystemType()
    {
        Assert.AreEqual(
            typeof(string),
            TypeHelper.Parse("System.String")
        );
        Assert.AreEqual(
            typeof(int),
            TypeHelper.Parse("System.Int32")
        );
    }

    /// Can generic system types be converted to strings?
    [TestMethod]
    public void StringifyGenericSystemType()
    {
        Assert.AreEqual(
            "System.Collections.Generic.Dictionary`2",
            TypeHelper.Stringify(typeof(Dictionary<,>))
        );
        Assert.AreEqual(
            "System.Collections.Generic.Dictionary`2<System.String, System.Int32>",
            TypeHelper.Stringify(typeof(Dictionary<string, int>))
        );
    }

    /// Can generic system types be parsed?
    [TestMethod]
    public void ParseGenericSystemType()
    {
        Assert.AreEqual(
            typeof(Dictionary<,>),
            TypeHelper.Parse("System.Collections.Generic.Dictionary`2")
        );
        Assert.AreEqual(
            typeof(Dictionary<string, int>),
            TypeHelper.Parse("System.Collections.Generic.Dictionary`2<System.String, System.Int32>")
        );
    }

    /// Can external types be converted to strings?
    [TestMethod]
    public void StringifyBasicExternalType()
    {
        Assert.AreEqual(
            "[CherryTypes.Tests]TypeMocks.Foo",
            TypeHelper.Stringify(typeof(TypeMocks.Foo))
        );
        Assert.AreEqual(
            "[CherryTypes.Tests]TypeMocks.Bar",
            TypeHelper.Stringify(typeof(TypeMocks.Bar))
        );
    }

    /// Can external types be parsed?
    [TestMethod]
    public void ParseBasicExternalType()
    {
        Assert.AreEqual(
            typeof(TypeMocks.Foo),
            TypeHelper.Parse("[CherryTypes.Tests]TypeMocks.Foo")
        );
        Assert.AreEqual(
            typeof(TypeMocks.Bar),
            TypeHelper.Parse("[CherryTypes.Tests]TypeMocks.Bar")
        );
    }

    /// Can generic external types be converted to strings?
    [TestMethod]
    public void StringifyGenericExternalType()
    {
        Assert.AreEqual(
            "[CherryTypes.Tests]TypeMocks.Baz`1<[CherryTypes.Tests]TypeMocks.Foo>",
            TypeHelper.Stringify(typeof(TypeMocks.Baz<TypeMocks.Foo>))
        );
    }

    /// Can generic external types be parsed?
    [TestMethod]
    public void ParseGenericExternalType()
    {
        Assert.AreEqual(
            typeof(TypeMocks.Baz<TypeMocks.Foo>),
            TypeHelper.Parse("[CherryTypes.Tests]TypeMocks.Baz`1<[CherryTypes.Tests]TypeMocks.Foo>")
        );
    }
}
