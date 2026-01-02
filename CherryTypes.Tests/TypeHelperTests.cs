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
    public void StringifyBasicResoniteType()
    {
        Assert.AreEqual(
            "[Elements.Core]Elements.Core.float3",
            TypeHelper.Stringify(typeof(Elements.Core.float3))
        );
        Assert.AreEqual(
            "[FrooxEngine]FrooxEngine.Slot",
            TypeHelper.Stringify(typeof(FrooxEngine.Slot))
        );
    }

    [TestMethod]
    public void ParseBasicResoniteType()
    {
        Assert.AreEqual(
            typeof(Elements.Core.float3),
            TypeHelper.Parse("[Elements.Core]Elements.Core.float3")
        );
        Assert.AreEqual(
            typeof(FrooxEngine.Slot),
            TypeHelper.Parse("[FrooxEngine]FrooxEngine.Slot")
        );
    }

    [TestMethod]
    public void StringifyGenericResoniteType()
    {
        Assert.AreEqual(
            "[FrooxEngine]FrooxEngine.ValueField`1<[Elements.Core]Elements.Core.float3>",
            TypeHelper.Stringify(typeof(FrooxEngine.ValueField<Elements.Core.float3>))
        );
        Assert.AreEqual(
            "[FrooxEngine]FrooxEngine.ReferenceField`1<[FrooxEngine]FrooxEngine.Slot>",
            TypeHelper.Stringify(typeof(FrooxEngine.ReferenceField<FrooxEngine.Slot>))
        );
    }

    [TestMethod]
    public void ParseGenericResoniteType()
    {
        Assert.AreEqual(
            typeof(FrooxEngine.ValueField<Elements.Core.float3>),
            TypeHelper.Parse("[FrooxEngine]FrooxEngine.ValueField`1<[Elements.Core]Elements.Core.float3>")
        );
        Assert.AreEqual(
            typeof(FrooxEngine.ReferenceField<FrooxEngine.Slot>),
            TypeHelper.Parse("[FrooxEngine]FrooxEngine.ReferenceField`1<[FrooxEngine]FrooxEngine.Slot>")
        );
    }
}
