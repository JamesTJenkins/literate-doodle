using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MonsterAI))]
public class MonsterAIEditor : Editor {
	private SerializedProperty agent;
	private SerializedProperty player;

	private SerializedProperty firstSectionTravelPoints;
	private SerializedProperty secondSectionTravelPoints;
	private SerializedProperty thirdSectionTravelPoints;

	private SerializedProperty playerYLocationForSecondSection;
	private SerializedProperty playerYLocationForThirdSection;

	private SerializedProperty playerLayerMask;
	private SerializedProperty rayCastOrigin;

	private SerializedProperty immediateAwarenessRange;
	private SerializedProperty sightRange;
	private SerializedProperty horizontalFov;

	private SerializedProperty monsterWalkingSpeed;
	private SerializedProperty monsterRunningSpeed;
	private SerializedProperty monsterSpeedRamp;
	private SerializedProperty timeToFullSpeed;

	private SerializedProperty monsterAttackDistance;

	private SerializedProperty monsterAnimator;
	private SerializedProperty killCamA;

	private SerializedProperty debugSphereRadius;

	private SerializedProperty coffinCheckDistance;
	private SerializedProperty coffinLayerMask;
	private SerializedProperty coffinCheckChancePercentage;

	private SerializedProperty timeToLoseSight;

	private (string, int) selectedPointIndex = (string.Empty, -1);

	private bool showFirstSectionTravelPoints = false;
	private bool displayFirstSectionTravelPoints = true;
	private bool showSecondSectionTravelPoints = false;
	private bool displaySecondSectionTravelPoints = true;
	private bool showThirdSectionTravelPoints = false;
	private bool displayThirdSectionTravelPoints = true;

	private void OnEnable() {
		agent = serializedObject.FindProperty("agent");
		player = serializedObject.FindProperty("player");

		firstSectionTravelPoints = serializedObject.FindProperty("firstSectionTravelPoints");
		secondSectionTravelPoints = serializedObject.FindProperty("secondSectionTravelPoints");
		thirdSectionTravelPoints = serializedObject.FindProperty("thirdSectionTravelPoints");

		playerYLocationForSecondSection = serializedObject.FindProperty("playerYLocationForSecondSection");
		playerYLocationForThirdSection = serializedObject.FindProperty("playerYLocationForThirdSection");

		playerLayerMask = serializedObject.FindProperty("playerLayerMask");
		rayCastOrigin = serializedObject.FindProperty("rayCastOrigin");

		immediateAwarenessRange = serializedObject.FindProperty("immediateAwarenessRange");
		sightRange = serializedObject.FindProperty("sightRange");
		horizontalFov = serializedObject.FindProperty("horizontalFov");

		monsterWalkingSpeed = serializedObject.FindProperty("monsterWalkingSpeed");
		monsterRunningSpeed = serializedObject.FindProperty("monsterRunningSpeed");
		monsterSpeedRamp = serializedObject.FindProperty("monsterSpeedRamp");
		timeToFullSpeed = serializedObject.FindProperty("timeToFullSpeed");

		monsterAttackDistance = serializedObject.FindProperty("monsterAttackDistance");

		monsterAnimator = serializedObject.FindProperty("monsterAnimator");
		killCamA = serializedObject.FindProperty("killCamA");

		debugSphereRadius = serializedObject.FindProperty("debugSphereRadius");

		coffinCheckDistance = serializedObject.FindProperty("coffinCheckDistance");
		coffinLayerMask = serializedObject.FindProperty("coffinLayerMask");
		coffinCheckChancePercentage = serializedObject.FindProperty("coffinCheckChancePercentage");

		timeToLoseSight = serializedObject.FindProperty("timeToLoseSight");
	}

	public override void OnInspectorGUI() {
		serializedObject.Update();

		EditorGUILayout.LabelField("Raycasting Options", EditorStyles.boldLabel);
		EditorGUILayout.PropertyField(agent);
		EditorGUILayout.PropertyField(player);
		EditorGUILayout.PropertyField(timeToLoseSight);
		EditorGUILayout.PropertyField(playerLayerMask);
		EditorGUILayout.PropertyField(rayCastOrigin);
		EditorGUILayout.PropertyField(immediateAwarenessRange);
		EditorGUILayout.PropertyField(sightRange);
		EditorGUILayout.PropertyField(horizontalFov);
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Monster Speed", EditorStyles.boldLabel);
		EditorGUILayout.PropertyField(monsterWalkingSpeed);
		EditorGUILayout.PropertyField(monsterRunningSpeed);
		EditorGUILayout.PropertyField(monsterSpeedRamp);
		EditorGUILayout.PropertyField(timeToFullSpeed);
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Monster Animator", EditorStyles.boldLabel);
		EditorGUILayout.PropertyField(monsterAnimator);
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Monster Attack", EditorStyles.boldLabel);
		EditorGUILayout.PropertyField(monsterAttackDistance);
		EditorGUILayout.LabelField("Kill Cameras", EditorStyles.boldLabel);
		EditorGUILayout.PropertyField(killCamA);
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Coffin Check", EditorStyles.boldLabel);
		EditorGUILayout.PropertyField(coffinCheckDistance);
		EditorGUILayout.PropertyField(coffinLayerMask);
		EditorGUILayout.PropertyField(coffinCheckChancePercentage);

		EditorGUILayout.PropertyField(playerYLocationForSecondSection);
		EditorGUILayout.PropertyField(playerYLocationForThirdSection);
		CreateTravelPointsDropdown("First Section Travel Points", ref showFirstSectionTravelPoints, ref displayFirstSectionTravelPoints, ref firstSectionTravelPoints);
		CreateTravelPointsDropdown("Second Section Travel Points", ref showSecondSectionTravelPoints, ref displaySecondSectionTravelPoints, ref secondSectionTravelPoints);
		CreateTravelPointsDropdown("Third Section Travel Points", ref showThirdSectionTravelPoints, ref displayThirdSectionTravelPoints, ref thirdSectionTravelPoints);

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

		if (displayFirstSectionTravelPoints) {
			DisplayTravelPoints("First Section Travel Points", ref ai.firstSectionTravelPoints, ai.debugSphereRadius, Color.red);
		}

		if (displaySecondSectionTravelPoints) {
			DisplayTravelPoints("Second Section Travel Points", ref ai.secondSectionTravelPoints, ai.debugSphereRadius, Color.magenta);
		}

		if (displayThirdSectionTravelPoints) {
			DisplayTravelPoints("Third Section Travel Points", ref ai.thirdSectionTravelPoints, ai.debugSphereRadius, Color.white);
		}
	}

