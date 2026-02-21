using System.Collections;

using System.Text.Json;
using System.Text.Json.Serialization;

namespace CherryTypes;

/// <summary>
/// Hybrid of a HashSet and a List. It preserves insertion order while mainaining O(1) search time and unique items.
/// Inserting duplicate types is silently ignored.
/// </summary>
[JsonConverter(typeof(TypeHashListConverter))]
public sealed class TypeHashList : IList<Type>, ICollection {
	private readonly Dictionary<Type, int> lookup = [];
	private readonly List<Type> list = [];

	/// <summary>
	/// Gets the entry at <paramref name="index"/>.
	/// </summary>
	public Type this[int index] {
		get => list[index]; set => Insert(index, value);
	}

	/// <inheritdoc/>
	public int Count => list.Count;

	/// <inheritdoc/>
	public bool IsReadOnly => false;

	bool ICollection.IsSynchronized => false;

	object ICollection.SyncRoot => this;

	/// <inheritdoc/>
	public void Add(Type item) {
		if (lookup.TryAdd(item, list.Count))
			list.Add(item);
	}

	/// <inheritdoc/>
	public void Clear() {
		lookup.Clear();
		list.Clear();
	}

	/// <inheritdoc/>
	public bool Contains(Type item) => lookup.ContainsKey(item);

	/// <inheritdoc/>
	public void CopyTo(Type[] array, int arrayIndex) => list.CopyTo(array, arrayIndex);

	void ICollection.CopyTo(Array array, int index) => (list as ICollection).CopyTo(array, index);

	/// <inheritdoc/>
	public IEnumerator<Type> GetEnumerator() => list.GetEnumerator();

	/// <inheritdoc/>
	public int IndexOf(Type item) => lookup.TryGetValue(item, out int index) ? index : -1;

	/// <inheritdoc/>
	public void Insert(int index, Type item) {
		if (lookup.TryAdd(item, index)) {
			for (int i = index; i < list.Count; i++) {
				lookup[list[i]]++;
			}
			list.Insert(index, item);
		}
	}

	/// <inheritdoc/>
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

	/// <inheritdoc/>
	public void RemoveAt(int index) {
		var type = list[index];
		lookup.Remove(type);
		list.RemoveAt(index);
		for (int i = index; i < list.Count; i++) {
			lookup[list[i]]--;
		}
	}

	IEnumerator IEnumerable.GetEnumerator() => list.GetEnumerator();

	/// <summary>
	/// Swaps the types at the specified indices in place.
	/// </summary>
	public void Swap(int a, int b) {
		var typeA = list[a];
		var typeB = list[b];
		list[a] = typeB;
		list[b] = typeA;
		lookup[typeA] = b;
		lookup[typeB] = a;
	}

	/// <summary>
	/// Swaps the specified types in this list in place.
	/// </summary>
	/// <exception cref="IndexOutOfRangeException">
	/// When a type is not in this list.
	/// </exception>
	public void Swap(Type a, Type b) {
		Swap(IndexOf(a), IndexOf(b));
	}

	/// <summary>
	/// Adds all <paramref name="values"/> to this list.
	/// </summary>
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

/// <summary>
/// <see cref="JsonConverter{T}"/> for <see cref="TypeHashList"/>.
/// </summary>
public class TypeHashListConverter : JsonConverter<TypeHashList> {
	// For unit tests, there must be no dependency on the mod class.
	// Instead, the mod changes the soft error action when the mod initializes.
	internal static Action<string> softError = msg => throw new JsonException(msg);

	/// <inheritdoc/>
	public override TypeHashList? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
		TypeHashList instance = [];

		switch (reader.TokenType) {
			case JsonTokenType.Null:
				return instance;

			case JsonTokenType.StartArray:
				reader.Read();
				while (reader.TokenType != JsonTokenType.EndArray) {
					string? typeString = reader.GetString();
					reader.Read();
					if (typeString == null)
						continue;

					var type = TypeHelper.Parse(typeString);

					if (type != null)
						instance.Add(type);
					else
						softError($"Type '{typeString}' was not found! Ignoring.");
				}
				return instance;

			default:
				throw new JsonException("Expected array");
		}
	}

	/// <inheritdoc/>
	public override void Write(Utf8JsonWriter writer, TypeHashList value, JsonSerializerOptions options) {
		writer.WriteStartArray();
		foreach (var type in value) {
			var typeString = TypeHelper.Stringify(type);

			if (typeString != null)
				writer.WriteStringValue(typeString);
			else
				softError($"Type '{type}' cannot be serialized! Skipping.");
		}
		writer.WriteEndArray();
	}
}
