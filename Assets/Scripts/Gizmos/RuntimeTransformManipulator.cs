using System;
using UnityEngine;

namespace Gizmos
{
	public class RuntimeTransformManipulator
	{
		private static EventType s_EventTypeBefore = EventType.Ignore;
		private static bool s_LockHandle = false;
		private static int s_HotControl = 0;
		private static Vector3 s_StartHandlePosition = Vector3.zero;
		private static TransformData[] s_MouseDownState = null;
		private static Vector3 s_StartLocalHandleOffset = Vector3.zero;

		private struct TransformData
		{
			public static Quaternion[] s_Alignments = new Quaternion[]
			{
				Quaternion.LookRotation(Vector3.right, Vector3.up),
				Quaternion.LookRotation(Vector3.right, Vector3.forward),
				Quaternion.LookRotation(Vector3.up, Vector3.forward),
				Quaternion.LookRotation(Vector3.up, Vector3.right),
				Quaternion.LookRotation(Vector3.forward, Vector3.right),
				Quaternion.LookRotation(Vector3.forward, Vector3.up)
			};

			public WorldProp prop;

			public Vector3 position;

			public Vector3 localPosition;

			public Quaternion rotation;

			public Vector3 scale;

			public Rect rect;

			public Vector2 anchoredPosition;

			public Vector2 sizeDelta;

			public static TransformData GetData(WorldProp t)
			{
				TransformData result = default(TransformData);
				result.SetupTransformValues(t);
				return result;
			}

			private Quaternion GetRefAlignment(Quaternion targetRotation, Quaternion ownRotation)
			{
				float num = float.NegativeInfinity;
				Quaternion result = Quaternion.identity;
				for (int i = 0; i < s_Alignments.Length; i++)
				{
					float num2 = Mathf.Min(new float[]
					{
						Mathf.Abs(Vector3.Dot(targetRotation * Vector3.right, ownRotation * s_Alignments[i] * Vector3.right)),
						Mathf.Abs(Vector3.Dot(targetRotation * Vector3.up, ownRotation * s_Alignments[i] * Vector3.up)),
						Mathf.Abs(Vector3.Dot(targetRotation * Vector3.forward, ownRotation * s_Alignments[i] * Vector3.forward))
					});
					if (num2 > num)
					{
						num = num2;
						result = s_Alignments[i];
					}
				}
				return result;
			}

			private void SetupTransformValues(WorldProp p)
			{
				this.prop = p;
				this.position = p.Position;
				this.localPosition = p.transform.localPosition;
				this.rotation = p.Rotation;
				this.scale = p.Scale;
			}

			private void SetScaleValue(Vector3 scale)
			{
				this.prop.SetScale(scale,true);
			}

			public void SetScaleDelta(Vector3 scaleDelta, Vector3 scalePivot, Quaternion scaleRotation)
			{
				this.SetPosition(scaleRotation * Vector3.Scale(Quaternion.Inverse(scaleRotation) * (this.position - scalePivot), scaleDelta) + scalePivot);
				Vector3 minDragDifference = RuntimeTransformManipulator.minDragDifference;
				if (this.prop.transform.parent != null)
				{
					minDragDifference.x /= this.prop.transform.parent.lossyScale.x;
					minDragDifference.y /= this.prop.transform.parent.lossyScale.y;
					minDragDifference.z /= this.prop.transform.parent.lossyScale.z;
				}
				Quaternion ownRotation = (!Tools.rectBlueprintMode) ? this.rotation : this.prop.transform.parent.rotation;
				Quaternion refAlignment = this.GetRefAlignment(scaleRotation, ownRotation);
				scaleDelta = refAlignment * scaleDelta;
				scaleDelta = Vector3.Scale(scaleDelta, refAlignment * Vector3.one);
				this.SetScaleValue(Vector3.Scale(this.scale, scaleDelta));
			}

			private void SetPosition(Vector3 newPosition)
			{
				this.SetPositionDelta(newPosition - this.position);
			}

			public void SetPositionDelta(Vector3 positionDelta)
			{
				Vector3 vector = positionDelta;
				Vector3 minDragDifference = RuntimeTransformManipulator.minDragDifference;
				if (this.prop.transform.parent != null)
				{
					vector = this.prop.transform.parent.InverseTransformVector(vector);
					minDragDifference.x /= this.prop.transform.parent.lossyScale.x;
					minDragDifference.y /= this.prop.transform.parent.lossyScale.y;
					minDragDifference.z /= this.prop.transform.parent.lossyScale.z;
				}
				bool flag = Mathf.Approximately(vector.x, 0f);
				bool flag2 = Mathf.Approximately(vector.y, 0f);
				bool flag3 = Mathf.Approximately(vector.z, 0f);
				Vector3 vector3 = this.localPosition + vector;
				vector3.z = ((!flag3) ? RoundBasedOnMinimumDifference(vector3.z, minDragDifference.z) : this.localPosition.z);
				this.prop.SetLocalPosition(vector3,true);
				Vector2 vector4 = this.anchoredPosition + new Vector2(vector.x, vector.y);
				vector4.x = ((!flag) ? RoundBasedOnMinimumDifference(vector4.x, minDragDifference.x) : this.anchoredPosition.x);
				vector4.y = ((!flag2) ? RoundBasedOnMinimumDifference(vector4.y, minDragDifference.y) : this.anchoredPosition.y);
			}

