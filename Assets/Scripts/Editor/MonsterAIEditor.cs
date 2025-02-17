using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MonsterAI))]
public class MonsterAIEditor : Editor {
	private SerializedProperty agent;
	private SerializedProperty player;
	private SerializedProperty travelPoints;
	private SerializedProperty playerLayerMask;
	private SerializedProperty rayCastOrigin;
	private SerializedProperty immediateAwarenessRange;
	private SerializedProperty sightRange;
	private SerializedProperty horizontalFov;
	private SerializedProperty monsterWalkingSpeed;
	private SerializedProperty monsterRunningSpeed;
	private SerializedProperty monsterAttackDistance;
	private SerializedProperty monsterAnimator;
	private SerializedProperty killCamA;
	private SerializedProperty debugSphereRadius;
	private SerializedProperty seenPlayerCoffin;
	private SerializedProperty coffinCheckDistance;
	private SerializedProperty coffinLayerMask;


	private int selectedPointIndex = -1;
	private bool showTravelPoints = false;
	private bool displayTravelPoints = true;

	private void OnEnable() {
		agent = serializedObject.FindProperty("agent");
		player = serializedObject.FindProperty("player");
		travelPoints = serializedObject.FindProperty("travelPoints");
		playerLayerMask = serializedObject.FindProperty("playerLayerMask");
		rayCastOrigin = serializedObject.FindProperty("rayCastOrigin");
		immediateAwarenessRange = serializedObject.FindProperty("immediateAwarenessRange");
		sightRange = serializedObject.FindProperty("sightRange");
		horizontalFov = serializedObject.FindProperty("horizontalFov");
		monsterWalkingSpeed = serializedObject.FindProperty("monsterWalkingSpeed");
		monsterRunningSpeed = serializedObject.FindProperty("monsterRunningSpeed");
		monsterAttackDistance = serializedObject.FindProperty("monsterAttackDistance");
		monsterAnimator = serializedObject.FindProperty("monsterAnimator");
		killCamA = serializedObject.FindProperty("killCamA");
		debugSphereRadius = serializedObject.FindProperty("debugSphereRadius");
		seenPlayerCoffin = serializedObject.FindProperty("playerSeenCoffin");
		coffinCheckDistance = serializedObject.FindProperty("coffinCheckDistance");
		coffinLayerMask = serializedObject.FindProperty("coffinLayerMask");
	}

	public override void OnInspectorGUI() {
		serializedObject.Update();

		EditorGUILayout.LabelField("Raycasting Options", EditorStyles.boldLabel);
		EditorGUILayout.PropertyField(agent);
		EditorGUILayout.PropertyField(player);
		EditorGUILayout.PropertyField(playerLayerMask);
		EditorGUILayout.PropertyField(rayCastOrigin);
		EditorGUILayout.PropertyField(immediateAwarenessRange);
		EditorGUILayout.PropertyField(sightRange);
		EditorGUILayout.PropertyField(horizontalFov);
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Monster Speed", EditorStyles.boldLabel);
		EditorGUILayout.PropertyField(monsterWalkingSpeed);
		EditorGUILayout.PropertyField(monsterRunningSpeed);
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Monster Animator", EditorStyles.boldLabel);
		EditorGUILayout.PropertyField(monsterAnimator);
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Monster Attack", EditorStyles.boldLabel);
		EditorGUILayout.PropertyField(monsterAttackDistance);
		EditorGUILayout.LabelField("Kill Cameras", EditorStyles.boldLabel);
		EditorGUILayout.PropertyField(killCamA);
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Player Sighting", EditorStyles.boldLabel);
		EditorGUILayout.PropertyField(seenPlayerCoffin);
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Coffin Check", EditorStyles.boldLabel);
		EditorGUILayout.PropertyField(coffinCheckDistance);
		EditorGUILayout.PropertyField(coffinLayerMask);


		showTravelPoints = EditorGUILayout.Foldout(showTravelPoints, "Travel Points");
		if (showTravelPoints) {
			EditorGUI.indentLevel++;
			displayTravelPoints = EditorGUILayout.Toggle(
				"Display Travel Points",
				displayTravelPoints
			);
			for (int i = 0; i < travelPoints.arraySize; i++) {
				EditorGUILayout.BeginHorizontal();
				GUI.color = i == selectedPointIndex ? Color.yellow : Color.white;
				EditorGUILayout.LabelField($"Point {i}", GUILayout.Width(80));
				if (GUILayout.Button("Select", GUILayout.Width(70))) {
					selectedPointIndex = i;
				}
				if (GUILayout.Button("Remove", GUILayout.Width(70))) {
					travelPoints.DeleteArrayElementAtIndex(i);
				}
				if (GUILayout.Button("Duplicate", GUILayout.Width(70))) {
					AddPoint(travelPoints.GetArrayElementAtIndex(i).vector3Value);
				}
				GUI.color = Color.white;
				if (travelPoints.arraySize > 0) {
					EditorGUILayout.PropertyField(
						travelPoints.GetArrayElementAtIndex(i),
						GUIContent.none
					);
				}
				EditorGUILayout.EndHorizontal();
			}
			EditorGUI.indentLevel--;
			EditorGUILayout.Space();
			if (GUILayout.Button("Add Point")) {
				AddPoint(Vector3.zero);
			}
		}

		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Editor", EditorStyles.boldLabel);
		EditorGUILayout.PropertyField(debugSphereRadius);

		serializedObject.ApplyModifiedProperties();
		SceneView.RepaintAll();
	}

