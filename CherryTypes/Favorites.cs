namespace CherryTypes;

/// <summary>
/// Favorite types by category.
/// <see cref="Types" /> is for generic type suggestions.
/// <see cref="Components" /> and <see cref="ProtoFluxNodes" />
/// are favorites for their respective component picker types.
/// </summary>
public class FavoritesByCategory() {
	/// <summary>
	/// Favorite types for generic type suggestions.
	/// </summary>
	public TypeHashList Types = [];

	/// <summary>
	/// Favorite components for the component selector.
	/// </summary>
	public TypeHashList Components = [];

	/// <summary>
	/// Favorite ProtoFlux nodes for the node selector.
	/// </summary>
	public TypeHashList ProtoFluxNodes = [];

	/// <summary>
	/// Access the respective favorites by its category.
	/// </summary>
	public TypeHashList this[FavoritesCategory category] => category switch {
		FavoritesCategory.Types => this.Types,
		FavoritesCategory.Components => this.Components,
		FavoritesCategory.ProtoFluxNodes => this.ProtoFluxNodes,
		_ => throw new IndexOutOfRangeException(),
	};
}

/// <summary>
/// Types appear in different places in the component selector.
/// </summary>
public enum FavoritesCategory {
	/// <summary>
	/// Favorite types for generic type suggestions.
	/// </summary>
	Types,

	/// <summary>
	/// Favorite components for the component selector.
	/// </summary>
	Components,

	/// <summary>
	/// Favorite ProtoFlux nodes for the node selector.
	/// </summary>
	ProtoFluxNodes,
}
