using System;
using UnityEngine;
using UnityEngine.Networking;

public class CustomNetworkManager : NetworkManager
{
	private static CustomNetworkManager instance;
	public event Action<NetworkConnection, short,GameObject> OnServerAddPlayerEvent;

	public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
	{
		var player = (GameObject)Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
		var SpawnPoints = GameObject.FindGameObjectsWithTag("Respawn");
		if (SpawnPoints.Length > 0)
		{
			var spawnPoint = SpawnPoints[UnityEngine.Random.Range(0, SpawnPoints.Length)];
			player.GetComponent<PlayerMovementController>().Look(spawnPoint.transform.rotation);

			player.transform.position = spawnPoint.transform.position;
		}
		NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
		if (OnServerAddPlayerEvent != null) OnServerAddPlayerEvent.Invoke(conn, playerControllerId,player);
	}

	public static CustomNetworkManager Instance
	{
		get
		{
			if (instance == null)
			{
				instance = FindObjectOfType<CustomNetworkManager>();
			}
			return instance; 
		}
	}
}