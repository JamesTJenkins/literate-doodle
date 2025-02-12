using System;

public static class PlayerEvents {
	public static Action<bool> togglePlayerInput;
	public static Action<bool> toggleUIInput;
	public static Action forceClosePauseMenu;
	public static Action togglePauseMenu;
	public static Action toggleDeathScreen;
	public static Action toggleEscapeMenu;
	public static Action showQuestsStart;
	public static Action showQuestsStopped;
	public static Action updateSensitivity;
	public static Action saveSettings;
	public static Action escapeEnabled;
	public static Action<string> displayHint;
	public static Action<string> itemPickedUp;

	public static void OnTogglePlayerInput(bool enable) {
		togglePlayerInput?.Invoke(enable);
	}

	public static void OnToggleUIInput(bool enable) {
		toggleUIInput?.Invoke(enable);
	}

	public static void OnTogglePauseMenu() {
		togglePauseMenu?.Invoke();
	}

	public static void OnForceClosePauseMenu() {
		forceClosePauseMenu?.Invoke();
	}

	public static void OnToggleDeathScreen() {
		toggleDeathScreen?.Invoke();
	}

	public static void OnToggleEscapeMenu() {
		toggleEscapeMenu?.Invoke();
	}

	public static void OnShowQuestsStart() {
		showQuestsStart?.Invoke();
	}

	public static void OnShowQuestsStopped() {
		showQuestsStopped?.Invoke();
	}

	public static void OnUpdateSensitivity() {
		updateSensitivity?.Invoke();
	}

	public static void OnSaveSettings() {
		saveSettings?.Invoke();
	}

	public static void OnEscapeEnabled() {
		escapeEnabled?.Invoke();
	}

	public static void OnDisplayHint(string hint) {
		displayHint?.Invoke(hint);
	}

	public static void OnItemPickup(string itemName) {
		itemPickedUp?.Invoke(itemName);
	}
}
