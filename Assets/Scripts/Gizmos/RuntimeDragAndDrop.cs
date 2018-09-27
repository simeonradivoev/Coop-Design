using UnityEngine;
using UnityEngine.Networking;

namespace Gizmos
{
	public class RuntimeDragAndDrop
	{
		public static readonly int DragHash = "RuntimeDragAndDropHash".GetHashCode(); 
		private static WorldProp draggedProp;
		private static string draggedGuid;

		public static void StartDrag(WorldProp DraggedProp, string guid)
		{
			draggedProp = DraggedProp;
			draggedGuid = guid;
			GUIUtility.hotControl = DragHash;
		}

		public static void TakeDrag()
		{
			draggedProp = null;
			draggedGuid = null;
			GUIUtility.hotControl = 0;
		}

		public static void ManageDragTaking(Rect rect)
		{
			var current = Event.current;

			if (DraggedProp != null && rect.Contains(current.mousePosition))
			{
				switch (current.type)
				{
					case EventType.MouseDrag:
						current.type = EventType.Ignore;
						break;
					case EventType.MouseUp:
						TakeDrag();
						current.Use();
						break;
				}
			}
		}

		public static WorldProp DraggedProp
		{
			get { return draggedProp; }
		}

		public static string DraggedGuid
		{
			get { return draggedGuid; }
		}
	}
}