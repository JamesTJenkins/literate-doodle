using System;

public static class PlayerEvents {
	public static Action<bool> togglePlayerInput;
	public static Action togglePauseMenu;
	public static Action updateSensitivity;
	public static Action saveSettings;
	public static Action<string> displayHint;

	public static void OnTogglePlayerInput(bool enable) {
		togglePlayerInput?.Invoke(enable);
	}

	public static void OnTogglePauseMenu() {
		togglePauseMenu?.Invoke();
	}

	public static void OnUpdateSensitivity() {
		updateSensitivity?.Invoke();
	}

	public static void OnSaveSettings() {
		saveSettings?.Invoke();
	}

	public static void OnDisplayHint(string hint) {
		displayHint?.Invoke(hint);
	}
}
