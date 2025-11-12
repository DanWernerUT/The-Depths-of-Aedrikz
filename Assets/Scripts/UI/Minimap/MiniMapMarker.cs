using UnityEngine;

public class MiniMapMarker : MonoBehaviour {
    public Transform player;
    public float heightOffset = 5f;
    public float followSpeed = 10f;

    void LateUpdate()
    {
        if (!player) return;

        Vector3 targetPos = player.position + Vector3.up * heightOffset;
        transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);
    }
}
