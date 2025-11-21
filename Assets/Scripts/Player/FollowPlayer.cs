using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public GameObject player;

    public Vector3 offset = new Vector3(0, 0, 0);
    public float rotationSpeed = 1f;
    [Header("Fixed Positioning")]
    public bool useFixedX = false;
    public bool useFixedY = false;
    public bool useFixedZ = false;
    public float fixedX = 0f;
    public float fixedY = 0f;  
    public float fixedZ = 0f;
    [Header("Fixed Rotation")]
    public bool useFixedXRotation = false;
    public bool useFixedYRotation = false;
    public bool useFixedZRotation = false;
    public float fixedXRotation = 0f;
    public float fixedYRotation = 0f;
    public float fixedZRotation = 0f;

    void LateUpdate()
    {
        if (player == null) return;
        
        Quaternion rotation = Quaternion.Euler(player.transform.eulerAngles.x, player.transform.eulerAngles.y, player.transform.eulerAngles.z);
        Vector3 targetPos = player.transform.position + rotation * offset;

        if (useFixedY) targetPos.y = fixedY;
        if (useFixedX) targetPos.x = fixedX;
        if (useFixedZ) targetPos.z = fixedZ;
        if (useFixedXRotation || useFixedYRotation || useFixedZRotation) {
            float rotX = useFixedXRotation ? fixedXRotation : transform.eulerAngles.x;
            float rotY = useFixedYRotation ? fixedYRotation : transform.eulerAngles.y;
            float rotZ = useFixedZRotation ? fixedZRotation : transform.eulerAngles.z;
            transform.rotation = Quaternion.Euler(rotX, rotY, rotZ);
        }
        else transform.LookAt(player.transform.position + Vector3.up * 2f);
        transform.position = targetPos;
    }
}
