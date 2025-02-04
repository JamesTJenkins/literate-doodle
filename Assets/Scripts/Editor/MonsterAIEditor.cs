using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MonsterAI))]
public class MonsterAIEditor : Editor {
	private SerializedProperty travelPoints;
	private SerializedProperty agent;
	private SerializedProperty player;
	private SerializedProperty debugSphereRadius;
	private SerializedProperty playerLayerMask;
	private SerializedProperty rayCastOrigin;
	private SerializedProperty rayCastTargetOffset;

	private int selectedPointIndex = -1;
	private bool showTravelPoints = false;
	private bool displayTravelPoints = true;

	private void OnEnable() {
		travelPoints = serializedObject.FindProperty("travelPoints");
		agent = serializedObject.FindProperty("agent");
		player = serializedObject.FindProperty("player");
		debugSphereRadius = serializedObject.FindProperty("debugSphereRadius");
		playerLayerMask = serializedObject.FindProperty("playerLayerMask");
		rayCastOrigin = serializedObject.FindProperty("rayCastOrigin");
		rayCastTargetOffset = serializedObject.FindProperty("rayCastTargetOffset");
	}

	public override void OnInspectorGUI() {
		serializedObject.Update();

		EditorGUILayout.PropertyField(agent);
		EditorGUILayout.PropertyField(player);
		EditorGUILayout.PropertyField(playerLayerMask);
		EditorGUILayout.PropertyField(rayCastOrigin);
		EditorGUILayout.PropertyField(rayCastTargetOffset);

		showTravelPoints = EditorGUILayout.Foldout(showTravelPoints, "Travel Points");
		if (showTravelPoints) {
			EditorGUI.indentLevel++;
			displayTravelPoints = EditorGUILayout.Toggle("Display Travel Points", displayTravelPoints);
			for (int i = 0; i < travelPoints.arraySize; i++) {
				EditorGUILayout.BeginHorizontal();
				GUI.color = i == selectedPointIndex ? Color.yellow : Color.white;
				EditorGUILayout.LabelField($"Point {i}", GUILayout.Width(80));
				if (GUILayout.Button("Select", GUILayout.Width(60))) {
					selectedPointIndex = i;
				}
				if (GUILayout.Button("Remove", GUILayout.Width(60))) {
					travelPoints.DeleteArrayElementAtIndex(i);
				}
				GUI.color = Color.white;
				if (travelPoints.arraySize > 0) {
					EditorGUILayout.PropertyField(travelPoints.GetArrayElementAtIndex(i), GUIContent.none);
				}
				EditorGUILayout.EndHorizontal();
			}
			EditorGUI.indentLevel--;
			EditorGUILayout.Space();
			if (GUILayout.Button("Add Point")) {
				travelPoints.InsertArrayElementAtIndex(travelPoints.arraySize);
				travelPoints.GetArrayElementAtIndex(travelPoints.arraySize - 1).vector3Value = Vector3.zero;
				selectedPointIndex = travelPoints.arraySize - 1;
			}
		}

		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Editor", EditorStyles.boldLabel);
		EditorGUILayout.PropertyField(debugSphereRadius);

		serializedObject.ApplyModifiedProperties();
		SceneView.RepaintAll();
	}

	private void OnSceneGUI() {
		MonsterAI monsterAI = (MonsterAI)target;
		if (displayTravelPoints) {
			foreach (Vector3 point in monsterAI.travelPoints) {
				Handles.color = Color.red;
				Handles.SphereHandleCap(0, point, Quaternion.identity, monsterAI.debugSphereRadius, EventType.Repaint);
				if (Handles.Button(point, Quaternion.identity, monsterAI.debugSphereRadius, monsterAI.debugSphereRadius, Handles.SphereHandleCap)) {
					selectedPointIndex = System.Array.IndexOf(monsterAI.travelPoints, point);
					Repaint();
				}
			}
			if (selectedPointIndex >= 0 && selectedPointIndex < monsterAI.travelPoints.Length) {
				Handles.color = Color.blue;
				Handles.SphereHandleCap(0, monsterAI.travelPoints[selectedPointIndex], Quaternion.identity, monsterAI.debugSphereRadius, EventType.Repaint);
				Vector3 newTargetPosition = Handles.PositionHandle(monsterAI.travelPoints[selectedPointIndex], Quaternion.identity);
				if (EditorGUI.EndChangeCheck()) {
					Undo.RecordObject(monsterAI, "Move Travel Point");
					monsterAI.travelPoints[selectedPointIndex] = newTargetPosition;
					EditorUtility.SetDirty(monsterAI);
				}
			}
		}
	}
}
