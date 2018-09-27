using UnityEngine;

namespace Gizmos
{
	public class RuntimeHandles
	{
		private enum PlaneHandle
		{
			xzPlane,
			xyPlane,
			yzPlane
		}

		private static Vector3[] verts = new Vector3[]
		{
			Vector3.zero,
			Vector3.zero,
			Vector3.zero,
			Vector3.zero
		};

		private static Vector3 s_PlanarHandlesOctant = Vector3.one;
		internal static Matrix4x4 s_Matrix = Matrix4x4.identity;
		internal static Matrix4x4 s_InverseMatrix = Matrix4x4.identity;
		public static Color s_Color;
		internal static Color s_XAxisColor = new Color(0.858823538f, 0.243137255f, 0.113725491f, 0.93f);
		internal static Color s_YAxisColor = new Color(0.6039216f, 0.9529412f, 0.282352954f, 0.93f);
		internal static Color s_ZAxisColor = new Color(0.227450982f, 0.478431374f, 0.972549f, 0.93f);
		internal static Color s_CenterColor = new Color(0.8f, 0.8f, 0.8f, 0.93f);
		internal static Color s_SelectedColor = new Color(0.9647059f, 0.9490196f, 0.196078435f, 0.89f);
		internal static Color s_SecondaryColor = new Color( 0.5f, 0.5f, 0.5f, 0.2f);
		internal static Color staticColor = new Color(0.5f, 0.5f, 0.5f, 0f);
		public static Color lineTransparency = new Color(1f, 1f, 1f, 0.75f);
		public static Mesh s_ConeMesh;
		public static Mesh s_CubeMesh;
		public static Mesh s_SphereMesh;
		public static Mesh s_CylinderMesh;
		public static Mesh s_QuadMesh;
		public static bool s_Lighting;
		internal static float staticBlend = 0;
		public delegate void DrawCapFunction(int controlID, Vector3 position, Quaternion rotation, float size);
		internal static int s_SliderHash = "SliderHash".GetHashCode();
		internal static int s_Slider2DHash = "Slider2DHash".GetHashCode();
		internal static int s_DiscHash = "DiscHash".GetHashCode();
		internal static int s_ScaleSliderHash = "ScaleSliderHash".GetHashCode();
		internal static int s_ScaleValueHandleHash = "ScaleValueHandleHash".GetHashCode();
		internal static int s_xAxisMoveHandleHash = "xAxisFreeMoveHandleHash".GetHashCode();
		internal static int s_yAxisMoveHandleHash = "yAxisFreeMoveHandleHash".GetHashCode();
		internal static int s_zAxisMoveHandleHash = "xAxisFreeMoveHandleHash".GetHashCode();
		internal static int s_FreeMoveHandleHash = "FreeMoveHandleHash".GetHashCode();
		internal static int s_xzAxisMoveHandleHash = "xzAxisFreeMoveHandleHash".GetHashCode();
		internal static int s_xyAxisMoveHandleHash = "xyAxisFreeMoveHandleHash".GetHashCode();
		internal static int s_yzAxisMoveHandleHash = "yzAxisFreeMoveHandleHash".GetHashCode();
		private static Vector3[] s_RectangleCapPointsCache = new Vector3[5];
		internal static Camera s_Camera;

		public static bool lighting
		{
			get { return s_Lighting; } 
			set { s_Lighting = value; }
		}

		public static Camera currentCamera
		{
			get
			{
				return s_Camera;
			}
			set
			{
				s_Camera = value; 
			}
		}

		public static Matrix4x4 matrix
		{
			get
			{
				return s_Matrix;
			}
			set
			{
				s_Matrix = value;
				s_InverseMatrix = value.inverse;
			}
		}

		public static Matrix4x4 inverseMatrix
		{
			get
			{
				return s_InverseMatrix;
			}
		}

		#region Color
		public static Color color
		{
			get
			{
				return s_Color;
			}
			set
			{
				s_Color = value;
			}
		}

