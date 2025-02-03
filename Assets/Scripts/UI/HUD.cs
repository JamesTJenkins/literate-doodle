using UnityEngine;
using TMPro;

public class HUD : MonoBehaviour {
	[SerializeField] private TMP_Text hintbox;

	private bool hintActive = false;

	private void Start() {
		PlayerEvents.displayHint += OnDisplayHint;
		PlayerEvents.togglePauseMenu += OnTogglePauseMenu;
	}

	private void OnDestroy() {
		PlayerEvents.displayHint -= OnDisplayHint;
		PlayerEvents.togglePauseMenu -= OnTogglePauseMenu;
	}

	private void OnTogglePauseMenu() {
		if (hintActive) {
			hintbox.gameObject.SetActive(!hintbox.gameObject.activeSelf);
		}
	}

	private void OnDisplayHint(string hint) {
		if (hint == string.Empty) {
			hintbox.gameObject.SetActive(false);
			hintActive = false;
			return;
		}

		hintbox.text = hint;
		hintbox.gameObject.SetActive(true);
		hintActive = true;
	}
}
