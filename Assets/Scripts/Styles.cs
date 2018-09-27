using UnityEngine;

namespace Assets.Scripts
{
	public static class Styles
	{
		private static GUISkin skin;
		public static GUIStyle miniButton;
		public static GUIStyle verticalScrollbarMenuStyle;
		public static GUIStyle menuElementStyle;
		public static GUIStyle blueButtonRightStyle;
		public static GUIStyle inputFieldLeftStyle;
		public static GUIStyle inspectorHeaderStyle;
		public static GUIStyle inspectorFooterStyle;
		public static GUIStyle inspectorBg;
		public static GUIStyle prefixLabelClose;
		public static GUIStyle prefabLabel;
		//chat
		public static GUIStyle chatLabelStyle;
		public static GUIStyle lastMessageBoxStyle;

		public static GUISkin Init()
		{
			if (skin == null)
			{
				skin = Resources.Load<GUISkin>("Skin");
				miniButton = skin.FindStyle("MiniButton");
				inspectorFooterStyle = skin.FindStyle("InspectorFooter");
				inspectorHeaderStyle = skin.FindStyle("InspectorHeader");
				inputFieldLeftStyle = skin.FindStyle("InputFieldLeft");
				blueButtonRightStyle = skin.FindStyle("ButtonBlueRight");
				menuElementStyle = skin.FindStyle("MenuElement");
				verticalScrollbarMenuStyle = skin.FindStyle("VerticalScrollbarMenu");
				chatLabelStyle = skin.FindStyle("ChatLabel");
				lastMessageBoxStyle = skin.FindStyle("LastMessageBox");
				inspectorBg = skin.FindStyle("InspectorBG");
				prefixLabelClose = skin.FindStyle("PrefixLabelClose");
				prefabLabel = skin.FindStyle("PrefabLabel");
			}
			return skin;
		}
	}
}