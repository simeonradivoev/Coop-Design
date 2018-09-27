using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{
	public InputField ip;

	public void Start()
	{
		ip.text = NetworkManager.singleton.networkAddress;
	}

	public string Address
	{
		get { return NetworkManager.singleton.networkAddress ; }
		set { NetworkManager.singleton.networkAddress = value; }
	}

	public void Host()
	{
		NetworkManager.singleton.StartHost();
	}

	public void Connect()
	{
		NetworkManager.singleton.StartClient();
	}
}
