using UnityEngine;
using UnityEngine.Networking;

public class WorldObject
{
	public GameObject obj;
	public string guid;
	public Vector3 pos;
	public Quaternion rotation;
	public Vector3 scale;
	public Color color;

	public void Deserialzie(NetworkReader reader)
	{
		guid = reader.ReadString();
		pos = reader.ReadVector3();
		rotation = reader.ReadQuaternion();
		scale = reader.ReadVector3();
		color = reader.ReadColor();
	}

	public void Serialize(NetworkWriter writer)
	{
		writer.Write(guid);
		writer.Write(pos);
		writer.Write(rotation);
		writer.Write(scale);
		writer.Write(color);
	}

	public void ToMessage()
	{
		
	}
}