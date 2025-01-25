using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {
    private void Start() {
		Cursor.lockState = CursorLockMode.None;
	}

	public void Play() {
		LoadScene(Consts.MAIN_LEVEL_NAME);
	}

	public void Quit() {
		Application.Quit();
	}

	private async void LoadScene(string levelName) {
		_ = SceneManager.LoadSceneAsync(Consts.LOAD_LEVEL_NAME, LoadSceneMode.Single);
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