			internal static float RoundBasedOnMinimumDifference(float valueToRound, float minDifference)
			{
				if (minDifference == 0f)
				{
					return DiscardLeastSignificantDecimal(valueToRound);
				}
				return (float)Math.Round((double)valueToRound, GetNumberOfDecimalsForMinimumDifference(minDifference), MidpointRounding.AwayFromZero);
			}

			internal static int GetNumberOfDecimalsForMinimumDifference(float minDifference)
			{
				return Mathf.Clamp(-Mathf.FloorToInt(Mathf.Log10(Mathf.Abs(minDifference))), 0, 15);
			}

			internal static float DiscardLeastSignificantDecimal(float v)
			{
				int digits = Mathf.Clamp((int)(5f - Mathf.Log10(Mathf.Abs(v))), 0, 15);
				return (float)Math.Round((double)v, digits, MidpointRounding.AwayFromZero);
			}
		}

		public static Vector3 minDragDifference
		{
			get;
			set;
		}

		public static bool individualSpace
		{
			get
			{
				return Tools.pivotRotation == PivotRotation.Local && Tools.pivotMode == PivotMode.Pivot;
			}
		}

		public static Vector3 mouseDownHandlePosition
		{
			get
			{
				return s_StartHandlePosition;
			}
		}

		public static void SetMinDragDifferenceForPos(Vector3 position)
		{
			minDragDifference = Vector3.one * (RuntimeHandlesUtility.GetHandleSize(position) / 80f);
		}

		private static void BeginEventCheck()
		{
			s_EventTypeBefore = Event.current.GetTypeForControl(s_HotControl);
		}

		public static void SetPositionDelta(Vector3 positionDelta)
		{
			if (s_MouseDownState == null)
			{
				return;
			}
			for (int i = 0; i < s_MouseDownState.Length; i++)
			{
				TransformData transformData = s_MouseDownState[i];
				UndoManager.PushUndo("Move",m => transformData.prop.SetPosition((Vector3)m,true),transformData.prop.Position);
			}
			for (int j = 0; j < s_MouseDownState.Length; j++)
			{
				s_MouseDownState[j].SetPositionDelta(positionDelta);
			}
		}

		public static void BeginManipulationHandling(bool lockHandleWhileDragging)
		{
			BeginEventCheck();
			s_LockHandle = lockHandleWhileDragging;
		}

		private static EventType EndEventCheck()
		{
			EventType eventType = (s_EventTypeBefore == Event.current.GetTypeForControl(s_HotControl)) ? EventType.Ignore : s_EventTypeBefore;
			s_EventTypeBefore = EventType.Ignore;
			if (eventType == EventType.MouseDown)
			{
				s_HotControl = GUIUtility.hotControl;
			}
			else if (eventType == EventType.MouseUp)
			{
				s_HotControl = 0;
			}
			return eventType;
		}

		public static EventType EndManipulationHandling()
		{
			EventType eventType = EndEventCheck();
			if (eventType == EventType.MouseDown)
			{
				RecordMouseDownState(RuntimeSelection.Props);
				s_StartHandlePosition = Tools.handlePosition;
				s_StartLocalHandleOffset = Tools.localHandleOffset;
				if (s_LockHandle)
				{
					Tools.LockHandlePosition();
				}
			}
			else if (s_MouseDownState != null && (eventType == EventType.MouseUp || GUIUtility.hotControl != s_HotControl))
			{
				s_MouseDownState = null;
				if (s_LockHandle)
				{
					Tools.UnlockHandlePosition();
				}
				DisableMinDragDifference();
			}
			return eventType;
		}

		public static void DisableMinDragDifference()
		{
			minDragDifference = Vector3.zero;
		}

		private static void RecordMouseDownState(WorldProp[] transforms)
		{
			s_MouseDownState = new TransformData[transforms.Length];
			for (int i = 0; i < transforms.Length; i++)
			{
				s_MouseDownState[i] = TransformData.GetData(transforms[i]);
			}
		}
	}
}