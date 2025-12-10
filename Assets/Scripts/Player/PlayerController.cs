using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 6f;
    public float sprintSpeed = 10f;
    public float jumpForce = 7f;
    public float fallMultiplier = 3f;

    private float MouseSensitivity => SensitivitySettings.sensitivity;
    private Rigidbody rb;
    private float yaw;

    [SerializeField] private AudioClip jumpSFX;
    [SerializeField] private AudioClip[] walkingSFX;
    [SerializeField] private AudioClip[] sprintingSFX;
    [SerializeField] private float walkStepInterval = 0.5f;
    [SerializeField] private float sprintStepInterval = 0.3f;
    [SerializeField] private float walkingSFXVolume = 0.5f;
    [SerializeField] private float sprintingSFXVolume = 0.6f;

    private float stepTimer;
    private bool isSprinting = false;
    private bool isGrounded = true;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (GameState.paused)
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            return;
        }

        float mouseX = Input.GetAxis("Mouse X") * MouseSensitivity * Time.deltaTime;
        yaw += mouseX;
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 move = (transform.forward * v + transform.right * h).normalized;

        isSprinting = !Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = isSprinting ? sprintSpeed : moveSpeed;

        Vector3 vel = rb.linearVelocity;
        Vector3 horizontalVel = move * currentSpeed;
        rb.linearVelocity = new Vector3(horizontalVel.x, vel.y, horizontalVel.z);

        // Check if grounded
        bool isGrounded = Mathf.Abs(rb.linearVelocity.y) < 0.001f;

        // Handle walking/sprinting sounds
        bool isMoving = move.magnitude > 0.1f;
        if (isGrounded && isMoving)
        {
            AudioClip[] currentSFX = isSprinting ? sprintingSFX : walkingSFX;
            float currentInterval = isSprinting ? walkStepInterval : sprintStepInterval;
            float currentVolume = isSprinting ? sprintingSFXVolume : walkingSFXVolume;

            if (currentSFX.Length > 0)
            {
                stepTimer -= Time.deltaTime;
                if (stepTimer <= 0f)
                {
                    // Play random footstep sound
                    AudioClip randomStep = currentSFX[Random.Range(0, currentSFX.Length)];
                    SoundFXManager.instance.PlaySoundFXClip(randomStep, transform, currentVolume);
                    stepTimer = currentInterval;
                }
            }
        }
        else
        {
            // Reset timer when not moving
            stepTimer = 0f;
        }

        // Jump
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isGrounded)
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                SoundFXManager.instance.PlaySoundFXClip(jumpSFX, transform, 1f);
            }
        }
    }

    public void FixedUpdate()
    {
        if (rb.linearVelocity.y < 0)
        {
            rb.AddForce(Vector3.up * Physics.gravity.y * (fallMultiplier - 1), ForceMode.Acceleration);
        }
    }

    public bool GetSprinting()
    {
        return !isSprinting;
    }

    public bool GetIsGrounded()
    {
        return isGrounded;
    }
}