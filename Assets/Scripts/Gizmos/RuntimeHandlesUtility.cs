using UnityEngine;

namespace Gizmos
{
	public class RuntimeHandlesUtility
	{
		public static float s_CustomPickDistance = 5f;
		public static float s_NearestDistance;
		public static int s_NearestControl;
		public static Material s_HandleMaterial;
		public static Material s_HandleWireMaterial;
		internal static WorldProp[] ignoreRaySnapObjects = null;
		private static bool s_UseYSign = false;
		private static bool s_UseYSignZoom = false;

		private static Vector3[] points = new Vector3[]
		{
			Vector3.zero,
			Vector3.zero,
			Vector3.zero,
			Vector3.zero,
			Vector3.zero
		};

		public static int nearestControl
		{
			get
			{
				return (s_NearestDistance > 5f) ? 0 : s_NearestControl;
			}
			set
			{
				s_NearestControl = value;
			}
		}

		public static float acceleration
		{
			get
			{
				return (float)((!Event.current.shift) ? 1 : 4) * ((!Event.current.alt) ? 1f : 0.25f);
			}
		}

		public static float niceMouseDelta
		{
			get
			{
				Vector2 delta = Event.current.delta;
				delta.y = -delta.y;
				if (Mathf.Abs(Mathf.Abs(delta.x) - Mathf.Abs(delta.y)) / Mathf.Max(Mathf.Abs(delta.x), Mathf.Abs(delta.y)) > 0.1f)
				{
					if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
					{
						s_UseYSign = false;
					}
					else
					{
						s_UseYSign = true;
					}
				}
				if (s_UseYSign)
				{
					return Mathf.Sign(delta.y) * delta.magnitude * acceleration;
				}
				return Mathf.Sign(delta.x) * delta.magnitude * acceleration;
			}
		}

		public static Material handleMaterial
		{
			get
			{
				if (!s_HandleMaterial)
				{
					s_HandleMaterial = Resources.Load<Material>("RuntimeSceneView/Handles");
				}
				return s_HandleMaterial;
			}
		}

		private static Material handleWireMaterial
		{
			get
			{
				InitHandleMaterials();
				return s_HandleWireMaterial;
			}
		}

		private static void InitHandleMaterials()
		{
			if (!s_HandleWireMaterial)
			{
				s_HandleWireMaterial = Resources.Load<Material>("RuntimeSceneView/HandleLines");
			}
		}

		public static void EndHandles()
		{
			EventType type = Event.current.type;
			if (type != EventType.Layout)
			{
				//GUIUtility.s_HasKeyboardFocus = false;
				//GUIUtility.s_EditorScreenPointOffset = Vector2.zero;
			}
			RuntimeEditorGUI.s_DelayedTextEditor.EndGUI(type);
		}

		public static void BeginHandles()
		{
			RuntimeHandles.Init();
			EventType type = Event.current.type;
			if (type == EventType.Layout)
			{
				s_NearestControl = 0;
				s_NearestDistance = 5f;
			}
			RuntimeHandles.lighting = true;
			RuntimeHandles.color = Color.white;
			s_CustomPickDistance = 5f;
			Camera.SetupCurrent(null);
			RuntimeEditorGUI.s_DelayedTextEditor.BeginGUI();
		}

		public static float CalcLineTranslation(Vector2 src, Vector2 dest, Vector3 srcPosition, Vector3 constraintDir)
		{
			srcPosition = RuntimeHandles.matrix.MultiplyPoint(srcPosition);
			constraintDir = RuntimeHandles.matrix.MultiplyVector(constraintDir);
			float num = 1f;
			Vector3 forward = RuntimeHandles.currentCamera.transform.forward;
			if (Vector3.Dot(constraintDir, forward) < 0f)
			{
				num = -1f;
			}
			Vector3 vector = constraintDir;
			vector.y = -vector.y;
			Camera current = RuntimeHandles.currentCamera;
			Vector2 vector2 = PixelsToPoints(current.WorldToScreenPoint(srcPosition));
			Vector2 vector3 = PixelsToPoints(current.WorldToScreenPoint(srcPosition + constraintDir * num));
			Vector2 x = dest;
			Vector2 x2 = src;
			if (vector2 == vector3)
			{
				return 0f;
			}
			x.y = -x.y;
			x2.y = -x2.y;
			float parametrization = GetParametrization(x2, vector2, vector3);
			float parametrization2 = GetParametrization(x, vector2, vector3);
			return (parametrization2 - parametrization) * num;
		}

		public static float GetHandleSize(Vector3 position)
		{
			Camera current = RuntimeHandles.currentCamera;
			position = RuntimeHandles.matrix.MultiplyPoint(position);
			if (current)
			{
				Transform transform = current.transform;
				Vector3 position2 = transform.position;
				float z = Vector3.Dot(position - position2, transform.TransformDirection(new Vector3(0f, 0f, 1f)));
				Vector3 a = current.WorldToScreenPoint(position2 + transform.TransformDirection(new Vector3(0f, 0f, z)));
				Vector3 b = current.WorldToScreenPoint(position2 + transform.TransformDirection(new Vector3(1f, 0f, z)));
				float magnitude = (a - b).magnitude;
				return 80f / Mathf.Max(magnitude, 0.0001f);
			}
			return 20f;
		}

