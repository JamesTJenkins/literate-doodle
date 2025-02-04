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

	public float debugSphereRadius;

	void Start() {
		agent.destination = travelPoints[Random.Range(0, travelPoints.Length)];
	}

	private void FixedUpdate() {

		rayCastTarget = player.transform.position + rayCastTargetOffset;

	if (Physics.Raycast(rayCastOrigin.transform.position, rayCastTarget - rayCastOrigin.transform.position, out hitInfo, playerLayerMask)) {
		if (hitInfo.collider.CompareTag("Player")) {
				lastKnownPlayerPosition = rayCastTarget;
				agent.destination = lastKnownPlayerPosition;
			}
		}
		if (agent.remainingDistance < 1f) {
			agent.destination = travelPoints[Random.Range(0, travelPoints.Length)];
		}
		transform.rotation = Quaternion.LookRotation(agent.velocity, Vector3.up);
	}

	private void OnDrawGizmos() {
		Gizmos.color = Color.magenta;
		Gizmos.DrawRay(rayCastOrigin.transform.position, rayCastTarget - rayCastOrigin.transform.position);

		Gizmos.color = Color.green;
		Gizmos.DrawSphere(lastKnownPlayerPosition, debugSphereRadius);

		Gizmos.color = Color.blue;
		Gizmos.DrawSphere(agent.destination, debugSphereRadius);
	}

	/*PSEUDO
	If player within distance, attack player
	 */
}
