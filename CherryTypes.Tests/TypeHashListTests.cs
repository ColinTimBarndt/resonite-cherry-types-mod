using System.Text.Encodings.Web;
using System.Text.Json;

namespace CherryTypes.Tests;

[TestClass]
public sealed class TypeHashListTests
{
    static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Strict)
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    [TestMethod]
    public void Serialize()
    {
        TypeHashList list = [
            typeof(int),
            typeof(string),
            typeof(TypeMocks.Foo),
            typeof(TypeMocks.Baz<int>),
        ];

        string output = JsonSerializer.Serialize(list, JsonOptions);

        Assert.AreEqual("""["System.Int32","System.String","[CherryTypes.Tests]TypeMocks.Foo","[CherryTypes.Tests]TypeMocks.Baz`1<System.Int32>"]""", output);
    }

    [TestMethod]
    public void Deserialize()
    {
        string input = """["System.Int32","System.String","[CherryTypes.Tests]TypeMocks.Foo","[CherryTypes.Tests]TypeMocks.Baz`1<System.Int32>"]""";

        var output = JsonSerializer.Deserialize<TypeHashList>(input);

        Assert.IsNotNull(output);
        Assert.AreEqual(4, output.Count);
        Assert.AreEqual(typeof(int), output[0]);
        Assert.AreEqual(typeof(string), output[1]);
        Assert.AreEqual(typeof(TypeMocks.Foo), output[2]);
        Assert.AreEqual(typeof(TypeMocks.Baz<int>), output[3]);
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
            typeof(TypeMocks.Foo),
            typeof(TypeMocks.Bar),
        ];

        list[1] = typeof(float);

        CollectionAssert.AreEqual(
            new List<Type>([typeof(int), typeof(float), typeof(string), typeof(TypeMocks.Foo), typeof(TypeMocks.Bar)]),
            list
        );
        Assert.AreEqual(0, list.IndexOf(typeof(int)));
        Assert.AreEqual(1, list.IndexOf(typeof(float)));
        Assert.AreEqual(2, list.IndexOf(typeof(string)));
        Assert.AreEqual(3, list.IndexOf(typeof(TypeMocks.Foo)));
        Assert.AreEqual(4, list.IndexOf(typeof(TypeMocks.Bar)));

        list[2] = typeof(int);

        CollectionAssert.AreEqual(
            new List<Type>([typeof(int), typeof(float), typeof(string), typeof(TypeMocks.Foo), typeof(TypeMocks.Bar)]),
            list
        );
    }

    [TestMethod]
    public void Removal()
    {
        TypeHashList list = [
            typeof(int),
            typeof(string),
            typeof(TypeMocks.Foo),
            typeof(TypeMocks.Bar),
        ];

        list.Remove(typeof(TypeMocks.Foo));

        CollectionAssert.AreEqual(
            new List<Type>([typeof(int), typeof(string), typeof(TypeMocks.Bar)]),
            list
        );
        Assert.AreEqual(0, list.IndexOf(typeof(int)));
        Assert.AreEqual(1, list.IndexOf(typeof(string)));
        Assert.AreEqual(-1, list.IndexOf(typeof(TypeMocks.Foo)));
        Assert.AreEqual(2, list.IndexOf(typeof(TypeMocks.Bar)));
    }
}