		public static Color xAxisColor
		{
			get
			{
				return s_XAxisColor;
			}
		}

		public static Color yAxisColor
		{
			get
			{
				return s_YAxisColor;
			}
		}

		public static Color zAxisColor
		{
			get
			{
				return s_ZAxisColor;
			}
		}

		public static Color centerColor
		{
			get
			{
				return s_CenterColor;
			}
		}

		public static Color selectedColor
		{
			get
			{
				return s_SelectedColor;
			}
		}

		public static Color secondaryColor
		{
			get
			{
				return s_SecondaryColor;
			}
		}

		internal static Color realHandleColor
		{
			get
			{
				return s_Color * new Color(1f, 1f, 1f, 0.5f) + ((!s_Lighting) ? new Color(0f, 0f, 0f, 0f) : new Color(0f, 0f, 0f, 0.5f));
			}
		}
		#endregion

		private static bool currentlyDragging
		{
			get
			{
				return GUIUtility.hotControl != 0;
			}
		}

		internal static void Init()
		{
			if (!s_CubeMesh)
			{
				s_ConeMesh = Resources.Load<Mesh>("RuntimeSceneView/Cone_Handle");
				s_CubeMesh = Resources.Load<Mesh>("RuntimeSceneView/Cube_Handle");
				s_SphereMesh = Resources.Load<Mesh>("RuntimeSceneView/Sphere_Handle");
				s_CylinderMesh = Resources.Load<Mesh>("RuntimeSceneView/Cylinder_Handle");
				s_QuadMesh = Resources.Load<Mesh>("RuntimeSceneView/Quad_Handle");
			}
		}

		internal static void SetupIgnoreRaySnapObjects()
		{
			RuntimeSelectionMode selectionMode = (RuntimeSelectionMode) 10;
			RuntimeHandlesUtility.ignoreRaySnapObjects = RuntimeSelection.GetProps(selectionMode);
		}

		public static void SetCamera(Rect position, Camera camera)
		{
			Rect rect = GUIClipUtil.Unclip(position);
			rect = RuntimeHandlesUtility.PointsToPixels(rect);
			Rect pixelRect = new Rect(rect.xMin, (float)Screen.height - rect.yMax, rect.width, rect.height);
			camera.pixelRect = pixelRect;
			Event current = Event.current;
			
			if (current.type == EventType.Repaint)
			{
				Camera.SetupCurrent(camera);
			}
			else
			{
				currentCamera = camera;
			}
		}

		internal static void SetDiscSectionPoints(Vector3[] dest, int count, Vector3 center, Vector3 normal, Vector3 from, float angle, float radius)
		{
			from.Normalize();
			Quaternion rotation = Quaternion.AngleAxis(angle / (float)(count - 1), normal);
			Vector3 vector = from * radius;
			for (int i = 0; i < count; i++)
			{
				dest[i] = center + vector;
				vector = rotation * vector;
			}
		}

		public static float SnapValue(float val, float snap)
		{
			if (RuntimeEditorGUI.actionKey && snap > 0f)
			{
				return Mathf.Round(val / snap) * snap;
			}
			return val;
		}

		public static void ArrowCap(int controlID, Vector3 position, Quaternion rotation, float size)
		{
			if (Event.current.type != EventType.Repaint)
			{
				return;
			}
			Vector3 vector = rotation * Vector3.forward;
			ConeCap(controlID, position + vector * size, Quaternion.LookRotation(vector), size * 0.2f);
			DrawLine(position, position + vector * size * 0.9f);
		}

		public static void ConeCap(int controlID, Vector3 position, Quaternion rotation, float size)
		{
			if (Event.current.type != EventType.Repaint)
			{
				return;
			}
			Graphics.DrawMeshNow(s_ConeMesh, StartCapDraw(position, rotation, size));
		}

