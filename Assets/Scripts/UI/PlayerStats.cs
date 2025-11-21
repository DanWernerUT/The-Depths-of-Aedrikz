using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    public float maxHealth = 100f;
    public float maxMana = 50f;
    public float currentHealth;
    public float currentMana;
    public Slider healthBar;
    public Slider manaBar;
    [SerializeField] private AudioClip damageSoundClip;
    void Start()
    {
        ResetHealth();
        ResetMana();
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        healthBar.maxValue = maxHealth;
        healthBar.value = currentHealth;
        Debug.Log($"[Health Reset] Health = {currentHealth}/{maxHealth}");
    }

    public void ResetMana()
    {
        currentMana = maxMana;
        manaBar.maxValue = maxMana;
        manaBar.value = currentMana;
        Debug.Log($"[Mana Reset] Mana = {currentMana}/{maxMana}");
    }

    public void LoseHealth(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth - amount, 0, maxHealth);
        healthBar.value = currentHealth;
        SoundFXManager.instance.PlaySoundFXClip(damageSoundClip, transform, 1f);
        Debug.Log($"[Lose Health] -{amount} ? {currentHealth}/{maxHealth}");
    }

    public void GainHealth(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        healthBar.value = currentHealth;
        Debug.Log($"[Gain Health] +{amount} ? {currentHealth}/{maxHealth}");
    }

    public void SpendMana(float amount)
    {
        currentMana = Mathf.Clamp(currentMana - amount, 0, maxMana);
        manaBar.value = currentMana;
        Debug.Log($"[Spend Mana] -{amount} ? {currentMana}/{maxMana}");
    }

    public void GainMana(float amount)
    {
        currentMana = Mathf.Clamp(currentMana + amount, 0, maxMana);
        manaBar.value = currentMana;
        Debug.Log($"[Gain Mana] +{amount} ? {currentMana}/{maxMana}");
    }
}
