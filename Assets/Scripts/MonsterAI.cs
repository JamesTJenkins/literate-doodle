using UnityEditorInternal;
using UnityEngine;
using UnityEngine.AI;

public class MonsterAI : MonoBehaviour {

	public NavMeshAgent agent;
	public Transform targetGoal;

	public Vector3[] travelPoints;

	[Header("Debug")]
	public float debugSphereRadius;
	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start() {
		agent.destination = travelPoints[Random.Range(0, travelPoints.Length)];
	}

	// Update is called once per frame
	void Update() {
		if (agent.remainingDistance < 1f) {
			agent.destination = travelPoints[Random.Range(0, travelPoints.Length)];
		}
	}

	/*PSEUDO
	Get list of points on navmesh
	Pick random point on map
	If player sighted target player
	If player within distance, attack player
	If player not sighted go to last known point
	If player not around last known point go to random point
	 */
}