		public static void CubeCap(int controlID, Vector3 position, Quaternion rotation, float size)
		{
			if (Event.current.type != EventType.Repaint)
			{
				return;
			}
			Graphics.DrawMeshNow(s_CubeMesh, StartCapDraw(position, rotation, size));
		}

		public static Quaternion Disc(Quaternion rotation, Vector3 position, Vector3 axis, float size, bool cutoffPlane, float snap)
		{
			int controlID = GUIUtility.GetControlID(s_DiscHash, FocusType.Keyboard);
			return Gizmos.Disc.Do(controlID, rotation, position, axis, size, cutoffPlane, snap);
		}

		public static void RectangleCap(int controlID, Vector3 position, Quaternion rotation, float size)
		{
			RectangleCap(controlID, position, rotation, new Vector2(size, size));
		}

		internal static void RectangleCap(int controlID, Vector3 position, Quaternion rotation, Vector2 size)
		{
			if (Event.current.type != EventType.Repaint)
			{
				return;
			}
			Vector3 b = rotation * new Vector3(size.x, 0f, 0f);
			Vector3 b2 = rotation * new Vector3(0f, size.y, 0f);
			s_RectangleCapPointsCache[0] = position + b + b2;
			s_RectangleCapPointsCache[1] = position + b - b2;
			s_RectangleCapPointsCache[2] = position - b - b2;
			s_RectangleCapPointsCache[3] = position - b + b2;
			s_RectangleCapPointsCache[4] = position + b + b2;
			DrawPolyLine(s_RectangleCapPointsCache);
		}

		public static float ScaleValueHandle(float value, Vector3 position, Quaternion rotation, float size, DrawCapFunction capFunc, float snap)
		{
			int controlID = GUIUtility.GetControlID(s_ScaleValueHandleHash, FocusType.Keyboard);
			return SliderScale.DoCenter(controlID, value, position, rotation, size, capFunc, snap);
		}

		public static void DrawLine(Vector3 p1, Vector3 p2)
		{
			if (!BeginLineDrawing(matrix, false))
			{
				return;
			}
			GL.Vertex(p1);
			GL.Vertex(p2);
			EndLineDrawing();
		}

		public static void DrawPolyLine(params Vector3[] points)
		{
			if (!BeginLineDrawing(matrix, false))
			{
				return;
			}
			for (int i = 1; i < points.Length; i++)
			{
				GL.Vertex(points[i]);
				GL.Vertex(points[i - 1]);
			}
			EndLineDrawing();
		}

		public static void DrawSolidArc(Vector3 center, Vector3 normal, Vector3 from, float angle, float radius)
		{
			if (Event.current.type != EventType.Repaint)
			{
				return;
			}
			Vector3[] array = new Vector3[60];
			SetDiscSectionPoints(array, 60, center, normal, from, angle, radius);
			Shader.SetGlobalColor("_HandleColor", color * new Color(1f, 1f, 1f, 0.5f));
			Shader.SetGlobalFloat("_HandleSize", 1f);
			RuntimeHandlesUtility.ApplyWireMaterial();
			GL.PushMatrix();
			GL.MultMatrix(matrix);
			GL.Begin(4);
			for (int i = 1; i < array.Length; i++)
			{
				GL.Color(color);
				GL.Vertex(center);
				GL.Vertex(array[i - 1]);
				GL.Vertex(array[i]);
				GL.Vertex(center);
				GL.Vertex(array[i]);
				GL.Vertex(array[i - 1]);
			}
			GL.End();
			GL.PopMatrix();
		}

		public static void DrawWireDisc(Vector3 center, Vector3 normal, float radius)
		{
			Vector3 from = Vector3.Cross(normal, Vector3.up);
			if (from.sqrMagnitude < 0.001f)
			{
				from = Vector3.Cross(normal, Vector3.right);
			}
			DrawWireArc(center, normal, from, 360f, radius);
		}

		public static void DrawWireArc(Vector3 center, Vector3 normal, Vector3 from, float angle, float radius)
		{
			Vector3[] array = new Vector3[60];
			SetDiscSectionPoints(array, 60, center, normal, from, angle, radius);
			DrawPolyLine(array);
		}

