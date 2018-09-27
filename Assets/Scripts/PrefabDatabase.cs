using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Prefab Database")]
public class PrefabDatabase : ScriptableObject
{
	private static PrefabDatabase instance;
	[SerializeField] private List<PrefabEntry> prefabs;
	[SerializeField] private List<string> labels;

	public PrefabEntry GetEntry(string guid)
	{
		return prefabs.FirstOrDefault(p => p.Guid == guid);
	}

	[Serializable]
	public class PrefabEntry
	{
		[SerializeField] private string name;
		[SerializeField] private string[] labels;
		[SerializeField] private string guid;
		[SerializeField] private string path;
		private WorldProp prop;
		private Texture2D icon;
		private bool loadedProp;
		private bool loadedIcon;

		public PrefabEntry()
		{
			loadedProp = false;
			loadedIcon = false;
			icon = null;
			prop = null;
		}

		public PrefabEntry(string name, string[] labels, string guid,string path)
		{
			this.name = name;
			this.labels = labels;
			this.guid = guid;
			this.path = path;
		}

		public string Name
		{
			get { return name; }
		}

		public string[] Labels
		{
			get { return labels; }
		}

		public string Guid
		{
			get { return guid; }
		}	

		public WorldProp Prop
		{
			get
			{
				if (!loadedProp)
				{
					loadedProp = true;
					UnityEngine.Object obj = Resources.Load<GameObject>(path + "/" + name);
					if (obj == null)
					{
						Debug.LogError("Missing Prefab at: " + path + "/" + name);
					}
					else
					{
						prop = ((GameObject)obj).GetComponent<WorldProp>();
					}

				}
				return prop;
			}
		}

		public Texture2D Icon
		{
			get
			{
				if (!loadedIcon)
				{
					loadedIcon = true;
					icon = Resources.Load<Texture2D>(path + "/" + name);
					if (icon == null)
					{
						Debug.LogWarning("Missing Prefab Icon at: " + path + "/" + name);
					}
				}
				return icon;
			}
		}

		public string Path
		{
			get { return path; }
		}
	}

	public List<PrefabEntry> Prefabs
	{
		get { return prefabs; }
	}

	public List<string> Labels
	{
		get { return labels; }
	} 

	public static PrefabDatabase Instance
	{
		get
		{
			if (instance == null)
			{
				instance = Resources.Load<PrefabDatabase>("PrefabDatabase");
			}
			return instance; 
		}
	}
}