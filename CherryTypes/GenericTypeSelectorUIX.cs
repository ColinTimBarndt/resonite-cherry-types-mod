using Elements.Core;

using FrooxEngine;
using FrooxEngine.UIX;

namespace CherryTypes;

/// <summary>
/// Builds UIX for picking generic component types.
/// </summary>
internal static class GenericTypeSelectorUIX {
	public static colorX COLOR_FAVORITE => RadiantUI_Constants.Sub.YELLOW;
	public static colorX COLOR_NORMAL => RadiantUI_Constants.Sub.CYAN;

	/// <summary>
	/// Builds UIX for manually providing the generic arguments.
	/// </summary>
	internal static void BuildCustomArgsUI(ComponentSelectorContext ctx, UIBuilder ui, Type genType) {
		var types = ctx.selector.World.Types;
		ui.PushStyle();
		var text = "ComponentSelector.CustomGenericArguments".AsLocaleKey();
		ui.Text(in text);

		var genTypeStr = types.EncodeType(genType);
		Type[] args = genType.GetGenericArguments();
		for (int j = 0; j < args.Length; j++) {
			text = args[j].Name;
			var root = ui.HorizontalLayout(spacing: 8).Slot;
			root.Name = "CustomGenericArgument";
			root.Tag = genTypeStr;
			ui.Style.FlexibleWidth = 1f;
			TextField textField = ui.HorizontalElementWithLabel(in text, 0.05f, () => ui.TextField(parseRTF: false));
			textField.Text.Content.Changed += OnTypeFieldChanged;
			ui.Style.FlexibleWidth = -1f;

			if (ctx.selector.GenericArgumentPrefiller.Target != null) {
				textField.TargetString = ctx.selector.GenericArgumentPrefiller.Target(genType, args[j]);
			}
			ctx._customGenericArguments.Add(textField);

			ui.Style.MinWidth = 32f;
			var btn = ui.Button(OfficialAssets.Common.Icons.Star, RadiantUI_Constants.Neutrals.MID, ComponentSelectorUIX.GetFavButtonColor(false));
			btn.Slot.PersistentSelf = false;
			btn.Slot.Name = "FavButton";
			btn.LocalReleased += OnTypeFieldFavPressed;
			ui.Style.MinWidth = -1f;
			OnTypeFieldChanged(textField.Text.Content);

			ui.NestOut();
		}
		ui.PopStyle();

		text = "";
		Button customTypeBtn = ui.Button(in text, RadiantUI_Constants.BUTTON_COLOR, ComponentSelectorPatch.onCreateCustomType(ctx.selector), 0.35f);
		ctx._customGenericTypeLabel.Target = customTypeBtn.Label.Content;
		ctx._customGenericTypeColor.Target = customTypeBtn.BaseColor;
	}

	private static void OnTypeFieldChanged(IChangeable changeable) {
		if (changeable is not Sync<string> text)
			return;

		var root = text.Slot.FindParent(slot => slot.Name == "CustomGenericArgument");

		var button = root.FindChild("FavButton");
		if (button == null)
			return;

		var parsed = string.IsNullOrEmpty(text) ? null : NiceTypeParser.TryParse(text);
		button.ActiveSelf = parsed != null;
		if (parsed == null)
			return;

		var image = button.Children.FirstOrDefault()?.GetComponent<Image>();
		if (image == null)
			return;

		var favorites = CherryTypes.FavoritesConfig!.Types;
		bool isFav = favorites.Contains(parsed);
		image.Tint.Value = ComponentSelectorUIX.GetFavButtonColor(isFav);
	}

	private static void OnTypeFieldFavPressed(IButton btn, ButtonEventData _) {
		var textField = btn.Slot.Parent.GetComponentInChildren<TextField>();
		if (textField == null || string.IsNullOrEmpty(textField.Text.Content))
			return;

		var parsed = NiceTypeParser.TryParse(textField.Text.Content);
		if (parsed == null)
			return;

		var favorites = CherryTypes.FavoritesConfig!.Types;
		bool isFav = favorites.Contains(parsed);

		if (isFav)
			favorites.Remove(parsed);
		else
			favorites.Add(parsed);

		var selector = btn.Slot.GetComponentInParents<ComponentSelector>();
		var root = btn.Slot.FindParent(slot => slot.Name == "CustomGenericArgument");
		if (selector == null || root?.Parent == null || string.IsNullOrEmpty(root.Tag))
			return;

		var types = btn.Slot.World.Types;
		var genType = types.DecodeType(root.Tag);
		if (genType.GetGenericArguments().Length != 1)
			return;

		Type component;
		try {
			component = genType.MakeGenericType([parsed]);
		} catch {
			return;
		}

		UpdateComponentButtonsFavType(selector, types, root.Parent, component);
	}

