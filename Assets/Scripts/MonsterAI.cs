using UnityEngine;
using UnityEngine.AI;

public class MonsterAI : MonoBehaviour {

	public NavMeshAgent agent;
	public GameObject player;
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

#if UNITY_EDITOR
	public float debugSphereRadius;
#endif

	void Start() {
		agent.destination = travelPoints[Random.Range(0, travelPoints.Length)];
	}

	private void FixedUpdate() {
		rayCastTarget = player.transform.position + rayCastTargetOffset;

		monsterAnimator.SetFloat(Consts.Anims.SPEED, agent.velocity.magnitude);
		transform.rotation = Quaternion.LookRotation(agent.velocity, Vector3.up);

		if (Physics.Raycast(rayCastOrigin.transform.position, rayCastTarget - rayCastOrigin.transform.position, out hitInfo, playerLayerMask)) {
			if (hitInfo.collider.CompareTag(Consts.Tags.PLAYER)) {
				lastKnownPlayerPosition = rayCastTarget;
				agent.destination = lastKnownPlayerPosition;
				isRunning = true;
			} else {
				isRunning = false;
			}

		}
		if (agent.remainingDistance <= agent.stoppingDistance && !hitInfo.collider.CompareTag(Consts.Tags.PLAYER)) {
			agent.destination = travelPoints[Random.Range(0, travelPoints.Length)];
		}

		if (agent.remainingDistance <= monsterAttackDistance && hitInfo.collider.CompareTag(Consts.Tags.PLAYER)) {
			isRunning = false;
			// Put Attacking Anim here :)
		}

		if (isRunning) {
			agent.speed = monsterRunningSpeed;
		} else {
			agent.speed = monsterWalkingSpeed;
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

	/*PSEUDO
	If player within distance, attack player
	 */
}
