using UnityEngine;

public class PauseMenu : MonoBehaviour {
	[SerializeField] private GameObject pauseMenu;
	[SerializeField] private GameObject menu;
	[SerializeField] private GameObject settings;
	[SerializeField] private GameObject crosshair;

	private bool crosshairState;

	private void Start() {
		PlayerEvents.togglePauseMenu += TogglePauseMenu;
		PlayerEvents.forceClosePauseMenu += ForceClosePauseMenu;
		PlayerEvents.updateCrosshair += UpdateCrosshair;

		UpdateCrosshair();
	}

	private void OnDestroy() {
		PlayerEvents.togglePauseMenu -= TogglePauseMenu;
		PlayerEvents.forceClosePauseMenu -= ForceClosePauseMenu;
		PlayerEvents.updateCrosshair -= UpdateCrosshair;
	}

	public void Restart() {
		Helper.LoadScene(Consts.Menu.MAIN_LEVEL_NAME);
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
		crosshair.SetActive(crosshairState ? !pauseMenu.activeSelf : false);
		PlayerEvents.OnTogglePlayerInput(!pauseMenu.activeSelf);

		if (settings.activeSelf) {
			Settings(false);
			PlayerEvents.OnSaveSettings();
		}
	}

	private void ForceClosePauseMenu() {
		pauseMenu.SetActive(false);
		crosshair.SetActive(crosshairState ? !pauseMenu.activeSelf : false);
		Cursor.lockState = CursorLockMode.Confined;
		Cursor.visible = false;
		PlayerEvents.OnTogglePlayerInput(true);
	}

	private void UpdateCrosshair() {
		crosshairState = Save.GetData().crosshair;

		if (!pauseMenu.activeSelf)
			crosshair.SetActive(crosshairState);
	}
}
