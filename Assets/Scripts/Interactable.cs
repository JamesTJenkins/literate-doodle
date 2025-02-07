using UnityEngine;

public enum InteractType {
	Item,
	Key,
	Door,
	Switch,
	Coffin,
	None
}

public class Interactable : MonoBehaviour {
	public string itemName;
	public InteractType interactType;
}
