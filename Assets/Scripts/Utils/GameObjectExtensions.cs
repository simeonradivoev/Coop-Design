using UnityEngine;

public static class GameObjectExtensions
{
	public static void SetLayerRecursive(this GameObject gameObject, int layer)
	{
		if (gameObject == null) return;
		gameObject.layer = layer;
		foreach (Transform child in gameObject.transform)
		{
			if (child == null) continue;
			child.gameObject.SetLayerRecursive(layer);
		}
	}

	public static Bounds GetColliderBounds(this GameObject gameObject, bool includeTriggers)
	{
		Bounds bounds = new Bounds(gameObject.transform.position, Vector3.zero);
		foreach (Transform child in gameObject.transform)
		{
			Collider[] colliders = child.GetComponents<Collider>();
			foreach (var collider in colliders)
			{
				if (!collider.enabled) continue;
				if (!includeTriggers && !collider.isTrigger) continue;
				bounds.Encapsulate(collider.bounds);
			}
		}
		return bounds;
	}

	public static Bounds GetRendererBounds(this GameObject gameObject)
	{
		Bounds? bounds = null;
		Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
		foreach (var renderer in renderers)
		{
			if (!renderer.enabled) continue;
			if (!bounds.HasValue)
			{
				bounds = renderer.bounds;
			}
			else
			{
				bounds.Value.Encapsulate(renderer.bounds);
			}
		}
		return bounds ?? new Bounds(gameObject.transform.position, Vector3.zero);
	}
}