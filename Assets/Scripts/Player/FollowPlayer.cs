using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public GameObject player;
    public Vector3 offset = new Vector3(0, 10, -20);
    public Vector3 firstPersonOffset = new Vector3(0, 1.6f, 0);
    public float rotationSpeed = 3f;

    private float yaw;
    private float pitch;
    private bool isFirstPerson = true;

    void Start()
    {

    }

    void LateUpdate()
    {
        if (player == null) return;

        if (Input.GetKeyDown(KeyCode.F5)) {
            isFirstPerson = !isFirstPerson;
        }

        if (Input.GetMouseButton(1) || isFirstPerson) {
            yaw += Input.GetAxis("Mouse X") * rotationSpeed;
            pitch -= Input.GetAxis("Mouse Y") * rotationSpeed;
            pitch = Mathf.Clamp(pitch, -30f, 60f);
        }

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

        if (isFirstPerson) {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            player.transform.rotation = Quaternion.Euler(0, yaw, 0);
            transform.position = player.transform.position + player.transform.TransformVector(firstPersonOffset);
            transform.rotation = Quaternion.Euler(pitch, yaw, 0);


        }
        else {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            Vector3 targetPos = player.transform.position + rotation * offset;
            transform.position = targetPos;
            transform.LookAt(player.transform.position + Vector3.up * 2f);
        }
    }
}
