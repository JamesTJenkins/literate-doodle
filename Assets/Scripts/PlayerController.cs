using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour {
	[Header("Camera")]
	[SerializeField] private Camera playerCamera;
	[SerializeField] private float sensitivity = 1;
	[SerializeField] private float maxLookDown = -89f;
	[SerializeField] private float maxLookUp = 89f;
	[Header("Movement")]
	[SerializeField] private float speed = 5f;

	private CharacterController cc;
	private InputSystem_Actions userInput = null;
	private float xRotation = 0f;

	private void Start() {
		userInput = new InputSystem_Actions();
		userInput.Player.Look.performed += Looking;
		userInput.Enable();

		cc = GetComponent<CharacterController>();

		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	private void OnDestroy() {
		userInput.Player.Look.performed -= Looking;
		userInput.Dispose();
	}

	private void FixedUpdate() {
		Vector2 input = userInput.Player.Move.ReadValue<Vector2>();

		transform.rotation = Quaternion.Euler(0, playerCamera.transform.eulerAngles.y, 0);

		Vector3 moveVector = new Vector3(input.x, 0f, input.y);
		Vector3 finalMovement = (transform.right * moveVector.x + transform.forward * moveVector.z) * speed * Time.fixedDeltaTime;

		cc.Move(finalMovement);
	}

	private void Looking(InputAction.CallbackContext context) {
		Vector2 mouse = context.ReadValue<Vector2>();
		mouse *= sensitivity;

		xRotation -= mouse.y;
		xRotation = Mathf.Clamp(xRotation, maxLookDown, maxLookUp);

		playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
		transform.Rotate(Vector3.up * mouse.x);
	}
}