		public static Vector3 ClosestPointToArc(Vector3 center, Vector3 normal, Vector3 from, float angle, float radius)
		{
			Vector3[] array = new Vector3[60];
			RuntimeHandles.SetDiscSectionPoints(array, 60, center, normal, from, angle, radius);
			return ClosestPointToPolyLine(array);
		}

		public static Vector3 ClosestPointToPolyLine(params Vector3[] vertices)
		{
			float num = DistanceToLine(vertices[0], vertices[1]);
			int num2 = 0;
			for (int i = 2; i < vertices.Length; i++)
			{
				float num3 = DistanceToLine(vertices[i - 1], vertices[i]);
				if (num3 < num)
				{
					num = num3;
					num2 = i - 1;
				}
			}
			Vector3 vector = vertices[num2];
			Vector3 vector2 = vertices[num2 + 1];
			Vector2 v = Event.current.mousePosition - WorldToGUIPoint(vector);
			Vector2 v2 = WorldToGUIPoint(vector2) - WorldToGUIPoint(vector);
			float magnitude = v2.magnitude;
			float num4 = Vector3.Dot(v2, v);
			if (magnitude > 1E-06f)
			{
				num4 /= magnitude * magnitude;
			}
			num4 = Mathf.Clamp01(num4);
			return Vector3.Lerp(vector, vector2, num4);
		}

		public static Vector3 ClosestPointToDisc(Vector3 center, Vector3 normal, float radius)
		{
			Vector3 from = Vector3.Cross(normal, Vector3.up);
			if (from.sqrMagnitude < 0.001f)
			{
				from = Vector3.Cross(normal, Vector3.right);
			}
			return ClosestPointToArc(center, normal, from, 360f, radius);
		}

		public static object RaySnap(Ray ray)
		{
			RaycastHit[] array = Physics.RaycastAll(ray, float.PositiveInfinity, Camera.current.cullingMask);
			float num = float.PositiveInfinity;
			int num2 = -1;
			if (ignoreRaySnapObjects != null)
			{
				for (int i = 0; i < array.Length; i++)
				{
					if (!array[i].collider.isTrigger && array[i].distance < num)
					{
						bool flag = false;
						for (int j = 0; j < ignoreRaySnapObjects.Length; j++)
						{
							if (array[i].transform == ignoreRaySnapObjects[j])
							{
								flag = true;
								break;
							}
						}
						if (!flag)
						{
							num = array[i].distance;
							num2 = i;
						}
					}
				}
			}
			else
			{
				for (int k = 0; k < array.Length; k++)
				{
					if (array[k].distance < num)
					{
						num = array[k].distance;
						num2 = k;
					}
				}
			}
			if (num2 >= 0)
			{
				return array[num2];
			}
			return null;
		}

		internal static float GetParametrization(Vector2 x0, Vector2 x1, Vector2 x2)
		{
			return -(Vector2.Dot(x1 - x0, x2 - x1) / (x2 - x1).sqrMagnitude);
		}

		public static float PointOnLineParameter(Vector3 point, Vector3 linePoint, Vector3 lineDirection)
		{
			return Vector3.Dot(lineDirection, point - linePoint) / lineDirection.sqrMagnitude;
		}

		public static Vector2 PixelsToPoints(Vector2 position)
		{
			float num = 1f;
			position.x *= num;
			position.y *= num;
			return position;
		}

		public static Vector2 PointsToPixels(Vector2 position)
		{
			float pixelsPerPoint = 1;
			position.x *= pixelsPerPoint;
			position.y *= pixelsPerPoint;
			return position;
		}

		public static Rect PointsToPixels(Rect rect)
		{
			float pixelsPerPoint = 1;
			rect.x *= pixelsPerPoint;
			rect.y *= pixelsPerPoint;
			rect.width *= pixelsPerPoint;
			rect.height *= pixelsPerPoint;
			return rect;
		}

		internal static void ApplyWireMaterial()
		{
			Material handleWireMaterial = RuntimeHandlesUtility.handleWireMaterial;
			handleWireMaterial.SetPass(0);
		}

		public static void AddControl(int controlId, float distance)
		{
			if (distance < s_CustomPickDistance && distance > 5f)
			{
				distance = 5f;
			}
			if (distance <= s_NearestDistance)
			{
				s_NearestDistance = distance;
				s_NearestControl = controlId;
			}
		}

		public static Vector2 WorldToGUIPoint(Vector3 world)
		{
			world = RuntimeHandles.matrix.MultiplyPoint(world);
			Camera current = RuntimeHandles.currentCamera;
			if (current)
			{
				Vector2 vector = current.WorldToScreenPoint(world);
				vector.y = (float)Screen.height - vector.y;
				vector = PixelsToPoints(vector);
				return GUIClipUtil.Clip(vector);
			}
			return new Vector2(world.x, world.y);
		}

