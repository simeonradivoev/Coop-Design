using UnityEngine;
using System.Collections;
using System.Linq;
using Gizmos;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PrefabSelectionUI : MonoBehaviour
{
	private PlayerPlacementController playerPlacementController;
	private PlayerMovementController movementController;
	private Camera camera;

	private void OnGUI()
	{
		if (camera == null || movementController == null || playerPlacementController == null)
		{
			camera = Player.Camera;
			movementController = Player.MovementController;
			playerPlacementController = Player.PlacementController;
			GUIUtility.ExitGUI();
			return;
		}

		var current = Event.current;

		switch (current.type)
		{
			case EventType.KeyDown:
				switch (current.keyCode)
				{
					case KeyCode.Escape:
						Tools.current = RuntimeTool.Move;
						break;
				}
				break;
			case EventType.MouseDown:
				if (GUIUtility.hotControl == 0 && current.button == 0)
				{
					RaycastHit raycastHit;
					Ray ray = camera.ScreenPointToRay(Input.mousePosition);

					if (playerPlacementController != null && Physics.Raycast(ray, out raycastHit, float.MaxValue, WorldManager.WorldLayerMask))
					{
						WorldProp prop = raycastHit.transform.GetComponentInParent<WorldProp>();
						if (prop != null)
						{
							RuntimeSelection.Props = new[] { prop };
						}
						else
						{
							RuntimeSelection.Props = new WorldProp[0];
						}
					}
					else
					{
						RuntimeSelection.Props = new WorldProp[0];
					}
				}
				break;
		}
	}
}
