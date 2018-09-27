using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Gizmos;
using UnityEngine.Networking;
using Utils;

public class WorldManager : NetworkBehaviour
{
	[SerializeField] private float updateTimePeriod = 1;
	public static WorldManager instance;
	private long objectIdCount;
	Dictionary<long,WorldObjectEntry> objects = new Dictionary<long,WorldObjectEntry>();
	private Dictionary<long, UpdatePropQueueEntry> updateQueue = new Dictionary<long, UpdatePropQueueEntry>();
	private List<long> updatesToRemove = new List<long>(); 
	private static int worldLayer;
	private static LayerMask worldLayerMask;

	#region server messages
	[Server]
	private void OnRequestSpawnMessage(NetworkMessage message)
	{
		var msg = message.ReadMessage<Messages.RequestSpawnMessage>();
		var objectEntry = CreateObjectServer(msg.obj);
		NetworkServer.SendToAll(Messages.SPAWN_PROP, new Messages.SpawnPropMessage(msg.obj, objectEntry.id));
	}

	[Server]
	private WorldObjectEntry CreateObjectServer(WorldObject obj)
	{
		WorldObjectEntry entry;
		entry = CreateObjectInternal(obj, ++objectIdCount);
		return entry;
	}

	[Server]
	private void OnUpdatePropMessageServer(NetworkMessage message)
	{
		var msg = message.ReadMessage<Messages.UpdateProp>();
		if (UpdateProp(msg))
		{
			foreach (var connection in NetworkServer.connections)
			{
				if (connection == message.conn) continue;
				connection.Send(Messages.UPDATE_PROP, new Messages.UpdateProp(msg));
			}
		}
	}

	[Server]
	private void OnRequestPropCommand(NetworkMessage message)
	{
		var msg = message.ReadMessage<Messages.PropCommandMessage>();
		WorldObjectEntry entry = null;

		switch (msg.Command)
		{
			case PropCommandEnum.Delete:
				if (DeleteProrpInternal(msg.Id, out entry))
				{
					NetworkServer.SendToAll(Messages.PROP_COMMAND, new Messages.PropCommandMessage(entry.id, msg.Command));
				}
				break;
			case PropCommandEnum.Clone:
				if (objects.TryGetValue(msg.Id,out entry))
				{
					WorldObject obj = new WorldObject();
					entry.Prop.SaveTo(obj);
					entry = CreateObjectServer(obj);
					NetworkServer.SendToAll(Messages.SPAWN_PROP, new Messages.SpawnPropMessage(obj, entry.id));
					NetworkServer.SendToClient(message.conn.connectionId,Messages.PROP_COMMAND, new Messages.PropCommandMessage(entry.id, msg.Command));
				}
				break;
			default:
				Debug.LogError("Unknow Commad: " + msg.Command);
				break;
		}
	}

	[Server]
	private void OnPlayerCreated(NetworkConnection connection,short controllerId,GameObject playerGameObject)
	{
		foreach (var objectEntry in objects.Values)
		{
			var obj = new WorldObject();
			objectEntry.Prop.SaveTo(obj);
			connection.Send(Messages.SPAWN_PROP, new Messages.SpawnPropMessage(obj, objectEntry.id));
		}
	}
	#endregion

	#region client messages
	[Client]
	private void OnSpawnPropMessage(NetworkMessage message)
	{
		var msg = message.ReadMessage<Messages.SpawnPropMessage>();
		CreateObjectInternal(msg.obj,msg.Id);
	}

	[Client]
	private void OnUpdatePropMessageClient(NetworkMessage message)
	{
		var msg = message.ReadMessage<Messages.UpdateProp>();
		UpdateProp(msg);
	}

