using UnityEngine;
using System.Collections;
using Gizmos;

public class ToolbarUI : MonoBehaviour
{
	[SerializeField] private Texture localSpace;
	[SerializeField] private Texture globalSpace;
	[SerializeField] private Texture zoomTool;
	[SerializeField] private Texture panTool;
	[SerializeField] private Texture viewTool;
	[SerializeField] private Texture moveTool;
	[SerializeField] private Texture rotateTool;
	[SerializeField] private Texture scaleTool;
	[SerializeField] private float opacity = 0.8f;

	private GUISkin skin;
	private GUIStyle toolButton;
	private GUIStyle toolButtonActive;

	private void Awake()
	{
		skin = Resources.Load<GUISkin>("Skin");
		toolButton = skin.FindStyle("ToolbarButton");
		toolButtonActive = skin.FindStyle("ToolbarButtonActive");
	}

	private const float margin = 20;
	private const float toolbarHeight = 38;
	private const float toolbarWidth = 146;

	private void OnGUI()
	{
		GUI.skin = skin;
		GUI.backgroundColor = new Color(1,1,1, opacity);
		ManageToolShortcuts();

		Rect toolsRect = new Rect(margin, margin, toolbarWidth, toolbarHeight);
		GUILayout.BeginArea(toolsRect, GUI.skin.box);
		GUILayout.BeginHorizontal();
		if (GUILayout.Button(new GUIContent(Tools.viewTool == RuntimeViewTool.FPS ? viewTool : Tools.viewTool == RuntimeViewTool.Pan ? panTool : Tools.viewTool == RuntimeViewTool.Zoom ? zoomTool : viewTool), Tools.viewToolActive ? toolButtonActive : toolButton, GUILayout.Width(toolbarHeight - 8), GUILayout.Height(toolbarHeight - 8)) && Tools.current != RuntimeTool.Move)
		{
			Tools.current = RuntimeTool.View;
		}
		GUILayout.Space(6);
		if (GUILayout.Button(new GUIContent(moveTool),Tools.current == RuntimeTool.Move ? toolButtonActive : toolButton, GUILayout.Width(toolbarHeight - 8), GUILayout.Height(toolbarHeight - 8)) && Tools.current != RuntimeTool.Move)
		{
			Tools.current = RuntimeTool.Move;
		}
		GUILayout.Space(6);
		if(GUILayout.Button(new GUIContent(rotateTool), Tools.current == RuntimeTool.Rotate ? toolButtonActive : toolButton, GUILayout.Width(toolbarHeight - 8), GUILayout.Height(toolbarHeight - 8)) && Tools.current != RuntimeTool.Rotate)
		{
			Tools.current = RuntimeTool.Rotate;
		}
		GUILayout.Space(6);
		if (GUILayout.Button(new GUIContent(scaleTool), Tools.current == RuntimeTool.Scale ? toolButtonActive : toolButton, GUILayout.Width(toolbarHeight - 8), GUILayout.Height(toolbarHeight - 8)) && Tools.current != RuntimeTool.Scale)
		{
			Tools.current = RuntimeTool.Scale;
		}
		GUILayout.EndHorizontal();
		GUILayout.EndArea();

		Rect spaceRect = new Rect(toolsRect.xMax + 10, toolsRect.yMin, toolbarHeight, toolsRect.height);
		GUILayout.BeginArea(spaceRect, GUI.skin.box);
		GUILayout.BeginHorizontal();
		if (GUILayout.Button(new GUIContent(Tools.pivotRotation == PivotRotation.Local ? localSpace : globalSpace), toolButton, GUILayout.Width(toolbarHeight - 8), GUILayout.Height(toolbarHeight - 8)))
		{
			Tools.pivotRotation = Tools.pivotRotation == PivotRotation.Local ? PivotRotation.Global : PivotRotation.Local;
		}
		GUILayout.EndHorizontal();
		GUILayout.EndArea();
	}

	private void ManageToolShortcuts()
	{
		var current = Event.current;

		if (current.type == EventType.KeyDown && !Tools.viewToolActive)
		{
			switch (current.keyCode)
			{
				case KeyCode.W:
					Tools.current = RuntimeTool.Move;
					break;
				case KeyCode.E:
					Tools.current = RuntimeTool.Rotate;
					break;
				case KeyCode.R:
					Tools.current = RuntimeTool.Scale;
					break;
			}
		}
	}
}
