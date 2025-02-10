using UnityEngine;

public enum InteractType {
	Item,
	Key,
	Door,
	Switch,
	Coffin,
	Escape,
	None
}

public class Interactable : MonoBehaviour {
	public string itemName;
	public InteractType interactType;
	public string doorCode; // This is how you set what key opens what door(s)
	public Vector3 inCoffinOffset;
	public Camera coffinCam;
	public GameObject doorToToggle;
}
