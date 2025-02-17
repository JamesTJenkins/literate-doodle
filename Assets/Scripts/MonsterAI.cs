using UnityEngine;
using UnityEngine.AI;

public class MonsterAI : MonoBehaviour {

	public NavMeshAgent agent;
	public PlayerController player;
	public Vector3[] travelPoints;

	public LayerMask playerLayerMask;
	public Vector3 lastKnownPlayerPosition;
	public GameObject rayCastOrigin;
	public float immediateAwarenessRange;
	public float sightRange;
	public float horizontalFov;

	public float monsterWalkingSpeed;
	public float monsterRunningSpeed;
	public bool isRunning;

	public float monsterAttackDistance;

	public Animator monsterAnimator;

	public Camera killCamA;

	public bool playerSeenCoffin;
	public bool canSee;

	private bool disable = false;
	private RaycastHit hitInfo;

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

		monsterAnimator.SetFloat(Consts.Anims.SPEED, agent.velocity.magnitude);
		transform.rotation = Quaternion.LookRotation(agent.velocity, Vector3.up);

		playerSeenCoffin = player.hidden && canSee;
		canSee = CanSeePlayer();

		if (canSee) {
			lastKnownPlayerPosition = player.transform.position;
			agent.destination = lastKnownPlayerPosition;
			isRunning = true;
		} else {
			isRunning = false;
		}

		if (agent.remainingDistance <= agent.stoppingDistance) {
			agent.destination = travelPoints[Random.Range(0, travelPoints.Length)];
		}

		if (canSee && Vector3.Distance(player.transform.position, transform.position) <= monsterAttackDistance) {
			disable = true;
			isRunning = false;
			agent.isStopped = true;

			PlayerEvents.OnDisplayHint(string.Empty);
			PlayerEvents.OnForceClosePauseMenu();	// Always do this before disabling input since force closing pause menu will renable inputs
			PlayerEvents.OnTogglePlayerInput(false);
			PlayerEvents.OnToggleUIInput(false);
			monsterAnimator.SetTrigger("Kill");
			if (!player.playerCamera.gameObject.activeSelf && player.lastInteracted.interactType == InteractType.Coffin) {
				player.lastInteracted.coffinCam.gameObject.SetActive(false);
			} else {
				player.GetComponentInChildren<Camera>().gameObject.SetActive(false);
			}
			killCamA.gameObject.SetActive(true);
		}
	}

	public bool CanSeePlayer() {
		if (player.hidden & !canSee)
			return false;

		float distance = Helper.FlatDistance(transform.position, player.transform.position);

		if (distance <= immediateAwarenessRange)
			return true;

		Vector3 targetDir = player.transform.position - transform.position;
		float horizAngle = Vector3.Angle(transform.forward, new Vector3(targetDir.x, 0, targetDir.z));

		if (horizAngle >= horizontalFov / 2)
			return false;

		if (distance <= sightRange) {
			if (Physics.Raycast(rayCastOrigin.transform.position, player.transform.position - rayCastOrigin.transform.position, out hitInfo, playerLayerMask) && hitInfo.collider.CompareTag(Consts.Tags.PLAYER)) {
				return true;
			}
		}

		return false;
	}


#if UNITY_EDITOR
	private void OnDrawGizmos() {
		Gizmos.color = Color.magenta;
		Gizmos.DrawRay(rayCastOrigin.transform.position, player.transform.position - rayCastOrigin.transform.position);

		Gizmos.color = Color.green;
		Gizmos.DrawSphere(lastKnownPlayerPosition, debugSphereRadius);

		Gizmos.color = Color.blue;
		Gizmos.DrawSphere(agent.destination, debugSphereRadius);
	}
#endif
}
