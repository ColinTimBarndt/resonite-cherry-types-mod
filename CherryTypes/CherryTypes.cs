using FrooxEngine;

using HarmonyLib;

using ResoniteModLoader;

namespace CherryTypes;

public class CherryTypes : ResoniteMod {
	internal const string VERSION_CONSTANT = "1.0.0"; //Changing the version here updates it in all locations needed
	public override string Name => "CherryTypes";
	public override string Author => "Colin Tim Barndt";
	public override string Version => VERSION_CONSTANT;
	public override string Link => "https://github.com/resonite-modding-group/CherryTypes/";

	internal static ModConfiguration? Config;

	public override void OnEngineInit() {
		Config = GetConfiguration();
		Harmony harmony = new("cat.colin.CherryTypes");
		harmony.PatchAll();
	}

	[AutoRegisterConfigKey]
	public static ModConfigurationKey<bool> Enabled = new("Enabled", "When checked, enables CherryTypes", computeDefault: () => true);

	[AutoRegisterConfigKey]
	public static ModConfigurationKey<List<Type>> FavoriteTypes = new("Favorite Types", computeDefault: () => []);

	[AutoRegisterConfigKey]
	public static ModConfigurationKey<FavoriteComponents> Favorites = new("Favorites", computeDefault: () => new());

	public class FavoriteComponents() {
		public List<Type> Components = [];
		public List<Type> ProtoFluxNodes = [];
	}
}
