using UnityEngine;
using System.Collections.Generic;

public class Room : MonoBehaviour
{
    [Header("Wall Objects")]
    [Tooltip("List of wall GameObjects that will be removed if they intersect with neighbor walls")]
    public List<GameObject> walls = new List<GameObject>();

    [Header("Detection Settings")]
    [Tooltip("Radius for detecting intersecting walls")]
    public float detectionRadius = 0.5f;

    void Start()
    {
        // Small delay to ensure all rooms are spawned
        Invoke(nameof(CheckAndRemoveIntersectingWalls), 0.1f);
    }

    void CheckAndRemoveIntersectingWalls()
    {
        foreach (var wall in walls)
        {
            if (wall == null || !wall.activeSelf) continue;

            // Check if this wall intersects with any other room's wall
            Collider[] hits = Physics.OverlapSphere(wall.transform.position, detectionRadius);

            foreach (var hit in hits)
            {
                // Skip if it's part of this room
                if (hit.transform.IsChildOf(transform))
                    continue;

                // Check if hit belongs to another room's wall
                Room otherRoom = hit.GetComponentInParent<Room>();
                if (otherRoom != null && otherRoom != this && otherRoom.walls.Contains(hit.gameObject))
                {
                    // Both walls are intersecting - delete both
                    Destroy(wall);
                    Destroy(hit.gameObject);

                    // Remove from the other room's list to prevent double-processing
                    otherRoom.walls.Remove(hit.gameObject);
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Call this to manually check walls again (if rooms move after generation)
    /// </summary>
    public void RefreshWalls()
    {
        CheckAndRemoveIntersectingWalls();
    }
}