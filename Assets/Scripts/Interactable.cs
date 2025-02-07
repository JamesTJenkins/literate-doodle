using UnityEngine;

public enum InteractType {
	Item,
	Door,
	Switch,
	None
}

public class Interactable : MonoBehaviour {
	public string itemName;
	public InteractType interactType;
}
