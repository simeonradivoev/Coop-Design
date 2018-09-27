using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gizmos
{
	public class UndoManager : MonoBehaviour
	{
		private static UndoManager instance;
		private static Stack<UndoOperation> operations = new Stack<UndoOperation>();
		private static UndoOperation lastOperation;

		public class UndoOperation
		{
			internal string name;
			internal object value;
			internal Action<object> action;
		}

		private void Awake()
		{
			if (instance != null)
			{
				DestroyImmediate(gameObject);
				return;
			}
			instance = this;
			DontDestroyOnLoad(instance.gameObject);
		}

		private void OnGUI()
		{
			var current = Event.current;

			switch (current.type)
			{
				case EventType.MouseDown:
					if (lastOperation != null)
					{
						operations.Push(lastOperation);
						lastOperation = null;
					}
					break;
				case EventType.KeyDown:
					if (current.keyCode == KeyCode.Z && current.shift)
					{
						if (lastOperation != null)
						{
							lastOperation.action(lastOperation.value);
							lastOperation = null;
						}
						else if(operations.Count > 0)
						{
							var op = operations.Pop();
							op.action(op.value);
						}
					}
					break;
			}
		}

		public static void PushUndo(string name, Action<object> action,object value)
		{
			if (lastOperation != null)
			{
				if (lastOperation.name == name)
				{
					//lastOperation.action = action;
					//lastOperation.value = value;
					return;
				}
				else
				{
					operations.Push(lastOperation);
					lastOperation = null;
				}
			}
			else
			{
				lastOperation = new UndoOperation() {action = action, name = name, value = value};
			}
		}
	}
}