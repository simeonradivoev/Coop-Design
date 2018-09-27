using UnityEngine;

namespace Assets.Scripts
{
	public class WorldPropGeneric : WorldProp
	{
		 [SerializeField] protected Renderer renderer;

		protected override bool ApplyColor(Color color)
		{
			if (renderer && renderer.material.color != color)
			{
				renderer.material.color = color;
				return true;
			}
			return false;
		}
	}
}