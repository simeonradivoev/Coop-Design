using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Gizmos;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;
using Utils;

public class PlayerPlacementController : NetworkBehaviour
{
	[SerializeField] private Camera camera;
	[SerializeField] private float maxDistance = 20;
	[SerializeField] private LayerMask hitMask;
	[SerializeField] private Material placeIndicatorMaterial;
	[SerializeField] private Mesh placeIndicatorMesh;
	[SerializeField] private Transform headTransform;
	[SerializeField] private float rotationSpeed = 10;
	[SerializeField] private float scaleSpeed = 10;

	private Vector3 rotation;
	private Vector3 scale = Vector3.one;
	private Color color;
	private PrefabSelectionUI prefabSelection;
	private WorldProp placingProp;

	// Use this for initialization
	void Start ()
	{
		prefabSelection = FindObjectOfType<PrefabSelectionUI>();
	}

	private void OnGUI()
	{
		if (!isLocalPlayer) return;

		var current = Event.current;

		if (RuntimeDragAndDrop.DraggedProp != null)
		{
			Ray ray = camera.ScreenPointToRay(new Vector3(current.mousePosition.x, Screen.height - current.mousePosition.y));
			RaycastHit raycastHit;
			switch (current.type)
			{
				case EventType.MouseDrag:
					if (GUIUtility.hotControl == RuntimeDragAndDrop.DragHash && Physics.Raycast(ray, out raycastHit, float.PositiveInfinity, hitMask))
					{
						ManagePrefabPlacement(raycastHit);
					}
					else if(placingProp != null && placingProp.gameObject.activeSelf)
					{
						placingProp.gameObject.SetActive(false);
					}
					break;
				case EventType.MouseUp:
					if (current.button == 0)
					{
						if (Physics.Raycast(ray, out raycastHit, float.PositiveInfinity, hitMask))
						{
							Quaternion rot = Quaternion.Euler(rotation) * Quaternion.LookRotation(Vector3.forward, raycastHit.normal);
							WorldManager.instance.CreateObject(new WorldObject() {guid = RuntimeDragAndDrop.DraggedGuid, pos = raycastHit.point, rotation = rot, scale = Vector3.one, color = Color.white});
						}
						RuntimeDragAndDrop.TakeDrag();
					}
					break;
				case EventType.Ignore:
					if (GUIUtility.hotControl == RuntimeDragAndDrop.DragHash && placingProp != null && placingProp.gameObject.activeSelf)
					{
						placingProp.gameObject.SetActive(false);
					}
					break;
			}
		}
	}

	// Update is called once per frame
	private void Update()
	{
		if (!isLocalPlayer) return;

		if (RuntimeDragAndDrop.DraggedProp != null && placingProp == null)
		{
			placingProp = Instantiate(RuntimeDragAndDrop.DraggedProp.gameObject).GetComponent<WorldProp>();
			placingProp.gameObject.SetActive(false);
			WorldManager.SetLayerRecursively(placingProp.gameObject, LayerMask.NameToLayer("Indicators"));
		}
		else if (placingProp != null && RuntimeDragAndDrop.DraggedProp == null)
		{
			Destroy(placingProp.gameObject);
			placingProp = null;
		}
	}

	

	private void ManagePrefabPlacement(RaycastHit raycastHit)
	{
		Quaternion rot = Quaternion.Euler(rotation) * Quaternion.LookRotation(Vector3.forward, raycastHit.normal);
		if (placingProp != null)
		{
			if(!placingProp.gameObject.activeSelf) placingProp.gameObject.SetActive(true);
			placingProp.SetPosition(raycastHit.point,false);
			placingProp.SetRotation(rot,false);
		}
	}

	public LayerMask HitMask
	{
		get { return hitMask; }
	}
}
