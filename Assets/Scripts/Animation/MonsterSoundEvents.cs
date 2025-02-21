using UnityEngine;

public class MonsterSoundEvents : MonoBehaviour {

	public AudioSource MonsterWalkSounds;

	public void StepEvent() {
		MonsterWalkSounds.Play();
	}
}
