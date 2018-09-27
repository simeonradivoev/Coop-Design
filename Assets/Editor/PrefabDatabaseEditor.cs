using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PrefabDatabase))]
public class PrefabDatabaseEditor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		if (GUILayout.Button("Populate"))
		{
			Populate();
		}
	}

	private void Populate()
	{
		Object[] assets = AssetDatabase.GetAllAssetPaths().Where(a => a.StartsWith("Assets/Resources/Props/")).Select(a => AssetDatabase.LoadAssetAtPath<GameObject>(a)).ToArray();
		PrefabDatabase database = target as PrefabDatabase;
		database.Prefabs.Clear();
		database.Labels.Clear();
		foreach (var asset in assets)
		{
			GameObject go = asset as GameObject;
			if (go != null)
			{
				string[] lables = AssetDatabase.GetLabels(asset);
				string path = AssetDatabase.GetAssetPath(asset);
				WorldProp prop = go.GetComponent<WorldProp>();
				if (prop != null)
				{
					database.Prefabs.Add(new PrefabDatabase.PrefabEntry(prop.name, lables,AssetDatabase.AssetPathToGUID(path), path.Replace("Assets/Resources/", "").Replace("/" + prop.name + ".prefab","")));
					database.Labels.AddRange(lables.Where(l => !database.Labels.Contains(l)));
				}
			}
		}
		EditorUtility.SetDirty(target);
		AssetDatabase.SaveAssets();
	}
}
