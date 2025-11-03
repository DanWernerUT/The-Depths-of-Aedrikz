using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    public float speed = 10f;
    public float turnSpeed = 90f;
    public float jumpForce = 7f;
    public float maxVelocity = 20f;

    private Rigidbody rb;
    private bool isGrounded = true;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // Keeps player upright
    }

    void Update()
    {
        float forwardInput = Input.GetAxis("Vertical");
        float horizontalInput = Input.GetAxis("Horizontal");

        // Forward/backward movement
        Vector3 move = transform.forward * forwardInput * speed;
        Vector3 newVelocity = new Vector3(move.x, rb.linearVelocity.y, move.z);
        rb.linearVelocity = Vector3.ClampMagnitude(newVelocity, maxVelocity);

        // Rotation
        transform.Rotate(Vector3.up * horizontalInput * turnSpeed * Time.deltaTime);

        // Jump
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Simple grounded check
        if (collision.contacts.Length > 0 && collision.contacts[0].normal.y > 0.5f)
            isGrounded = true;
    }
}
