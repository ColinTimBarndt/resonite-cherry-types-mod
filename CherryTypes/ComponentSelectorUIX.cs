using System.Diagnostics.CodeAnalysis;
using System.Reflection;

using Elements.Core;

using FrooxEngine;
using FrooxEngine.UIX;

namespace CherryTypes;

internal static class ComponentSelectorUIX {
	internal static void SetupUIBuilder(UIBuilder ui) {
		RadiantUI_Constants.SetupEditorStyle(ui, extraPadding: true);
		ui.Style.TextAlignment = Alignment.MiddleLeft;
		ui.Style.ButtonTextAlignment = Alignment.MiddleLeft;
		ui.Style.MinHeight = 32f;
	}

	internal static void BuildGenericTypeUI(ComponentSelectorContext ctx, string? group, bool doNotGenerateBack) {
		string sourcePath = PathUtility.GetDirectoryName(ctx.path).Replace('\\', '/');
		Type? genType = Type.GetType(PathUtility.GetFileName(ctx.path));
		if (genType == null || !genType.IsGenericType)
			return;

		var ui = ctx.ui;

		ctx._genericType.Value = genType!;
		if (!doNotGenerateBack) {
			if (group != null) {
				sourcePath = sourcePath + "?" + group;
			}
			ui.Button(
				"ComponentSelector.Back".AsLocaleKey(),
				RadiantUI_Constants.BUTTON_COLOR,
				ComponentSelectorPatch.onOpenCategoryPressed(ctx.selector),
				sourcePath,
				doublePressDelay: 0.35f
			);
		}

		GenericTypeSelectorUIX.BuildCustomArgsUI(ctx, ui, genType);
		GenericTypeSelectorUIX.BuildCommonTypesUI(ctx, ui, genType);
	}

	internal static colorX GetFavButtonColor(bool isFav)
		=> isFav
			? RadiantUI_Constants.Hero.YELLOW
			: RadiantUI_Constants.DarkLight.YELLOW;

	internal static bool TryGetKind(this ComponentSelector selector, [NotNullWhen(true)] out FavoritesCategory category) {
		var type = selector.ComponentSelected.Target?.Method.DeclaringType;

		if (type == typeof(FrooxEngine.SceneInspector)) {
			category = FavoritesCategory.Components;
			return true;
		}

		if (type == typeof(FrooxEngine.ProtoFlux.ProtoFluxTool)) {
			category = FavoritesCategory.ProtoFluxNodes;
			return true;
		}

		category = (FavoritesCategory)(-1);
		return false;
	}

	/// <summary>
	/// Button event handler for all component favorite buttons.
	/// </summary>
	internal static void OnFavPressed(IButton btn, ButtonEventData _) {
		// Tag: <category> <add/remove> <type>
		// category: 'T' = Type, 'C' = Component, 'P' = ProtoFlux Node
		// add/remove: '+' or '-'
		string? tag = btn.Slot.Tag;
		if (string.IsNullOrEmpty(tag) || tag.Length < 3) return;
		sbyte setFav = tag[1] switch {
			'+' => 1,
			'-' => 0,
			_ => -1,
		};
		if (setFav == -1)
			return;

		var types = btn.World.Types;
		var typeStr = tag[2..];
		var type = types.DecodeType(typeStr);
		if (type == null)
			return;

		var selector = btn.Slot.GetComponentInParents<ComponentSelector>();
		if (selector == null)
			return;

		FavoritesCategory category;
		Type? favType = type;
		switch (tag[0]) {
			case 'T':
				category = FavoritesCategory.Types;
				favType = type.GetGenericArguments().FirstOrDefault();
				if (favType == null)
					return;
				break;

			case 'C':
				category = FavoritesCategory.Components;
				break;

			case 'P':
				category = FavoritesCategory.ProtoFluxNodes;
				break;

			default:
				return;
		}

		var favorites = CherryTypes.FavoritesConfig![category];
		if (setFav == 1)
			favorites.Add(favType);
		else
			favorites.Remove(favType);

		if (category != FavoritesCategory.Types)
			return;

		var root = btn.Slot?.Parent?.Parent;
		if (root == null)
			return;

		GenericTypeSelectorUIX.UpdateComponentButtonsFavType(selector, types, root, type);
	}

