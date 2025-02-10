using UnityEngine;

public class EscapeMenu : MonoBehaviour {
	[SerializeField] private GameObject escapeMenu;
	[SerializeField] private GameObject crosshair;

	private void Start() {
		PlayerEvents.toggleEscapeMenu += OnToggleEscapeMenu;
	}

	private void OnDestroy() {
		PlayerEvents.toggleEscapeMenu -= OnToggleEscapeMenu;
	}

	private void OnToggleEscapeMenu() {
		escapeMenu.SetActive(true);
		crosshair.SetActive(false);
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
	}
}
