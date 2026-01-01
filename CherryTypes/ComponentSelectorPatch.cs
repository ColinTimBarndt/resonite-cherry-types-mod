using System.Reflection;

using Elements.Core;

using FrooxEngine;
using FrooxEngine.UIX;

using HarmonyLib;

namespace CherryTypes;

internal record ComponentSelectorContext {
	public required string? path;
	public required UIBuilder ui;
	public required Sync<string> _rootPath;
	public required SyncRef<Slot> _uiRoot;
	public required SyncRefList<TextField> _customGenericArguments;
	public required SyncType _genericType;
	public required FieldDrive<string> _customGenericTypeLabel;
	public required FieldDrive<colorX> _customGenericTypeColor;
}

[HarmonyPatch(typeof(ComponentSelector), nameof(ComponentSelector.BuildUI))]
internal static class ComponentSelectorPatch {
	internal readonly static Func<ComponentSelector, ButtonEventHandler> onCancelPressed, onCreateCustomType;
	internal readonly static Func<ComponentSelector, ButtonEventHandler<string?>> onOpenCategoryPressed, openGenericTypesPressed, openGroupPressed, onAddComponentPressed;

	static ComponentSelectorPatch() {
		bool success = true;

		onCancelPressed = GetDelegate<ButtonEventHandler>("OnCancelPressed", ref success);
		onCreateCustomType = GetDelegate<ButtonEventHandler>("OnCreateCustomType", ref success);

		onOpenCategoryPressed = GetDelegate<ButtonEventHandler<string?>>("OnOpenCategoryPressed", ref success);
		openGenericTypesPressed = GetDelegate<ButtonEventHandler<string?>>("OpenGenericTypesPressed", ref success);
		openGroupPressed = GetDelegate<ButtonEventHandler<string?>>("OpenGroupPressed", ref success);
		onAddComponentPressed = GetDelegate<ButtonEventHandler<string?>>("OnAddComponentPressed", ref success);

		initialized = success;
	}

	static private readonly bool initialized;

	private static Func<ComponentSelector, T> GetDelegate<T>(string name, ref bool success) where T : Delegate {
		var method = typeof(ComponentSelector)
			.GetMethod(name, BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
		if (method == null) {
			success = false;
			CherryTypes.Error($"Delegate ComponentSelector.{name} was not found!");
			return null!;
		}
		return (selector) => method.CreateDelegate<T>(selector);
	}

	[HarmonyPrefix]
	static bool BuildUI_Prefix(ComponentSelector __instance,
		string? path, bool genericType, string? group, bool doNotGenerateBack,
		Sync<string> ____rootPath,
		SyncRef<Slot> ____uiRoot,
		SyncRefList<TextField> ____customGenericArguments,
		SyncType ____genericType,
		FieldDrive<string> ____customGenericTypeLabel,
		FieldDrive<colorX> ____customGenericTypeColor
	) {
		if (!(initialized && CherryTypes.Config!.GetValue(CherryTypes.Enabled)))
			return true;

		string? normPath = path;
		if (path != null && path.StartsWith("/")) {
			normPath = path[1..];
		}

		var noBack = doNotGenerateBack;
		if (doNotGenerateBack) {
			____rootPath.Value = normPath!;
		} else if (____rootPath.Value == normPath && group == null) {
			noBack = true;
		}
		____uiRoot.Target.DestroyChildren();
		____customGenericArguments.Clear();
		____genericType.Value = null!;
		UIBuilder ui = new(____uiRoot.Target);
		RadiantUI_Constants.SetupEditorStyle(ui, extraPadding: true);
		ui.Style.TextAlignment = Alignment.MiddleLeft;
		ui.Style.ButtonTextAlignment = Alignment.MiddleLeft;
		ui.Style.MinHeight = 32f;
		LocaleString text;
		colorX? tint;

		ComponentSelectorContext ctx = new() {
			path = normPath,
			ui = ui,
			_rootPath = ____rootPath,
			_uiRoot = ____uiRoot,
			_customGenericArguments = ____customGenericArguments,
			_genericType = ____genericType,
			_customGenericTypeLabel = ____customGenericTypeLabel,
			_customGenericTypeColor = ____customGenericTypeColor,
		};

		if (genericType) {
			ComponentSelectorUIX.BuildGenericTypeUI(__instance, ctx, group, noBack);
		} else {
			ComponentSelectorUIX.BuildCategoryUI(__instance, ctx, group, noBack);
		}

		text = "General.Cancel".AsLocaleKey();
		tint = RadiantUI_Constants.Sub.RED;
		ui.Button(in text, in tint, onCancelPressed(__instance), 0.35f).Slot.OrderOffset = long.MaxValue;
		return false;
	}
}
