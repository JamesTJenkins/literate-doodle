using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class GameData : ICloneable {
	// Sensitivity
	public float sensitivity;
	// Video settings
	public int windowMode;
	public int resWidth;
	public int resHeight;
	// Audio settings
	public float masterVol;
	public float musicVol;
	public float sfxVol;

	public GameData() {
		switch (Screen.fullScreenMode) {
			case FullScreenMode.ExclusiveFullScreen:
			windowMode = 0;
			break;
			case FullScreenMode.Windowed:
			windowMode = 1;
			break;
			case FullScreenMode.FullScreenWindow:
			windowMode = 2;
			break;
		}

		Resolution res = Screen.currentResolution;
		resWidth = res.width;
		resHeight = res.height;

		masterVol = 0.5f;
		musicVol = 0.5f;
		sfxVol = 0.5f;

		sensitivity = 1f;
	}

	public void UpdateVideoSettings(int width, int height, int _windowMode) {
		windowMode = _windowMode;
		resWidth = width;
		resHeight = height;
	}

	public void UpdateAudioSettings(float _masterVol, float _musicVol, float _sfxVol) {
		masterVol = _masterVol;
		musicVol = _musicVol;
		sfxVol = _sfxVol;
	}

	public void UpdateSensitivity(float _sensitivity) {
		sensitivity = _sensitivity;
	}

	public object Clone() {
		return this.MemberwiseClone();
	}
}

// Basic single save system with caching support
public static class Save {
	private static GameData cachedData = null;
	private static readonly object fileLock = new object();

#if UNITY_EDITOR
	public static string GetSavePath() => Application.dataPath + "/Saves/save.mcsave";
#else
	public static string GetSavePath() => Application.persistentDataPath + "/save.mcsave";
#endif

	public static bool SaveExists() => File.Exists(GetSavePath());

	// Non async load of gamedata which will stall the game and load the data. Generally dont use this if you dont have to
	public static GameData GetData() {
		if (cachedData != null)
			return cachedData;

		GameData data = LoadData(GetSavePath());
		cachedData = data;
		return data;
	}

	// Async load of gamedata
	public static async Task<GameData> AsyncGetData() {
		if (cachedData != null)
			return cachedData;

		GameData data = await Task.Run(() => { return LoadData(GetSavePath()); });
		cachedData = data;
		return cachedData;
	}

	public static async Task SaveAsync(GameData data) {
		// Cache new gamedata
		cachedData = data;
		// Save to file
		string path = GetSavePath();
		GameData dataCopy = (GameData)data.Clone();
		await Task.Run(() => SaveData(dataCopy, path));
	}

	public static async Task DeleteAsync() {
		// Clear cached data
		cachedData = null;
		// Delete file
		string path = GetSavePath();
		await Task.Run(() => DeleteData(path));
	}

	private static void SaveData(GameData data, string path) {
#if UNITY_EDITOR
		if (!Directory.Exists(Application.dataPath + "/Saves")) {
			Directory.CreateDirectory(Application.dataPath + "/Saves");
		}
#endif

		lock (fileLock) {
			try {
				using (FileStream fileStream = new FileStream(path, FileMode.Create)) {
					BinaryFormatter formatter = new BinaryFormatter();
					formatter.Serialize(fileStream, data);
				}
				Debug.Log("Data saved successfully.");
			} catch (IOException ioEx) {
				Debug.LogError("IOException: " + ioEx.Message);
			}
		}
	}

	private static GameData LoadData(string path) {
		if (File.Exists(path)) {
			lock (fileLock) {
				try {
					using (FileStream fileStream = new FileStream(path, FileMode.Open)) {
						if (fileStream.Length == 0) {
							Debug.Log("File is empty");
							return new GameData();
						}

						BinaryFormatter formatter = new BinaryFormatter();
						GameData data = (GameData)formatter.Deserialize(fileStream);
						return data;
					}
				} catch (IOException ioEx) {
					Debug.LogError("IOException: " + ioEx.Message);
					return new GameData();
				}
			}
		} else {
			return new GameData();
		}
	}

	private static bool DeleteData(string path) {
		if (File.Exists(path)) {
			File.Delete(path);
			return true;
		}

		return false;
	}
}
