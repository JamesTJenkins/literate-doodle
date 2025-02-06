using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour {
	[Header("References")]
	public CharacterController characterController;
	public InputSystem_Actions userInput;
	public Camera playerCamera;

	[Header("Looking")]
	private float sensitivity;
	private bool invertLook;

	public float minLookAngle, maxLookAngle; //-89f, 89f
	private float xRotation;

	[Header("Movement")]
	public float movementBaseSpeed; //5f
	public float sprintingAddedSpeed; //2.5f
	private bool isSprinting;

	private float stamina = 100; 
	public float staminaDrainRate; //10f
	public float staminaRegenRate; //5f

	public bool sprintingEnabled;
	public float sprintEnableValue; //50f

	private bool isGrounded;
	public LayerMask groundMask;
	public float checkSphereRadius;
	public float groundOffset;

	private void Start() {
		userInput = new InputSystem_Actions();
		userInput.UI.Pause.performed += OnPause;
		userInput.Enable();

		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;

		UpdateSensitivity();
		PlayerEvents.updateSensitivity += UpdateSensitivity;
		PlayerEvents.togglePlayerInput += TogglePlayerInput;
		stamina = 100f;
		sprintingEnabled = true;
	}

	private void OnDestroy() {
		userInput.Disable();
		userInput.UI.Pause.performed -= OnPause;
		userInput.Dispose();

		PlayerEvents.updateSensitivity -= UpdateSensitivity;
		PlayerEvents.togglePlayerInput -= TogglePlayerInput;
	}

	private void Update() {
		Looking(userInput.Player.Look.ReadValue<Vector2>());
	}

	private void FixedUpdate() {
		Moving(userInput.Player.Move.ReadValue<Vector2>());
		Sprinting(userInput.Player.Sprint.IsPressed());
	}

	private void OnPause(InputAction.CallbackContext context) {
		PlayerEvents.OnTogglePauseMenu();
	}

	private void TogglePlayerInput(bool enable) {
		if (enable)
			userInput.Player.Enable();
		else
			userInput.Player.Disable();
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

	private void OnDrawGizmosSelected() {
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(transform.position + (Vector3.down * groundOffset), checkSphereRadius);
	}


}
