using System;
using UnityEngine;

public class DeathScreen : MonoBehaviour {
	[SerializeField] private GameObject deathScreen;

	private void Start() {
		PlayerEvents.toggleDeathScreen += OnToggleDeathScreen;
	}

	private void OnDestroy() {
		PlayerEvents.toggleDeathScreen -= OnToggleDeathScreen;
	}

	private void OnToggleDeathScreen() {
		deathScreen.SetActive(true);
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
	}

	public void Restart() {
		Helper.LoadScene(Consts.Menu.MAIN_LEVEL_NAME);
	}
}
