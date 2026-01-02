namespace CherryTypes;

public class FavoritesByCategory() {
	public TypeHashList Types = [];
	public TypeHashList Components = [];
	public TypeHashList ProtoFluxNodes = [];

	public TypeHashList this[FavoritesCategory category] => category switch {
		FavoritesCategory.Types => this.Types,
		FavoritesCategory.Components => this.Components,
		FavoritesCategory.ProtoFluxNodes => this.ProtoFluxNodes,
		_ => throw new IndexOutOfRangeException(),
	};
}

public enum FavoritesCategory {
	Types,
	Components,
	ProtoFluxNodes,
}
