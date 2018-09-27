using System.IO;
using System.Threading;
using UnityEditor;
using UnityEngine;

public static class GenerateAssetPreviewTexture
{
	private static GameObject activeObject;

	[MenuItem("GameObject/Generate Asset Preview")]
	private static void GeneratePreview()
	{
		Camera camera = new GameObject("Preview Camera", typeof(Camera)).GetComponent<Camera>();
		camera.fieldOfView = 40;
		camera.nearClipPlane = 0.1f;
		camera.farClipPlane = 100;
		camera.clearFlags = CameraClearFlags.SolidColor;
		camera.backgroundColor = new Color(0, 0, 0, 0);

		foreach (var obj in Selection.GetFiltered(typeof (GameObject), SelectionMode.Assets))
		{
			/*int counter = 0;
			Texture2D thumbnail = null;
			while (thumbnail == null && counter < 75)
			{
				thumbnail = AssetPreview.GetAssetPreview(obj);
				counter++;
				System.Threading.Thread.Sleep(15);
			}

			if (thumbnail != null)
			{
				string path = Path.Combine(Application.dataPath.Replace("/Assets",""), string.Format("{0}/{1}.png", Path.GetDirectoryName(AssetDatabase.GetAssetPath(obj)), obj.name));
				File.WriteAllBytes(path, thumbnail.EncodeToPNG());
			}*/

			RenderTexture renderTexture = RenderTexture.GetTemporary(128, 128, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, 8);
			GameObject go = (GameObject) PrefabUtility.InstantiatePrefab(obj);
			go.transform.position = new Vector3(10000, 10000, 10000);
			Bounds bounds = go.GetRendererBounds();
			

			Vector3 max = bounds.size;
			// Get the radius of a sphere circumscribing the bounds
			float radius = max.magnitude / 2f;
			// Get the horizontal FOV, since it may be the limiting of the two FOVs to properly encapsulate the objects
			float horizontalFOV = 2f * Mathf.Atan(Mathf.Tan(camera.fieldOfView * Mathf.Deg2Rad / 2f) * camera.aspect) * Mathf.Rad2Deg;
			// Use the smaller FOV as it limits what would get cut off by the frustum        
			float fov = Mathf.Min(camera.fieldOfView, horizontalFOV);
			float dist = radius / (Mathf.Sin(fov * Mathf.Deg2Rad / 2f));

			camera.transform.position = bounds.center + new Vector3(0, 0, -1) * dist;
			camera.transform.RotateAround(bounds.center, Vector3.right, 25);
			camera.transform.RotateAround(bounds.center, Vector3.up, 45);
			camera.transform.LookAt(bounds.center);
			camera.targetTexture = renderTexture;
			camera.Render();
			RenderTexture.active = camera.targetTexture;
			Texture2D thumbnail = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
			thumbnail.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
			RenderTexture.active = null;
			RenderTexture.ReleaseTemporary(renderTexture);
			camera.targetTexture = null;
			string path = Path.Combine(Application.dataPath.Replace("/Assets", ""), string.Format("{0}/{1}.png", Path.GetDirectoryName(AssetDatabase.GetAssetPath(obj)), obj.name));
			File.WriteAllBytes(path, thumbnail.EncodeToPNG());
			UnityEngine.Object.DestroyImmediate(go);
		}

		UnityEngine.Object.DestroyImmediate(camera.gameObject);

		AssetDatabase.Refresh();
	}

	[MenuItem("GameObject/Generate Asset Preview", true)]
	private static bool GeneratePreviewValidate()
	{
		return Selection.activeGameObject != null;
	}

	private static void Update()
	{
		if (activeObject != null)
		{
			Texture2D currentAssetTexture2D = AssetPreview.GetAssetPreview(activeObject);
			if (currentAssetTexture2D == null) return;
			string path = Path.Combine(Application.dataPath, string.Format("{0}/{1}.png", AssetDatabase.GetAssetPath(activeObject), activeObject.name));
			File.WriteAllBytes(path, currentAssetTexture2D.EncodeToPNG());
		}

		activeObject = null;
		EditorApplication.update -= Update;
	}
}