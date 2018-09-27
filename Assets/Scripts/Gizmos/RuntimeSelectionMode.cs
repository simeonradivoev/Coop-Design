namespace Gizmos
{
	public enum RuntimeSelectionMode
	{
		Unfiltered,
		TopLevel,
		Deep,
		ExcludePrefab = 4,
		Editable = 8,
		Assets = 16,
		DeepAssets = 32,
		OnlyUserModifiable = 8
	}
}