using System.Reflection;

using Elements.Core;

using FrooxEngine;
using FrooxEngine.UIX;

namespace CherryTypes;

internal static class ComponentSelectorUIX {
	internal static void BuildGenericTypeUI(ComponentSelector selector, ComponentSelectorContext ctx, string? group, bool doNotGenerateBack) {
		string sourcePath = PathUtility.GetDirectoryName(ctx.path).Replace('\\', '/');
		Type? genType = Type.GetType(PathUtility.GetFileName(ctx.path));
		if (genType == null)
			return;

		var ui = ctx.ui;
		LocaleString text;
		colorX? tint;

		ctx._genericType.Value = genType!;
		if (!doNotGenerateBack) {
			if (group != null) {
				sourcePath = sourcePath + "?" + group;
			}
			text = "ComponentSelector.Back".AsLocaleKey();
			tint = RadiantUI_Constants.BUTTON_COLOR;
			ui.Button(in text, in tint, ComponentSelectorPatch.onOpenCategoryPressed(selector), sourcePath, 0.35f);
		}
		text = "ComponentSelector.CustomGenericArguments".AsLocaleKey();
		ui.Text(in text);
		Type[] args = genType.GetGenericArguments();
		for (int j = 0; j < args.Length; j++) {
			text = args[j].Name;
			TextField textField = ui.HorizontalElementWithLabel(in text, 0.05f, () => ui.TextField(parseRTF: false));
			if (selector.GenericArgumentPrefiller.Target != null) {
				textField.TargetString = selector.GenericArgumentPrefiller.Target(genType, args[j]);
			}
			ctx._customGenericArguments.Add(textField);
		}
		text = "";
		tint = RadiantUI_Constants.BUTTON_COLOR;
		Button button2 = ui.Button(in text, in tint, ComponentSelectorPatch.onCreateCustomType(selector), 0.35f);
		ctx._customGenericTypeLabel.Target = button2.Label.Content;
		ctx._customGenericTypeColor.Target = button2.BaseColor;
		text = "ComponentSelector.CommonGenericTypes".AsLocaleKey();
		ui.Text(in text);

		var onAddComponentPressed = ComponentSelectorPatch.onAddComponentPressed(selector);

		int i = 0;
		foreach (Type commonType in WorkerInitializer.GetCommonGenericTypes(genType)) {
			try {
				if (!commonType.IsValidGenericType(validForInstantiation: true) || !selector.World.Types.IsSupported(commonType))
					continue;
			} catch (Exception ex) {
				CherryTypes.Warn($"Exception checking validity of a generic type: {commonType?.ToString()} for {genType?.ToString()}\n" + ex);
				continue;
			}
			text = commonType.GetNiceName();
			tint = RadiantUI_Constants.Sub.CYAN;
			var button = ui.Button(in text, in tint, onAddComponentPressed, selector.World.Types.EncodeType(commonType), 0.35f);
			button.Label.ParseRichText.Value = false;
			if (commonType.GetGenericArguments()[0] == typeof(float3)) button.Slot.OrderOffset = 1;
			else button.Slot.OrderOffset = 10 + i++;
		}


	}

	internal static void BuildCategoryUI(ComponentSelector selector, ComponentSelectorContext ctx, string? group, bool doNotGenerateBack) {
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
			ui.Button(in text, in tint, ComponentSelectorPatch.onOpenCategoryPressed(selector), (group == null) ? root.Parent.GetPath() : ctx.path, 0.35f);
		}
		KeyCounter<string>? groupCounter = null;
		HashSet<string>? generatedGroups = null;
		if (group == null) {
			// Buttons for subcategories
			foreach (CategoryNode<Type> cat in root.Subcategories) {
				text = cat.Name + " >";
				tint = RadiantUI_Constants.Sub.YELLOW;
				ui.Button(in text, in tint, ComponentSelectorPatch.onOpenCategoryPressed(selector), ctx.path + "/" + cat.Name, 0.35f)
					.Label.ParseRichText.Value = false;
			}

			groupCounter = [];
			generatedGroups = [];
			foreach (Type type2 in root.Elements) {
				if (!selector.World.Types.IsSupported(type2))
					continue;

				var grouping2 = type2.GetCustomAttribute<GroupingAttribute>();
				if (grouping2 != null) {
					groupCounter.Increment(grouping2.GroupName);
				}
			}
		}

		List<Button> navButtons = Pool.BorrowList<Button>();
		var openGroupPressed = ComponentSelectorPatch.openGroupPressed(selector);
		var openGenericTypesPressed = ComponentSelectorPatch.openGenericTypesPressed(selector);
		var onAddComponentPressed = ComponentSelectorPatch.onAddComponentPressed(selector);

		foreach (Type type in root.Elements) {
			if (!selector.World.Types.IsSupported(type) || (selector.ComponentFilter.Target != null && !selector.ComponentFilter.Target(type))) {
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
				button = ui.Button(in text, in tint, onAddComponentPressed, selector.World.Types.EncodeType(type), 0.35f);
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
