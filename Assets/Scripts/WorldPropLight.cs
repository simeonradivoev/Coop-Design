
using Assets.Scripts;
using UnityEngine;

public class WorldPropLight : WorldPropGeneric
{
	[SerializeField] private Light light;

	protected override bool ApplyColor(Color color)
	{
		bool hasChanged = false;
		if (light.color != color)
		{
			light.color = color;
			hasChanged = true;
		}
		hasChanged |= base.ApplyColor(color);
		return hasChanged;
	}

	protected override void ApplyScale(Vector3 size)
	{
		light.range = size.magnitude * 10;
	}
}