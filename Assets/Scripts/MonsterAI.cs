using UnityEngine;
using UnityEngine.AI;

public class MonsterAI : MonoBehaviour {

	public NavMeshAgent agent;
	public GameObject player;
	public Vector3[] travelPoints;

	public float debugSphereRadius;

	void Start() {
		agent.destination = travelPoints[Random.Range(0, travelPoints.Length)];
	}

	private void FixedUpdate() {

	if (Physics.Raycast(transform.position, player.transform.position - transform.position)) {
		Debug.Log("Player sighted");
		}

		if (agent.remainingDistance < 1f) {
			agent.destination = travelPoints[Random.Range(0, travelPoints.Length)];
		}

		transform.rotation = Quaternion.LookRotation(agent.velocity.normalized);
	}

	private void OnDrawGizmos() {
		Gizmos.color = Color.magenta;
		Gizmos.DrawRay(transform.position, player.transform.position - transform.position);
	}

	/*PSEUDO
	If player sighted target player
	If player within distance, attack player
	If player not sighted go to last known point
	If player not around last known point go to random point
	 */
}
