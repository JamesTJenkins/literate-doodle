using UnityEngine;

public class MainMenu : MonoBehaviour {
	[SerializeField] private GameObject menu;
	[SerializeField] private GameObject settings;

	private void Start() {
		Cursor.lockState = CursorLockMode.None;
	}

	public void Play() {
		Helper.LoadScene(Consts.Menu.MAIN_LEVEL_NAME);
	}

	public void Settings(bool open) {
		settings.SetActive(open);
		menu.SetActive(!open);
	}

	public void Quit() {
		Application.Quit();
	}
}
