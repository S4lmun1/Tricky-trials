using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
	[Header("Movement")]
	public float movementSpeed = 10f;
	public float sprintSpeed = 16f;

	// How quickly the player reaches full speed
	public float acceleration = 35f;

	// How quickly the player slows down after releasing WASD
	public float deceleration = 45f;

	[Header("Jump")]
	public float jumpForce = 8f;

	[Header("Mouse Look")]
	[Tooltip("Mouse look speed. Start at 1. This replaces the old 800-based setting.")]
	[Range(0.1f, 5f)]
	// Deliberately a new serialized field: old scenes contain mouseSensitivity = 800.
	public float lookSensitivity = 1f;
	public Transform playerCamera;

	[Header("Score")]
	public int score;
	public TextMeshProUGUI ScoreTxt;

	private Rigidbody rb;

	private float horizontalInput;
	private float verticalInput;

	private float cameraRotation = 0f;
	private float cameraYaw;
	private bool skipLookFrame;

	private bool isGrounded;


	void Start()
	{
		rb = GetComponent<Rigidbody>();

		// Makes Rigidbody movement visually smoother
		rb.interpolation = RigidbodyInterpolation.Interpolate;
        // Physics moves the body; mouse look only rotates the camera.
        rb.constraints |= RigidbodyConstraints.FreezeRotation;
        cameraYaw = playerCamera.eulerAngles.y;
        cameraRotation = Mathf.DeltaAngle(0f, playerCamera.eulerAngles.x);
        skipLookFrame = true;

		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}


	void Update()
	{
		// --------------------
		// WASD INPUT
		// --------------------

		horizontalInput = Input.GetAxisRaw("Horizontal");
		verticalInput = Input.GetAxisRaw("Vertical");


		// --------------------
		// JUMP
		// --------------------

		if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
		{
			rb.linearVelocity = new Vector3(
				rb.linearVelocity.x,
				jumpForce,
				rb.linearVelocity.z
			);

			isGrounded = false;
		}


		// --------------------
		// MOUSE LOOK
		// --------------------

		if (Cursor.lockState == CursorLockMode.Locked && !skipLookFrame)
		{
			float mouseX =
				Input.GetAxisRaw("Mouse X") *
				Mathf.Clamp(lookSensitivity, 0.1f, 5f);

			float mouseY =
				Input.GetAxisRaw("Mouse Y") *
				Mathf.Clamp(lookSensitivity, 0.1f, 5f);


			cameraYaw = Mathf.Repeat(cameraYaw + mouseX, 360f);


			cameraRotation -= mouseY;

			cameraRotation = Mathf.Clamp(
				cameraRotation,
				-90f,
				90f
			);
		}


		// --------------------
		// Mouse axes already represent frame displacement; no deltaTime scaling.
		skipLookFrame = false;

		// UNLOCK MOUSE
		// --------------------

		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}


		// --------------------
		// LOCK MOUSE AGAIN
		// --------------------

		if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked)
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
			skipLookFrame = true;
		}
	}


    void LateUpdate()
    {
        // Apply the final view once per rendered frame, after movement updates.
        playerCamera.rotation = Quaternion.Euler(cameraRotation, cameraYaw, 0f);
    }

    void OnApplicationFocus(bool hasFocus)
    {
        // Discard the first mouse delta after returning to the Game view.
        skipLookFrame = true;
    }

	void FixedUpdate()
	{
		// --------------------
		// MOVEMENT DIRECTION
		// --------------------

		Vector3 moveDirection =
			(Quaternion.Euler(0f, cameraYaw, 0f) * Vector3.right) * horizontalInput +
			(Quaternion.Euler(0f, cameraYaw, 0f) * Vector3.forward) * verticalInput;

		moveDirection = moveDirection.normalized;


		// --------------------
		// WALK / SPRINT
		// --------------------

		float currentSpeed = movementSpeed;

		if (Input.GetKey(KeyCode.LeftShift))
		{
			currentSpeed = sprintSpeed;
		}


		// Velocity we WANT to reach
		Vector3 targetVelocity =
			moveDirection * currentSpeed;


		// Current horizontal velocity
		Vector3 currentVelocity = new Vector3(
			rb.linearVelocity.x,
			0f,
			rb.linearVelocity.z
		);


		// --------------------
		// ACCELERATION
		// --------------------

		float speedChange;

		if (moveDirection.magnitude > 0.1f)
		{
			speedChange = acceleration;
		}
		else
		{
			speedChange = deceleration;
		}


		// Smoothly move current speed toward desired speed
		Vector3 smoothVelocity = Vector3.MoveTowards(
			currentVelocity,
			targetVelocity,
			speedChange * Time.fixedDeltaTime
		);


		rb.linearVelocity = new Vector3(
			smoothVelocity.x,
			rb.linearVelocity.y,
			smoothVelocity.z
		);
	}


	// --------------------
	// GROUND CHECK
	// --------------------

	private void OnCollisionStay(Collision collision)
	{
		foreach (ContactPoint contact in collision.contacts)
		{
			if (Vector3.Dot(contact.normal, Vector3.up) > 0.5f)
			{
				isGrounded = true;
				break;
			}
		}
	}


	private void OnCollisionExit(Collision collision)
	{
		isGrounded = false;
	}


	// --------------------
    // SCORE
    // --------------------
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle"))
        {
            score++;
            if (ScoreTxt != null) ScoreTxt.text = score.ToString();
        }
    }

    // --------------------
	// RESPAWN
	// --------------------

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.CompareTag("Obstacle"))
		{
			SceneManager.LoadScene(
				SceneManager.GetActiveScene().name
			);
		}
	}
}