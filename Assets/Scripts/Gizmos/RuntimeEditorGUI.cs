using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Gizmos
{
	public class RuntimeEditorGUI
	{
		public static RecycledTextEditor activeEditor;
		public static DelayedTextEditor s_DelayedTextEditor = new DelayedTextEditor();
		public static RecycledTextEditor s_RecycledEditor = new RecycledTextEditor();
		public static string s_OriginalText;
		private static Stack<bool> s_ChangedStack = new Stack<bool>();
		internal static readonly string s_AllowedCharactersForInt = "0123456789-*/+%^()";
		internal static readonly string s_AllowedCharactersForFloat = "inftynaeINFTYNAE0123456789.,-*/+%^()";
		internal static string s_RecycledCurrentEditingString;
		internal static double s_RecycledCurrentEditingFloat;
		internal static long s_RecycledCurrentEditingInt;
		internal static bool s_ShowMixedValue;
		internal static bool s_DragToPosition = true;
		internal static bool s_SelectAllOnMouseUp = true;
		internal static bool s_Dragged = false;
		internal static bool s_PostPoneMove = false;
		private static GUIContent s_MixedValueContent = RuntimeEditorGUIUtility.TextContent("—|Mixed Values");
		internal static string s_UnitString = string.Empty;
		internal static Color s_MixedValueContentColorTemp;
		internal static Color s_MixedValueContentColor;
		private static int s_DelayedTextFieldHash = "DelayedEditorTextField".GetHashCode();
		internal static string kFloatFieldFormatString = "g7";
		private static int s_FloatFieldHash = "EditorTextField".GetHashCode();
		private static Stack<bool> s_EnabledStack = new Stack<bool>();

		public struct DisabledScope : IDisposable
		{
			private bool m_Disposed;

			public DisabledScope(bool disabled)
			{
				this.m_Disposed = false;
				BeginDisabled(disabled);
			}

			public void Dispose()
			{
				if (this.m_Disposed)
				{
					return;
				}
				this.m_Disposed = true;
				EndDisabled();
			}
		}

		internal static void BeginDisabled(bool disabled)
		{
			s_EnabledStack.Push(GUI.enabled);
			GUI.enabled &= !disabled;
		}

		internal static void EndDisabled()
		{
			if (s_EnabledStack.Count > 0)
			{
				GUI.enabled = s_EnabledStack.Pop();
			}
		}

		public static bool actionKey
		{
			get
			{
				if (Event.current == null)
				{
					return false;
				}
				if (Application.platform == RuntimePlatform.OSXEditor)
				{
					return Event.current.command;
				}
				return Event.current.control;
			}
		}

		public static bool showMixedValue
		{
			get
			{
				return s_ShowMixedValue;
			}
			set
			{
				s_ShowMixedValue = value;
			}
		}

		public static bool editingTextField
		{
			get
			{
				return RecycledTextEditor.s_ActuallyEditing;
			}
			set
			{
				RecycledTextEditor.s_ActuallyEditing = value;
			}
		}

		public static void BeginChangeCheck()
		{
			s_ChangedStack.Push(GUI.changed);
			GUI.changed = false;
		}

		public static bool EndChangeCheck()
		{
			bool changed = GUI.changed;
			GUI.changed |= s_ChangedStack.Pop();
			return changed;
		}

		internal static void ResetGUIState()
		{
			GUI.skin = null;
			Color white = Color.white;
			GUI.contentColor = white;
			GUI.backgroundColor = white;
			GUI.enabled = true;
			GUI.changed = false;
			s_ChangedStack.Clear();
		}

		internal static bool SupportsRectLayout(Transform tr)
		{
			return !(tr == null) && !(tr.parent == null) && !(tr.GetComponent<RectTransform>() == null) && !(tr.parent.GetComponent<RectTransform>() == null);
		}

		private static Bounds GetLocalBounds(GameObject gameObject)
		{
			RectTransform component = gameObject.GetComponent<RectTransform>();
			if (component)
			{
				return new Bounds(component.rect.center, component.rect.size);
			}
			Renderer component2 = gameObject.GetComponent<Renderer>();
			if (component2 is MeshRenderer)
			{
				MeshFilter component3 = component2.GetComponent<MeshFilter>();
				if (component3 != null && component3.sharedMesh != null)
				{
					return component3.sharedMesh.bounds;
				}
			}
			return new Bounds(Vector3.zero, Vector3.zero);
		}

		internal static Bounds CalculateSelectionBoundsInSpace(Vector3 position, Quaternion rotation, bool rectBlueprintMode)
		{
			Quaternion rotation2 = Quaternion.Inverse(rotation);
			Vector3 vector = new Vector3(3.40282347E+38f, 3.40282347E+38f, 3.40282347E+38f);
			Vector3 vector2 = new Vector3(-3.40282347E+38f, -3.40282347E+38f, -3.40282347E+38f);
			Vector3[] array = new Vector3[2];
			WorldProp[] gameObjects = RuntimeSelection.Props;
			for (int i = 0; i < gameObjects.Length; i++)
			{
				WorldProp gameObject = gameObjects[i];
				Bounds localBounds = GetLocalBounds(gameObject.gameObject);
				array[0] = localBounds.min;
				array[1] = localBounds.max;
				for (int j = 0; j < 2; j++)
				{
					for (int k = 0; k < 2; k++)
					{
						for (int l = 0; l < 2; l++)
						{
							Vector3 vector3 = new Vector3(array[j].x, array[k].y, array[l].z);
							if (rectBlueprintMode && SupportsRectLayout(gameObject.transform))
							{
								Vector3 localPosition = gameObject.transform.localPosition;
								localPosition.z = 0f;
								vector3 = gameObject.transform.parent.TransformPoint(vector3 + localPosition);
							}
							else
							{
								vector3 = gameObject.transform.TransformPoint(vector3);
							}
							vector3 = rotation2 * (vector3 - position);
							for (int m = 0; m < 3; m++)
							{
								vector[m] = Mathf.Min(vector[m], vector3[m]);
								vector2[m] = Mathf.Max(vector2[m], vector3[m]);
							}
						}
					}
				}
			}
			return new Bounds((vector + vector2) * 0.5f, vector2 - vector);
		}

		private static bool HasKeyboardFocus(int controlID)
		{
			return GUIUtility.keyboardControl == controlID;
		}

		internal static void BeginHandleMixedValueContentColor()
		{
			s_MixedValueContentColorTemp = GUI.contentColor;
			GUI.contentColor = ((!showMixedValue) ? GUI.contentColor : (GUI.contentColor * s_MixedValueContentColor));
		}

		internal static void EndHandleMixedValueContentColor()
		{
			GUI.contentColor = s_MixedValueContentColorTemp;
		}

		internal static float FloatFieldInternal(Rect position, float value, GUIStyle style)
		{
			int controlID = GUIUtility.GetControlID(s_FloatFieldHash, FocusType.Keyboard, position);
			return DoFloatField(s_RecycledEditor, position, new Rect(0f, 0f, 0f, 0f), controlID, value, kFloatFieldFormatString, style);
		}

		internal static float DoFloatField(RecycledTextEditor editor, Rect position, Rect dragHotZone, int id, float value, string formatString, GUIStyle style)
		{
			long num = 0L;
			double value2 = (double)value;
			DoNumberField(editor, position, dragHotZone, id, true, ref value2, ref num, formatString, style);
			return (float)value2;
		}

		internal static void DoNumberField(RecycledTextEditor editor, Rect position, Rect dragHotZone, int id, bool isDouble, ref double doubleVal, ref long longVal, string formatString, GUIStyle style)
		{
			string allowedletters = (!isDouble) ? s_AllowedCharactersForInt : s_AllowedCharactersForFloat;
			//if (draggable)
			//{
				//EditorGUI.DragNumberValue(editor, position, dragHotZone, id, isDouble, ref doubleVal, ref longVal, formatString, style, dragSensitivity);
			//}
			Event current = Event.current;
			string text;
			if (HasKeyboardFocus(id) || (current.type == EventType.MouseDown && current.button == 0 && position.Contains(current.mousePosition)))
			{
				if (!editor.IsEditingControl(id))
				{
					text = (s_RecycledCurrentEditingString = ((!isDouble) ? longVal.ToString(formatString) : doubleVal.ToString(formatString)));
				}
				else
				{
					text = s_RecycledCurrentEditingString;
					if (current.type == EventType.ValidateCommand && current.commandName == "UndoRedoPerformed")
					{
						text = ((!isDouble) ? longVal.ToString(formatString) : doubleVal.ToString(formatString));
					}
				}
			}
			else
			{
				text = ((!isDouble) ? longVal.ToString(formatString) : doubleVal.ToString(formatString));
			}
			if (GUIUtility.keyboardControl == id)
			{
				bool flag;
				text = DoTextField(editor, id, position, text, style, allowedletters, out flag, false, false, false);
				if (flag)
				{
					GUI.changed = true;
					s_RecycledCurrentEditingString = text;
					if (isDouble)
					{
						string a = text.ToLower();
						if (a == "inf" || a == "infinity")
						{
							doubleVal = double.PositiveInfinity;
						}
						else if (a == "-inf" || a == "-infinity")
						{
							doubleVal = double.NegativeInfinity;
						}
						else
						{
							text = text.Replace(',', '.');
							if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat, out doubleVal))
							{
								//s_RecycledCurrentEditingFloat = doubleVal;
								//doubleVal = (EditorGUI.s_RecycledCurrentEditingFloat = ExpressionEvaluator.Evaluate<double>(text));
								return;
							}
							if (double.IsNaN(doubleVal))
							{
								doubleVal = 0.0;
							}
							s_RecycledCurrentEditingFloat = doubleVal;
						}
					}
					else
					{
						if (!long.TryParse(text, out longVal))
						{
							//longVal = (EditorGUI.s_RecycledCurrentEditingInt = ExpressionEvaluator.Evaluate<long>(text));
							return;
						}
						s_RecycledCurrentEditingInt = longVal;
					}
				}
			}
			else
			{
				bool flag;
				text = DoTextField(editor, id, position, text, style, allowedletters, out flag, false, false, false);
			}
		}

		internal static string DoTextField(RecycledTextEditor editor, int id, Rect position, string text, GUIStyle style, string allowedletters, out bool changed, bool reset, bool multiline, bool passwordField)
		{
			Event current = Event.current;
			string result = text;
			if (text == null)
			{
				text = string.Empty;
			}
			if (showMixedValue)
			{
				text = string.Empty;
			}
			if (HasKeyboardFocus(id) && Event.current.type != EventType.Layout)
			{
				if (editor.IsEditingControl(id))
				{
					editor.position = position;
					editor.style = style;
					editor.controlID = id;
					editor.multiline = multiline;
					editor.isPasswordField = passwordField;
					editor.DetectFocusChange();
				}
				else if (editingTextField)
				{
					editor.BeginEditing(id, text, position, style, multiline, passwordField);
					if (GUI.skin.settings.cursorColor.a > 0f)
					{
						editor.SelectAll();
					}
				}
			}
			if (editor.controlID == id && GUIUtility.keyboardControl != id)
			{
				editor.controlID = 0;
			}
			bool flag = false;
			string text2 = editor.text;
			EventType typeForControl = current.GetTypeForControl(id);
			switch (typeForControl)
			{
				case EventType.MouseDown:
					if (position.Contains(current.mousePosition) && current.button == 0)
					{
						if (editor.IsEditingControl(id))
						{
							if (Event.current.clickCount == 2 && GUI.skin.settings.doubleClickSelectsWord)
							{
								editor.MoveCursorToPosition(Event.current.mousePosition);
								editor.SelectCurrentWord();
								editor.MouseDragSelectsWholeWords(true);
								editor.DblClickSnap(TextEditor.DblClickSnapping.WORDS);
								s_DragToPosition = false;
							}
							else if (Event.current.clickCount == 3 && GUI.skin.settings.tripleClickSelectsLine)
							{
								editor.MoveCursorToPosition(Event.current.mousePosition);
								editor.SelectCurrentParagraph();
								editor.MouseDragSelectsWholeWords(true);
								editor.DblClickSnap(TextEditor.DblClickSnapping.PARAGRAPHS);
								s_DragToPosition = false;
							}
							else
							{
								editor.MoveCursorToPosition(Event.current.mousePosition);
								s_SelectAllOnMouseUp = false;
							}
						}
						else
						{
							GUIUtility.keyboardControl = id;
							editor.BeginEditing(id, text, position, style, multiline, passwordField);
							editor.MoveCursorToPosition(Event.current.mousePosition);
							if (GUI.skin.settings.cursorColor.a > 0f)
							{
								s_SelectAllOnMouseUp = true;
							}
						}
						GUIUtility.hotControl = id;
						current.Use();
					}
					break;
				case EventType.MouseUp:
					if (GUIUtility.hotControl == id)
					{
						if (s_Dragged && s_DragToPosition)
						{
							editor.MoveSelectionToAltCursor();
							flag = true;
						}
						else if (s_PostPoneMove)
						{
							editor.MoveCursorToPosition(Event.current.mousePosition);
						}
						else if (s_SelectAllOnMouseUp)
						{
							if (GUI.skin.settings.cursorColor.a > 0f)
							{
								editor.SelectAll();
							}
							s_SelectAllOnMouseUp = false;
						}
						editor.MouseDragSelectsWholeWords(false);
						s_DragToPosition = true;
						s_Dragged = false;
						s_PostPoneMove = false;
						if (current.button == 0)
						{
							GUIUtility.hotControl = 0;
							current.Use();
						}
					}
					break;
				case EventType.MouseMove:
				case EventType.KeyUp:
				case EventType.ScrollWheel:
					switch (typeForControl)
					{
						case EventType.ValidateCommand:
							if (GUIUtility.keyboardControl == id)
							{
								string commandName = current.commandName;
								switch (commandName)
								{
									case "Cut":
									case "Copy":
										if (editor.hasSelection)
										{
											current.Use();
										}
										break;
									case "Paste":
										if (editor.CanPaste())
										{
											current.Use();
										}
										break;
									case "SelectAll":
										current.Use();
										break;
									case "UndoRedoPerformed":
										editor.text = text;
										current.Use();
										break;
									case "Delete":
										current.Use();
										break;
								}
							}
							break;
						case EventType.ExecuteCommand:
							if (GUIUtility.keyboardControl == id)
							{
								string commandName = current.commandName;
								switch (commandName)
								{
									case "OnLostFocus":
										if (activeEditor != null)
										{
											activeEditor.EndEditing();
										}
										current.Use();
										break;
									case "Cut":
										editor.BeginEditing(id, text, position, style, multiline, passwordField);
										editor.Cut();
										flag = true;
										break;
									case "Copy":
										editor.Copy();
										current.Use();
										break;
									case "Paste":
										editor.BeginEditing(id, text, position, style, multiline, passwordField);
										editor.Paste();
										flag = true;
										break;
									case "SelectAll":
										editor.SelectAll();
										current.Use();
										break;
									case "Delete":
										editor.BeginEditing(id, text, position, style, multiline, passwordField);
										if (Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXDashboardPlayer || Application.platform == RuntimePlatform.OSXEditor || (Application.platform == RuntimePlatform.WebGLPlayer && SystemInfo.operatingSystem.StartsWith("Mac")))
										{
											editor.Delete();
										}
										else
										{
											editor.Cut();
										}
										flag = true;
										current.Use();
										break;
								}
							}
							break;
						case EventType.DragExited:
							break;
						case EventType.ContextClick:
							if (position.Contains(current.mousePosition))
							{
								if (!editor.IsEditingControl(id))
								{
									GUIUtility.keyboardControl = id;
									editor.BeginEditing(id, text, position, style, multiline, passwordField);
									editor.MoveCursorToPosition(Event.current.mousePosition);
								}
								//ShowTextEditorPopupMenu();
								Event.current.Use();
							}
							break;
						default:
							break;
					}
					break;
				case EventType.MouseDrag:
					if (GUIUtility.hotControl == id)
					{
						if (!current.shift && editor.hasSelection && s_DragToPosition)
						{
							editor.MoveAltCursorToPosition(Event.current.mousePosition);
						}
						else
						{
							if (current.shift)
							{
								editor.MoveCursorToPosition(Event.current.mousePosition);
							}
							else
							{
								editor.SelectToPosition(Event.current.mousePosition);
							}
							s_DragToPosition = false;
							s_SelectAllOnMouseUp = !editor.hasSelection;
						}
						s_Dragged = true;
						current.Use();
					}
					break;
				case EventType.KeyDown:
					if (GUIUtility.keyboardControl == id)
					{
						char character = current.character;
						if (editor.IsEditingControl(id) && editor.HandleKeyEvent(current))
						{
							current.Use();
							flag = true;
						}
						else if (current.keyCode == KeyCode.Escape)
						{
							if (editor.IsEditingControl(id))
							{
								/*if (style == EditorStyles.toolbarSearchField || style == EditorStyles.searchField)
								{
									EditorGUI.s_OriginalText = string.Empty;
								}*/
								editor.text = s_OriginalText;
								editor.EndEditing();
								flag = true;
							}
						}
						else if (character == '\n' || character == '\u0003')
						{
							if (!editor.IsEditingControl(id))
							{
								editor.BeginEditing(id, text, position, style, multiline, passwordField);
								editor.SelectAll();
							}
							else
							{
								if (multiline && !current.alt && !current.shift && !current.control)
								{
									editor.Insert(character);
									flag = true;
									break;
								}
								editor.EndEditing();
							}
							current.Use();
						}
						else if (character == '\t' || current.keyCode == KeyCode.Tab)
						{
							if (multiline && editor.IsEditingControl(id))
							{
								bool flag2 = allowedletters == null || allowedletters.IndexOf(character) != -1;
								bool flag3 = !current.alt && !current.shift && !current.control && character == '\t';
								if (flag3 && flag2)
								{
									editor.Insert(character);
									flag = true;
								}
							}
						}
						else if (character != '\u0019' && character != '\u001b')
						{
							if (editor.IsEditingControl(id))
							{
								bool flag4 = (allowedletters == null || allowedletters.IndexOf(character) != -1) && character != '\0';
								if (flag4)
								{
									editor.Insert(character);
									flag = true;
								}
								else
								{
									if (Input.compositionString != string.Empty)
									{
										editor.ReplaceSelection(string.Empty);
										flag = true;
									}
									current.Use();
								}
							}
						}
					}
					break;
				case EventType.Repaint:
					{
						string text3;
						if (editor.IsEditingControl(id))
						{
							text3 = ((!passwordField) ? editor.text : string.Empty.PadRight(editor.text.Length, '*'));
						}
						else if (showMixedValue)
						{
							text3 = s_MixedValueContent.text;
						}
						else
						{
							text3 = ((!passwordField) ? text : string.Empty.PadRight(text.Length, '*'));
						}
						if (!string.IsNullOrEmpty(s_UnitString) && !passwordField)
						{
							text3 = text3 + " " + s_UnitString;
						}
						if (GUIUtility.hotControl == 0)
						{
							//EditorGUIUtility.AddCursorRect(position, MouseCursor.Text);
						}
						if (!editor.IsEditingControl(id))
						{
							BeginHandleMixedValueContentColor();
							style.Draw(position, RuntimeEditorGUIUtility.TempContent(text3), id, false);
							EndHandleMixedValueContentColor();
						}
						else
						{
							editor.DrawCursor(text3);
						}
						break;
					}
			}
			if (GUIUtility.keyboardControl == id)
			{
				//GUIUtility.textFieldInput = true;
			}
			editor.UpdateScrollOffsetIfNeeded();
			changed = false;
			if (flag)
			{
				changed = (text2 != editor.text);
				current.Use();
			}
			if (changed)
			{
				GUI.changed = true;
				return editor.text;
			}
			RecycledTextEditor.s_AllowContextCutOrPaste = true;
			return result;
		}

		public class RecycledTextEditor : TextEditor
		{
			internal static bool s_ActuallyEditing;

			internal static bool s_AllowContextCutOrPaste = true;

			internal bool IsEditingControl(int id)
			{
				return GUIUtility.keyboardControl == id && this.controlID == id && s_ActuallyEditing;
			}

			public virtual void BeginEditing(int id, string newText, Rect position, GUIStyle style, bool multiline, bool passwordField)
			{
				if (this.IsEditingControl(id))
				{
					return;
				}
				if (activeEditor != null)
				{
					activeEditor.EndEditing();
				}
				activeEditor = this;
				this.controlID = id;
				s_OriginalText = newText;
				base.text = newText;
				this.multiline = multiline;
				this.style = style;
				base.position = position;
				this.isPasswordField = passwordField;
				s_ActuallyEditing = true;
				this.scrollOffset = Vector2.zero;
				//UnityEditor.Undo.IncrementCurrentGroup();
			}

			public virtual void EndEditing()
			{
				if (activeEditor == this)
				{
					activeEditor = null;
				}
				this.controlID = 0;
				s_ActuallyEditing = false;
				s_AllowContextCutOrPaste = true;
				//UnityEditor.Undo.IncrementCurrentGroup();
			}
		}

		public sealed class DelayedTextEditor : RecycledTextEditor
		{
			private int controlThatHadFocus;

			private int messageControl;

			internal string controlThatHadFocusValue = string.Empty;

			private int controlThatLostFocus;

			private bool m_IgnoreBeginGUI;

			public void BeginGUI()
			{
				if (this.m_IgnoreBeginGUI)
				{
					return;
				}
				if (GUIUtility.keyboardControl == this.controlID)
				{
					this.controlThatHadFocus = GUIUtility.keyboardControl;
					this.controlThatHadFocusValue = base.text;
				}
				else
				{
					this.controlThatHadFocus = 0;
				}
			}

			public void EndGUI(EventType type)
			{
				int num = 0;
				if (this.controlThatLostFocus != 0)
				{
					this.messageControl = this.controlThatLostFocus;
					this.controlThatLostFocus = 0;
				}
				if (this.controlThatHadFocus != 0 && this.controlThatHadFocus != GUIUtility.keyboardControl)
				{
					num = this.controlThatHadFocus;
					this.controlThatHadFocus = 0;
				}
				if (num != 0)
				{
					this.messageControl = num;
					this.m_IgnoreBeginGUI = true;
					this.m_IgnoreBeginGUI = false;
					this.messageControl = 0;
				}
			}

			public override void EndEditing()
			{
				base.EndEditing();
				this.messageControl = 0;
			}

			public string OnGUI(int id, string value, out bool changed)
			{
				Event current = Event.current;
				if (current.type == EventType.ExecuteCommand && current.commandName == "DelayedControlShouldCommit" && id == this.messageControl)
				{
					changed = (value != this.controlThatHadFocusValue);
					current.Use();
					return this.controlThatHadFocusValue;
				}
				changed = false;
				return value;
			}
		}
	}
}