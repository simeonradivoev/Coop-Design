using UnityEngine;

namespace Gizmos
{
	public class Tools : ScriptableObject
	{
		public static readonly int ViewToolHash = "ViewToolHash".GetHashCode();
		internal delegate void OnToolChangedFunc(RuntimeTool from, RuntimeTool to);
		internal static event OnToolChangedFunc onToolChanged;
		internal static bool vertexDragging;
		internal static bool s_Hidden = true;
		private static bool s_LockHandlePositionActive = false;
		private static Vector3 s_LockHandlePosition;
		private bool m_RectBlueprintMode;
		private static Tools s_Get;
		private PivotRotation m_PivotRotation;
		private PivotMode m_PivotMode = PivotMode.Pivot;
		internal static Vector3 handleOffset;
		internal static Vector3 localHandleOffset;
		private RuntimeTool currentTool = RuntimeTool.Move;
		internal Quaternion m_GlobalHandleRotation = Quaternion.identity;
		internal static RuntimeViewTool s_LockedViewTool = RuntimeViewTool.None;
		internal static int s_ButtonDown = -1;
		private RuntimeViewTool m_ViewTool = RuntimeViewTool.Pan;

		private void OnEnable()
		{
			Tools.s_Get = this;
			Tools.pivotMode = PivotMode.Pivot;
			//Tools.pivotMode = (PivotMode)PlayerPrefs.GetInt("PivotMode", 1);
			Tools.pivotRotation = (PivotRotation)PlayerPrefs.GetInt("PivotRotation", 0);
		}

		private static Tools get
		{
			get
			{
				if (!Tools.s_Get)
				{
					Tools.s_Get = ScriptableObject.CreateInstance<Tools>();
					Tools.s_Get.hideFlags = HideFlags.HideAndDontSave;
				}
				return Tools.s_Get;
			}
		}

		internal static bool viewToolActive
		{
			get
			{
				return (GUIUtility.hotControl == 0 || Tools.s_LockedViewTool != RuntimeViewTool.None) && (Tools.s_LockedViewTool != RuntimeViewTool.None || Tools.current == RuntimeTool.View || (Event.current != null && Event.current.alt) || Tools.s_ButtonDown == 1 || Tools.s_ButtonDown == 2);
			}
		}

		public static RuntimeViewTool viewTool
		{
			get
			{
				Event current = Event.current;
				if (current != null && Tools.viewToolActive)
				{
					if (Tools.s_LockedViewTool == RuntimeViewTool.None)
					{
						bool flag = current.control && Application.platform == RuntimePlatform.OSXEditor;
						bool actionKey = RuntimeEditorGUI.actionKey;
						bool flag2 = !actionKey && !flag && !current.alt;
						if ((Tools.s_ButtonDown <= 0 && flag2) || (Tools.s_ButtonDown <= 0 && actionKey) || Tools.s_ButtonDown == 2)
						{
							Tools.get.m_ViewTool = RuntimeViewTool.Pan;
						}
						else if ((Tools.s_ButtonDown <= 0 && flag) || (Tools.s_ButtonDown == 1 && current.alt))
						{
							Tools.get.m_ViewTool = RuntimeViewTool.Zoom;
						}
						else if (Tools.s_ButtonDown <= 0 && current.alt)
						{
							Tools.get.m_ViewTool = RuntimeViewTool.Orbit;
						}
						else if (Tools.s_ButtonDown == 1 && !current.alt)
						{
							Tools.get.m_ViewTool = RuntimeViewTool.FPS;
						}
					}
				}
				else
				{
					Tools.get.m_ViewTool = RuntimeViewTool.Pan;
				}
				return Tools.get.m_ViewTool;
			}
			set
			{
				Tools.get.m_ViewTool = value;
			}
		}

		public static RuntimeTool current
		{
			get
			{
				return Tools.get.currentTool;
			}
			set
			{
				if (get.currentTool != value)
				{
					RuntimeTool from = Tools.get.currentTool;
					get.currentTool = value;
					if (onToolChanged != null)
					{
						onToolChanged(from, value);
					}
				}
			}
		}

		public static bool hidden
		{
			get
			{
				return Tools.s_Hidden;
			}
			set
			{
				Tools.s_Hidden = value;
			}
		}

		public static Vector3 handlePosition
		{
			get
			{
				WorldProp activeTransform = RuntimeSelection.activeProp;
				if (!activeTransform)
				{
					return new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
				}
				if (Tools.s_LockHandlePositionActive)
				{
					return Tools.s_LockHandlePosition;
				}
				return Tools.GetHandlePosition();
			}
		}

		internal static Vector3 GetHandlePosition()
		{
			WorldProp activeTransform = RuntimeSelection.activeProp;
			if (!activeTransform)
			{
				return new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
			}
			Vector3 b = Tools.handleOffset + Tools.handleRotation * Tools.localHandleOffset;
			PivotMode pivotMode = Tools.get.m_PivotMode;
			if (pivotMode != PivotMode.Pivot)
			{
				return new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
			}
			return activeTransform.Position + b;
		}

		public static PivotRotation pivotRotation
		{
			get
			{
				return Tools.get.m_PivotRotation;
			}
			set
			{
				if (Tools.get.m_PivotRotation != value)
				{
					Tools.get.m_PivotRotation = value;
					PlayerPrefs.SetInt("PivotRotation", (int)Tools.pivotRotation);
				}
			}
		}

		public static PivotMode pivotMode
		{
			get
			{
				return Tools.get.m_PivotMode;
			}
			set
			{
				if (Tools.get.m_PivotMode != value)
				{
					Tools.get.m_PivotMode = value;
					PlayerPrefs.SetInt("PivotMode", (int)Tools.pivotMode);
				}
			}
		}

		public static Quaternion handleRotation
		{
			get
			{
				PivotRotation pivotRotation = Tools.get.m_PivotRotation;
				if (pivotRotation == PivotRotation.Local)
				{
					return Tools.handleLocalRotation;
				}
				if (pivotRotation != PivotRotation.Global)
				{
					return Quaternion.identity;
				}
				return Tools.get.m_GlobalHandleRotation;
			}
			set
			{
				if (Tools.get.m_PivotRotation == PivotRotation.Global)
				{
					Tools.get.m_GlobalHandleRotation = value;
				}
			}
		}

		internal static Quaternion handleLocalRotation
		{
			get
			{
				WorldProp activeTransform = RuntimeSelection.activeProp;
				if (!activeTransform)
				{
					return Quaternion.identity;
				}
				return activeTransform.Rotation;
			}
		}

		public static bool rectBlueprintMode
		{
			get
			{
				return Tools.get.m_RectBlueprintMode;
			}
			set
			{
				if (Tools.get.m_RectBlueprintMode != value)
				{
					Tools.get.m_RectBlueprintMode = value;
					PlayerPrefs.SetInt("RectBlueprintMode", rectBlueprintMode ? 1 : 0);
				}
			}
		}

		internal static void LockHandlePosition()
		{
			Tools.LockHandlePosition(Tools.handlePosition);
		}

		internal static void LockHandlePosition(Vector3 pos)
		{
			Tools.s_LockHandlePosition = pos;
			Tools.s_LockHandlePositionActive = true;
		}

		internal static void UnlockHandlePosition()
		{
			Tools.s_LockHandlePositionActive = false;
		}
	}
}