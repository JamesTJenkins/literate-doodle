using UnityEngine;

public class DeathAnimEvent : MonoBehaviour {
    public void Death() {
		PlayerEvents.OnToggleDeathScreen();
	}
}