	private void AddPoint(Vector3 position, string name, ref SerializedProperty travelPoints) {
		travelPoints.InsertArrayElementAtIndex(travelPoints.arraySize);
		travelPoints.GetArrayElementAtIndex(travelPoints.arraySize - 1).vector3Value = position;
		selectedPointIndex = (name, travelPoints.arraySize - 1);
	}

	private void AddPoint(Vector3 position, string name, ref Vector3[] travelPoints) {
		Insert(ref travelPoints, position, travelPoints.Length);
		travelPoints[travelPoints.Length - 1] = position;
		selectedPointIndex = (name, travelPoints.Length - 1);
	}

	private void CreateTravelPointsDropdown(string name, ref bool showArray, ref bool displayTravelPoints, ref SerializedProperty travelPoints) {
		showArray = EditorGUILayout.Foldout(showArray, name);
		if (showArray) {
			EditorGUI.indentLevel++;
			displayTravelPoints = EditorGUILayout.Toggle(
				"Display Travel Points",
				displayTravelPoints
			);
			for (int i = 0; i < travelPoints.arraySize; i++) {
				EditorGUILayout.BeginHorizontal();
				GUI.color = (name == selectedPointIndex.Item1 && i == selectedPointIndex.Item2) ? Color.yellow : Color.white;
				EditorGUILayout.LabelField($"Point {i}", GUILayout.Width(80));
				if (GUILayout.Button("Select", GUILayout.Width(70))) {
					selectedPointIndex = (name, i);
					displayTravelPoints = true;
				}
				if (GUILayout.Button("Remove", GUILayout.Width(70))) {
					travelPoints.DeleteArrayElementAtIndex(i);
					return;
				}
				if (GUILayout.Button("Duplicate", GUILayout.Width(70))) {
					AddPoint(travelPoints.GetArrayElementAtIndex(i).vector3Value, name, ref travelPoints);
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
				AddPoint(Vector3.zero, name, ref travelPoints);
			}
		}
	}

	private void DisplayTravelPoints(string name, ref Vector3[] travelPoints, float debugSphereRadius, Color nonSelectedColor) {
		foreach (Vector3 point in travelPoints) {
			Handles.color = nonSelectedColor;
			Handles.SphereHandleCap(0, point, Quaternion.identity, debugSphereRadius, EventType.Repaint);
			if (Handles.Button(point, Quaternion.identity, debugSphereRadius, debugSphereRadius, Handles.SphereHandleCap)) {
				selectedPointIndex = (name, System.Array.IndexOf(travelPoints, point));
				Repaint();
			}
		}
		if (name == selectedPointIndex.Item1 && selectedPointIndex.Item2 >= 0 && selectedPointIndex.Item2 < travelPoints.Length) {
			MonsterAI ai = (MonsterAI)target;
			Event e = Event.current;

			if (e.type == EventType.KeyDown && e.keyCode == KeyCode.D) {
				Undo.RecordObject(ai, "Duplicate Travel Point");
				AddPoint(travelPoints[selectedPointIndex.Item2], name, ref travelPoints);
				EditorUtility.SetDirty(ai);
			}

			Handles.color = Color.blue;
			Handles.SphereHandleCap(0, travelPoints[selectedPointIndex.Item2], Quaternion.identity, debugSphereRadius, EventType.Repaint);
			Vector3 newTargetPosition = Handles.PositionHandle(travelPoints[selectedPointIndex.Item2], Quaternion.identity);
			if (EditorGUI.EndChangeCheck()) {
				Undo.RecordObject(ai, "Move Travel Point");
				travelPoints[selectedPointIndex.Item2] = newTargetPosition;
				EditorUtility.SetDirty(ai);
			}
		}
	}

	private void Insert(ref Vector3[] array, Vector3 point, int index) {
		Vector3[] newArray = new Vector3[array.Length + 1];

		for (int i = 0; i < index; i++) {
			newArray[i] = array[i];
		}

		newArray[index] = point;

		for (int i = index; i < array.Length; i++) {
			newArray[i + 1] = array[i];
		}

		array = newArray;
	}
}
