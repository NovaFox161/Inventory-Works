using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class MouseLocation : EditorWindow {
	static MouseLocation instance;
	bool round = true;
	float range = 1000;
	Vector3 currentPosition;

	static MouseLocation() {
		SceneView.onSceneGUIDelegate += Update;
	}

	void OnEnable() {
		instance = this;
	}

	[MenuItem("Tools/Nova Utils/Mouse Location")]
	public static void ShowWindow() {
		//Show existing window instance. If one doesn't exist, make one.
		EditorWindow.GetWindow(typeof(MouseLocation), false, "Mouse Location");
		instance = EditorWindow.GetWindow<MouseLocation>();
	}

	public static void Update(SceneView view) {
		if (instance != null) {
			if (!EditorApplication.isPlaying && Event.current.capsLock) {
				Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
				RaycastHit hit;
				if (Physics.Raycast(ray, out hit, instance.range)) {
					if (instance.round) {
						float roundedX = Mathf.Round(hit.point.x * 1000) / 1000;
						float roundedY = Mathf.Round(hit.point.y * 1000) / 1000;
						float roundedZ = Mathf.Round(hit.point.z * 1000) / 1000;
						instance.currentPosition = new Vector3(roundedX, roundedY, roundedZ);
					} else {
						instance.currentPosition = hit.point;
					}
					EditorWindow.GetWindow(typeof(MouseLocation)).Repaint();
				} else {
					if (instance.currentPosition != Vector3.zero) {
						instance.currentPosition = Vector3.zero;
						EditorWindow.GetWindow(typeof(MouseLocation)).Repaint();
					}
				}
			}
		}
	}

	void OnGUI() {
		if (instance == null) {
			instance = this;
		}
		GUILayout.Label("Settings", EditorStyles.boldLabel);
		GUILayout.Label("Round to the nearest 1000th?", EditorStyles.wordWrappedMiniLabel);
		round = EditorGUILayout.Toggle("Round", round);
		GUILayout.Label("In Unity Units", EditorStyles.wordWrappedMiniLabel);
		range = EditorGUILayout.FloatField("Range", range);

		GUILayout.Label("Mouse Location", EditorStyles.boldLabel);
		currentPosition = EditorGUILayout.Vector3Field("Unity World Space", currentPosition);
	}
}