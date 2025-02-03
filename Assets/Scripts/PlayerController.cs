using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour {
	[Header("References")]
	public CharacterController characterController;
	public InputSystem_Actions userInput;
	public Camera playerCamera;

	[Header("Interact")]
	[SerializeField] private float interactDistance = 5f;
	[SerializeField] private LayerMask interactLayers;

	[Header("Looking")]
	public float sensitivity;
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

	private void Start() {
		userInput = new InputSystem_Actions();
		userInput.UI.Pause.performed += OnPause;
		userInput.Player.Interact.performed += OnInteract;
		userInput.Enable();

		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;

		UpdateSensitivity();
		PlayerEvents.updateSensitivity += UpdateSensitivity;
		PlayerEvents.togglePlayerInput += TogglePlayerInput;
		PlayerEvents.togglePlayerInput += ToggleUIInput;
		
		stamina = 100f;
		sprintingEnabled = true;
	}

	private void OnDestroy() {
		userInput.Disable();
		userInput.UI.Pause.performed -= OnPause;
		userInput.Player.Interact.performed -= OnInteract;
		userInput.Dispose();

		PlayerEvents.updateSensitivity -= UpdateSensitivity;
		PlayerEvents.togglePlayerInput -= TogglePlayerInput;
		PlayerEvents.togglePlayerInput -= ToggleUIInput;
	}

	private void Update() {
		Looking(userInput.Player.Look.ReadValue<Vector2>());
	}

	private void FixedUpdate() {
		Moving(userInput.Player.Move.ReadValue<Vector2>());
		Sprinting(userInput.Player.Sprint.IsPressed());

		if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit newHit, interactDistance, interactLayers, QueryTriggerInteraction.Ignore)) {
			if (newHit.collider.gameObject != prevHit) {
				prevHit = newHit.collider.gameObject;
				PlayerEvents.OnDisplayHint(Consts.Menu.INTERACT_HINT + prevHit.GetComponent<Interactable>().itemName);
			}
		} else {
			if (prevHit != null) {
				prevHit = null;
				PlayerEvents.OnDisplayHint(string.Empty);
			}
		}
	}

	private void OnPause(InputAction.CallbackContext context) {
		PlayerEvents.OnTogglePauseMenu();
	}

	private void OnInteract(InputAction.CallbackContext context) {
		if (prevHit == null)
			return;

		prevHit.SetActive(false);
		// TODO: actually have effects to interactions
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

	#if UNITY_EDITOR
	private void OnDrawGizmosSelected() {
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(transform.position + (Vector3.down * groundOffset), checkSphereRadius);
	}
	#endif

}
