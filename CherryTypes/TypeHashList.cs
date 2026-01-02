using System.Collections;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CherryTypes;

/// <summary>
/// Hybrid of a HashSet and a List. It preserves insertion order while mainaining O(1) search time and unique items.
/// Inserting duplicate types is silently ignored.
/// </summary>
[JsonConverter(typeof(TypeHashListConverter))]
public sealed class TypeHashList : IList<Type>, ICollection {
	private readonly Dictionary<Type, int> lookup = [];
	private readonly List<Type> list = [];

	public Type this[int index] {
		get => list[index]; set => Insert(index, value);
	}

	public int Count => list.Count;

	public bool IsReadOnly => false;

	bool ICollection.IsSynchronized => false;

	object ICollection.SyncRoot => this;

	public void Add(Type item) {
		if (lookup.TryAdd(item, list.Count))
			list.Add(item);
	}

	public void Clear() {
		lookup.Clear();
		list.Clear();
	}

	public bool Contains(Type item) => lookup.ContainsKey(item);

	public void CopyTo(Type[] array, int arrayIndex) => list.CopyTo(array, arrayIndex);

	void ICollection.CopyTo(Array array, int index) => (list as ICollection).CopyTo(array, index);

	public IEnumerator<Type> GetEnumerator() => list.GetEnumerator();

	public int IndexOf(Type item) => lookup.TryGetValue(item, out int index) ? index : -1;

	public void Insert(int index, Type item) {
		if (lookup.TryAdd(item, index)) {
			for (int i = index; i < list.Count; i++) {
				lookup[list[i]]++;
			}
			list.Insert(index, item);
		}
	}

	public bool Remove(Type item) {
		if (lookup.Remove(item, out int index)) {
			list.RemoveAt(index);
			for (int i = index; i < list.Count; i++) {
				lookup[list[i]]--;
			}
			return true;
		}
		return false;
	}

	public void RemoveAt(int index) {
		var type = list[index];
		lookup.Remove(type);
		list.RemoveAt(index);
		for (int i = index; i < list.Count; i++) {
			lookup[list[i]]--;
		}
	}

	IEnumerator IEnumerable.GetEnumerator() => list.GetEnumerator();

	public void Swap(int a, int b) {
		var typeA = list[a];
		var typeB = list[b];
		list[a] = typeB;
		list[b] = typeA;
		lookup[typeA] = b;
		lookup[typeB] = a;
	}

	public void Swap(Type a, Type b) {
		Swap(IndexOf(a), IndexOf(b));
	}

	public void AddRange(IEnumerable<Type> values) {
		if (list.Count == 0) {
			list.AddRange(values);
			int count = list.Count;
			for (int i = 0; i < count; i++)
				lookup.Add(list[i], i);
			return;
		}
		foreach (var type in values)
			Add(type);
	}
}

public class TypeHashListConverter : JsonConverter<TypeHashList> {
	// For unit tests, there must be no dependency on the mod class.
	// Instead, the mod changes the soft error action when the mod initializes.
	internal static Action<string> softError = msg => throw new JsonSerializationException(msg);

	public override TypeHashList? ReadJson(JsonReader reader, Type objectType, TypeHashList? existingValue, bool hasExistingValue, JsonSerializer serializer) {
		TypeHashList? instance = existingValue;
		if (instance == null)
			instance = [];
		else
			instance.Clear();

		if (reader.TokenType == JsonToken.Null)
			return instance;

		foreach (var token in JArray.Load(reader)) {
			if (token.Type != JTokenType.String)
				throw new JsonSerializationException("Expected string when deserializing TypeHashList.");

			string typeString = token.Value<string>()!;
			var type = TypeHelper.Parse(typeString);

			if (type != null)
				instance.Add(type);
			else
				softError($"Type '{typeString}' was not found! Ignoring.");
		}

		return instance;
	}

	public override void WriteJson(JsonWriter writer, TypeHashList? value, JsonSerializer serializer) {
		writer.WriteStartArray();
		if (value != null) {
			foreach (var type in value) {
				var typeString = TypeHelper.Stringify(type);

				if (typeString != null)
					writer.WriteValue(typeString);
				else
					softError($"Type '{type}' cannot be serialized! Skipping.");
			}
		}
		writer.WriteEndArray();
	}
}