		private static bool BeginLineDrawing(Matrix4x4 matrix, bool dottedLines)
		{
			if (Event.current.type != EventType.Repaint)
			{
				return false;
			}
			Color c = s_Color * lineTransparency;
			RuntimeHandlesUtility.ApplyWireMaterial();
			GL.PushMatrix();
			GL.MultMatrix(matrix);
			GL.Begin(1);
			GL.Color(c);
			return true;
		}

		private static void EndLineDrawing()
		{
			GL.End();
			GL.PopMatrix();
		}

		internal static Matrix4x4 StartCapDraw(Vector3 position, Quaternion rotation, float size)
		{
			Shader.SetGlobalColor("_HandleColor", realHandleColor);
			Shader.SetGlobalFloat("_HandleSize", size);
			Matrix4x4 matrix4x = matrix * Matrix4x4.TRS(position, rotation, Vector3.one * size);
			Shader.SetGlobalMatrix("_ObjectToWorld", matrix4x);
			RuntimeHandlesUtility.handleMaterial.SetPass(0);
			return matrix4x;
		}

		#region Slider2D
		public static Vector3 Slider2D(int id, Vector3 handlePos, Vector3 offset, Vector3 handleDir, Vector3 slideDir1, Vector3 slideDir2, float handleSize, DrawCapFunction drawFunc, Vector2 snap)
		{
			bool drawHelper = false;
			return Slider2D(id, handlePos, offset, handleDir, slideDir1, slideDir2, handleSize, drawFunc, snap, drawHelper);
		}

		public static Vector3 Slider2D(int id, Vector3 handlePos, Vector3 offset, Vector3 handleDir, Vector3 slideDir1, Vector3 slideDir2, float handleSize, DrawCapFunction drawFunc, Vector2 snap, bool drawHelper)
		{
			return Gizmos.Slider2D.Do(id, handlePos, offset, handleDir, slideDir1, slideDir2, handleSize, drawFunc, snap, drawHelper);
		}

		public static Vector3 Slider2D(Vector3 handlePos, Vector3 handleDir, Vector3 slideDir1, Vector3 slideDir2, float handleSize, DrawCapFunction drawFunc, Vector2 snap)
		{
			bool drawHelper = false;
			return Slider2D(handlePos, handleDir, slideDir1, slideDir2, handleSize, drawFunc, snap, drawHelper);
		}

		public static Vector3 Slider2D(Vector3 handlePos, Vector3 handleDir, Vector3 slideDir1, Vector3 slideDir2, float handleSize, DrawCapFunction drawFunc, Vector2 snap, bool drawHelper)
		{
			int controlID = GUIUtility.GetControlID(s_Slider2DHash, FocusType.Keyboard);
			return Gizmos.Slider2D.Do(controlID, handlePos, new Vector3(0f, 0f, 0f), handleDir, slideDir1, slideDir2, handleSize, drawFunc, snap, drawHelper);
		}

		public static Vector3 Slider2D(int id, Vector3 handlePos, Vector3 handleDir, Vector3 slideDir1, Vector3 slideDir2, float handleSize, DrawCapFunction drawFunc, Vector2 snap)
		{
			bool drawHelper = false;
			return Slider2D(id, handlePos, handleDir, slideDir1, slideDir2, handleSize, drawFunc, snap, drawHelper);
		}

		public static Vector3 Slider2D(int id, Vector3 handlePos, Vector3 handleDir, Vector3 slideDir1, Vector3 slideDir2, float handleSize, DrawCapFunction drawFunc, Vector2 snap,bool drawHelper)
		{
			return Gizmos.Slider2D.Do(id, handlePos, new Vector3(0f, 0f, 0f), handleDir, slideDir1, slideDir2, handleSize, drawFunc, snap, drawHelper);
		}