		public static Ray GUIPointToWorldRay(Vector2 position)
		{
			if (!RuntimeHandles.currentCamera)
			{
				Debug.LogError("Unable to convert GUI point to world ray if a camera has not been set up!");
				return new Ray(Vector3.zero, Vector3.forward);
			}
			Vector2 position2 = GUIClipUtil.Unclip(position);
			Vector2 vector = PointsToPixels(position2);
			vector.y = (float)Screen.height - vector.y;
			Camera current = RuntimeHandles.currentCamera;
			return current.ScreenPointToRay(new Vector2(vector.x, vector.y));
		}

		public static float DistanceToLine(Vector3 p1, Vector3 p2)
		{
			p1 = WorldToGUIPoint(p1);
			p2 = WorldToGUIPoint(p2);
			Vector2 mousePosition = Event.current.mousePosition;
			float num = DistancePointLine(mousePosition, p1, p2);
			if (num < 0f)
			{
				num = 0f;
			}
			return num;
		}

		public static float DistanceToCircle(Vector3 position, float radius)
		{
			Vector2 a = WorldToGUIPoint(position);
			Camera current = RuntimeHandles.currentCamera;
			Vector2 b = Vector2.zero;
			if (current)
			{
				b = WorldToGUIPoint(position + current.transform.right * radius);
				radius = (a - b).magnitude;
			}
			float magnitude = (a - Event.current.mousePosition).magnitude;
			if (magnitude < radius)
			{
				return 0f;
			}
			return magnitude - radius;
		}

		public static float DistanceToRectangle(Vector3 position, Quaternion rotation, float size)
		{
			Vector3 b = rotation * new Vector3(size, 0f, 0f);
			Vector3 b2 = rotation * new Vector3(0f, size, 0f);
			points[0] = WorldToGUIPoint(position + b + b2);
			points[1] = WorldToGUIPoint(position + b - b2);
			points[2] = WorldToGUIPoint(position - b - b2);
			points[3] = WorldToGUIPoint(position - b + b2);
			points[4] = points[0];
			Vector2 mousePosition = Event.current.mousePosition;
			bool flag = false;
			int num = 4;
			for (int i = 0; i < 5; i++)
			{
				if (points[i].y > mousePosition.y != points[num].y > mousePosition.y && mousePosition.x < (points[num].x - points[i].x) * (mousePosition.y - points[i].y) / (points[num].y - points[i].y) + points[i].x)
				{
					flag = !flag;
				}
				num = i;
			}
			if (!flag)
			{
				float num2 = -1f;
				num = 1;
				for (int j = 0; j < 4; j++)
				{
					float num3 = DistancePointToLineSegment(mousePosition, points[j], points[num++]);
					if (num3 < num2 || num2 < 0f)
					{
						num2 = num3;
					}
				}
				return num2;
			}
			return 0f;
		}

		public static float DistanceToArc(Vector3 center, Vector3 normal, Vector3 from, float angle, float radius)
		{
			Vector3[] dest = new Vector3[60];
			RuntimeHandles.SetDiscSectionPoints(dest, 60, center, normal, from, angle, radius);
			return DistanceToPolyLine(dest);
		}

		public static float DistanceToDisc(Vector3 center, Vector3 normal, float radius)
		{
			Vector3 from = Vector3.Cross(normal, Vector3.up);
			if (from.sqrMagnitude < 0.001f)
			{
				from = Vector3.Cross(normal, Vector3.right);
			}
			return DistanceToArc(center, normal, from, 360f, radius);
		}

		public static float DistanceToPolyLine(params Vector3[] points)
		{
			float num = DistanceToLine(points[0], points[1]);
			for (int i = 2; i < points.Length; i++)
			{
				float num2 = DistanceToLine(points[i - 1], points[i]);
				if (num2 < num)
				{
					num = num2;
				}
			}
			return num;
		}

		public static float DistancePointLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
		{
			return Vector3.Magnitude(ProjectPointLine(point, lineStart, lineEnd) - point);
		}

		public static Vector3 ProjectPointLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
		{
			Vector3 rhs = point - lineStart;
			Vector3 vector = lineEnd - lineStart;
			float magnitude = vector.magnitude;
			Vector3 vector2 = vector;
			if (magnitude > 1E-06f)
			{
				vector2 /= magnitude;
			}
			float num = Vector3.Dot(vector2, rhs);
			num = Mathf.Clamp(num, 0f, magnitude);
			return lineStart + vector2 * num;
		}

		public static float DistancePointToLineSegment(Vector2 p, Vector2 a, Vector2 b)
		{
			float sqrMagnitude = (b - a).sqrMagnitude;
			if ((double)sqrMagnitude == 0.0)
			{
				return (p - a).magnitude;
			}
			float num = Vector2.Dot(p - a, b - a) / sqrMagnitude;
			if ((double)num < 0.0)
			{
				return (p - a).magnitude;
			}
			if ((double)num > 1.0)
			{
				return (p - b).magnitude;
			}
			Vector2 b2 = a + num * (b - a);
			return (p - b2).magnitude;
		}
	}
}