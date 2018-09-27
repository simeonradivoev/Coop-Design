using Gizmos;
using UnityEngine;

public class SelectedPropCommands : MonoBehaviour
{
	[SerializeField] private KeyCode deleteKey = KeyCode.Delete;
	[SerializeField] private KeyCode cloneKey = KeyCode.D;

	private void OnGUI()
	{
		var current = Event.current;

		switch (current.type)
		{
			case EventType.KeyDown:
				if (current.keyCode == deleteKey)
				{
					if (RuntimeSelection.Props.Length > 0 && GUIUtility.hotControl == 0)
					{
						foreach (var prop in RuntimeSelection.Props)
						{
							WorldManager.instance.DeleteProp(prop);
						}
					}
				}

				if (RuntimeSelection.activeProp)
				{
					if (current.keyCode == cloneKey && current.control)
					{
						WorldManager.instance.CloneProp(RuntimeSelection.activeProp);
					}
				}
				break;
		}
	}
}