	private void OnSceneGUI() {
		MonsterAI ai = (MonsterAI)target;
		Handles.color = Color.yellow;

		// Hori fov
		Vector3 leftBoundary = Quaternion.AngleAxis(-ai.horizontalFov / 2, ai.transform.up) * ai.transform.forward;
		Vector3 rightBoundary = Quaternion.AngleAxis(ai.horizontalFov / 2, ai.transform.up) * ai.transform.forward;
		Handles.DrawLine(ai.transform.position, ai.transform.position + leftBoundary * ai.sightRange);
		Handles.DrawLine(ai.transform.position, ai.transform.position + rightBoundary * ai.sightRange);
		Handles.DrawWireArc(ai.transform.position, ai.transform.transform.up, leftBoundary, ai.horizontalFov, ai.sightRange);

		// Immediate awareness range
		Handles.DrawWireDisc(ai.transform.position, Vector3.up, ai.immediateAwarenessRange);

		// Kill range
		Handles.color = Color.red;
		Handles.DrawWireDisc(ai.transform.position, Vector3.up, ai.monsterAttackDistance);

		// Coffin check range
		Handles.color = Color.magenta;
		Handles.DrawWireDisc(ai.transform.position, Vector3.up, ai.coffinCheckDistance);

		if (displayTravelPoints) {
			foreach (Vector3 point in ai.travelPoints) {
				Handles.color = Color.red;
				Handles.SphereHandleCap(0, point, Quaternion.identity, ai.debugSphereRadius, EventType.Repaint);
				if (
					Handles.Button(point, Quaternion.identity, ai.debugSphereRadius, ai.debugSphereRadius, Handles.SphereHandleCap)
				) {
					selectedPointIndex = System.Array.IndexOf(ai.travelPoints, point);
					Repaint();
				}
			}
			if (selectedPointIndex >= 0 && selectedPointIndex < ai.travelPoints.Length) {
				Handles.color = Color.blue;
				Handles.SphereHandleCap(0, ai.travelPoints[selectedPointIndex], Quaternion.identity, ai.debugSphereRadius, EventType.Repaint);
				Vector3 newTargetPosition = Handles.PositionHandle(ai.travelPoints[selectedPointIndex], Quaternion.identity);
				if (EditorGUI.EndChangeCheck()) {
					Undo.RecordObject(ai, "Move Travel Point");
					ai.travelPoints[selectedPointIndex] = newTargetPosition;
					EditorUtility.SetDirty(ai);
				}
			}
		}
	}

	private void AddPoint(Vector3 position) {
		travelPoints.InsertArrayElementAtIndex(travelPoints.arraySize);
		travelPoints.GetArrayElementAtIndex(travelPoints.arraySize - 1).vector3Value = position;
		selectedPointIndex = travelPoints.arraySize - 1;
	}
}
