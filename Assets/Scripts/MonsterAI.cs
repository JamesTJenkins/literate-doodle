using UnityEngine;
using UnityEngine.AI;

public class MonsterAI : MonoBehaviour {

	public NavMeshAgent agent;
	public PlayerController player;
	public Vector3[] travelPoints;

	public LayerMask playerLayerMask;
	public Vector3 lastKnownPlayerPosition;
	public GameObject rayCastOrigin;
	public Vector3 rayCastTarget;
	public Vector3 rayCastTargetOffset;
	private RaycastHit hitInfo;

	public float monsterWalkingSpeed;
	public float monsterRunningSpeed;
	public bool isRunning;

	public float monsterAttackDistance;

	public Animator monsterAnimator;

	public Camera killCamA;

	private bool disable = false;

#if UNITY_EDITOR
	public float debugSphereRadius;
#endif

	private void Start() {
		agent.destination = travelPoints[Random.Range(0, travelPoints.Length)];
		PlayerEvents.toggleDeathScreen += OnDisableAI;
		PlayerEvents.toggleEscapeMenu += OnDisableAI;
	}

	private void OnDestroy() {
		PlayerEvents.toggleDeathScreen -= OnDisableAI;
		PlayerEvents.toggleEscapeMenu -= OnDisableAI;
	}

	private void OnDisableAI() {
		disable = true;
	}

	private void FixedUpdate() {
		if (disable)
			return;

		rayCastTarget = player.transform.position + rayCastTargetOffset;

		monsterAnimator.SetFloat(Consts.Anims.SPEED, agent.velocity.magnitude);
		transform.rotation = Quaternion.LookRotation(agent.velocity, Vector3.up);

		if (!player.hidden && Physics.Raycast(rayCastOrigin.transform.position, rayCastTarget - rayCastOrigin.transform.position, out hitInfo, playerLayerMask)) {
			if (hitInfo.collider.CompareTag(Consts.Tags.PLAYER)) {
				lastKnownPlayerPosition = rayCastTarget;
				agent.destination = lastKnownPlayerPosition;
				isRunning = true;
			} else {
				isRunning = false;
			}
		}

		if (agent.remainingDistance <= agent.stoppingDistance) {
			agent.destination = travelPoints[Random.Range(0, travelPoints.Length)];
		}

		if (!player.hidden && Vector3.Distance(player.transform.position, transform.position) <= monsterAttackDistance && hitInfo.collider.CompareTag(Consts.Tags.PLAYER)) {
			disable = true;
			isRunning = false;
			agent.isStopped = true;

			PlayerEvents.OnForceClosePauseMenu();	// Always do this before disabling input since force closing pause menu will renable inputs
			PlayerEvents.OnTogglePlayerInput(false);
			PlayerEvents.OnToggleUIInput(false);
			monsterAnimator.SetTrigger("Kill");
			player.GetComponentInChildren<Camera>().gameObject.SetActive(false);
			killCamA.gameObject.SetActive(true);
		}
	}

#if UNITY_EDITOR
	private void OnDrawGizmos() {
		Gizmos.color = Color.magenta;
		Gizmos.DrawRay(rayCastOrigin.transform.position, rayCastTarget - rayCastOrigin.transform.position);

		Gizmos.color = Color.green;
		Gizmos.DrawSphere(lastKnownPlayerPosition, debugSphereRadius);

		Gizmos.color = Color.blue;
		Gizmos.DrawSphere(agent.destination, debugSphereRadius);
	}
#endif
}
