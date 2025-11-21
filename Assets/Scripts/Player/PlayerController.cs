using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 6f;
    public float jumpForce = 7f;
    public float mouseSensitivity = 500f;

    private Rigidbody rb;
    private float yaw;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;                 // Prevent tilt
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        // Yaw rotation
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        yaw += mouseX;
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);

        // Jump only when grounded
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // grounded check = vertical velocity ~= 0
            if (Mathf.Abs(rb.linearVelocity.y) < 0.001f)
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    private void FixedUpdate()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 move = (transform.forward * v + transform.right * h).normalized;
        
        Vector3 vel = rb.linearVelocity;
        Vector3 horizontalVel = move * moveSpeed;

        rb.linearVelocity = new Vector3(horizontalVel.x, vel.y, horizontalVel.z);
    }
}
