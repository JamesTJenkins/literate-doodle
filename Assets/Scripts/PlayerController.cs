using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour {

	public CharacterController characterController;
	public InputSystem_Actions userInput;
	public Camera playerCamera;

	[Header("Looking")]
	public float sensitivity = 1;
	public float minLookAngle = -89f, maxLookAngle = 89f;
	private float xRotation = 0f;
	public bool invertLook;
	[Header("Movement")]
	public float moveSpeed = 5f;
	public float groundOffset;
	public float checkSphereRadius;
	public LayerMask groundMask;
	public bool isGrounded;
	public bool isSprinting;


	private void Start() {
		userInput = new InputSystem_Actions();
		userInput.Player.Look.performed += Looking;
		userInput.Enable();

		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	private void OnDestroy() {
		userInput.Player.Look.performed -= Looking;
		userInput.Dispose();
	}

	private void FixedUpdate() {
		Moving(userInput.Player.Move.ReadValue<Vector2>());

		if (Physics.CheckSphere(transform.position + (Vector3.down * groundOffset), checkSphereRadius, groundMask)) {
			isGrounded = true;
			Debug.Log("Grounded");
		} else {
			isGrounded = false;
		}
	}

	private void Looking(InputAction.CallbackContext context) {
		Vector2 mouse = context.ReadValue<Vector2>();
		mouse *= sensitivity;

		xRotation += invertLook ? mouse.y : -mouse.y;
		xRotation = Mathf.Clamp(xRotation, minLookAngle, maxLookAngle);

		playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
		transform.Rotate(Vector3.up * mouse.x);
	}

	private void Moving(Vector2 input) {
		transform.rotation = Quaternion.Euler(0f, playerCamera.transform.eulerAngles.y, 0f);

		Vector3 gravity = isGrounded ? Vector3.zero : Vector3.up * Consts.Physics.GRAVITY;
		Vector3 movement = transform.right * input.x + transform.forward * input.y + gravity;
		Vector3 finalMovement = movement.normalized * moveSpeed * Time.fixedDeltaTime;

		characterController.Move(finalMovement);
	}
	/*TODO:
	 * Add Sprinting
	 * Add Interaction
	 * Add Jumping?
	 * Crouching?
	*/
}
