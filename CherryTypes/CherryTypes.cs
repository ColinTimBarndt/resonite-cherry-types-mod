using HarmonyLib;

using ResoniteModLoader;

namespace CherryTypes;

public sealed class CherryTypes : ResoniteMod {
	internal const string VERSION_CONSTANT = "1.0.1"; //Changing the version here updates it in all locations needed
	public override string Name => "CherryTypes";
	public override string Author => "Colin Tim Barndt";
	public override string Version => VERSION_CONSTANT;
	public override string Link => "https://github.com/ColinTimBarndt/resonite-cherry-types-mod";

	internal static ModConfiguration? Config;

	static CherryTypes() {
		// OnEngine init is called after the config was already read.
		// Resonite has already loaded all data model assemblies when this mod is
		// loaded, so initializing the types is fine here.
		TypeHelper.Init();

		// Only use the mod's logger when executed as a mod, not unit test.
		TypeHashListConverter.softError = Warn;
	}

	public override void OnEngineInit() {
		Config = GetConfiguration();
		FavoritesConfig!.Types.Add(typeof(int));
		Config!.Save(true);

		Harmony harmony = new("cat.colin.CherryTypes");
		harmony.PatchAll();
	}

	internal static FavoritesByCategory? FavoritesConfig => Config?.GetValue<FavoritesByCategory>(Favorites);

	[AutoRegisterConfigKey]
	public static ModConfigurationKey<bool> Enabled = new("Enabled", "When checked, enables CherryTypes", computeDefault: () => true);

	[AutoRegisterConfigKey]
	public static ModConfigurationKey<FavoritesByCategory> Favorites = new("Favorites", computeDefault: () => new());
}
