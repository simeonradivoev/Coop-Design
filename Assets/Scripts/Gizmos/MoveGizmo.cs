using System;
using UnityEngine;

namespace Gizmos
{
	public class MoveGizmo : MonoBehaviour
	{
		private Camera camera;

		private void OnGUI()
		{
			if (camera == null)
			{
				camera = Player.Camera;
				return;
			}

			var current = Event.current;
			RuntimeHandles.SetCamera(camera.pixelRect,camera);
			RuntimeHandlesUtility.BeginHandles();
			RuntimeHandles.currentCamera = camera;
			WorldProp selection = RuntimeSelection.activeProp;
			if (selection != null)
			{
				switch (Tools.current)
				{
					case RuntimeTool.Move:
						MoveToolGUI(MoveToolGUI);
						break;
					case RuntimeTool.Rotate:
						RuntimeEditorGUI.BeginChangeCheck();
						Quaternion rotation = RuntimeHandles.DoRotationHandle(selection.Rotation, Tools.handlePosition);
						if (RuntimeEditorGUI.EndChangeCheck())
						{
							UndoManager.PushUndo("Rotate",r => selection.SetRotation((Quaternion)r,true),selection.Rotation);
						}
						selection.SetRotation(rotation,true);
						break;
					case RuntimeTool.Scale:
						Vector3 scale = RuntimeHandles.DoScaleHandle(selection.Scale, Tools.handlePosition, Tools.handleRotation);
						selection.SetScale(scale,true);
						break;
				}
			}
			RuntimeHandlesUtility.EndHandles();
		}

		protected virtual void MoveToolGUI(Action<Vector3,bool> function)
		{
			if (!RuntimeSelection.activeProp)
			{
				return;
			}
			using (new RuntimeEditorGUI.DisabledScope(false))
			{
				Vector3 handlePosition = Tools.handlePosition;
				function(handlePosition, false);
				//RuntimeHandles.ShowStaticLabelIfNeeded(handlePosition);
			}
		}

		private void MoveToolGUI(Vector3 handlePosition, bool isStatic)
		{

			RuntimeTransformManipulator.BeginManipulationHandling(false);
			RuntimeEditorGUI.BeginChangeCheck();
			Vector3 a = RuntimeHandles.DoPositionHandle(handlePosition, Tools.handleRotation);
			if (RuntimeEditorGUI.EndChangeCheck() && !isStatic)
			{
				Vector3 positionDelta = a - RuntimeTransformManipulator.mouseDownHandlePosition;
				RuntimeTransformManipulator.SetMinDragDifferenceForPos(handlePosition);
				RuntimeTransformManipulator.SetPositionDelta(positionDelta);
			}
			RuntimeTransformManipulator.EndManipulationHandling();
		}
	}
}