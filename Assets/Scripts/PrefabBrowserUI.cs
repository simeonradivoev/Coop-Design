using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts;
using Gizmos;
using UnityEngine;
using UnityEngine.Networking;

public class PrefabBrowserUI : MonoBehaviour
{
	[SerializeField] private float opacity = 0.8f;
	[SerializeField] private Texture searchTexture;
	[SerializeField] private Texture prefabsTexture;
	[SerializeField] private Texture missingIconTexture;

	private Vector2 prefabsScroll;
	private GUISkin skin;
	private string search = "";
	private bool s_minimized;
	private bool s_showIcons;
	private HashSet<string> selectedLabels;

	private const float prebabScrollHeight = 256;
	private const float prefabScrollWidth = 256;
	private const float margin = 20;
	private const float prefabIconSize = 42;
	private const float labelsWidth = 64;

	private void Awake()
	{
		selectedLabels = new HashSet<string>();
		skin = Styles.Init();
		s_minimized = PlayerPrefs.GetInt("PrefabBrowserMinimized", 0) > 0;
		s_showIcons = PlayerPrefs.GetInt("PrefabBrowserShowIcons", 1) > 0;
		if (PlayerPrefs.HasKey("PrefabBrowserSelected"))
		{
			string[] selected = PlayerPrefs.GetString("PrefabBrowserSelected").Split(',');
			foreach (var label in selected)
			{
				selectedLabels.Add(label);
			}
		}
		else
		{
			foreach (var label in PrefabDatabase.Instance.Labels)
			{
				selectedLabels.Add(label);
			}
		}
	}

	private void OnDisable()
	{
		string selectedString = string.Join(",", selectedLabels.ToArray());
		PlayerPrefs.SetString("PrefabBrowserSelected", selectedString);
		PlayerPrefs.SetInt("PrefabBrowserMinimized", s_minimized ? 1 : 0);
		PlayerPrefs.SetInt("PrefabBrowserShowIcons", s_showIcons ? 1 : 0);
		PlayerPrefs.Save();
	}

	private void OnGUI()
	{
		GUI.skin = skin;
		GUI.backgroundColor = new Color(1,1,1,opacity);
		var current = Event.current;

		if (!Minimized)
		{
			Rect prefabsScrollRect = new Rect(Screen.width - prefabScrollWidth - margin, Screen.height - prebabScrollHeight - margin, prefabScrollWidth, prebabScrollHeight);
			//take dragging
			RuntimeDragAndDrop.ManageDragTaking(prefabsScrollRect);

			GUILayout.BeginArea(prefabsScrollRect);
			GUILayout.BeginHorizontal(Styles.inspectorHeaderStyle, GUILayout.ExpandWidth(true));
			search = GUILayout.TextField(search, Styles.inputFieldLeftStyle, GUILayout.ExpandWidth(true));
			GUILayout.Button(new GUIContent(searchTexture), Styles.blueButtonRightStyle, GUILayout.Height(24), GUILayout.Width(24));
			GUILayout.EndHorizontal();
			prefabsScroll = GUILayout.BeginScrollView(prefabsScroll, GUIStyle.none, Styles.verticalScrollbarMenuStyle, GUILayout.ExpandHeight(true));
			foreach (var prefab in PrefabDatabase.Instance.Prefabs)
			{
				var prop = prefab.Prop;
				if (prop == null || prop.name.IndexOf(search, StringComparison.InvariantCultureIgnoreCase) < 0 || !selectedLabels.Overlaps(prefab.Labels)) continue;
				GUIContent content = new GUIContent(prefab.Icon == null ? missingIconTexture : prefab.Icon) { text = prop.name};
				Rect elementRect = GUILayoutUtility.GetRect(content, Styles.menuElementStyle, GUILayout.Height(prefabIconSize));
				
				switch (current.type)
				{
					case EventType.MouseDown:
						if (RuntimeDragAndDrop.DraggedProp == null && elementRect.Contains(current.mousePosition) && current.button == 0 && GUIUtility.hotControl == 0)
						{
							RuntimeDragAndDrop.StartDrag(prop, prefab.Guid);
						}
						break;
					case EventType.Repaint:

						Styles.menuElementStyle.Draw(elementRect, content, elementRect.Contains(current.mousePosition), RuntimeDragAndDrop.DraggedProp == prop, RuntimeDragAndDrop.DraggedProp == prop, false);
						break;
				}
			}
			GUILayout.BeginVertical(GUIContent.none, Styles.inspectorBg, GUILayout.ExpandHeight(true));
			GUILayout.EndVertical();
			GUILayout.EndScrollView();
			GUILayout.BeginHorizontal(Styles.inspectorFooterStyle);
			GUILayout.FlexibleSpace();
			if (GUILayout.Button(new GUIContent(prefabsTexture),Styles.miniButton, GUILayout.Height(24), GUILayout.Width(24)))
			{
				Minimized = true;
			}
			GUILayout.EndHorizontal();
			GUILayout.EndArea();

			Rect labelsRect = new Rect(prefabsScrollRect.xMin - labelsWidth - 5,prefabsScrollRect.yMin, labelsWidth, prefabsScrollRect.height);
			GUILayout.BeginArea(labelsRect);
			GUILayout.BeginVertical();
			foreach (var label in PrefabDatabase.Instance.Labels)
			{
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				GUIContent content = new GUIContent(label);
				Rect elementRect = GUILayoutUtility.GetRect(content, Styles.prefabLabel);
				switch (current.type)
				{
					case EventType.MouseDown:
						if (elementRect.Contains(current.mousePosition) && current.button == 0 && GUIUtility.hotControl == 0)
						{
							if (selectedLabels.Contains(label))
							{
								selectedLabels.Remove(label);
							}
							else
							{
								selectedLabels.Add(label);
							}
						}
						break;
					case EventType.Repaint:
						Styles.prefabLabel.Draw(elementRect, content, elementRect.Contains(current.mousePosition), false, selectedLabels.Contains(label), false);
						break;
				}
				GUILayout.EndHorizontal();
				GUILayout.Space(2);
			}
			GUILayout.EndVertical();
			GUILayout.EndArea();

			if (current.type == EventType.MouseDown && GUIUtility.hotControl == 0 && prefabsScrollRect.Contains(current.mousePosition))
			{
				current.Use();
			}
		}
		else
		{
			Rect prefabsBrowserButtonRect = new Rect(Screen.width - margin - 32, Screen.height - margin - 32, 32, 32);
			if (GUI.Button(prefabsBrowserButtonRect,new GUIContent(prefabsTexture)))
			{
				Minimized = false;
			}
		}
	}

	public bool Minimized
	{
		get { return s_minimized; }
		set
		{
			s_minimized = value;
		}
	}

	public bool ShowIcons
	{
		get { return s_showIcons; }
		set
		{
			s_showIcons = value;
		}
	}
}