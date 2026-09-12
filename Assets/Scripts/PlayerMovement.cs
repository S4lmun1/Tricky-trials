using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
	[Header("Movement")]
	public float movementSpeed = 10f;
    [Tooltip("Zero friction prevents wall sticking; movement braking is controlled by Deceleration.")]
    public PhysicsMaterial movementMaterial;
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

	[Header("Input (legacy Input Manager axis names)")]
    public string horizontalAxis = "Horizontal";
    public string verticalAxis = "Vertical";
    public string mouseXAxis = "Mouse X";
    public string mouseYAxis = "Mouse Y";
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode unlockCursorKey = KeyCode.Escape;
    public KeyCode lockCursorKey = KeyCode.Mouse0;
    [Header("Look Limits")]
    public float minimumPitch = -90f;
    public float maximumPitch = 90f;
    [Header("Ground and Legacy Course Triggers")]
    [Range(0f, 1f)] public float groundNormalThreshold = 0.5f;
    public string hazardTag = "Obstacle";
    public string scoreTag = "Obstacle";
    public bool reloadOnHazard = true;
    public bool scoreOnTrigger = true;
    bool controlsEnabled = true;
    public bool ControlsEnabled => enabled && controlsEnabled;
    public Quaternion ViewYawRotation => Quaternion.Euler(0f, cameraYaw, 0f);
    readonly System.Collections.Generic.HashSet<Collider> groundContacts = new System.Collections.Generic.HashSet<Collider>();
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
        if (movementMaterial != null)
            foreach (var collider in GetComponents<Collider>()) collider.sharedMaterial = movementMaterial;
        if (playerCamera == null)
        {
            var camera = GetComponentInChildren<Camera>();
            if (camera != null) playerCamera = camera.transform;
        }
        if (playerCamera == null)
        {
            Debug.LogError("Assign a Player Camera to PlayerMovement.", this);
            enabled = false;
            return;
        }

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
        if (!controlsEnabled) return;
		// --------------------
		// WASD INPUT
		// --------------------

		horizontalInput = Input.GetAxisRaw(horizontalAxis);
		verticalInput = Input.GetAxisRaw(verticalAxis);


		// --------------------
		// JUMP
		// --------------------

		if (Input.GetKeyDown(jumpKey) && isGrounded)
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
				Input.GetAxisRaw(mouseXAxis) *
				Mathf.Clamp(lookSensitivity, 0.1f, 5f);

			float mouseY =
				Input.GetAxisRaw(mouseYAxis) *
				Mathf.Clamp(lookSensitivity, 0.1f, 5f);


			cameraYaw = Mathf.Repeat(cameraYaw + mouseX, 360f);


			cameraRotation -= mouseY;

			cameraRotation = Mathf.Clamp(
				cameraRotation,
				minimumPitch,
				maximumPitch
			);
		}


		// --------------------
		// Mouse axes already represent frame displacement; no deltaTime scaling.
		skipLookFrame = false;

		// UNLOCK MOUSE
		// --------------------

		if (Input.GetKeyDown(unlockCursorKey))
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}


		// --------------------
		// LOCK MOUSE AGAIN
		// --------------------

		if (Input.GetKeyDown(lockCursorKey) && Cursor.lockState != CursorLockMode.Locked)
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
			skipLookFrame = true;
		}
	}


    void LateUpdate()
    {
        // Apply the final view once per rendered frame, after movement updates.
        if (playerCamera != null) playerCamera.rotation = Quaternion.Euler(cameraRotation, cameraYaw, 0f);
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

		if (Input.GetKey(sprintKey))
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
        bool supportsPlayer = false;
        foreach (var contact in collision.contacts)
            if (Vector3.Dot(contact.normal, Vector3.up) > groundNormalThreshold) { supportsPlayer = true; break; }
        if (supportsPlayer) groundContacts.Add(collision.collider);
        else groundContacts.Remove(collision.collider);
        isGrounded = groundContacts.Count > 0;
    }

    private void OnCollisionExit(Collision collision)
    {
        groundContacts.Remove(collision.collider);
        isGrounded = groundContacts.Count > 0;
    }

    public void TeleportTo(Transform destination)
    {
        if (destination == null || rb == null) return;
        rb.position = destination.position;
        transform.position = destination.position;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        cameraYaw = destination.eulerAngles.y;
        cameraRotation = 0f;
        groundContacts.Clear();
        isGrounded = false;
        skipLookFrame = true;
        if (playerCamera != null) playerCamera.rotation = Quaternion.Euler(cameraRotation, cameraYaw, 0f);
    }

    public void SetControlsEnabled(bool value)
    {
        controlsEnabled = value;
        horizontalInput = verticalInput = 0f;
        skipLookFrame = true;
        if (!value && rb != null) rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
    }



	// --------------------
    // SCORE
    // --------------------
    private void OnTriggerEnter(Collider other)
    {
        if (scoreOnTrigger && !string.IsNullOrEmpty(scoreTag) && other.tag == scoreTag)
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
		if (reloadOnHazard && !string.IsNullOrEmpty(hazardTag) && collision.gameObject.tag == hazardTag)
		{
			SceneManager.LoadScene(
				SceneManager.GetActiveScene().name
			);
		}
	}
}