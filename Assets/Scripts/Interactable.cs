using UnityEngine;

public enum InteractType {
	Item,
	Door,
	Switch,
	Coffin,
	None
}

public class Interactable : MonoBehaviour {
	public string itemName;
	public InteractType interactType;
}
