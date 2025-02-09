using UnityEngine;

public class PauseMenu : MonoBehaviour {
	[SerializeField] private GameObject pauseMenu;
	[SerializeField] private GameObject menu;
	[SerializeField] private GameObject settings;
	[SerializeField] private GameObject crosshair;

	private void Start() {
		PlayerEvents.togglePauseMenu += TogglePauseMenu;
		PlayerEvents.forceClosePauseMenu += ForceClosePauseMenu;
	}

	private void OnDestroy() {
		PlayerEvents.togglePauseMenu -= TogglePauseMenu;
		PlayerEvents.forceClosePauseMenu -= ForceClosePauseMenu;
	}

	public void MainMenu() {
		Helper.LoadScene(Consts.Menu.MAINMENU_LEVEL_NAME);
	}

	public void Settings(bool open) {
		settings.SetActive(open);
		menu.SetActive(!open);
	}

	public void TogglePauseMenu() {
		pauseMenu.SetActive(!pauseMenu.activeSelf);
		Cursor.lockState = pauseMenu.activeSelf ? CursorLockMode.None : CursorLockMode.Confined;
		Cursor.visible = pauseMenu.activeSelf;
		crosshair.SetActive(!pauseMenu.activeSelf);
		PlayerEvents.OnTogglePlayerInput(!pauseMenu.activeSelf);

		if (settings.activeSelf) {
			Settings(false);
			PlayerEvents.OnSaveSettings();
		}
	}

	private void ForceClosePauseMenu() {
		pauseMenu.SetActive(false);
		Cursor.lockState = CursorLockMode.Confined;
		Cursor.visible = false;
		PlayerEvents.OnTogglePlayerInput(true);
	}
}
