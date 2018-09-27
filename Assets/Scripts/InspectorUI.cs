using Assets.Scripts;
using Gizmos;
using UnityEngine;

public class InspectorUI : MonoBehaviour
{
	[SerializeField] private float opacity = 0.9f;
	private GUISkin skin;
	private Vector2 scroll;

	private const float inspectorWidth = 256 + 64;
	private const float inspectorHeight = 256 + 128;
	private const float margin = 20;

	private void Awake()
	{
		skin = Styles.Init();
	}

	private void OnGUI()
	{
		RuntimeEditorGUI.ResetGUIState();
		GUI.skin = skin;
		GUI.backgroundColor = new Color(1, 1, 1, opacity);
		var current = Event.current;

		
		if (RuntimeSelection.activeProp != null)
		{
			Rect inspectorRect = new Rect(Screen.width - inspectorWidth - margin, margin, inspectorWidth, inspectorHeight - margin * 2 - 32 - 5);
			RuntimeDragAndDrop.ManageDragTaking(inspectorRect);

			GUILayout.BeginArea(inspectorRect);
			GUILayout.BeginHorizontal(new GUIContent(RuntimeSelection.activeProp.name),Styles.inspectorHeaderStyle, GUILayout.Height(32));
			GUILayout.Space(8);
			GUILayout.EndHorizontal();

			GUILayout.BeginVertical(Styles.inspectorBg);
			scroll = GUILayout.BeginScrollView(scroll, GUIStyle.none, Styles.verticalScrollbarMenuStyle, GUILayout.ExpandHeight(true));

			DoInspector(RuntimeSelection.activeProp);
			GUILayout.EndScrollView();
			GUILayout.EndVertical();

			GUILayout.BeginHorizontal(Styles.inspectorFooterStyle, GUILayout.Height(32));
			GUILayout.Space(8);
			GUILayout.EndHorizontal();
			GUILayout.EndArea();

			if (current.type == EventType.MouseDown && GUIUtility.hotControl == 0 && inspectorRect.Contains(current.mousePosition))
			{
				current.Use();
			}
		}
	}

	private void DoInspector(WorldProp prop)
	{
		GUILayout.BeginHorizontal();
		Vector3 pos = prop.Position;
		GUILayout.Label(new GUIContent("Position"),GUILayout.Width(90));
		GUILayout.Label(new GUIContent("X"),Styles.prefixLabelClose);
		pos.x = RuntimeEditorGUI.FloatFieldInternal(GUILayoutUtility.GetRect(GUIContent.none, GUI.skin.textField), pos.x, GUI.skin.textField);
		GUILayout.Label(new GUIContent("Y"), Styles.prefixLabelClose);
		pos.y = RuntimeEditorGUI.FloatFieldInternal(GUILayoutUtility.GetRect(GUIContent.none, GUI.skin.textField), pos.y, GUI.skin.textField);
		GUILayout.Label(new GUIContent("Z"), Styles.prefixLabelClose);
		pos.z = RuntimeEditorGUI.FloatFieldInternal(GUILayoutUtility.GetRect(GUIContent.none, GUI.skin.textField), pos.z, GUI.skin.textField);
		prop.SetPosition(pos,true);
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		Vector3 eualrRotation = prop.Rotation.eulerAngles;
		GUILayout.Label(new GUIContent("Rotation"), GUILayout.Width(90));
		GUILayout.Label(new GUIContent("X"), Styles.prefixLabelClose);
		eualrRotation.x = RuntimeEditorGUI.FloatFieldInternal(GUILayoutUtility.GetRect(GUIContent.none, GUI.skin.textField), eualrRotation.x, GUI.skin.textField);
		GUILayout.Label(new GUIContent("Y"), Styles.prefixLabelClose);
		eualrRotation.y = RuntimeEditorGUI.FloatFieldInternal(GUILayoutUtility.GetRect(GUIContent.none, GUI.skin.textField), eualrRotation.y, GUI.skin.textField);
		GUILayout.Label(new GUIContent("Z"), Styles.prefixLabelClose);
		eualrRotation.z = RuntimeEditorGUI.FloatFieldInternal(GUILayoutUtility.GetRect(GUIContent.none, GUI.skin.textField), eualrRotation.z, GUI.skin.textField);
		prop.SetRotation(Quaternion.Euler(eualrRotation), true);
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		Vector3 scale = prop.Scale;
		GUILayout.Label(new GUIContent("Scale"), GUILayout.Width(90));
		GUILayout.Label(new GUIContent("X"), Styles.prefixLabelClose);
		scale.x = RuntimeEditorGUI.FloatFieldInternal(GUILayoutUtility.GetRect(GUIContent.none, GUI.skin.textField), scale.x, GUI.skin.textField);
		GUILayout.Label(new GUIContent("Y"), Styles.prefixLabelClose);
		scale.y = RuntimeEditorGUI.FloatFieldInternal(GUILayoutUtility.GetRect(GUIContent.none, GUI.skin.textField), scale.y, GUI.skin.textField);
		GUILayout.Label(new GUIContent("Z"), Styles.prefixLabelClose);
		scale.z = RuntimeEditorGUI.FloatFieldInternal(GUILayoutUtility.GetRect(GUIContent.none, GUI.skin.textField), scale.z, GUI.skin.textField);
		prop.SetScale(scale, true);
		GUILayout.EndHorizontal();
	}
}