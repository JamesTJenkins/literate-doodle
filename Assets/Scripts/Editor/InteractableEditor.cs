using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Interactable))]
public class InteractableEditor : Editor {
	private SerializedProperty itemName;
	private SerializedProperty interactType;
	private SerializedProperty doorCode;
	private SerializedProperty inCoffinOffset;
	private SerializedProperty coffinCam;
	private SerializedProperty doorToToggle;
	//private SerializedProperty used;	// Dont show this as not needed in inspector but leaving code in incase

	private bool showDoorCodeSuggestions = false;
	private bool showNameSuggestions = false;

	private void OnEnable() {
		itemName = serializedObject.FindProperty("itemName");
		interactType = serializedObject.FindProperty("interactType");
		doorCode = serializedObject.FindProperty("doorCode");
		inCoffinOffset = serializedObject.FindProperty("inCoffinOffset");
		coffinCam = serializedObject.FindProperty("coffinCam");
		doorToToggle = serializedObject.FindProperty("doorToToggle");
		//used = serializedObject.FindProperty("used");
	}

	public override void OnInspectorGUI() {
		serializedObject.Update();

		Interactable[] allInstances = FindObjectsByType<Interactable>(UnityEngine.FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.None);

		DisplayNames(allInstances);
		EditorGUILayout.PropertyField(interactType);

		switch ((InteractType)interactType.enumValueIndex) {
		case InteractType.Key:
		case InteractType.Door:
			DisplayDoorcodes(allInstances);
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

		//EditorGUILayout.PropertyField(used);

		serializedObject.ApplyModifiedProperties();
		SceneView.RepaintAll();
	}

	private void OnSceneGUI() {
		if (doorToToggle.objectReferenceValue != null) {
			Handles.color = Color.red;
			Handles.SphereHandleCap(0, ((GameObject)doorToToggle.objectReferenceValue).GetComponent<Transform>().position, Quaternion.identity, 1f, EventType.Repaint);
		}

		if ((InteractType)interactType.enumValueIndex == InteractType.Key) {
			Handles.color = Color.red;
			Interactable[] allInstances = FindObjectsByType<Interactable>(UnityEngine.FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.None);
			foreach (Interactable instance in allInstances) {
				if (instance.interactType == InteractType.Door && instance.doorCode == doorCode.stringValue) {
					Handles.SphereHandleCap(0, instance.transform.position, Quaternion.identity, 1f, EventType.Repaint);
				}
			}
		}

		if ((InteractType)interactType.enumValueIndex == InteractType.Door) {
			Handles.color = new Color(1f, 0f, 0f, .5f);	// So i can actually see the key when its highlighted
			Interactable[] allInstances = FindObjectsByType<Interactable>(UnityEngine.FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.None);
			foreach (Interactable instance in allInstances) {
				if (instance.interactType == InteractType.Key && instance.doorCode == doorCode.stringValue) {
					Handles.SphereHandleCap(0, instance.transform.position, Quaternion.identity, 1f, EventType.Repaint);
				}
			}
		}
	}

	private void DisplayDoorcodes(Interactable[] allInstances) {
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PropertyField(doorCode);

		if (GUILayout.Button("▼", GUILayout.Width(20))) {
			showDoorCodeSuggestions = !showDoorCodeSuggestions;
		}
		EditorGUILayout.EndHorizontal();

		if (!showDoorCodeSuggestions)
			return;

		Interactable i = (Interactable)target;
		List<string> suggestions = new() { doorCode.stringValue };
		foreach (Interactable instance in allInstances) {
			if (instance != i && instance.doorCode != string.Empty && !suggestions.Contains(instance.doorCode)) {
				suggestions.Add(instance.doorCode);
			}
		}

		int selectedIndex = 0;
		if (suggestions.Count > 1) {
			selectedIndex = EditorGUILayout.Popup("Suggestions", selectedIndex, suggestions.ToArray());

			if (selectedIndex > 0) {
				doorCode.stringValue = suggestions[selectedIndex];
				EditorUtility.SetDirty(i);
				showDoorCodeSuggestions = false;
			}
		}
	}

	private void DisplayNames(Interactable[] allInstances) {
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PropertyField(itemName);

		if (GUILayout.Button("▼", GUILayout.Width(20))) {
			showNameSuggestions = !showNameSuggestions;
		}
		EditorGUILayout.EndHorizontal();

		if (!showNameSuggestions)
			return;

		Interactable i = (Interactable)target;
		List<string> suggestions = new() { itemName.stringValue };
		foreach (Interactable instance in allInstances) {
			if (instance != i && instance.itemName != string.Empty && !suggestions.Contains(instance.itemName)) {
				suggestions.Add(instance.itemName);
			}
		}

		int selectedIndex = 0;
		if (suggestions.Count > 1) {
			selectedIndex = EditorGUILayout.Popup("Suggestions", selectedIndex, suggestions.ToArray());

			if (selectedIndex > 0) {
				itemName.stringValue = suggestions[selectedIndex];
				EditorUtility.SetDirty(i);
				showNameSuggestions = false;
			}
		}
	}
}