		public static Vector3 Slider2D(Vector3 handlePos, Vector3 handleDir, Vector3 slideDir1, Vector3 slideDir2, float handleSize, DrawCapFunction drawFunc, float snap)
		{
			bool drawHelper = false;
			return Slider2D(handlePos, handleDir, slideDir1, slideDir2, handleSize, drawFunc, snap, drawHelper);
		}

		public static Vector3 Slider2D(Vector3 handlePos, Vector3 handleDir, Vector3 slideDir1, Vector3 slideDir2, float handleSize, DrawCapFunction drawFunc, float snap,bool drawHelper)
		{
			int controlID = GUIUtility.GetControlID(s_Slider2DHash, FocusType.Keyboard);
			return Slider2D(controlID, handlePos, new Vector3(0f, 0f, 0f), handleDir, slideDir1, slideDir2, handleSize, drawFunc, new Vector2(snap, snap), drawHelper);
		}
		#endregion

		public static Vector3 Slider(Vector3 position, Vector3 direction)
		{
			return Slider(position, direction, RuntimeHandlesUtility.GetHandleSize(position), ArrowCap, -1f);
		}

		public static Vector3 Slider(Vector3 position, Vector3 direction, float size, DrawCapFunction drawFunc, float snap)
		{
			int controlID = GUIUtility.GetControlID(s_SliderHash, FocusType.Keyboard);
			return Slider1D.Do(controlID, position, direction, size, drawFunc, snap);
		}

		public static float ScaleSlider(float scale, Vector3 position, Vector3 direction, Quaternion rotation, float size, float snap)
		{
			int controlID = GUIUtility.GetControlID(s_ScaleSliderHash, FocusType.Keyboard);
			return SliderScale.DoAxis(controlID, scale, position, direction, rotation, size, snap);
		}

		public static Vector3 DoPositionHandle(Vector3 position, Quaternion rotation)
		{
			Event current = Event.current;
			switch (current.type)
			{
				case EventType.KeyUp:
					position = DoPositionHandle_Internal(position, rotation);
					return position;
			}
			return DoPositionHandle_Internal(position, rotation);
		}

		public static Quaternion DoRotationHandle(Quaternion rotation, Vector3 position)
		{
			float handleSize = RuntimeHandlesUtility.GetHandleSize(position);
			Color color = RuntimeHandles.color;
			bool flag = !Tools.s_Hidden;
			RuntimeHandles.color = ((!flag) ? xAxisColor : Color.Lerp(xAxisColor, staticColor, staticBlend));
			rotation = Disc(rotation, position, rotation * Vector3.right, handleSize, true, RuntimeSnapSettings.rotation);
			RuntimeHandles.color = ((!flag) ? yAxisColor : Color.Lerp(yAxisColor, staticColor, staticBlend));
			rotation = Disc(rotation, position, rotation * Vector3.up, handleSize, true, RuntimeSnapSettings.rotation);
			RuntimeHandles.color = ((!flag) ? zAxisColor : Color.Lerp(zAxisColor, staticColor, staticBlend));
			rotation = Disc(rotation, position, rotation * Vector3.forward, handleSize, true, RuntimeSnapSettings.rotation);
			if (!flag)
			{
				RuntimeHandles.color = centerColor;
				rotation = Disc(rotation, position, currentCamera.transform.forward, handleSize * 1.1f, false, 0f);
				//rotation = FreeRotateHandle(rotation, position, handleSize);
			}
			RuntimeHandles.color = color;
			return rotation;
		}

		public static Vector3 DoScaleHandle(Vector3 scale, Vector3 position, Quaternion rotation)
		{
			return DoScaleHandle(scale, position, rotation, RuntimeHandlesUtility.GetHandleSize(position));
		}

