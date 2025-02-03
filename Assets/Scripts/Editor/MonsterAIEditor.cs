using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MonsterAI))]
public class MonsterAIEditor : Editor {
	private SerializedProperty travelPoints;
	private int selectedPointIndex = -1;
	private bool showTravelPoints = true;

	private void OnEnable() {
		travelPoints = serializedObject.FindProperty("travelPoints");
	}

	public override void OnInspectorGUI() {
		serializedObject.Update();

		showTravelPoints = EditorGUILayout.Foldout(showTravelPoints, "Travel Points");
		if (showTravelPoints) {
			EditorGUI.indentLevel++;
			for (int i = 0; i < travelPoints.arraySize; i++) {
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField($"Point {i}", GUILayout.Width(100));
				if (GUILayout.Button("Select", GUILayout.Width(60))) {
					selectedPointIndex = i;
				}
				if (GUILayout.Button("Remove", GUILayout.Width(60))) {
					travelPoints.DeleteArrayElementAtIndex(i);
				}
				EditorGUILayout.EndHorizontal();
			}
			EditorGUI.indentLevel--;

			EditorGUILayout.Space();

			if (GUILayout.Button("Add Point")) {
				travelPoints.InsertArrayElementAtIndex(travelPoints.arraySize);
				travelPoints.GetArrayElementAtIndex(travelPoints.arraySize - 1).vector3Value = Vector3.zero;
			}
		}
		serializedObject.ApplyModifiedProperties();
	}

	private void OnSceneGUI() {
		MonsterAI monsterAI = (MonsterAI)target;

		foreach (Vector3 point in monsterAI.travelPoints) {
			Handles.color = Color.red;
			Handles.SphereHandleCap(0, point, Quaternion.identity, monsterAI.debugSphereRadius, EventType.Repaint);
			if (Handles.Button(point, Quaternion.identity, monsterAI.debugSphereRadius, monsterAI.debugSphereRadius, Handles.SphereHandleCap)) {
				selectedPointIndex = System.Array.IndexOf(monsterAI.travelPoints, point);
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
