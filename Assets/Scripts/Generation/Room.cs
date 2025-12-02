using UnityEngine;
using System.Collections.Generic;

public class Room : MonoBehaviour
{
    [Header("Wall Objects")]
    public List<GameObject> walls = new List<GameObject>();

    [Header("Detection Settings")]
    public float detectionRadius = 0.5f;

    [Header("Culling Settings")]
    public Transform player;
    public RoomPathGenerator pathGenerator;
    public float activeRadius = 0f;
    public float updateInterval = 0f;

    private float updateTimer;
    private float sqrActiveRadius;
    private float actualActiveRadius;
    private float actualUpdateInterval;
    private Renderer[] renderers;
    private Collider[] colliders;
    private bool isActive = true;
    private bool wallsChecked = false;

    void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
            else
                Debug.LogWarning("[Room] No player found! Culling will not work.");
        }

        if (pathGenerator == null)
        {
            pathGenerator = FindObjectOfType<RoomPathGenerator>();
        }

        if (pathGenerator != null)
        {
            actualActiveRadius = activeRadius > 0 ? activeRadius : pathGenerator.GetActiveRadius();
        }
        else
        {
            actualActiveRadius = activeRadius > 0 ? activeRadius : 100f;
            actualUpdateInterval = updateInterval > 0 ? updateInterval : 0.5f;
            Debug.LogWarning("[Room] RoomPathGenerator not found. Using local or default values.");
        }

        sqrActiveRadius = actualActiveRadius * actualActiveRadius;
        renderers = GetComponentsInChildren<Renderer>(true);
        colliders = GetComponentsInChildren<Collider>(true);

        Invoke(nameof(CheckAndRemoveIntersectingWalls), 0.1f);
    }

    void Update()
    {
        if (player == null) return;

        updateTimer += Time.deltaTime;
        if (updateTimer < actualUpdateInterval) return;
        updateTimer = 0f;

        UpdateCullingState();
    }

    private void UpdateCullingState()
    {
        if (player == null) return;

        Vector3 offset = transform.position - player.position;
        float sqrDist = offset.sqrMagnitude;
        bool shouldBeActive = sqrDist < sqrActiveRadius;

        if (shouldBeActive != isActive)
        {
            SetRoomActive(shouldBeActive);
        }
    }

    private void SetRoomActive(bool active)
    {
        isActive = active;

        foreach (var renderer in renderers)
        {
            if (renderer != null)
                renderer.enabled = active;
        }

        foreach (var collider in colliders)
        {
            if (collider != null)
                collider.enabled = active;
        }
    }

    void CheckAndRemoveIntersectingWalls()
    {
        if (wallsChecked) return;
        wallsChecked = true;

        foreach (var wall in walls)
        {
            if (wall == null || !wall.activeSelf) continue;

            Collider[] hits = Physics.OverlapSphere(wall.transform.position, detectionRadius);

            foreach (var hit in hits)
            {
                if (hit.transform.IsChildOf(transform))
                    continue;

                Room otherRoom = hit.GetComponentInParent<Room>();
                if (otherRoom != null && otherRoom != this && otherRoom.walls.Contains(hit.gameObject))
                {
                    Destroy(wall);
                    Destroy(hit.gameObject);
                    otherRoom.walls.Remove(hit.gameObject);
                    break;
                }
            }
        }

        renderers = GetComponentsInChildren<Renderer>(true);
        colliders = GetComponentsInChildren<Collider>(true);
    }

    public void RefreshWalls()
    {
        wallsChecked = false;
        CheckAndRemoveIntersectingWalls();
    }

    public void ForceActive(bool active)
    {
        SetRoomActive(active);
    }

    public bool IsActive()
    {
        return isActive;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        float radiusToShow = actualActiveRadius > 0 ? actualActiveRadius : activeRadius > 0 ? activeRadius : 100f;
        Gizmos.DrawWireSphere(transform.position, radiusToShow);
    }
}