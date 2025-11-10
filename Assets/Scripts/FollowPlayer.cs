using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public GameObject player;
    public Vector3 offset = new Vector3(0, 10, -20);
    public Vector3 firstPersonOffset = new Vector3(0, 1.6f, 0); // Eye level offset
    public float rotationSpeed = 3f;

    private float yaw;
    private float pitch;
    private bool isFirstPerson = false;

    void Start()
    {

    }

    void LateUpdate()
    {
        if (player == null) return;

        // Toggle between first and third person
        if (Input.GetKeyDown(KeyCode.F5))
        {
            isFirstPerson = !isFirstPerson;
        }

        if (Input.GetMouseButton(1)) // right mouse held
        {
            yaw += Input.GetAxis("Mouse X") * rotationSpeed;
            pitch -= Input.GetAxis("Mouse Y") * rotationSpeed;
            pitch = Mathf.Clamp(pitch, -30f, 60f);
        }

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

        if (isFirstPerson)
        {
            // First person mode - camera at player's head
            Vector3 targetPos = player.transform.position + player.transform.rotation * firstPersonOffset;
            transform.position = targetPos;
            transform.rotation = rotation;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            // Third person mode - camera behind player
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Vector3 targetPos = player.transform.position + rotation * offset;
            transform.position = targetPos;
            transform.LookAt(player.transform.position + Vector3.up * 2f);
        }
    }
}
