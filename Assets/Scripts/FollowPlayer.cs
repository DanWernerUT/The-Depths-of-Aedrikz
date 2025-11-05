using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public GameObject player;
    public Vector3 offset = new Vector3(0, 10, -20);
    public float rotationSpeed = 3f;

    private float yaw;
    private float pitch;

    void LateUpdate()
    {
        if (player == null) return;

        if (Input.GetMouseButton(1)) // right mouse held
        {
            yaw += Input.GetAxis("Mouse X") * rotationSpeed;
            pitch -= Input.GetAxis("Mouse Y") * rotationSpeed;
            pitch = Mathf.Clamp(pitch, -30f, 60f);
        }

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        Vector3 targetPos = player.transform.position + rotation * offset;

        transform.position = targetPos;
        transform.LookAt(player.transform.position + Vector3.up * 2f);
    }
}
