namespace CherryTypes.Tests;

[TestClass]
public sealed class TypeHelperTests
{
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

    [TestMethod]
    public void StringifyGenericSystemType()
    {
        Assert.AreEqual(
            "System.Collections.Generic.Dictionary`2",
            TypeHelper.Stringify(typeof(System.Collections.Generic.Dictionary<,>))
        );
        Assert.AreEqual(
            "System.Collections.Generic.Dictionary`2<System.String, System.Int32>",
            TypeHelper.Stringify(typeof(System.Collections.Generic.Dictionary<string, int>))
        );
    }

    [TestMethod]
    public void ParseGenericSystemType()
    {
        Assert.AreEqual(
            typeof(System.Collections.Generic.Dictionary<,>),
            TypeHelper.Parse("System.Collections.Generic.Dictionary`2")
        );
        Assert.AreEqual(
            typeof(System.Collections.Generic.Dictionary<string, int>),
            TypeHelper.Parse("System.Collections.Generic.Dictionary`2<System.String, System.Int32>")
        );
    }

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

    [TestMethod]
    public void StringifyGenericExternalType()
    {
        Assert.AreEqual(
            "[CherryTypes.Tests]TypeMocks.Baz`1<[CherryTypes.Tests]TypeMocks.Foo>",
            TypeHelper.Stringify(typeof(TypeMocks.Baz<TypeMocks.Foo>))
        );
    }

    [TestMethod]
    public void ParseGenericExternalType()
    {
        Assert.AreEqual(
            typeof(TypeMocks.Baz<TypeMocks.Foo>),
            TypeHelper.Parse("[CherryTypes.Tests]TypeMocks.Baz`1<[CherryTypes.Tests]TypeMocks.Foo>")
        );
    }
}