	// TODO: Move parts into its own class
	internal static void BuildCategoryUI(ComponentSelectorContext ctx, string? group, bool doNotGenerateBack) {
		var types = ctx.selector.World.Types;
		var ui = ctx.ui;
		LocaleString text;
		colorX? tint;

		CategoryNode<Type> root;
		if (string.IsNullOrEmpty(ctx.path) || ctx.path == "/") {
			root = WorkerInitializer.ComponentLibrary;
		} else {
			root = WorkerInitializer.ComponentLibrary.GetSubcategory(ctx.path);
			if (root == null) {
				root = WorkerInitializer.ComponentLibrary;
				ctx.path = "";
			}
		}

		if (root != WorkerInitializer.ComponentLibrary && !doNotGenerateBack) {
			text = "ComponentSelector.Back".AsLocaleKey();
			tint = RadiantUI_Constants.BUTTON_COLOR;
			ui.Button(in text, in tint, ComponentSelectorPatch.onOpenCategoryPressed(ctx.selector), (group == null) ? root.Parent.GetPath() : ctx.path, 0.35f);
		}
		KeyCounter<string>? groupCounter = null;
		HashSet<string>? generatedGroups = null;
		if (group == null) {
			// Buttons for subcategories
			foreach (CategoryNode<Type> cat in root.Subcategories) {
				text = cat.Name + " >";
				tint = RadiantUI_Constants.Sub.YELLOW;
				ui.Button(in text, in tint, ComponentSelectorPatch.onOpenCategoryPressed(ctx.selector), ctx.path + "/" + cat.Name, 0.35f)
					.Label.ParseRichText.Value = false;
			}

			groupCounter = [];
			generatedGroups = [];
			foreach (Type type2 in root.Elements) {
				if (!types.IsSupported(type2))
					continue;

				var grouping2 = type2.GetCustomAttribute<GroupingAttribute>();
				if (grouping2 != null) {
					groupCounter.Increment(grouping2.GroupName);
				}
			}
		}

		List<Button> navButtons = Pool.BorrowList<Button>();
		var openGroupPressed = ComponentSelectorPatch.openGroupPressed(ctx.selector);
		var openGenericTypesPressed = ComponentSelectorPatch.openGenericTypesPressed(ctx.selector);
		var onAddComponentPressed = ComponentSelectorPatch.onAddComponentPressed(ctx.selector);

		foreach (Type type in root.Elements) {
			if (!types.IsSupported(type) || (ctx.selector.ComponentFilter.Target != null && !ctx.selector.ComponentFilter.Target(type))) {
				continue;
			}
			var grouping = type.GetCustomAttribute<GroupingAttribute>();
			if (group != null && grouping?.GroupName != group) {
				continue;
			}
			Button button;
			// if group is null, then groupCounter and generatedGroups are not null
			if (group == null && grouping != null && groupCounter![grouping.GroupName] > 1) {
				// This is a group of types (a special kind of subcategory in a sense)
				if (!generatedGroups!.Add(grouping.GroupName)) {
					continue;
				}
				var name = grouping.GroupName.Split('.')?.Last();
				text = name;
				tint = RadiantUI_Constants.Sub.PURPLE;
				button = ui.Button(in text, in tint, openGroupPressed, ctx.path + ":" + grouping.GroupName, 0.35f);
				navButtons.Add(button);
			} else if (type.IsGenericTypeDefinition) {
				// This is a generic type
				if (ctx.path == null || type.AssemblyQualifiedName == null)
					continue;

				string newPath = Path.Combine(ctx.path, type.AssemblyQualifiedName);
				if (group != null) {
					newPath = newPath + "?" + group;
				}
				text = type.GetNiceName();
				tint = RadiantUI_Constants.Sub.GREEN;
				button = ui.Button(in text, in tint, openGenericTypesPressed, newPath, 0.35f);
				navButtons.Add(button);
			} else {
				// This is a regular type
				text = type.GetNiceName();
				tint = RadiantUI_Constants.Sub.CYAN;
				button = ui.Button(in text, in tint, onAddComponentPressed, types.EncodeType(type), 0.35f);
			}
			button.Label.ParseRichText.Value = false;
			navButtons.Add(button);
		}
		navButtons.Sort((a, b) => a.LabelText.CompareTo(b.LabelText));
		for (int i = 0; i < navButtons.Count; i++) {
			navButtons[i].Slot.OrderOffset = 10 + i;
		}
		Pool.Return(ref navButtons);
	}
}