		public static Vector3 DoScaleHandle(Vector3 scale, Vector3 position, Quaternion rotation, float size)
		{
			
			bool flag = !Tools.s_Hidden;
			color = ((!flag) ? xAxisColor : Color.Lerp(xAxisColor, staticColor, staticBlend));
			scale.x = ScaleSlider(scale.x, position, rotation * Vector3.right, rotation, size, RuntimeSnapSettings.scale);
			color = ((!flag) ? yAxisColor : Color.Lerp(yAxisColor, staticColor, staticBlend));
			scale.y = ScaleSlider(scale.y, position, rotation * Vector3.up, rotation, size, RuntimeSnapSettings.scale);
			color = ((!flag) ? zAxisColor : Color.Lerp(zAxisColor, staticColor, staticBlend));
			scale.z = ScaleSlider(scale.z, position, rotation * Vector3.forward, rotation, size, RuntimeSnapSettings.scale);
			color = centerColor;
			GUI.changed = false;
			float num = ScaleValueHandle(scale.x, position, rotation, size, new DrawCapFunction(CubeCap), RuntimeSnapSettings.scale);
			if (GUI.changed)
			{
				float num2 = num / scale.x;
				scale.x = num;
				scale.y *= num2;
				scale.z *= num2;
			}
			return scale;
		}

		private static Vector3 DoPositionHandle_Internal(Vector3 position, Quaternion rotation)
		{
			float handleSize = RuntimeHandlesUtility.GetHandleSize(position);
			Color color = RuntimeHandles.color;
			bool flag = !Tools.s_Hidden;
			RuntimeHandles.color = ((!flag) ? xAxisColor : Color.Lerp(xAxisColor, staticColor, staticBlend));
			GUI.SetNextControlName("xAxis");
			position = Slider(position, rotation * Vector3.right, handleSize, ArrowCap, RuntimeSnapSettings.move.x);
			RuntimeHandles.color = ((!flag) ? yAxisColor : Color.Lerp(yAxisColor, staticColor, staticBlend));
			GUI.SetNextControlName("yAxis");
			position = Slider(position, rotation * Vector3.up, handleSize, ArrowCap, RuntimeSnapSettings.move.x);
			RuntimeHandles.color = ((!flag) ? zAxisColor : Color.Lerp(zAxisColor, staticColor, staticBlend));
			GUI.SetNextControlName("zAxis");
			position = Slider(position, rotation * Vector3.forward, handleSize, ArrowCap, RuntimeSnapSettings.move.x);
			position = DoPlanarHandle(PlaneHandle.xzPlane, position, rotation, handleSize * 0.25f);
			position = DoPlanarHandle(PlaneHandle.xyPlane, position, rotation, handleSize * 0.25f);
			position = DoPlanarHandle(PlaneHandle.yzPlane, position, rotation, handleSize * 0.25f);
			RuntimeHandles.color = color;
			return position;
		}

