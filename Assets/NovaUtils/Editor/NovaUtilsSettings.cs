using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class NovaUtilsSettings : EditorWindow {
	static NovaUtilsSettings instance;

	//Settings


	static NovaUtilsSettings() {}

	void OnEnable() {
		instance = this;
	}

	[MenuItem("Tools/Nova Utils/Options")]
	public static void ShowWindow() {
		//Show existing window instance. If one doesn't exist, make one.
		EditorWindow.GetWindow(typeof(NovaUtilsSettings), false, "Nova Util Options");
		instance = EditorWindow.GetWindow<NovaUtilsSettings>();
	}

	void OnGUI() {
		if (instance == null) {
			instance = this;
		}
		GUILayout.Label("Settings", EditorStyles.boldLabel);

		//Actual settings

	}
}