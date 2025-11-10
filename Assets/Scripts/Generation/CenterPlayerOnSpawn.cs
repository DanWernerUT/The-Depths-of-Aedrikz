using UnityEngine;

public class CenterPlayerOnSpawn : MonoBehaviour
{
    [Tooltip("The player object to center in this prefab.")]
    public Transform player;

    [Tooltip("Optional height offset above the prefab center.")]
    public float heightOffset = 1f;

    void Start()
    {
        if (!player)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }

        if (!player) return;
        Vector3 center = GetPrefabCenter();
        player.position = center + Vector3.up * heightOffset;
    }

    Vector3 GetPrefabCenter()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return transform.position;

        Bounds bounds = renderers[0].bounds;
        foreach (Renderer r in renderers)
            bounds.Encapsulate(r.bounds);

        return bounds.center;
    }
}
