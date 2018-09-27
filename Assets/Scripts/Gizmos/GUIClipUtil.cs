using System;
using System.Reflection;
using UnityEngine;

namespace Gizmos
{
	public class GUIClipUtil
	{
		public static Type type = typeof (GUI).Assembly.GetType("UnityEngine.GUIClip");
		public static MethodInfo ClipVectorInfo = type.GetMethod("Clip",BindingFlags.Static | BindingFlags.Public,null, new Type[] { typeof(Vector2) },null);
		public static MethodInfo UnclipVectorInfo = type.GetMethod("Unclip", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(Vector2) }, null);
		public static MethodInfo UnclipRectInfo = type.GetMethod("Unclip", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(Rect) }, null);

		public static Vector2 Unclip(Vector2 pos)
		{
			object[] par = { pos };
			UnclipVectorInfo.Invoke(null, par);
			return (Vector2)par[0];
		}

		public static Rect Unclip(Rect rect)
		{
			object[] par = { rect };
			UnclipRectInfo.Invoke(null, par);
			return (Rect)par[0];
		}

		public static Vector2 Clip(Vector2 absolutePos)
		{
			object[] par = {absolutePos};
			ClipVectorInfo.Invoke(null, par);
			return (Vector2)par[0];
		}
	}
}