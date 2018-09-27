using UnityEngine;

namespace Gizmos
{
	public class SliderScale
	{
		private static float s_StartScale;

		private static float s_ScaleDrawLength = 1f;

		private static float s_ValueDrag;

		private static Vector2 s_StartMousePosition;

		private static Vector2 s_CurrentMousePosition;

		public static float DoAxis(int id, float scale, Vector3 position, Vector3 direction, Quaternion rotation, float size, float snap)
		{
			Event current = Event.current;
			switch (current.GetTypeForControl(id))
			{
				case EventType.MouseDown:
					if ((RuntimeHandlesUtility.nearestControl == id && current.button == 0) || (GUIUtility.keyboardControl == id && current.button == 2))
					{
						GUIUtility.keyboardControl = id;
						GUIUtility.hotControl = id;
						SliderScale.s_CurrentMousePosition = (SliderScale.s_StartMousePosition = current.mousePosition);
						SliderScale.s_StartScale = scale;
						current.Use();
						//EditorGUIUtility.SetWantsMouseJumping(1);
					}
					break;
				case EventType.MouseUp:
					if (GUIUtility.hotControl == id && (current.button == 0 || current.button == 2))
					{
						GUIUtility.hotControl = 0;
						current.Use();
						//EditorGUIUtility.SetWantsMouseJumping(0);
					}
					break;
				case EventType.MouseDrag:
					if (GUIUtility.hotControl == id)
					{
						SliderScale.s_CurrentMousePosition += current.delta;
						float num = 1f + RuntimeHandlesUtility.CalcLineTranslation(SliderScale.s_StartMousePosition, SliderScale.s_CurrentMousePosition, position, direction) / size;
						num = RuntimeHandles.SnapValue(num, snap);
						scale = SliderScale.s_StartScale * num;
						GUI.changed = true;
						current.Use();
					}
					break;
				case EventType.Repaint:
					{
						Color color = Color.white;
						if (id == GUIUtility.keyboardControl)
						{
							color = RuntimeHandles.color;
							RuntimeHandles.color = RuntimeHandles.selectedColor;
						}
						float num2 = size;
						if (GUIUtility.hotControl == id)
						{
							num2 = size * scale / SliderScale.s_StartScale;
						}
						RuntimeHandles.CubeCap(id, position + direction * num2 * SliderScale.s_ScaleDrawLength, rotation, size * 0.1f);
						RuntimeHandles.DrawLine(position, position + direction * (num2 * SliderScale.s_ScaleDrawLength - size * 0.05f));
						if (id == GUIUtility.keyboardControl)
						{
							RuntimeHandles.color = color;
						}
						break;
					}
				case EventType.Layout:
					RuntimeHandlesUtility.AddControl(id, RuntimeHandlesUtility.DistanceToLine(position, position + direction * size));
					RuntimeHandlesUtility.AddControl(id, RuntimeHandlesUtility.DistanceToCircle(position + direction * size, size * 0.2f));
					break;
			}
			return scale;
		}

		public static float DoCenter(int id, float value, Vector3 position, Quaternion rotation, float size, RuntimeHandles.DrawCapFunction capFunc, float snap)
		{
			Event current = Event.current;
			switch (current.GetTypeForControl(id))
			{
				case EventType.MouseDown:
					if ((RuntimeHandlesUtility.nearestControl == id && current.button == 0) || (GUIUtility.keyboardControl == id && current.button == 2))
					{
						GUIUtility.keyboardControl = id;
						GUIUtility.hotControl = id;
						SliderScale.s_StartScale = value;
						SliderScale.s_ValueDrag = 0f;
						current.Use();
						//EditorGUIUtility.SetWantsMouseJumping(1);
					}
					break;
				case EventType.MouseUp:
					if (GUIUtility.hotControl == id && (current.button == 0 || current.button == 2))
					{
						GUIUtility.hotControl = 0;
						SliderScale.s_ScaleDrawLength = 1f;
						current.Use();
						//EditorGUIUtility.SetWantsMouseJumping(0);
					}
					break;
				case EventType.MouseDrag:
					if (GUIUtility.hotControl == id)
					{
						SliderScale.s_ValueDrag += RuntimeHandlesUtility.niceMouseDelta * 0.01f;
						value = (RuntimeHandles.SnapValue(SliderScale.s_ValueDrag, snap) + 1f) * SliderScale.s_StartScale;
						SliderScale.s_ScaleDrawLength = value / SliderScale.s_StartScale;
						GUI.changed = true;
						current.Use();
					}
					break;
				case EventType.KeyDown:
					if (GUIUtility.hotControl == id && current.keyCode == KeyCode.Escape)
					{
						value = SliderScale.s_StartScale;
						SliderScale.s_ScaleDrawLength = 1f;
						GUIUtility.hotControl = 0;
						GUI.changed = true;
						current.Use();
					}
					break;
				case EventType.Repaint:
					{
						Color color = Color.white;
						if (id == GUIUtility.keyboardControl)
						{
							color = RuntimeHandles.color;
							RuntimeHandles.color = RuntimeHandles.selectedColor;
						}
						capFunc(id, position, rotation, size * 0.15f);
						if (id == GUIUtility.keyboardControl)
						{
							RuntimeHandles.color = color;
						}
						break;
					}
				case EventType.Layout:
					RuntimeHandlesUtility.AddControl(id, RuntimeHandlesUtility.DistanceToCircle(position, size * 0.15f));
					break;
			}
			return value;
		}
	}
}