using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    [Header("Player Settings")]
    public GameObject player;           // Assign your player GameObject

    [Header("Prefab Settings")]
    public GameObject prefabToSpawn;    // Prefab to spawn
    public Vector3 spawnOffset = Vector3.zero; // Optional spawn offset
    public bool spawnOnce = true;       // If true, prefab spawns only once

    [Header("Toggle Settings")]
    public GameObject objectToToggle;   // GameObject to toggle on/off

    private bool hasSpawned = false;    // Track if prefab has already spawned

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == player)
        {
            // Spawn prefab if not already spawned
            if (!hasSpawned || !spawnOnce)
            {
                SpawnPrefab();
                hasSpawned = true;
            }

            // Toggle object OFF
            if (objectToToggle != null)
                objectToToggle.SetActive(false);

            // Optional: animate plate down
            AnimatePlateDown();

            // Destroy the pressure plate after activating
            Destroy(gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == player)
        {
            // Animate plate up (optional, may not matter since plate will be destroyed)
            AnimatePlateUp();
        }
    }

    void SpawnPrefab()
    {
        if (prefabToSpawn != null)
        {
            Instantiate(prefabToSpawn, transform.position + spawnOffset, Quaternion.identity);
            Debug.Log("Prefab spawned!");
        }
    }

    void AnimatePlateDown()
    {
        transform.position -= new Vector3(0, 0.05f, 0);
    }

    void AnimatePlateUp()
    {
        transform.position += new Vector3(0, 0.05f, 0);
    }
}
