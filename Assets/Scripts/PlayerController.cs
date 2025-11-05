using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float jumpForce = 7f;
    public float acceleration = 10f;
    public float maxVelocity = 10f;
    public float rotationSpeed = 3f;

    private Rigidbody rb;
    private bool isGrounded = true;
    private Vector3 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // Movement direction relative to player facing
        moveInput = (transform.forward * vertical + transform.right * horizontal).normalized;

        // Rotate player with mouse when right button held
        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
            transform.Rotate(Vector3.up * mouseX);
        }

        // Jump
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    void FixedUpdate()
    {
        Vector3 targetVelocity = moveInput * moveSpeed;
        Vector3 velocityChange = targetVelocity - new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        velocityChange = Vector3.ClampMagnitude(velocityChange, acceleration * Time.fixedDeltaTime);

        rb.AddForce(velocityChange, ForceMode.VelocityChange);

        // Cap horizontal velocity
        Vector3 horizontalVel = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        if (horizontalVel.magnitude > maxVelocity)
        {
            horizontalVel = horizontalVel.normalized * maxVelocity;
            rb.linearVelocity = new Vector3(horizontalVel.x, rb.linearVelocity.y, horizontalVel.z);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.contacts.Length > 0 && collision.contacts[0].normal.y > 0.5f)
            isGrounded = true;
    }
}
