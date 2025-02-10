using UnityEngine;

public class DeathScreen : MonoBehaviour {
	[SerializeField] private GameObject deathScreen;
	[SerializeField] private GameObject crosshair;

	private void Start() {
		PlayerEvents.toggleDeathScreen += OnToggleDeathScreen;
	}

	private void OnDestroy() {
		PlayerEvents.toggleDeathScreen -= OnToggleDeathScreen;
	}

	private void OnToggleDeathScreen() {
		deathScreen.SetActive(true);
		crosshair.SetActive(false);
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
	}
}
