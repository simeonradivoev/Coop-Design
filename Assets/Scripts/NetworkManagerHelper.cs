using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class NetworkManagerHelper : MonoBehaviour {

	// Use this for initialization
	void Start ()
	{
		if (!NetworkServer.active)
		{
			NetworkManager.singleton.StartHost();
		}
	}
}
