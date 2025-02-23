using UnityEngine;

public class MonsterSoundEvents : MonoBehaviour {

	public AudioSource MonsterWalkSounds;

	public AudioSource HeadTiltSound, MouthOpenSound, KillSound, GoreSound;

	public void StepEvent() {
		MonsterWalkSounds.Play();
	}

	public void HeadTiltEvent() {
		HeadTiltSound.Play();
	}

	public void MouthOpenEvent() {
		MouthOpenSound.Play();
	}

	public void KillSoundEvent() {
		KillSound.Play();
	}
	 
	public void GoreEvent() {
		GoreSound.Play();
	}
}