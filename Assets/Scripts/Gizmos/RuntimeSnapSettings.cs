using UnityEngine;

namespace Gizmos
{
	public class RuntimeSnapSettings
	{
		private static bool s_Initialized;
		private static float s_MoveSnapX;
		private static float s_MoveSnapY;
		private static float s_MoveSnapZ;
		private static float s_ScaleSnap;
		private static float s_RotationSnap;

		private static void Initialize()
		{
			if (!s_Initialized)
			{
				s_MoveSnapX = PlayerPrefs.GetFloat("MoveSnapX", 1f);
				s_MoveSnapY = PlayerPrefs.GetFloat("MoveSnapY", 1f);
				s_MoveSnapZ = PlayerPrefs.GetFloat("MoveSnapZ", 1f);
				s_ScaleSnap = PlayerPrefs.GetFloat("ScaleSnap", 0.1f);
				s_RotationSnap = PlayerPrefs.GetFloat("RotationSnap", 15f);
				s_Initialized = true;
			}
		}

		public static Vector3 move
		{
			get
			{
				Initialize();
				return new Vector3(s_MoveSnapX, s_MoveSnapY, s_MoveSnapZ);
			}
			set
			{
				PlayerPrefs.SetFloat("MoveSnapX", value.x);
				s_MoveSnapX = value.x;
				PlayerPrefs.SetFloat("MoveSnapY", value.y);
				s_MoveSnapY = value.y;
				PlayerPrefs.SetFloat("MoveSnapZ", value.z);
				s_MoveSnapZ = value.z;
			}
		}

		public static float scale
		{
			get
			{
				Initialize();
				return s_ScaleSnap;
			}
			set
			{
				PlayerPrefs.SetFloat("ScaleSnap", value);
				s_ScaleSnap = value;
			}
		}

		public static float rotation
		{

			get
			{
				Initialize();
				return s_RotationSnap;
			}
			set
			{
				PlayerPrefs.SetFloat("RotationSnap", value);
				s_RotationSnap = value;
			}
		}
	}
}