using System.Collections;
using UnityEngine;

namespace Gizmos
{
	public class RuntimeEditorGUIUtility
	{
		
		private static Hashtable s_TextGUIContents = new Hashtable();
		private static GUIContent s_Text = new GUIContent();

		internal static GUIContent TempContent(string t)
		{
			s_Text.text = t;
			return s_Text;
		}

		internal static GUIContent TextContent(string textAndTooltip)
		{
			if (textAndTooltip == null)
			{
				textAndTooltip = string.Empty;
			}
			GUIContent gUIContent = (GUIContent)s_TextGUIContents[textAndTooltip];
			if (gUIContent == null)
			{
				string[] nameAndTooltipString = GetNameAndTooltipString(textAndTooltip);
				gUIContent = new GUIContent(nameAndTooltipString[1]);
				if (nameAndTooltipString[2] != null)
				{
					gUIContent.tooltip = nameAndTooltipString[2];
				}
				s_TextGUIContents[textAndTooltip] = gUIContent;
			}
			return gUIContent;
		}

		internal static string[] GetNameAndTooltipString(string nameAndTooltip)
		{
			string[] array = new string[3];
			string[] array2 = nameAndTooltip.Split(new char[]
			{
				'|'
			});
			switch (array2.Length)
			{
				case 0:
					array[0] = string.Empty;
					array[1] = string.Empty;
					break;
				case 1:
					array[0] = array2[0].Trim();
					array[1] = array[0];
					break;
				case 2:
					array[0] = array2[0].Trim();
					array[1] = array[0];
					array[2] = array2[1].Trim();
					break;
				default:
					Debug.LogError("Error in Tooltips: Too many strings in line beginning with '" + array2[0] + "'");
					break;
			}
			return array;
		}
	}
}