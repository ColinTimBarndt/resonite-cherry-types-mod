using HarmonyLib;

using ResoniteModLoader;

namespace CherryTypes;

/// <summary>
/// Smarter generic type suggestions for component selectors.
/// </summary>
public sealed class CherryTypes : ResoniteMod {

	/// <inheritdoc/>
	public override string Name => "CherryTypes";

	/// <inheritdoc/>
	public override string Author => "Colin Tim Barndt";

	/// <inheritdoc/>
	public override string Version { get; } =
		System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0";

	/// <inheritdoc/>
	public override string Link => "https://github.com/ColinTimBarndt/resonite-cherry-types-mod";

	internal static ModConfiguration? Config;

	/// <summary>
	/// Construct a new instance of this mod. Only one instance can ever be created.
	/// </summary>
	public CherryTypes() {
		// OnEngine init is called after the config was already read.
		// Resonite has already loaded all data model assemblies when this mod is
		// loaded, so initializing the types is fine here.
		TypeHelper.Init();

		// Only use the mod's logger when executed as a mod, not unit test.
		TypeHashListConverter.softError = Warn;
	}

	/// <inheritdoc/>
	public override void OnEngineInit() {
		Config = GetConfiguration();
		Config!.Save(true);

		Harmony harmony = new("cat.colin.CherryTypes");
		harmony.PatchAll();
	}

	internal static FavoritesByCategory? FavoritesConfig => Config?.GetValue<FavoritesByCategory>(Favorites);

	/// <summary>When checked, enables CherryTypes.</summary>
	[AutoRegisterConfigKey]
	public static ModConfigurationKey<bool> Enabled = new("Enabled", "When checked, enables CherryTypes", computeDefault: () => true);

	/// <summary>Favorite types configuration.</summary>
	[AutoRegisterConfigKey]
	public static ModConfigurationKey<FavoritesByCategory> Favorites = new("Favorites", computeDefault: () => new());
}
