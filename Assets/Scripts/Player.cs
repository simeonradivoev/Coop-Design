using UnityEngine;
using UnityEngine.Networking;

public class Player : NetworkBehaviour
{
	[SerializeField]
	private Camera camera;
	[SerializeField] private PlayerPlacementController placementController;
	[SerializeField] private PlayerMovementController movementController;
	private static Player instance;

	public override void OnStartLocalPlayer()
	{
		base.OnStartLocalPlayer();
		instance = this;
	}

	public static Camera Camera
	{
		get
		{
			if (Instance != null)
			{
				return Instance.camera;
			}
			return null;
		}
	}

	public static PlayerMovementController MovementController
	{
		get
		{
			if (Instance != null)
			{
				return Instance.movementController;
			}
			return null;
		}
	}

	public static PlayerPlacementController PlacementController
	{
		get
		{
			if (Instance != null)
			{
				return Instance.placementController;
			}
			return null;
		}
	}

	public static Player Instance
	{
		get { return instance; }
	}
}