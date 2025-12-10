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
    public bool isOversizeRoom = false;

    [Header("Oversize Room Settings")]
    public Vector2 roomSize = new Vector2(1, 1); // In tiles

    private float updateTimer;
    private float sqrActiveRadius;
    private float actualActiveRadius;
    private float actualUpdateInterval;
    private Renderer[] renderers;
    private Collider[] colliders;
    private bool isActive = true;
    private bool wallsChecked = false;

    // Cache for oversize room check points
    private Vector3[] checkPoints;

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
            pathGenerator = FindFirstObjectByType<RoomPathGenerator>();
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

        // Calculate check points for oversize rooms
        if (isOversizeRoom)
        {
            CalculateCheckPoints();
        }

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

    private void CalculateCheckPoints()
    {
        // Get the room's bounds based on its size
        float tileSize = pathGenerator != null ? pathGenerator.GetTileSize() : 15f;
        float halfWidth = (roomSize.x * tileSize) / 2f;
        float halfHeight = (roomSize.y * tileSize) / 2f;

        Vector3 center = transform.position;

        checkPoints = new Vector3[]
        {
            // Center
            center,
            
            // Four corners
            center + new Vector3(-halfWidth, 0, -halfHeight), // Bottom-left
            center + new Vector3(halfWidth, 0, -halfHeight),  // Bottom-right
            center + new Vector3(-halfWidth, 0, halfHeight),  // Top-left
            center + new Vector3(halfWidth, 0, halfHeight),   // Top-right
            
            // Four wall centers
            center + new Vector3(0, 0, -halfHeight),  // Bottom wall
            center + new Vector3(0, 0, halfHeight),   // Top wall
            center + new Vector3(-halfWidth, 0, 0),   // Left wall
            center + new Vector3(halfWidth, 0, 0)     // Right wall
        };
    }

    public void UpdateCullingState()
    {
        if (player == null) return;

        bool shouldBeActive;

        if (isOversizeRoom && checkPoints != null)
        {
            // Check if ANY of the check points are within range
            shouldBeActive = false;
            foreach (Vector3 point in checkPoints)
            {
                Vector3 offset = point - player.position;
                float sqrDist = offset.sqrMagnitude;
                if (sqrDist < sqrActiveRadius)
                {
                    shouldBeActive = true;
                    break;
                }
            }
        }
        else
        {
            // Normal room - check from center only
            Vector3 offset = transform.position - player.position;
            float sqrDist = offset.sqrMagnitude;
            shouldBeActive = sqrDist < sqrActiveRadius;
        }

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

    public void CheckAndRemoveIntersectingWalls()
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

        // Draw check points for oversize rooms
        if (isOversizeRoom && Application.isPlaying && checkPoints != null)
        {
            Gizmos.color = Color.cyan;
            foreach (Vector3 point in checkPoints)
            {
                Gizmos.DrawWireSphere(point, radiusToShow);
                Gizmos.DrawLine(transform.position, point);
            }
        }
    }
}