	[Client]
	private void OnPropCommandMessageClient(NetworkMessage message)
	{
		var msg = message.ReadMessage<Messages.PropCommandMessage>();
		WorldObjectEntry entry;

		switch (msg.Command)
		{
			case PropCommandEnum.Delete:
				if (DeleteProrpInternal(msg.Id, out entry))
				{

				}
				break;
			case PropCommandEnum.Clone:
				if (objects.TryGetValue(msg.Id, out entry))
				{
					RuntimeSelection.activeProp = entry.Prop;
				}
				break;
			default:
				Debug.LogError("Unknow Commad: " + msg.Command);
				break;
		}
	}
	#endregion

	#region Public calls
	public void SendWorldPropUpdate(Messages.UpdateProp msg)
	{
		UpdatePropQueueEntry otherMsg;
		if (updateQueue.TryGetValue(msg.Id, out otherMsg))
		{
			otherMsg.message.Add(msg);
		}
		else
		{
			updateQueue.Add(msg.Id, new UpdatePropQueueEntry(updateTimePeriod, msg));
		}
	}

	private bool UpdateProp(Messages.UpdateProp msg)
	{
		var type = msg.Type;
		WorldObjectEntry entry;
		if (objects.TryGetValue(msg.Id, out entry))
		{
			if (type.IsFlagSet(Messages.UpdateProp.UpdateType.Color))
			{
				entry.Prop.SetColor(msg.Color, false);
			}
			if (type.IsFlagSet(Messages.UpdateProp.UpdateType.Position))
			{
				entry.Prop.SetPosition(msg.Position, false);
			}
			if (type.IsFlagSet(Messages.UpdateProp.UpdateType.Rotation))
			{
				entry.Prop.SetRotation(msg.Rotation, false);
			}
			if (type.IsFlagSet(Messages.UpdateProp.UpdateType.Scale))
			{
				entry.Prop.SetScale(msg.Scale, false);
			}
			return true;
		}
		return false;
	}

	public void CreateObject(WorldObject obj)
	{
		if (isServer)
		{
			var objectEntry = CreateObjectServer(obj);
			NetworkServer.SendToAll(Messages.SPAWN_PROP, new Messages.SpawnPropMessage(obj, objectEntry.id));
		}
		else if(isClient)
		{
			NetworkManager.singleton.client.Send(Messages.REQUEST_SPAWN, new Messages.RequestSpawnMessage(obj));
		}
	}

	public void DeleteProp(WorldProp prop)
	{
		if (isServer && prop != null && prop.IsValid)
		{
			WorldObjectEntry entry;
			if (DeleteProrpInternal(prop.Id,out entry))
			{
				NetworkServer.SendToAll(Messages.PROP_COMMAND, new Messages.PropCommandMessage(entry.id,PropCommandEnum.Delete));
			}
		}
		else if (isClient && prop != null && prop.IsValid)
		{
			NetworkManager.singleton.client.Send(Messages.PROP_COMMAND, new Messages.PropCommandMessage(prop.Id, PropCommandEnum.Delete));
		}
	}

	public void CloneProp(WorldProp prop)
	{
		if(prop == null || !prop.IsValid) return;
		if (isServer)
		{
			WorldObjectEntry entry;
			WorldObject obj = new WorldObject();
			prop.SaveTo(obj);
			entry = CreateObjectServer(obj);
			RuntimeSelection.activeProp = entry.Prop;
			NetworkServer.SendToAll(Messages.SPAWN_PROP, new Messages.SpawnPropMessage(obj, entry.id));
		}
		else if (isClient)
		{
			NetworkManager.singleton.client.Send(Messages.PROP_COMMAND, new Messages.PropCommandMessage(prop.Id, PropCommandEnum.Clone));
		}
	}
	#endregion

	#region Internal implementations

	private WorldProp CreatePropGameObjectInternal(WorldObject obj)
	{
		var prefab = PrefabDatabase.Instance.GetEntry(obj.guid);
		GameObject instacne = Instantiate(prefab.Prop.gameObject);
		SetLayerRecursively(instacne, WorldLayer);
		WorldProp prop = instacne.GetComponent<WorldProp>();

		prop.SetPrefab(prefab);
		prop.SetPosition(obj.pos, false);
		prop.SetRotation(obj.rotation, false);
		prop.SetScale(obj.scale, false);
		prop.SetColor(obj.color, false);
		return prop;
	}

