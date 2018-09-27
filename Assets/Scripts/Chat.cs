using System.Collections.Generic;
using Assets.Scripts;
using Gizmos;
using UnityEngine;
using UnityEngine.Networking;

public class Chat : NetworkBehaviour
{
	public int maxMessages = 128;
	private List<string> messages = new List<string>(); 
	private string currentMessage = "";
	private GUISkin skin;
	[SerializeField] private Texture chatIcon;
	[SerializeField] private float chatOpacity = 0.9f;
	[SerializeField] private float newMessageNotificationTime = 1;
	[SerializeField] private Gradient notificationIconGradient;
	private float newMessageTimer;

	private void Awake()
	{
		skin = Styles.Init();
		s_minimized = PlayerPrefs.GetInt("ChatMinimized", 0) == 1;
	}

	public override void OnStartServer()
	{
		NetworkServer.RegisterHandler(Messages.CHAT_MSG, OnMessageReciveFromClient);
	}

	public override void PreStartClient()
	{
		if (!isServer && NetworkManager.singleton.client != null)
			NetworkManager.singleton.client.RegisterHandler(Messages.CHAT_MSG, OnMessageReciveFromServer);
	}

	private const float inputFieldHeight = 24;
	private const float inputFieldWidth = 256;
	private const float margins = 20;
	private const float chatScrollHeight = 180;
	private Vector2 chatScroll;
	private bool s_minimized;

	private void OnGUI()
	{
		GUI.skin = skin;
		GUI.backgroundColor = new Color(1, 1, 1, chatOpacity);
		var current = Event.current;

		if (!Minimized)
		{
			Rect inputTextRect = new Rect(margins, Screen.height - inputFieldHeight - margins, inputFieldWidth, 24);
			currentMessage = GUI.TextField(inputTextRect, currentMessage);
			if (Event.current.isKey && Event.current.keyCode == KeyCode.Return)
			{
				SendMessage();
				GUIUtility.ExitGUI();
				return;
			}
			Rect sendButtonRect = new Rect(inputTextRect.xMax + 5, inputTextRect.y, 64, inputTextRect.height);
			if (GUI.Button(sendButtonRect, new GUIContent("Send")))
			{
				SendMessage();
				GUIUtility.ExitGUI();
				return;
			}

			Rect chatScrollRect = new Rect(margins, inputTextRect.yMin - 5 - chatScrollHeight, inputFieldWidth + 5 + 64, chatScrollHeight);
			GUILayout.BeginArea(chatScrollRect, GUI.skin.box);
			chatScroll = GUILayout.BeginScrollView(chatScroll);
			foreach (var message in messages)
			{
				GUILayout.Label(new GUIContent(message), Styles.chatLabelStyle);
			}
			GUILayout.EndScrollView();
			GUILayout.EndArea();
			Rect chatToggleBtnRect = new Rect(margins, chatScrollRect.yMin - 3 - 32, 32, 32);
			GUI.contentColor = notificationIconGradient.Evaluate(newMessageTimer / newMessageNotificationTime);
			if (GUI.Button(chatToggleBtnRect, new GUIContent(chatIcon)))
			{
				Minimized = true;
			}
			GUI.contentColor = Color.white;
			Rect fullRect = new Rect(chatScrollRect.xMin, chatScrollRect.yMin, chatScrollRect.xMax - chatScrollRect.xMin, inputTextRect.yMax - chatScrollRect.yMin);
			RuntimeDragAndDrop.ManageDragTaking(fullRect);
			if (current.type == EventType.MouseDown && GUIUtility.hotControl == 0 && fullRect.Contains(current.mousePosition))
			{
				current.Use();
			}
		}
		else
		{
			Rect chatToggleBtnRect = new Rect(margins, Screen.height - margins - 32, 32, 32);
			GUI.contentColor = notificationIconGradient.Evaluate(newMessageTimer/newMessageNotificationTime);
			if (GUI.Button(chatToggleBtnRect, new GUIContent(chatIcon)))
			{
				Minimized = false;
			}
			GUI.contentColor = Color.white;
			Rect lastMessageRect = new Rect(chatToggleBtnRect.xMax + 5, chatToggleBtnRect.yMin, inputFieldWidth, 32);
			GUIContent lastMessageContent = messages.Count > 0 ? new GUIContent(messages[messages.Count - 1]) : GUIContent.none;
			GUI.Box(lastMessageRect, lastMessageContent, Styles.lastMessageBoxStyle);

			Rect fullRect = new Rect(chatToggleBtnRect.xMin, chatToggleBtnRect.yMin, lastMessageRect.xMax - chatToggleBtnRect.xMin, lastMessageRect.yMax - lastMessageRect.yMin);
			RuntimeDragAndDrop.ManageDragTaking(fullRect);
			if (current.type == EventType.MouseDown && GUIUtility.hotControl == 0 && fullRect.Contains(current.mousePosition))
			{
				current.Use();
			}
		}
	}

	private void Update()
	{
		if (newMessageTimer > 0)
		{
			newMessageTimer = Mathf.Max(0, newMessageTimer -Time.deltaTime);
		}
	}

	[Server]
	private void OnMessageReciveFromClient(NetworkMessage message)
	{
		var msg = message.ReadMessage<Messages.ChatMessage>();
		Color col = Color.white;
		UnityEngine.Random.InitState(message.conn.hostId);
		col = UnityEngine.Random.ColorHSV(0, 1, 1, 1, 1, 1);
		string sMsg = string.Format("<color=#{2}>[player{0}]: {1}</color>", message.conn.hostId, msg.Message,col);
		AddMessage(sMsg);
		newMessageTimer += newMessageNotificationTime;
		NetworkServer.SendToAll(Messages.CHAT_MSG, new Messages.ChatMessage(message.conn.hostId,sMsg));
	}

	private void AddMessage(string message)
	{
		if (messages.Count > maxMessages)
		{
			messages.RemoveAt(0);
		}
		messages.Add(message);
		chatScroll.y = float.MaxValue;
		//string msg = messages.Aggregate("", (current, m) => current + (m + "\n"));
		//references.ChatText.text = msg;
		//references.InputScrollRect.verticalScrollbar.value = 0;
	}

	[Client]
	private void OnMessageReciveFromServer(NetworkMessage message)
	{
		var msg = message.ReadMessage<Messages.ChatMessage>();
		AddMessage(msg.Message);
		Debug.Log(NetworkManager.singleton.client.connection.hostId);
		newMessageTimer += newMessageNotificationTime;
	}

	public void SendMessage()
	{
		if (!string.IsNullOrEmpty(currentMessage))
		{
			if (isServer)
			{
				string msg = string.Format("<color=#ffa500ff>[server]: {0}</color>", currentMessage);
				AddMessage(msg);
				NetworkServer.SendToAll(Messages.CHAT_MSG, new Messages.ChatMessage(msg));
			}
			else if (isClient && NetworkManager.singleton.client != null)
			{
				NetworkManager.singleton.client.Send(Messages.CHAT_MSG, new Messages.ChatMessage(currentMessage));
			}
			currentMessage = "";
		}
	}

	public bool Minimized
	{
		get { return s_minimized; }
		set
		{
			s_minimized = value;
			PlayerPrefs.SetInt("ChatMinimized",value ? 1 : 0);
		}
	}
}