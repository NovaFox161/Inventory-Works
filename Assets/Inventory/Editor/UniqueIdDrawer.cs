using UnityEngine;
using System.Collections;
using UnityEditor;
using System;

[CustomPropertyDrawer(typeof(UniqueIdentifierAttribute))]
public class UniqueIdentifierDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)    {
		if (prop.stringValue == "") {
			Guid guid = Guid.NewGuid();
			prop.stringValue = guid.ToString();
		}

		Rect textFieldPosition = position;
		textFieldPosition.height = 16;
		DrawLabelField(textFieldPosition, prop, label);
	}

	void DrawLabelField(Rect position, SerializedProperty prop, GUIContent label)    {
		EditorGUI.LabelField(position, label, new GUIContent(prop.stringValue));
	}
}