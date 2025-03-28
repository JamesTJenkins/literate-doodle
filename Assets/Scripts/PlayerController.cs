using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour {
	[Header("References")]
	public CharacterController characterController;
	public InputSystem_Actions userInput;
	public Camera playerCamera;
	public Interactable lastInteracted;
	public AudioSource playerWalkingSoundSource;
	public AudioSource playerKeyPickupSource;
	public AudioSource PlayerCoffinInteractSource;
	public AudioSource PlayerEscapeSource;

	[Header("Interact")]
	[SerializeField] private float interactDistance = 5f;
	[SerializeField] private LayerMask interactLayers;
	public bool hidden = false;
	public bool setKill = false;

	private HashSet<string> doorKeys = new();
	private HashSet<string> items = new();

	[Header("Looking")]
	private float sensitivity;
	private bool invertLook;

	public float minLookAngle, maxLookAngle;
	private float xRotation;

	[Header("Movement")]
	public float movementBaseSpeed;
	public float sprintingAddedSpeed;
	private bool isSprinting;

	private float stamina = 100;
	public float staminaDrainRate;
	public float staminaRegenRate;

	public bool sprintingEnabled;
	public float sprintEnableValue;

	private bool isGrounded;
	public LayerMask groundMask;
	public float checkSphereRadius;
	public float groundOffset;

	private GameObject prevHit;
	private bool escapeEnabled = false;

	[Header("Sound")]
	private bool soundQueued = false;
	public float walkingSoundDelay;
	public float sprintingSoundDelay;

	private void Start() {
		userInput = new InputSystem_Actions();
		userInput.UI.Pause.performed += OnPause;
		userInput.UI.Quest.started += OnQuestViewStart;
		userInput.UI.Quest.canceled += OnQuestViewStopped;
		userInput.Player.Interact.performed += OnInteract;
		userInput.Enable();

		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;

		UpdateSensitivity();
		PlayerEvents.updateSensitivity += UpdateSensitivity;
		PlayerEvents.updateInvertLook += UpdateInvertLook;
		PlayerEvents.togglePlayerInput += TogglePlayerInput;
		PlayerEvents.toggleUIInput += ToggleUIInput;
		PlayerEvents.escapeEnabled += OnEscapeEnabled;

		stamina = 100f;
		sprintingEnabled = true;
	}

	private void OnDestroy() {
		userInput.Disable();
		userInput.UI.Pause.performed -= OnPause;
		userInput.UI.Quest.started -= OnQuestViewStart;
		userInput.UI.Quest.canceled -= OnQuestViewStopped;
		userInput.Player.Interact.performed -= OnInteract;
		userInput.Dispose();

		PlayerEvents.updateSensitivity -= UpdateSensitivity;
		PlayerEvents.updateInvertLook -= UpdateInvertLook;
		PlayerEvents.togglePlayerInput -= TogglePlayerInput;
		PlayerEvents.toggleUIInput -= ToggleUIInput;
		PlayerEvents.escapeEnabled -= OnEscapeEnabled;
	}

	private void Update() {
		if (hidden)
			return;

		Looking(userInput.Player.Look.ReadValue<Vector2>());
	}

	private void FixedUpdate() {
		if (hidden)
			return;

		Moving(userInput.Player.Move.ReadValue<Vector2>());
		Sprinting(userInput.Player.Sprint.IsPressed());
		PlayStepSound();

		if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit newHit, interactDistance, interactLayers, QueryTriggerInteraction.Ignore)) {
			Interactable interact = newHit.collider.gameObject.GetComponent<Interactable>();

			if (newHit.collider.gameObject != prevHit && !interact.used) {
				prevHit = interact.gameObject;
				PlayerEvents.OnDisplayHint(Consts.Hints.INTERACT_HINT + interact.itemName);
			}
		} else {
			if (prevHit != null) {
				prevHit = null;
				PlayerEvents.OnDisplayHint(string.Empty);
			}
		}
	}

	private void PlayStepSound() {
		if (userInput.Player.Move.ReadValue<Vector2>() != Vector2.zero && soundQueued == false) {
			soundQueued = true;
			StartCoroutine(PlayWalkSound());
		}
	}

	private IEnumerator PlayWalkSound() {
		yield return new WaitForSeconds(isSprinting ? sprintingSoundDelay : walkingSoundDelay);
		soundQueued = false;
		playerWalkingSoundSource.Play();
		yield return null;
	}

	private void OnPause(InputAction.CallbackContext context) {
		PlayerEvents.OnTogglePauseMenu();
	}

	private void OnQuestViewStart(InputAction.CallbackContext context) {
		PlayerEvents.OnShowQuestsStart();
	}

	private void OnQuestViewStopped(InputAction.CallbackContext context) {
		PlayerEvents.OnShowQuestsStopped();
	}

	private void OnEscapeEnabled() {
		escapeEnabled = true;
	}

	private void OnInteract(InputAction.CallbackContext context) {
		if (prevHit == null)
			return;

		Interactable interactable = prevHit.GetComponent<Interactable>();
		lastInteracted = interactable;
		switch (interactable.interactType) {
		case InteractType.Door:
			if (doorKeys.Contains(interactable.doorCode)) {
				prevHit.GetComponent<Animator>().SetTrigger(Consts.Anims.OPEN);
				ClearInteract(interactable);
				PlayerEvents.OnResetMonsterInvalidPoints();
			} else {
				PlayerEvents.OnDisplayHint(Consts.Hints.DOOR_LOCKED);
			}
			break;
		case InteractType.Item:
			Debug.Log(interactable.itemName);
			items.Add(interactable.itemName);
			prevHit.SetActive(false);
			PlayerEvents.OnItemPickup(interactable.itemName);
			break;
		case InteractType.Key:
			prevHit.SetActive(false);
			doorKeys.Add(interactable.doorCode);
			playerKeyPickupSource.Play();
			break;
		case InteractType.Switch:
			prevHit.GetComponent<Animator>().SetTrigger(Consts.Anims.ON);
			interactable.doorToToggle.GetComponent<Animator>().SetTrigger(Consts.Anims.OPEN);
			interactable.gameObject.GetComponentInChildren<AudioSource>().Play();
			ClearInteract(interactable);
			PlayerEvents.OnResetMonsterInvalidPoints();
			break;
		case InteractType.Coffin:
			if (hidden) {
				hidden = false;
				prevHit.transform.position -= interactable.inCoffinOffset;
				interactable.coffinCam.gameObject.SetActive(false);
				playerCamera.gameObject.SetActive(true);
				PlayerCoffinInteractSource.Play();
			} else {
				hidden = true;
				prevHit.transform.position += interactable.inCoffinOffset;
				playerCamera.gameObject.SetActive(false);
				interactable.coffinCam.gameObject.SetActive(true);
				PlayerCoffinInteractSource.Play();
			}
			break;
		case InteractType.Escape:
			if (escapeEnabled) {
				TogglePlayerInput(false);
				ToggleUIInput(false);
				PlayerEvents.OnToggleEscapeMenu();
				PlayerEvents.OnDisplayHint(string.Empty);
				PlayerEscapeSource.Play();
			} else {
				PlayerEvents.OnDisplayHint(Consts.Hints.CANT_ESCAPE_YET);
			}
			break;
		default:
			Debug.Log(interactable.itemName);
			Debug.LogWarning("Interactable type not found");
			break;
		}
	}

	private void TogglePlayerInput(bool enable) {
		if (enable)
			userInput.Player.Enable();
		else
			userInput.Player.Disable();
	}

	private void ToggleUIInput(bool enable) {
		if (enable)
			userInput.UI.Enable();
		else
			userInput.UI.Disable();
	}

	private void UpdateSensitivity() {
		sensitivity = Save.GetData().sensitivity;
	}

	private void UpdateInvertLook() {
		invertLook = Save.GetData().invertLook;
	}

	private void Looking(Vector2 axisInput) {
		axisInput *= sensitivity;

		xRotation += invertLook ? axisInput.y : -axisInput.y;
		xRotation = Mathf.Clamp(xRotation, minLookAngle, maxLookAngle);

		playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
		transform.Rotate(Vector3.up * axisInput.x);
	}

	private void Moving(Vector2 axisInput) {
		isGrounded = Physics.CheckSphere(transform.position + (Vector3.down * groundOffset), checkSphereRadius, groundMask);
		Vector3 gravity = isGrounded ? Vector3.zero : Vector3.up * Consts.Physics.GRAVITY;

		transform.rotation = Quaternion.Euler(0f, playerCamera.transform.eulerAngles.y, 0f);
		Vector3 movement = transform.right * axisInput.x + transform.forward * axisInput.y;
		Vector3 finalMovement = (movement * (movementBaseSpeed + (isSprinting ? sprintingAddedSpeed : 0)) + gravity) * Time.fixedDeltaTime;

		characterController.Move(finalMovement);
	}

	private void Sprinting(bool buttonInput) {
		stamina = Mathf.Clamp(stamina, 0, 100);
		CheckSprintRecharge();

		if (buttonInput && sprintingEnabled) {
			stamina -= staminaDrainRate * Time.fixedDeltaTime;
			isSprinting = true;
		} else {
			isSprinting = false;
			stamina += staminaRegenRate * Time.fixedDeltaTime;
		}
	}

	private void CheckSprintRecharge() {
		if (stamina <= 0) {
			sprintingEnabled = false;
		}

		if (stamina >= sprintEnableValue) {
			sprintingEnabled = true;
		}
	}

	private void ClearInteract(Interactable interact) {
		interact.used = true;
		prevHit = null;
		PlayerEvents.OnDisplayHint(string.Empty);
	}

#if UNITY_EDITOR
	private void OnDrawGizmosSelected() {
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(transform.position + (Vector3.down * groundOffset), checkSphereRadius);

		Gizmos.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * interactDistance);
	}
#endif

}
