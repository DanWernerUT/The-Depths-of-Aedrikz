using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject projectile;
    public float castDistance = 2f;
    public float castHeight = 2f;

    void Update()
    {
        if (GameState.paused) return;
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 spawnPos =
                transform.position +
                transform.forward * castDistance +
                Vector3.up * castHeight;

            Instantiate(projectile, spawnPos, transform.rotation);
        }
    }
}
