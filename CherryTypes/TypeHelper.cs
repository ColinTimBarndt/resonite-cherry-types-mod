using System.Reflection;
using System.Text;

using Elements.Core;

namespace CherryTypes;

/// <summary>
/// Converts between strings and types in a consistent representation independent of the data model.
/// </summary>
public static class TypeHelper {
	private static readonly Dictionary<Assembly, string?> assemblyNames = [];
	private static readonly Dictionary<string, Assembly> assemblies = [];
	private static readonly Assembly system = typeof(int).Assembly;

	public static void Init() {
		if (assemblies.Count != 0)
			throw new InvalidOperationException("Already initialized");

		foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
			var name = assembly.GetName().Name;
			if (name != null)
				assemblyNames.TryAdd(assembly, name);
		}

		assemblyNames[system] = null;

		foreach (var (assembly, name) in assemblyNames) {
			if (name != null)
				assemblies.TryAdd(name, assembly);
		}
	}

	public static string? Stringify(Type type) {
		if (type.IsGenericParameter)
			return null;

		bool hasParams = type.ContainsGenericParameters;
		if (hasParams)
			type = type.GetGenericTypeDefinition();

		var fullName = (!type.IsGenericType || type.IsGenericTypeDefinition ? type : type.GetGenericTypeDefinition()).FullName;
		if (!assemblyNames.TryGetValue(type.Assembly, out var assembly) || fullName == null)
			return null;

		var baseName = assembly == null ? fullName : $"[{assembly}]{fullName}";

		if (!type.IsGenericType)
			return baseName;

		if (hasParams)
			return baseName;

		var args = type.GenericTypeArguments;

		var builder = new StringBuilder(baseName);
		builder.Append("<");
		for (int i = 0; i < args.Length; i++) {
			if (i != 0) builder.Append(", ");
			var argStr = Stringify(args[i]);
			if (argStr == null) return null;
			builder.Append(argStr);
		}
		builder.Append(">");

		return builder.ToString();
	}

	public static Type? Parse(string text) {
		int pos = 0;
		return ParseInternal(text, ref pos);
	}

	private static Type? ParseInternal(string text, ref int pos) {
		SkipWhitespace(text, ref pos);
		char ch = '\0';

		Assembly? assembly;
		if (pos == text.Length)
			return null;
		if (text[pos] == '[') {
			int assemblyStart = ++pos;
			// Find end of assembly name
			while (pos < text.Length && text[pos] != ']')
				pos++;

			var assemblyName = text[assemblyStart..pos];
			if (!assemblies.TryGetValue(assemblyName, out assembly))
				return null;
			pos++;
		} else {
			assembly = system;
		}

		int typeStart = pos;

		// Find end of type name
		while (pos < text.Length && (ch = text[pos]) != '<' && ch != '>' && ch != ',')
			pos++;

		var typeName = text[typeStart..pos];
		var baseType = assembly.GetType(typeName);
		if (baseType == null)
			return null;

		if (ch != '<')
			return baseType;

		if (++pos >= text.Length)
			return null;

		// Parse generic parameters
		var args = Pool.BorrowList<Type>();
		try {
			while (true) {
				var genArg = ParseInternal(text, ref pos);
				if (genArg == null)
					return null;
				args.Add(genArg);
				switch (text[pos]) {
					case ',':
						pos++;
						SkipWhitespace(text, ref pos);
						continue;
					case '>':
						pos++;
						goto End;
					default:
						return null;
				}
			}
		End:
			try {
				return baseType.MakeGenericType([.. args]);
			} catch {
				return null;
			}
		} finally {
			Pool.Return(ref args);
		}
	}

	private static void SkipWhitespace(string text, ref int pos) {
		while (pos < text.Length && char.IsWhiteSpace(text[pos]))
			pos++;
	}
}
