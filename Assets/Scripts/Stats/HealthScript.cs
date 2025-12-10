using UnityEngine;
using UnityEngine.UI;

public class HealthScript : MonoBehaviour
{
    [SerializeField] private float maxHealth = 50f;
    [SerializeField] private float currentHealth;
    [SerializeField] private Slider HealthBar;
    [SerializeField] private AudioClip HealthUpSoundClip;
    [SerializeField] private AudioClip HealthDownSoundClip;
    private bool isDead = false;
    void Start()
    {
        ResetHealth();
    }

    private void Update()
    {
        if (currentHealth <= 0 && !isDead)
        {
            isDead = true;
            Debug.Log($"{gameObject.name} has died.");

            AIController ai = GetComponent<AIController>();
            if (ai != null)
                ai.Die();
        }
    }

    public void ResetHealth()
    {
        HealthBar.maxValue = maxHealth;
        SetHealth(maxHealth);
        if (HealthUpSoundClip) SoundFXManager.instance.PlaySoundFXClip(HealthUpSoundClip, transform, 1f);
        Debug.Log($"[Health Reset] Health = {currentHealth}/{maxHealth}");
    }

    public void LoseHealth(float amount)
    {
        SetHealth(GetHealth() - amount);
        if (HealthDownSoundClip) SoundFXManager.instance.PlaySoundFXClip(HealthDownSoundClip, transform, 1f);
        Debug.Log($"[Spend Health] -{amount} ? {currentHealth}/{maxHealth}");
    }

    public void GainHealth(float amount)
    {
        SetHealth(GetHealth() + amount);
        if (HealthUpSoundClip) SoundFXManager.instance.PlaySoundFXClip(HealthUpSoundClip, transform, 1f);
        Debug.Log($"[Gain Health] +{amount} ? {currentHealth}/{maxHealth}");
    }

    public float GetHealth()
    {
        return currentHealth;
    }

    private void SetHealth(float amount)
    {
        currentHealth = amount;
        if(HealthBar != null) HealthBar.value = currentHealth;
    }
    public bool IsDead()
    {
        return isDead;
    }
}
