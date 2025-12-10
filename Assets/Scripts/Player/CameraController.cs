using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform player;
    private float pitch;

    [Header("Settings")]
    private float MouseSensitivity => SensitivitySettings.sensitivity;
    public bool enableHeadbob = true;

    [Header("Headbob")]
    public float walkingBobFrequency = 7.5f;
    public float sprintingBobFrequency = 15f;
    public float bobAmplitude = 0.25f;
    private float bobTimer = 0f;
    private Vector3 bobOffset;

    [Header("Camera Modes")]
    public bool firstPerson = true;
    public float firstPersonHeight = 2.5f;
    public float thirdPersonDistance = 4f;
    public float thirdPersonHeight = 1.7f;

    [Header("Smoothing")]
    public float positionSmoothing = 10f;

    private PlayerController playerController;

    void Awake()
    {
        playerController = FindAnyObjectByType<PlayerController>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
            firstPerson = !firstPerson;

        bool isMoving = Input.GetAxisRaw("Horizontal") != 0 ||
                Input.GetAxisRaw("Vertical") != 0;

        if (enableHeadbob && firstPerson && isMoving) { 
            if (playerController.GetSprinting() == false) 
                bobTimer += Time.deltaTime * walkingBobFrequency;
            else 
                bobTimer += Time.deltaTime * sprintingBobFrequency;
        }
        else
            bobTimer = Mathf.Lerp(bobTimer, 0f, Time.deltaTime * 5f);
    }

    void LateUpdate()
    {
        if (GameState.paused)
            return;

        float mouseY = Input.GetAxis("Mouse Y") * MouseSensitivity * Time.deltaTime;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -60f, 80f);
        bobOffset = Vector3.zero;

        if (enableHeadbob && firstPerson)
        {
            float bob = Mathf.Sin(bobTimer) * bobAmplitude;
            float sway = Mathf.Cos(bobTimer * 0.5f) * bobAmplitude * 0.5f;
            bobOffset = new Vector3(sway, bob, 0f);
        }

        if (firstPerson) 
            HandleFirstPerson();
        else 
            HandleThirdPerson();
    }

    private void HandleFirstPerson()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Vector3 targetPos = player.position + Vector3.up * firstPersonHeight;

        targetPos += bobOffset;
        transform.SetPositionAndRotation(
            Vector3.Lerp(transform.position, 
            targetPos, 
            positionSmoothing * Time.deltaTime), 
            Quaternion.Euler(pitch, player.eulerAngles.y, 0f)
        );
    }

    private void HandleThirdPerson()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        Vector3 pivot = player.position + Vector3.up * thirdPersonHeight;
        Vector3 offset = Quaternion.Euler(pitch, player.eulerAngles.y, 0f) * new Vector3(0, 0, -thirdPersonDistance);
        Vector3 targetPos = pivot + offset;
        
        // Smooth the position
        transform.position = Vector3.Lerp(
            transform.position, 
            targetPos, 
            positionSmoothing * Time.deltaTime
        );
        
        transform.LookAt(pivot);
    }
}