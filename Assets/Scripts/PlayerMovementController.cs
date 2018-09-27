using UnityEngine;
using System.Collections;
using Gizmos;
using UnityEngine.EventSystems;
using UnityEngine.Networking;

public class PlayerMovementController : NetworkBehaviour
{
	[SerializeField] private Renderer[] renderers;
	[SerializeField] private float speed = 1;
	[SerializeField] private float lookSpeed = 1;
	[SerializeField] private Transform headTransform;
	[SerializeField] private CharacterController characterController;
	[SerializeField] private Camera camera;
	[SerializeField] private float jumpSpeed;
	[SerializeField] private float groundMoveSmoothing = 0.9f;
	[SerializeField] private float airMoveSmoothing = 0.01f;
	[SerializeField] private float panSpeed = 1;
	[SerializeField] private float zoomSpeed = 1;
	private float xRotation = 0;
	private float yRotation = 0;
	private Vector3 moveDirection;
	private RuntimeTool lastTool;
	private bool mouseHidden;

	// Use this for initialization
	private void Start ()
	{
		if (isLocalPlayer)
		{
			foreach (var r in renderers)
			{
				r.enabled = false;
			}
		}
		else
		{
			Destroy(camera.gameObject);
		}
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		Random.InitState(playerControllerId);
		Color c = Random.ColorHSV(0, 1,1,1,1,1);
		foreach (var r in renderers)
		{
			r.material.color = c;
		}
	}

	void OnGUI()
	{
		if (!isLocalPlayer) return;

		var current = Event.current;

		if (current.type == EventType.MouseDown)
		{
			Tools.s_ButtonDown = current.button;
		}
		else if(current.type == EventType.MouseUp)
		{
			Tools.s_ButtonDown = 0;
		}

		if (current.type != EventType.Repaint) return;

		mouseHidden = Tools.current == RuntimeTool.FPS;

		if (Tools.viewToolActive)
		{
			mouseHidden = true;
			Vector3 newMoveDirection;
			switch (Tools.viewTool)
			{
				case RuntimeViewTool.FPS:
					xRotation += Input.GetAxis("Mouse X") * lookSpeed;
					yRotation -= Input.GetAxis("Mouse Y") * lookSpeed;

					Quaternion xRot = Quaternion.AngleAxis(xRotation, transform.up);
					Quaternion yRot = Quaternion.AngleAxis(yRotation, new Vector3(1, 0, 0));

					headTransform.localRotation = yRot;
					characterController.transform.localRotation = xRot;

					newMoveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
					newMoveDirection = headTransform.TransformDirection(newMoveDirection);
					newMoveDirection *= speed;
					if (Input.GetButton("Run"))
					{
						newMoveDirection *= speed;
					}
					moveDirection = Vector3.Lerp(moveDirection, newMoveDirection, groundMoveSmoothing);
					characterController.Move(moveDirection * Time.deltaTime);
					break;
				case RuntimeViewTool.Pan:
					newMoveDirection = new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"), 0);
					newMoveDirection = headTransform.TransformDirection(newMoveDirection);
					newMoveDirection *= panSpeed;

					moveDirection = Vector3.Lerp(moveDirection, newMoveDirection, groundMoveSmoothing);
					characterController.Move(moveDirection * Time.deltaTime);
					break;
				case RuntimeViewTool.Zoom:
					newMoveDirection = new Vector3(0, 0, Input.GetAxis("Mouse Y") + Input.GetAxis("Mouse X"));
					newMoveDirection = headTransform.TransformDirection(newMoveDirection);
					newMoveDirection *= zoomSpeed;

					moveDirection = Vector3.Lerp(moveDirection, newMoveDirection, groundMoveSmoothing);
					characterController.Move(moveDirection * Time.deltaTime);
					break;
			}
		}
	}

	// Update is called once per frame
	void Update ()
	{
		if (!isLocalPlayer) return;

		Cursor.visible = !MouseHidden;
		Cursor.lockState = Cursor.visible ? CursorLockMode.None : CursorLockMode.Locked;
		if (Tools.current == RuntimeTool.FPS)
		{
			xRotation += Input.GetAxis("Mouse X") * lookSpeed;
			yRotation -= Input.GetAxis("Mouse Y") * lookSpeed;

			Quaternion xRot = Quaternion.AngleAxis(xRotation, transform.up);
			Quaternion yRot = Quaternion.AngleAxis(yRotation, new Vector3(1, 0, 0));

			headTransform.localRotation = yRot;
			characterController.transform.localRotation = xRot;
		}

		Vector3 newMoveDirection = Vector3.zero;
		Vector3 gravity = Vector3.zero;
		float speedSmoothing = characterController.isGrounded || Tools.current != RuntimeTool.FPS ? groundMoveSmoothing : airMoveSmoothing;
		if (Tools.current == RuntimeTool.FPS)
		{
			newMoveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
			newMoveDirection = transform.TransformDirection(newMoveDirection);
			newMoveDirection *= speed;
			if (characterController.isGrounded)
			{
				if (Input.GetButton("Jump"))
					newMoveDirection.y = jumpSpeed;
			}
			gravity = Physics.gravity;
		}

		moveDirection = Vector3.Lerp(moveDirection, newMoveDirection, speedSmoothing);
		characterController.Move((moveDirection + gravity) * Time.deltaTime);
	}

	public void Look(Quaternion rotation)
	{
		float bodyAngle = rotation.eulerAngles.y;
		xRotation = bodyAngle;

		Quaternion bodyRotation = Quaternion.AngleAxis(bodyAngle, transform.up);
		characterController.transform.localRotation = bodyRotation;

		float headAngle = rotation.eulerAngles.x;
		yRotation = headAngle;
		Quaternion headRotation = Quaternion.AngleAxis(headAngle, new Vector3(1, 0, 0));
		headTransform.localRotation = headRotation;
		
	}

	public bool MouseHidden
	{
		get { return mouseHidden; }
	}

	public RuntimeTool LastTool
	{
		get { return lastTool; }
	}
}
