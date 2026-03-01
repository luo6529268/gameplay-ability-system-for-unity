#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Kako.Common
{
	public abstract class KakoEditor<T> : Editor where T : Object
	{
		protected T Target => (T)target;

		private static readonly List<Color> Colors = new ();
		private static readonly List<Color> BackgroundColors = new ();
		private static readonly Dictionary<string, bool> Foldouts = new ();
		
		public static void DrawLabel(string label, TextAnchor alignment = TextAnchor.MiddleLeft, Color labelColor = default, float height = 20f)
		{
			GUIStyle style = new GUIStyle(EditorStyles.boldLabel)
			{
				alignment = alignment,
				fontStyle = FontStyle.Bold,
				normal = { textColor = labelColor == default ? Color.white : labelColor }
			};

			EditorGUILayout.LabelField(label, style, GUILayout.Height(height), GUILayout.ExpandWidth(true), GUILayout.MinWidth(0.0f));
		}

		public static bool DrawButton(string label, float height = 20f, Color buttonColor = default, Color labelColor = default)
		{
			GUIStyle buttonStyle = new GUIStyle(GUI.skin.button) { fontStyle = FontStyle.Bold };
			if (labelColor != default)
				buttonStyle.normal.textColor = labelColor;
			
			BeginBackgroundColor(buttonColor == default ? GUI.backgroundColor : buttonColor);
			bool button = GUILayout.Button(label, buttonStyle, GUILayout.Height(height));
			EndBackgroundColor();
			return button;
		}

		public static bool DrawFoldout(string label)
		{
			if (!Foldouts.ContainsKey(label))
				Foldouts.Add(label, false);

			Rect rect = EditorGUILayout.GetControlRect();
			Foldouts[label] = GUI.Toggle(rect, Foldouts[label], GUIContent.none, EditorStyles.foldout);
			rect.xMin += rect.height;
			EditorGUI.LabelField(rect, label, EditorStyles.boldLabel);
			return Foldouts[label];
		}
		
		public static bool DrawToggleFoldout(string label, SerializedProperty boolProperty)
		{
			if (!Foldouts.ContainsKey(label))
				Foldouts.Add(label, false);

			Rect rect = EditorGUILayout.GetControlRect();
			var rect2 = rect;
			
			rect2.width = rect.height;
			Foldouts[label] = GUI.Toggle(rect2, Foldouts[label], GUIContent.none, EditorStyles.foldout);
			
			GUIStyle toggleStyle = new GUIStyle(GUI.skin.toggle);
			toggleStyle.normal.background = MakeColorTex(Color.white);
			toggleStyle.active.background = MakeColorTex( Color.white);
			toggleStyle.onNormal.background = MakeColorTex(Color.white);
			toggleStyle.onActive.background = MakeColorTex(Color.white);
			
			rect2.x = rect2.xMax;
			boolProperty.boolValue = GUI.Toggle(rect2, boolProperty.boolValue, GUIContent.none, toggleStyle);
			
			rect2.x = rect2.xMax;
			rect2.xMax = rect.xMax;
			
			GUIStyle buttonStyle = new GUIStyle(EditorStyles.boldLabel)
			{
				fontStyle = FontStyle.Bold,
				normal = { textColor = Color.white },
				active = { textColor = Color.white },
				hover = { textColor = Color.white }
			};
			Foldouts[label] = GUI.Toggle(rect2, Foldouts[label], label, buttonStyle);
			return Foldouts[label];
		}
		
		public static void DrawSeparatorLine(float height, Color color) => EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, height), color);

		public static Rect Reserve(float height = 19.0f)
		{
			var rect =
			EditorGUILayout.BeginVertical();
				EditorGUILayout.LabelField(string.Empty, GUILayout.Height(height), GUILayout.ExpandWidth(true), GUILayout.MinWidth(0.0f));
			EditorGUILayout.EndVertical();

			return rect;
		}

		public static void Info(string message, bool wide) => EditorGUILayout.HelpBox(message, MessageType.Info, wide);
		public static void Warning(string message, bool wide) => EditorGUILayout.HelpBox(message, MessageType.Warning, wide);
		public static void Error(string message, bool wide) => EditorGUILayout.HelpBox(message, MessageType.Error, wide);
		public static void Separator() => EditorGUILayout.Separator();
		public static void BeginIndent() => EditorGUI.indentLevel += 1;
		public static void EndIndent() => EditorGUI.indentLevel -= 1;

		public static void BeginDisabled(bool disabled = true) => EditorGUI.BeginDisabledGroup(disabled);
		public static void EndDisabled() => EditorGUI.EndDisabledGroup();

		public static void BeginColor(Color color, bool show = true)
		{
			Colors.Add(GUI.color);
			GUI.color = color;
		}

		public static void EndColor()
		{
			if (Colors.Count <= 0) return;
			var index = Colors.Count - 1;

			GUI.color = Colors[index];

			Colors.RemoveAt(index);
		}
		public static void BeginBackgroundColor(Color color, bool show = true)
		{
			BackgroundColors.Add(GUI.backgroundColor);
			GUI.backgroundColor = color;
		}

		public static void EndBackgroundColor()
		{
			if (BackgroundColors.Count <= 0) return;
			var index = BackgroundColors.Count - 1;

			GUI.backgroundColor = BackgroundColors[index];

			BackgroundColors.RemoveAt(index);
		}

		public static void BeginVerticalLayoutBox(Color color = default)
		{
			if(color == default) color.a = 1f;
			BeginColor(color);
			EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			EndColor();
		}

		public static void BeginHorizontalLayoutBox() => EditorGUILayout.BeginVertical(EditorStyles.helpBox);
		public static void EndVerticalLayoutBox() => EditorGUILayout.EndVertical();
		public static void EndHorizontalLayoutBox() => EditorGUILayout.EndHorizontal();

		public SerializedProperty GetProperty(string propertyName) => serializedObject.FindProperty(propertyName);
		public bool DrawProperty(SerializedProperty property) => EditorGUILayout.PropertyField(property);
		public bool DrawProperty(string propertyName) => EditorGUILayout.PropertyField(GetProperty(propertyName));
		
		private static Texture2D MakeColorTex(Color color, int width = 1, int height = 1)
		{
			Color[] pix = new Color[width * height];
			for (int i = 0; i < pix.Length; ++i)
			{
				pix[i] = color;
			}
			Texture2D result = new Texture2D(width, height);
			result.SetPixels(pix);
			result.Apply();
			return result;
		}
	}
}
#endif