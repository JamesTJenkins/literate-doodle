using UnityEngine;

public class OpenDoorAnimEvent : MonoBehaviour {
    public void HideDoor() {
		gameObject.SetActive(false);
	}

	public void PlayDoorSound() {
		gameObject.GetComponentInChildren<AudioSource>().Play();
	}
}
