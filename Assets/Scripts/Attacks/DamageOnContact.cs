using UnityEngine;

public class DamageOnContact : MonoBehaviour
{
    [SerializeField] private float damageAmount = 10f;
    [SerializeField] private float damageInterval = 1f;

    private bool playerInRange = false;
    private float timer = 0f;
    private HealthScript targetHealth;

    void OnTriggerEnter(Collider other)
    {
        HealthScript health = other.GetComponent<HealthScript>();

        if (health != null)
        {
            targetHealth = health;
            playerInRange = true;
            timer = 0f; // reset so damage is instant on first touch if wanted
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<HealthScript>() == targetHealth)
        {
            playerInRange = false;
            targetHealth = null;
        }
    }

    void Update()
    {
        if (!playerInRange || targetHealth == null) return;

        timer += Time.deltaTime;

        if (timer >= damageInterval)
        {
            targetHealth.LoseHealth(damageAmount);
            timer = 0f;
        }
    }
}
