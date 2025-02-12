using UnityEditor;

[CustomEditor(typeof(Interactable))]
public class InteractableEditor : Editor {
	private SerializedProperty itemName;
	private SerializedProperty interactType;
	private SerializedProperty doorCode;
	private SerializedProperty inCoffinOffset;
	private SerializedProperty coffinCam;
	private SerializedProperty doorToToggle;
	private SerializedProperty used;

	private void OnEnable() {
		itemName = serializedObject.FindProperty("itemName");
		interactType = serializedObject.FindProperty("interactType");
		doorCode = serializedObject.FindProperty("doorCode");
		inCoffinOffset = serializedObject.FindProperty("inCoffinOffset");
		coffinCam = serializedObject.FindProperty("coffinCam");
		doorToToggle = serializedObject.FindProperty("doorToToggle");
		used = serializedObject.FindProperty("used");
	}

	public override void OnInspectorGUI() {
		serializedObject.Update();

		EditorGUILayout.PropertyField(itemName);
		EditorGUILayout.PropertyField(interactType);

		switch ((InteractType)interactType.enumValueIndex) {
		case InteractType.Key:
		case InteractType.Door:
			EditorGUILayout.PropertyField(doorCode);
			break;
		case InteractType.Coffin:
			EditorGUILayout.PropertyField(inCoffinOffset);
			EditorGUILayout.PropertyField(coffinCam);
			break;
		case InteractType.Switch:
			EditorGUILayout.PropertyField(doorToToggle);
			break;
		default:
			break;
		}

		EditorGUILayout.PropertyField(used);

		serializedObject.ApplyModifiedProperties();
		SceneView.RepaintAll();
	}
}
