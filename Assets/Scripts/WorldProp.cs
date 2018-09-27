using UnityEngine;
using UnityEngine.Networking;

public abstract class WorldProp : MonoBehaviour
{
	protected PrefabDatabase.PrefabEntry prefab;
	protected long id;
	protected Color color;
	protected Vector3 position;
	protected Quaternion rotation;
	protected Vector3 scale;

	protected abstract bool ApplyColor(Color color);

	protected virtual void ApplyPosition(Vector3 pos)
	{
		transform.position = pos;
	}

	protected virtual void ApplyScale(Vector3 size)
	{
		transform.localScale = size;
	}

	protected virtual void ApplyRotation(Quaternion rot)
	{
		transform.rotation = rot;
	}

	public virtual void Translate(Vector3 dir,Space space)
	{
		transform.Translate(dir,space);
		SetPosition(transform.position,true);
	}

	public virtual void Rotate(Vector3 rotation, Space space)
	{
		transform.Rotate(rotation, space);
		SetRotation(transform.rotation,true);
	}

	public virtual void Rotate(Vector3 axis,float amount, Space space)
	{
		transform.Rotate(axis, amount, space);
		SetRotation(transform.rotation, true);
	}

	public virtual void SaveTo(WorldObject worldObject)
	{
		worldObject.guid = prefab.Guid;
		worldObject.color = color;
		worldObject.pos = position;
		worldObject.rotation = rotation;
		worldObject.scale = scale;
	}

	public void SetLocalPosition(Vector3 pos, bool sendUpdates)
	{
		this.transform.localPosition = pos;
		this.position = transform.position;
		if (sendUpdates && IsValid)
		{
			if (pos != this.position)
			{
				WorldManager.instance.SendWorldPropUpdate(new Messages.UpdateProp(id) { Position = transform.position });
			}
		}
	}

	public void SetPosition(Vector3 pos,bool sendUpdates)
	{
		if (sendUpdates && IsValid)
		{
			if (pos != this.position)
			{
				WorldManager.instance.SendWorldPropUpdate(new Messages.UpdateProp(id) { Position = pos });
			}
		}

		this.position = pos;
		ApplyPosition(pos);
	}

	public void SetRotation(Quaternion rotation, bool sendUpdates)
	{
		if (sendUpdates && IsValid)
		{
			if (rotation != this.rotation)
			{
				WorldManager.instance.SendWorldPropUpdate(new Messages.UpdateProp(id) { Rotation = rotation });
			}
		}

		this.rotation = rotation;
		ApplyRotation(rotation);
	}

	public void SetScale(Vector3 scale, bool sendUpdates)
	{
		if (sendUpdates && IsValid)
		{
			if (scale != this.scale)
			{
				WorldManager.instance.SendWorldPropUpdate(new Messages.UpdateProp(id) { Scale = scale });
			}
		}

		this.scale = scale;
		ApplyScale(scale);
	}

	public void SetColor(Color color, bool sendUpdates)
	{
		if (sendUpdates && IsValid)
		{
			this.color = color;
			if (ApplyColor(color))
			{
				WorldManager.instance.SendWorldPropUpdate(new Messages.UpdateProp(id) { Color = color });
			}
		}
		else
		{
			this.color = color;
			ApplyColor(color);
		}
	}

	public void SetId(long id)
	{
		this.id = id;
	}

	public long Id
	{
		get { return id; }
	}

	public void SetPrefab(PrefabDatabase.PrefabEntry prefabEntry)
	{
		prefab = prefabEntry;
	}

	public PrefabDatabase.PrefabEntry Prefab
	{
		get { return prefab; }
	}

	public bool IsValid
	{
		get { return id != 0; }
	}

	public Color Color
	{
		get { return color; }
	}

	public Vector3 Position
	{
		get { return position; }
	}

	public Vector3 LocalPosition
	{
		get { return transform.localPosition; }
	}

	public Quaternion Rotation
	{
		get { return rotation; }
	}

	public Vector3 Scale
	{
		get { return scale; }
	}
}
