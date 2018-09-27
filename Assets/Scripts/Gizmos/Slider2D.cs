using UnityEngine;

namespace Gizmos
{
	public class Slider2D
	{
		private static Vector2 s_CurrentMousePosition;

		private static Vector3 s_StartPosition;

		private static Vector2 s_StartPlaneOffset;

		public static Vector3 Do(int id, Vector3 handlePos, Vector3 handleDir, Vector3 slideDir1, Vector3 slideDir2, float handleSize, RuntimeHandles.DrawCapFunction drawFunc, float snap, bool drawHelper)
		{
			return Slider2D.Do(id, handlePos, new Vector3(0f, 0f, 0f), handleDir, slideDir1, slideDir2, handleSize, drawFunc, new Vector2(snap, snap), drawHelper);
		}

		public static Vector3 Do(int id, Vector3 handlePos, Vector3 offset, Vector3 handleDir, Vector3 slideDir1, Vector3 slideDir2, float handleSize, RuntimeHandles.DrawCapFunction drawFunc, float snap, bool drawHelper)
		{
			return Slider2D.Do(id, handlePos, offset, handleDir, slideDir1, slideDir2, handleSize, drawFunc, new Vector2(snap, snap), drawHelper);
		}

		public static Vector3 Do(int id, Vector3 handlePos, Vector3 offset, Vector3 handleDir, Vector3 slideDir1, Vector3 slideDir2, float handleSize, RuntimeHandles.DrawCapFunction drawFunc, Vector2 snap, bool drawHelper)
		{
			bool changed = GUI.changed;
			GUI.changed = false;
			Vector2 vector = Slider2D.CalcDeltaAlongDirections(id, handlePos, offset, handleDir, slideDir1, slideDir2, handleSize, drawFunc, snap, drawHelper);
			if (GUI.changed)
			{
				handlePos = Slider2D.s_StartPosition + slideDir1 * vector.x + slideDir2 * vector.y;
			}
			GUI.changed |= changed;
			return handlePos;
		}

		private static Vector2 CalcDeltaAlongDirections(int id, Vector3 handlePos, Vector3 offset, Vector3 handleDir, Vector3 slideDir1, Vector3 slideDir2, float handleSize, RuntimeHandles.DrawCapFunction drawFunc, Vector2 snap, bool drawHelper)
		{
			Vector2 vector = new Vector2(0f, 0f);
			Event current = Event.current;
			switch (current.GetTypeForControl(id))
			{
				case EventType.MouseDown:
					if (((RuntimeHandlesUtility.nearestControl == id && current.button == 0) || (GUIUtility.keyboardControl == id && current.button == 2)) && GUIUtility.hotControl == 0)
					{
						Plane plane = new Plane(RuntimeHandles.matrix.MultiplyVector(handleDir), RuntimeHandles.matrix.MultiplyPoint(handlePos));
						Ray ray = RuntimeHandlesUtility.GUIPointToWorldRay(current.mousePosition);
						float distance = 0f;
						plane.Raycast(ray, out distance);
						GUIUtility.keyboardControl = id;
						GUIUtility.hotControl = id;
						Slider2D.s_CurrentMousePosition = current.mousePosition;
						Slider2D.s_StartPosition = handlePos;
						Vector3 a = RuntimeHandles.s_InverseMatrix.MultiplyPoint(ray.GetPoint(distance));
						Vector3 lhs = a - handlePos;
						Slider2D.s_StartPlaneOffset.x = Vector3.Dot(lhs, slideDir1);
						Slider2D.s_StartPlaneOffset.y = Vector3.Dot(lhs, slideDir2);
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
						Slider2D.s_CurrentMousePosition += current.delta;
						Vector3 a2 = RuntimeHandles.matrix.MultiplyPoint(handlePos);
						Vector3 normalized = RuntimeHandles.matrix.MultiplyVector(slideDir1).normalized;
						Vector3 normalized2 = RuntimeHandles.matrix.MultiplyVector(slideDir2).normalized;
						Ray ray2 = RuntimeHandlesUtility.GUIPointToWorldRay(Slider2D.s_CurrentMousePosition);
						Plane plane2 = new Plane(a2, a2 + normalized, a2 + normalized2);
						float distance2 = 0f;
						if (plane2.Raycast(ray2, out distance2))
						{
							Vector3 point = RuntimeHandles.s_InverseMatrix.MultiplyPoint(ray2.GetPoint(distance2));
							vector.x = RuntimeHandlesUtility.PointOnLineParameter(point, Slider2D.s_StartPosition, slideDir1);
							vector.y = RuntimeHandlesUtility.PointOnLineParameter(point, Slider2D.s_StartPosition, slideDir2);
							vector -= Slider2D.s_StartPlaneOffset;
							if (snap.x > 0f || snap.y > 0f)
							{
								vector.x = RuntimeHandles.SnapValue(vector.x, snap.x);
								vector.y = RuntimeHandles.SnapValue(vector.y, snap.y);
							}
							GUI.changed = true;
						}
						current.Use();
					}
					break;
				case EventType.Repaint:
					if (drawFunc != null)
					{
						Vector3 vector2 = handlePos + offset;
						Quaternion rotation = Quaternion.LookRotation(handleDir, slideDir1);
						Color color = Color.white;
						if (id == GUIUtility.keyboardControl)
						{
							color = RuntimeHandles.color;
							RuntimeHandles.color = RuntimeHandles.selectedColor;
						}
						drawFunc(id, vector2, rotation, handleSize);
						if (id == GUIUtility.keyboardControl)
						{
							RuntimeHandles.color = color;
						}
						if (drawHelper && GUIUtility.hotControl == id)
						{
							Vector3[] array = new Vector3[4];
							float d = handleSize * 10f;
							array[0] = vector2 + (slideDir1 * d + slideDir2 * d);
							array[1] = array[0] - slideDir1 * d * 2f;
							array[2] = array[1] - slideDir2 * d * 2f;
							array[3] = array[2] + slideDir1 * d * 2f;
							Color color2 = RuntimeHandles.color;
							RuntimeHandles.color = Color.white;
							float num = 0.6f;
							RuntimeHandles.DrawSolidRectangleWithOutline(array, new Color(1f, 1f, 1f, 0.05f), new Color(num, num, num, 0.4f));
							RuntimeHandles.color = color2;
						}
					}
					break;
				case EventType.Layout:
					if (drawFunc == RuntimeHandles.ArrowCap)
					{
						RuntimeHandlesUtility.AddControl(id, RuntimeHandlesUtility.DistanceToLine(handlePos + offset, handlePos + handleDir * handleSize));
						RuntimeHandlesUtility.AddControl(id, RuntimeHandlesUtility.DistanceToCircle(handlePos + offset + handleDir * handleSize, handleSize * 0.2f));
					}
					else if (drawFunc == RuntimeHandles.RectangleCap)
					{
						RuntimeHandlesUtility.AddControl(id, RuntimeHandlesUtility.DistanceToRectangle(handlePos + offset, Quaternion.LookRotation(handleDir, slideDir1), handleSize));
					}
					else
					{
						RuntimeHandlesUtility.AddControl(id, RuntimeHandlesUtility.DistanceToCircle(handlePos + offset, handleSize * 0.5f));
					}
					break;
			}
			return vector;
		}
	}
}