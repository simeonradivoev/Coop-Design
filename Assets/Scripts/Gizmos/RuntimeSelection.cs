using UnityEngine;

namespace Gizmos
{
	public class RuntimeSelection
	{
		private static WorldProp[] props = new WorldProp[0];

		public static WorldProp activeProp
		{
			get { return props.Length > 0 ? props[0] : null; } 
			set { props = new[] {value}; }
		}

		public static WorldProp[] GetProps(RuntimeSelectionMode selectionMode)
		{
			return new WorldProp[] {activeProp};
		}

		public static WorldProp[] Props
		{
			get { return props; }
			set { props = value; }
		}
	}
}