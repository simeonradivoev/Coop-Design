using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking;
using Utils;

public static class Messages
{
	public const short CHAT_MSG = 200;
	public const short REQUEST_SPAWN = 201;
	public const short UPDATE_PROP = 202;
	public const short SPAWN_PROP = 203;
	public const short PROP_COMMAND = 204;

	public class PropCommandMessage : MessageBase
	{
		private long id;
		private PropCommandEnum command;

		public PropCommandMessage()
		{
		}

		public PropCommandMessage(long id, PropCommandEnum command)
		{
			this.id = id;
			this.command = command;
		}

		public override void Deserialize(NetworkReader reader)
		{
			base.Deserialize(reader);
			id = reader.ReadInt64();
			command = (PropCommandEnum)reader.ReadInt16();
		}

		public override void Serialize(NetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(id);
			writer.Write((short)command);
		}

		public long Id
		{
			get { return id; }
		}

		public PropCommandEnum Command
		{
			get { return command; }
		}
	}

	public class ChatMessage : MessageBase
	{
		private int from;
		private string message;

		public ChatMessage()
		{
		}

		public ChatMessage(string message)
		{
			this.message = message;
		}

		public ChatMessage(int from, string message)
		{
			this.from = from;
			this.message = message;
		}

		public override void Deserialize(NetworkReader reader)
		{
			base.Deserialize(reader);
			from = reader.ReadInt32();
			message = reader.ReadString();
		}

		public override void Serialize(NetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(from);
			writer.Write(message);
		}

		public int From
		{
			get { return from; }
		}

		public string Message
		{
			get { return message; }
		}
	}

	public class DeletePropMessage : MessageBase
	{
		private long id;

		public DeletePropMessage()
		{
		}

		public DeletePropMessage(long id)
		{
			this.id = id;
		}

		public override void Serialize(NetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(id);
		}

		public override void Deserialize(NetworkReader reader)
		{
			base.Deserialize(reader);
			id = reader.ReadInt64();
		}

		public long Id
		{
			get { return id; }
		}
	}

	public class UpdateProp : MessageBase
	{
		private long id;
		private UpdateType type;
		private Color color;
		private Vector3 position;
		private Quaternion rotation;
		private Vector3 scale;

		public UpdateProp()
		{
		}

		public UpdateProp(long id)
		{
			this.id = id;
		}

		public UpdateProp(UpdateProp other)
		{
			id = other.id;
			type = other.type;
			color = other.color;
			position = other.position;
			rotation = other.rotation;
			scale = other.scale;
		}

		public override void Deserialize(NetworkReader reader)
		{
			id = reader.ReadInt64();
			type = (UpdateType)reader.ReadInt32();
			if (type.IsFlagSet(UpdateType.Color))
			{
				color = reader.ReadColor();
			}
			if (type.IsFlagSet(UpdateType.Position))
			{
				position = reader.ReadVector3();
			}
			if (type.IsFlagSet(UpdateType.Rotation))
			{
				rotation = reader.ReadQuaternion();
			}
			if (type.IsFlagSet(UpdateType.Scale))
			{
				scale = reader.ReadVector3();
			}
		}

		public override void Serialize(NetworkWriter writer)
		{
			writer.Write(id);
			writer.Write((int)type);
			if (type.IsFlagSet(UpdateType.Color))
			{
				writer.Write(color);
			}
			if (type.IsFlagSet(UpdateType.Position))
			{
				writer.Write(position);
			}
			if (type.IsFlagSet(UpdateType.Rotation))
			{
				writer.Write(rotation);
			}
			if (type.IsFlagSet(UpdateType.Scale))
			{
				writer.Write(scale);
			}
		}

		public void Add(UpdateProp other)
		{
			var otherType = other.Type;
			if (otherType.IsFlagSet(UpdateType.Color))
			{
				type = type.SetFlags(UpdateType.Color);
				color = other.color;
			}
			if (otherType.IsFlagSet(UpdateType.Position))
			{
				type = type.SetFlags(UpdateType.Position);
				position = other.position;
			}
			if (otherType.IsFlagSet(UpdateType.Rotation))
			{
				type = type.SetFlags(UpdateType.Rotation);
				rotation = other.rotation;
			}
			if (otherType.IsFlagSet(UpdateType.Scale))
			{
				type = type.SetFlags(UpdateType.Scale);
				scale = other.scale;
			}
		}

		public UpdateType Type
		{
			get { return type; }
		}

		public long Id
		{
			get { return id; }
		}

		public Color Color
		{
			get { return color; }
			set
			{
				color = value;
				type = type.SetFlags(UpdateType.Color);
			}
		}

		public Vector3 Position
		{
			get { return position; }
			set
			{
				position = value;
				type = type.SetFlags(UpdateType.Position);
			}
		}

		public Quaternion Rotation
		{
			get { return rotation; }
			set
			{
				rotation = value;
				type = type.SetFlags(UpdateType.Rotation);
			}
		}

		public Vector3 Scale
		{
			get { return scale; }
			set
			{
				scale = value;
				type = type.SetFlags(UpdateType.Scale);
			}
		}

		[Flags]
		public enum UpdateType
		{
			Color = 1 << 0,
			Position = 1 << 2,
			Rotation = 1 << 3,
			Scale = 1 << 4
		}
	}

	public class SpawnPropMessage : MessageBase
	{
		public WorldObject obj;
		public long id;

		public SpawnPropMessage()
		{
		}

		public SpawnPropMessage(WorldObject obj, long id)
		{
			this.obj = obj;
			this.id = id;
		}

		public override void Deserialize(NetworkReader reader)
		{
			base.Deserialize(reader);
			obj = new WorldObject();
			obj.Deserialzie(reader);
			id = reader.ReadInt64();
		}

		public override void Serialize(NetworkWriter writer)
		{
			base.Serialize(writer);
			obj.Serialize(writer);
			writer.Write(id);
		}

		public WorldObject Obj
		{
			get { return obj; }
		}

		public long Id
		{
			get { return id; }
		}
	}

	public class RequestSpawnMessage : MessageBase
	{
		public WorldObject obj;

		public RequestSpawnMessage()
		{
		}

		public RequestSpawnMessage(WorldObject obj)
		{
			this.obj = obj;
		}

		public override void Deserialize(NetworkReader reader)
		{
			base.Deserialize(reader);
			obj = new WorldObject();
			obj.Deserialzie(reader);
		}

		public override void Serialize(NetworkWriter writer)
		{
			base.Serialize(writer);
			obj.Serialize(writer);
		}
	}
}
