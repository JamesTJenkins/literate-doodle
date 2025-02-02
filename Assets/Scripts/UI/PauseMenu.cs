using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour {
	[SerializeField] private GameObject pauseMenu;
	[SerializeField] private GameObject menu;
	[SerializeField] private GameObject settings;

	private void Start() {
		PlayerEvents.togglePauseMenu += TogglePauseMenu;
	}

	private void OnDestroy() {
		PlayerEvents.togglePauseMenu -= TogglePauseMenu;
	}

	public void MainMenu() {
		LoadScene(Consts.Menu.MAINMENU_LEVEL_NAME);
	}

	public void Settings(bool open) {
		settings.SetActive(open);
		menu.SetActive(!open);
	}

	public void TogglePauseMenu() {
		pauseMenu.SetActive(!pauseMenu.activeSelf);
		Cursor.lockState = pauseMenu.activeSelf ? CursorLockMode.None : CursorLockMode.Confined;
		Cursor.visible = pauseMenu.activeSelf;
		PlayerEvents.OnTogglePlayerInput(!pauseMenu.activeSelf);

		if (settings.activeSelf) {
			Settings(false);
			PlayerEvents.OnSaveSettings();
		}
	}

	private async void LoadScene(string levelName) {
		_ = SceneManager.LoadSceneAsync(Consts.Menu.LOAD_LEVEL_NAME, LoadSceneMode.Single);
		AsyncOperation newLevel = SceneManager.LoadSceneAsync(levelName, LoadSceneMode.Additive);

#if UNITY_EDITOR
		// To stop error from leaving scenes in playmode
		if (newLevel == null)
			return;
#endif

		newLevel.allowSceneActivation = true;
		while (!newLevel.isDone)
			await Task.Yield();

		await SceneManager.UnloadSceneAsync(1);
	}
}
