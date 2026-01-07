using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

using UnityEditor;

namespace MoreMountains.Tools
{
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public class InspectorTextAttribute : PropertyAttribute
    {
#if UNITY_EDITOR
        public string Text;
        public InspectorTextAttribute(string text)
        {
            Text = text;

            EditorGUILayout.HelpBox(text, MessageType.Info);
        }
#endif
    }
}
