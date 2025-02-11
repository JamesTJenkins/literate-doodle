using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Helper {
	public static async void LoadScene(string levelName) {
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

	public static float FlatDistance(Vector3 a, Vector3 b) {
		a.y = 0;
		b.y = 0;
		return Vector3.Distance(a, b);
	}
}