	private WorldObjectEntry CreateObjectInternal(WorldObject obj,long id)
	{
		var prop = CreatePropGameObjectInternal(obj);
		var objectEntry = new WorldObjectEntry(id, prop);
		prop.SetId(objectEntry.id);
		prop.name = string.Format("[{0}] {1}", objectEntry.id, prop.name);
		objects.Add(objectEntry.id, objectEntry);
		return objectEntry;
	}

	private bool DeleteProrpInternal(long id,out WorldObjectEntry entry)
	{
		if (objects.TryGetValue(id, out entry) && objects.Remove(id))
		{
			Destroy(entry.Prop.gameObject);
			return true;
		}
		return false;
	}
	#endregion

	public override void OnStartServer()
	{
		base.OnStartServer();
		NetworkServer.RegisterHandler(Messages.REQUEST_SPAWN, OnRequestSpawnMessage);
		NetworkServer.RegisterHandler(Messages.UPDATE_PROP, OnUpdatePropMessageServer);
		NetworkServer.RegisterHandler(Messages.PROP_COMMAND, OnRequestPropCommand);
		CustomNetworkManager.Instance.OnServerAddPlayerEvent += OnPlayerCreated;
	}

	public override void PreStartClient()
	{
		if (!isServer)
		{
			NetworkManager.singleton.client.RegisterHandler(Messages.SPAWN_PROP, OnSpawnPropMessage);
			NetworkManager.singleton.client.RegisterHandler(Messages.UPDATE_PROP, OnUpdatePropMessageClient);
			NetworkManager.singleton.client.RegisterHandler(Messages.PROP_COMMAND, OnPropCommandMessageClient);
		}
	}

	private void Update()
	{
		foreach (var entry in updateQueue)
		{
			entry.Value.remainningTime -= Time.deltaTime;
			if (entry.Value.remainningTime <= 0)
			{
				if (isServer)
				{
					NetworkServer.SendToAll(Messages.UPDATE_PROP, entry.Value.message);
				}
				else
				{
					NetworkManager.singleton.client.Send(Messages.UPDATE_PROP, entry.Value.message);
				}
				updatesToRemove.Add(entry.Key);
			}
		}

		if (updatesToRemove.Count > 0)
		{
			foreach (var l in updatesToRemove)
			{
				updateQueue.Remove(l);
			}
			updatesToRemove.Clear();
		}
	}

	public static void SetLayerRecursively(GameObject obj, int newLayer)
	{
		obj.layer = newLayer;

		foreach (Transform child in obj.transform)
		{
			SetLayerRecursively(child.gameObject, newLayer);
		}
	}

	void Awake()
	{
		instance = this;
	}


	void OnDisable()
	{
		if(CustomNetworkManager.Instance) CustomNetworkManager.Instance.OnServerAddPlayerEvent -= OnPlayerCreated;
	}

	public static int WorldLayer
	{
		get
		{
			if (worldLayer == 0)
			{
				worldLayer = LayerMask.NameToLayer("World");
			}
			return worldLayer; 
			
		}
	}

	public static LayerMask WorldLayerMask
	{
		get
		{
			if (worldLayerMask == 0)
			{
				worldLayerMask = LayerMask.GetMask("World");
			}
			return worldLayerMask; 
			
		}
	}

	public class UpdatePropQueueEntry
	{
		public float remainningTime;
		public Messages.UpdateProp message;

		public UpdatePropQueueEntry(float remainningTime, Messages.UpdateProp message)
		{
			this.remainningTime = remainningTime;
			this.message = message;
		}
	}

	public class WorldObjectEntry
	{
		public readonly long id;
		public readonly WorldProp Prop;

		public WorldObjectEntry(long id, WorldProp prop)
		{
			this.id = id;
			Prop = prop;
		}
	}
}
