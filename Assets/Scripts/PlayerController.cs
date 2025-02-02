using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour {
	[Header("References")]
	public CharacterController characterController;
	public InputSystem_Actions userInput;
	public Camera playerCamera;

	[Header("Looking")]
	public float sensitivity; //1f
	
	public float minLookAngle, maxLookAngle; //-89f, 89f
	private float xRotation;
	public bool invertLook;
	
	[Header("Movement")]
	public float movementBaseSpeed; //5f
	public float sprintingAddedSpeed; //2.5f
	private bool isSprinting;
	
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
		isSprinting = Sprinting(userInput.Player.Sprint.IsPressed());
	}

	private void FixedUpdate() {
		Moving(userInput.Player.Move.ReadValue<Vector2>());
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

	private bool Sprinting(bool buttonInput) {
		if (buttonInput) {
			return true;
		}
		return false;
	}

	private void OnDrawGizmosSelected() {
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(transform.position + (Vector3.down * groundOffset), checkSphereRadius);
	}
	/*TODO:
	 * Add Interaction
	 * Add Jumping?
	 * Add Stamina
	 * Crouching?
	*/
}
