using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MonsterAI : MonoBehaviour {

	public NavMeshAgent agent;
	public PlayerController player;
	public Vector3[] firstSectionTravelPoints;
	public Vector3[] secondSectionTravelPoints;
	public Vector3[] thirdSectionTravelPoints;
	public float playerYLocationForSecondSection;
	public float playerYLocationForThirdSection;

	public LayerMask playerLayerMask;
	public float timeToLoseSight;
	public Vector3 lastKnownPlayerPosition;
	public GameObject rayCastOrigin;
	public float immediateAwarenessRange;
	public float sightRange;
	public float horizontalFov;

	public float monsterWalkingSpeed;
	public float monsterRunningSpeed;

	public AnimationCurve monsterSpeedRamp;
	private float moveDuration = 0f;
	public float timeToFullSpeed;

	public bool isRunning;

	public float monsterAttackDistance;

	public Animator monsterAnimator;

	public Camera killCamA;

	public bool canSee;
	public bool checkLastKnown = false;
	public float coffinCheckChancePercentage;

	public float coffinCheckDistance;
	public LayerMask coffinLayerMask;
	private bool checkingCoffin = false;

	private bool disable = false;
	private RaycastHit hitInfo;

	private HashSet<int> firstSectionInvalidPointIndexes = new();
	private HashSet<int> secondSectionInvalidPointIndexes = new();
	private HashSet<int> thirdSectionInvalidPointIndexes = new();

	private float lostSightTime;

#if UNITY_EDITOR
	public float debugSphereRadius;
#endif

	private void Start() {
		PlayerEvents.toggleDeathScreen += OnDisableAI;
		PlayerEvents.toggleEscapeMenu += OnDisableAI;
		PlayerEvents.resetMonsterInvalidPoints += OnResetMonsterInvalidPoints;

		SetNewPatrolPoint();
	}

	private void OnDestroy() {
		PlayerEvents.toggleDeathScreen -= OnDisableAI;
		PlayerEvents.toggleEscapeMenu -= OnDisableAI;
		PlayerEvents.resetMonsterInvalidPoints -= OnResetMonsterInvalidPoints;
	}

	private void OnResetMonsterInvalidPoints() {
		firstSectionInvalidPointIndexes.Clear();
		secondSectionInvalidPointIndexes.Clear();
		thirdSectionInvalidPointIndexes.Clear();
	}

	private void OnDisableAI() {
		disable = true;
	}

	private void FixedUpdate() {
		if (disable)
			return;

		monsterAnimator.SetFloat(Consts.Anims.SPEED, agent.velocity.magnitude);
		transform.rotation = Quaternion.LookRotation(agent.velocity, Vector3.up);

		canSee = CanSeePlayer();

		if (canSee) {
			lastKnownPlayerPosition = player.transform.position;
			agent.destination = lastKnownPlayerPosition;
			checkLastKnown = true;
			isRunning = true;
			lostSightTime = timeToLoseSight;

			moveDuration = Mathf.Clamp(moveDuration + Time.fixedDeltaTime, 0, timeToFullSpeed);
			agent.speed = monsterSpeedRamp.Evaluate(moveDuration / timeToFullSpeed);
			monsterAnimator.speed = agent.speed - Mathf.Sqrt(agent.speed);
		} else {
			if (checkLastKnown) {
				if (lostSightTime > 0f) {
					lostSightTime -= Time.fixedDeltaTime;
					lastKnownPlayerPosition = player.transform.position;
					agent.destination = lastKnownPlayerPosition;
				}

				if (agent.remainingDistance < agent.stoppingDistance) {
					SelectCoffinToCheck();
					checkLastKnown = false;
				}
			} else {
				isRunning = false;

				moveDuration = Mathf.Clamp(moveDuration - Time.fixedDeltaTime, 0, timeToFullSpeed);
				agent.speed = agent.speed > monsterRunningSpeed ? monsterSpeedRamp.Evaluate(moveDuration / timeToFullSpeed) : monsterWalkingSpeed;
				monsterAnimator.speed = agent.speed > monsterRunningSpeed ? agent.speed - Mathf.Sqrt(agent.speed) : monsterWalkingSpeed;
			}
		}

		if (agent.remainingDistance <= agent.stoppingDistance && Vector3.Distance(player.transform.position, transform.position) <= monsterAttackDistance + 2.5f && checkingCoffin) {
			KillEvent();
		}

		if (agent.remainingDistance <= agent.stoppingDistance) {
			SetNewPatrolPoint();
			checkingCoffin = false;
		}

		if (canSee && Vector3.Distance(player.transform.position, transform.position) <= monsterAttackDistance) {
			KillEvent();
		}
	}

	private Vector3[] GetSectionPoints() {
		if (player.transform.position.y < playerYLocationForSecondSection) {
			if (player.transform.position.y < playerYLocationForThirdSection) {
				return thirdSectionTravelPoints;
			}

			return secondSectionTravelPoints;
		}

		return firstSectionTravelPoints;
	}

	private HashSet<int> GetSectionValidIndexes() {
		if (player.transform.position.y < playerYLocationForSecondSection) {
			if (player.transform.position.y < playerYLocationForThirdSection) {
				return thirdSectionInvalidPointIndexes;
			}

			return secondSectionInvalidPointIndexes;
		}

		return firstSectionInvalidPointIndexes;
	}

	private void SetNewPatrolPoint() {
		// This will need some changes if more parts are added but will do for now
		Vector3[] points = GetSectionPoints();
		HashSet<int> indexes = GetSectionValidIndexes();

		bool validPoint = false;
		NavMeshPath path = new();
		while (!validPoint) {
			// Picks random index
			int index = Random.Range(0, points.Length);
			// If invalid increment by 1 until find a valid index
			while (indexes.Contains(index)) {
				index++;
				if (index >= points.Length) {
					index = 0;
				}
			}

			// Check if travel point is valid and adds index to invalid if cant be done so doesnt check again
			if (agent.CalculatePath(points[index], path)) {
				validPoint = true;
			} else {
				indexes.Add(index);
			}
		}

		agent.path = path;
	}

	public void KillEvent() {
		disable = true;
		isRunning = false;
		agent.isStopped = true;
		agent.velocity = Vector3.zero;
		monsterAnimator.speed = 1;

		PlayerEvents.OnDisplayHint(string.Empty);
		PlayerEvents.OnForceClosePauseMenu();   // Always do this before disabling input since force closing pause menu will renable inputs
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

	public void SelectCoffinToCheck() {
		Collider[] coffinCheckHit = Physics.OverlapSphere(rayCastOrigin.transform.position, coffinCheckDistance, coffinLayerMask);
		foreach (Collider castHit in coffinCheckHit) {
			if (castHit.CompareTag(Consts.Tags.INTERACT) && Random.Range(0, 100) >= coffinCheckChancePercentage) {
				agent.destination = castHit.transform.position;
				checkingCoffin = true;
				return;
			}
		}
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