		private static Vector3 DoPlanarHandle(PlaneHandle planeID, Vector3 position, Quaternion rotation, float handleSize)
		{
			int num = 0;
			int num2 = 0;
			int hint = 0;
			bool flag = !Tools.s_Hidden;
			switch (planeID)
			{
				case PlaneHandle.xzPlane:
					num = 0;
					num2 = 2;
					RuntimeHandles.color = ((!flag) ? yAxisColor : staticColor);
					hint = s_xzAxisMoveHandleHash;
					break;
				case PlaneHandle.xyPlane:
					num = 0;
					num2 = 1;
					RuntimeHandles.color = ((!flag) ? zAxisColor : staticColor);
					hint = s_xyAxisMoveHandleHash;
					break;
				case PlaneHandle.yzPlane:
					num = 1;
					num2 = 2;
					RuntimeHandles.color = ((!flag) ? xAxisColor : staticColor);
					hint = s_yzAxisMoveHandleHash;
					break;
			}
			int index = 3 - num2 - num;
			Color color = RuntimeHandles.color;
			Matrix4x4 matrix4x = Matrix4x4.TRS(position, rotation, Vector3.one);
			Vector3 normalized;
			if (currentCamera.orthographic)
			{
				normalized = matrix4x.inverse.MultiplyVector(currentCamera.transform.rotation * -Vector3.forward).normalized;
			}
			else
			{
				normalized = matrix4x.inverse.MultiplyPoint(currentCamera.transform.position).normalized;
			}
			int controlID = GUIUtility.GetControlID(hint, FocusType.Keyboard);
			if (Mathf.Abs(normalized[index]) < 0.05f && GUIUtility.hotControl != controlID)
			{
				RuntimeHandles.color = color;
				return position;
			}
			if (!currentlyDragging)
			{
				s_PlanarHandlesOctant[num] = (float)((normalized[num] >= -0.01f) ? 1 : -1);
				s_PlanarHandlesOctant[num2] = (float)((normalized[num2] >= -0.01f) ? 1 : -1);
			}
			Vector3 vector = s_PlanarHandlesOctant;
			vector[index] = 0f;
			vector = rotation * (vector * handleSize * 0.5f);
			Vector3 vector2 = Vector3.zero;
			Vector3 vector3 = Vector3.zero;
			Vector3 vector4 = Vector3.zero;
			vector2[num] = 1f;
			vector3[num2] = 1f;
			vector4[index] = 1f;
			vector2 = rotation * vector2;
			vector3 = rotation * vector3;
			vector4 = rotation * vector4;
			verts[0] = position + vector + (vector2 + vector3) * handleSize * 0.5f;
			verts[1] = position + vector + (-vector2 + vector3) * handleSize * 0.5f;
			verts[2] = position + vector + (-vector2 - vector3) * handleSize * 0.5f;
			verts[3] = position + vector + (vector2 - vector3) * handleSize * 0.5f;
			DrawSolidRectangleWithOutline(RuntimeHandles.verts, new Color(RuntimeHandles.color.r, RuntimeHandles.color.g, RuntimeHandles.color.b, 0.1f), new Color(0f, 0f, 0f, 0f));
			position = Slider2D(controlID, position, vector, vector4, vector2, vector3, handleSize * 0.5f, RectangleCap, new Vector2(RuntimeSnapSettings.move[num], RuntimeSnapSettings.move[num2]));
			RuntimeHandles.color = color;
			return position;
		}

		public static void DrawSolidRectangleWithOutline(Rect rectangle, Color faceColor, Color outlineColor)
		{
			Vector3[] array = new Vector3[]
			{
				new Vector3(rectangle.xMin, rectangle.yMin, 0f),
				new Vector3(rectangle.xMax, rectangle.yMin, 0f),
				new Vector3(rectangle.xMax, rectangle.yMax, 0f),
				new Vector3(rectangle.xMin, rectangle.yMax, 0f)
			};
			DrawSolidRectangleWithOutline(array, faceColor, outlineColor);
		}

		public static void DrawSolidRectangleWithOutline(Vector3[] verts, Color faceColor, Color outlineColor)
		{
			if (Event.current.type != EventType.Repaint)
			{
				return;
			}
			RuntimeHandlesUtility.ApplyWireMaterial();
			GL.PushMatrix();
			GL.MultMatrix(matrix);
			if (faceColor.a > 0f)
			{
				Color c = faceColor * color;
				GL.Begin(4);
				for (int i = 0; i < 2; i++)
				{
					GL.Color(c);
					GL.Vertex(verts[i * 2]);
					GL.Vertex(verts[i * 2 + 1]);
					GL.Vertex(verts[(i * 2 + 2) % 4]);
					GL.Vertex(verts[i * 2]);
					GL.Vertex(verts[(i * 2 + 2) % 4]);
					GL.Vertex(verts[i * 2 + 1]);
				}
				GL.End();
			}
			if (outlineColor.a > 0f)
			{
				Color c2 = outlineColor * color;
				GL.Begin(1);
				GL.Color(c2);
				for (int j = 0; j < 4; j++)
				{
					GL.Vertex(verts[j]);
					GL.Vertex(verts[(j + 1) % 4]);
				}
				GL.End();
			}
			GL.PopMatrix();
		}
	}
}