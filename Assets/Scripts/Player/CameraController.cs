using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform player;

    [Header("Settings")]
    public float mouseSensitivity = 500f;

    [Header("Camera Modes")]
    public bool firstPerson = true;
    public float firstPersonHeight = 1.7f;

    public float thirdPersonDistance = 4f;
    public float thirdPersonHeight = 1.7f;

    private float pitch;

    void Update()
    {
        // Toggle modes
        if (Input.GetKeyDown(KeyCode.F5))
            firstPerson = !firstPerson;
    }

    void LateUpdate()
    {
        // Vertical look only
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -60f, 80f);

        if (firstPerson)
            HandleFirstPerson();
        else
            HandleThirdPerson();
    }

    private void HandleFirstPerson()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Vector3 headPos = player.position + Vector3.up * firstPersonHeight;

        transform.position = headPos;
        transform.rotation = Quaternion.Euler(pitch, player.eulerAngles.y, 0f);
    }

    private void HandleThirdPerson()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Vector3 pivot = player.position + Vector3.up * thirdPersonHeight;

        Vector3 offset =
            Quaternion.Euler(pitch, player.eulerAngles.y, 0f) *
            new Vector3(0, 0, -thirdPersonDistance);

        transform.position = pivot + offset;
        transform.LookAt(pivot);
    }
}