	/// <summary>
	/// Builds UIX for picking from a list of common types.
	/// </summary>
	internal static void BuildCommonTypesUI(ComponentSelectorContext ctx, UIBuilder ui, Type genType) {
		var types = ctx.selector.World.Types;
		var text = "ComponentSelector.CommonGenericTypes".AsLocaleKey();
		ui.Text(in text);

		colorX? tint;

		var onAddComponentPressed = ComponentSelectorPatch.onAddComponentPressed(ctx.selector);
		var favorites = CherryTypes.FavoritesConfig!.Types;

		var favList = Pool.BorrowHashSet<Type>();

		ui.PushStyle();
		int i;
		if (genType.GetGenericArguments().Length == 1) {
			tint = COLOR_FAVORITE;
			for (i = 0; i < favorites.Count; i++) {
				Type favType = favorites[i];
				try {
					favType = genType.MakeGenericType([favType]);
				} catch {
					continue;
				}
				if (!ShouldShowType(types, genType, favType))
					continue;

				favList.Add(favType);

				var btn = ui.AddComponentRow(ref text, in tint, types, favType, onAddComponentPressed, fav: true);
				btn.OrderOffset = 10 + i;
			}
		}

		i = int.MaxValue >> 4;
		tint = COLOR_NORMAL;
		foreach (Type commonType in WorkerInitializer.GetCommonGenericTypes(genType)) {
			if (!ShouldShowType(types, genType, commonType))
				continue;

			var btn = ui.AddComponentRow(ref text, in tint, types, commonType, onAddComponentPressed);
			btn.OrderOffset = 10 + i++;
			btn.ActiveSelf = !favList.Contains(commonType);
		}

		Pool.Return(ref favList);
		ui.PopStyle();
	}

	private static bool ShouldShowType(TypeManager types, Type generic, Type specific) {
		try {
			return specific.IsValidGenericType(validForInstantiation: true) && types.IsSupported(specific);
		} catch (Exception ex) {
			CherryTypes.Warn($"Exception checking validity of a generic type: {specific} for {generic}\n" + ex);
			return false;
		}
	}

	/// <summary>
	/// Creates a component option option.
	/// </summary>
	private static Slot AddComponentRow(this UIBuilder ui, ref LocaleString text, in colorX? tint, TypeManager types, Type type, ButtonEventHandler<string?> onAdd, bool fav = false) {
		var slot = ui.HorizontalLayout(spacing: 8).Slot;

		text = type.GetNiceName();
		ui.Style.FlexibleWidth = 1f;
		var typeStr = types.EncodeType(type);
		var btn = ui.Button(in text, in tint, onAdd, typeStr, 0.35f);
		btn.Label.ParseRichText.Value = false;
		ui.Style.FlexibleWidth = -1f;

		ui.Style.MinWidth = 32f;

		if (type.GetGenericArguments().Length == 1) {
			btn = ui.Button(OfficialAssets.Common.Icons.Star, RadiantUI_Constants.Neutrals.MID, ComponentSelectorUIX.GetFavButtonColor(fav));
			btn.Slot.PersistentSelf = false;
			btn.LocalReleased += ComponentSelectorUIX.OnFavPressed;
			btn.Slot.Tag = $"T{(fav ? '-' : '+')}{typeStr}";
			slot.Tag = $"{(fav ? 'F' : 'T')}:{typeStr}";
		}

		ui.Style.MinWidth = -1f;

		ui.NestOut();
		return slot;
	}

	/// <summary>
	/// Updates the UIX buttons to reflect the current favorited-state of the given type.
	/// </summary>
	internal static void UpdateComponentButtonsFavType(ComponentSelector selector, TypeManager types, Slot root, Type type) {
		var favorites = CherryTypes.FavoritesConfig!.Types;

		var favType = type.GetGenericArguments().FirstOrDefault();
		if (favType == null)
			return;

		bool isFav = favorites.Contains(favType);

		Slot? slot;
		var typeStr = types.EncodeType(type);
		string tag = $"T:{typeStr}";
		slot = root.FindChild(slot => slot.Tag == tag, maxDepth: 0);
		slot?.ActiveSelf = !isFav;

		// Update buttons on custom generic args to reflect current favorite state
		foreach (Slot child in root.Children) {
			if (child.Name != "CustomGenericArgument")
				continue;

			var textField = child.GetComponentInChildren<TextField>();
			var favBtn = child.FindChild("FavButton");
			if (textField == null || favBtn == null || !favBtn.ActiveSelf || string.IsNullOrEmpty(textField.Text.Content))
				continue;

			var parsed = NiceTypeParser.TryParse(textField.Text.Content);
			if (parsed != favType)
				continue;

			var image = favBtn.Children.FirstOrDefault()?.GetComponent<Image>();
			if (image == null)
				return;

			image.Tint.Value = ComponentSelectorUIX.GetFavButtonColor(isFav);
		}

		tag = $"F:{typeStr}";
		slot = root.FindChild(slot => slot.Tag == tag);
		// Remove the favorite entry if the item is not longer a favorite
		if (!isFav) {
			if (slot == null)
				return;

			int i = slot.ChildIndex;
			slot.Destroy();
			for (; i < root.ChildrenCount; i++) {
				var other = root[i];
				if (!other.Tag.StartsWith("F:"))
					break;
				other.OrderOffset -= 10;
			}

			return;
		}

		if (slot != null)
			return;

		// Create a favorite entry if the item is a new favorite
		var onAddComponentPressed = ComponentSelectorPatch.onAddComponentPressed(selector);
		var ui = new UIBuilder(root);
		ComponentSelectorUIX.SetupUIBuilder(ui);
		LocaleString text = null;
		colorX? tint = COLOR_FAVORITE;
		var btn = ui.AddComponentRow(ref text, in tint, types, type, onAddComponentPressed, fav: true);
		btn.OrderOffset = 10 + favorites.IndexOf(favType);
	}
}
