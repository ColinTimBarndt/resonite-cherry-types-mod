using Newtonsoft.Json;

namespace CherryTypes.Tests;

[TestClass]
public sealed class TypeHashListTests
{
    [TestMethod]
    public void Serialize()
    {
        TypeHashList list = [
            typeof(int),
            typeof(string),
            typeof(FrooxEngine.Slot),
            typeof(Elements.Core.float3),
        ];

        string output = JsonConvert.SerializeObject(list);

        Assert.AreEqual("""["System.Int32","System.String","[FrooxEngine]FrooxEngine.Slot","[Elements.Core]Elements.Core.float3"]""", output);
    }

    [TestMethod]
    public void Deserialize()
    {
        string input = """["System.Int32","System.String","[FrooxEngine]FrooxEngine.Slot","[Elements.Core]Elements.Core.float3"]""";

        var output = JsonConvert.DeserializeObject<TypeHashList>(input);

        Assert.IsNotNull(output);
        Assert.AreEqual(4, output.Count);
        Assert.AreEqual(typeof(int), output[0]);
        Assert.AreEqual(typeof(string), output[1]);
        Assert.AreEqual(typeof(FrooxEngine.Slot), output[2]);
        Assert.AreEqual(typeof(Elements.Core.float3), output[3]);
    }

    [TestMethod]
    public void Addition()
    {
        TypeHashList list = [typeof(int)];

        list.Add(typeof(string));

        CollectionAssert.AreEqual(
            new List<Type>([typeof(int), typeof(string)]),
            list
        );
        Assert.IsTrue(list.Contains(typeof(string)));
        Assert.AreEqual(1, list.IndexOf(typeof(string)));
    }

    [TestMethod]
    public void Insertion()
    {
        TypeHashList list = [
            typeof(int),
            typeof(string),
            typeof(FrooxEngine.Slot),
            typeof(Elements.Core.float3),
        ];

        list[1] = typeof(float);

        CollectionAssert.AreEqual(
            new List<Type>([typeof(int), typeof(float), typeof(string), typeof(FrooxEngine.Slot), typeof(Elements.Core.float3)]),
            list
        );
        Assert.AreEqual(0, list.IndexOf(typeof(int)));
        Assert.AreEqual(1, list.IndexOf(typeof(float)));
        Assert.AreEqual(2, list.IndexOf(typeof(string)));
        Assert.AreEqual(3, list.IndexOf(typeof(FrooxEngine.Slot)));
        Assert.AreEqual(4, list.IndexOf(typeof(Elements.Core.float3)));

        list[2] = typeof(int);

        CollectionAssert.AreEqual(
            new List<Type>([typeof(int), typeof(float), typeof(string), typeof(FrooxEngine.Slot), typeof(Elements.Core.float3)]),
            list
        );
    }

    [TestMethod]
    public void Removal()
    {
        TypeHashList list = [
            typeof(int),
            typeof(string),
            typeof(FrooxEngine.Slot),
            typeof(Elements.Core.float3),
        ];

        list.Remove(typeof(FrooxEngine.Slot));

        CollectionAssert.AreEqual(
            new List<Type>([typeof(int), typeof(string), typeof(Elements.Core.float3)]),
            list
        );
        Assert.AreEqual(0, list.IndexOf(typeof(int)));
        Assert.AreEqual(1, list.IndexOf(typeof(string)));
        Assert.AreEqual(-1, list.IndexOf(typeof(FrooxEngine.Slot)));
        Assert.AreEqual(2, list.IndexOf(typeof(Elements.Core.float3)));
    }
}