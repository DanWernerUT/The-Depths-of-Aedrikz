using UnityEngine;
using UnityEngine.UI;

public class ManaScript : MonoBehaviour
{
    [SerializeField] private float maxMana = 50f;
    [SerializeField] private float currentMana;
    [SerializeField] private Slider manaBar;
    [SerializeField] private AudioClip manaUpSoundClip;
    [SerializeField] private AudioClip manaDownSoundClip;
    void Start()
    {
        ResetMana();
    }

    public void ResetMana()
    {
        manaBar.maxValue = maxMana;
        SetMana(maxMana);
        SoundFXManager.instance.PlaySoundFXClip(manaUpSoundClip, transform, 1f);
        Debug.Log($"[Mana Reset] Mana = {currentMana}/{maxMana}");
    }

    public void SpendMana(float amount)
    {
        SetMana(GetMana() - amount);
        SoundFXManager.instance.PlaySoundFXClip(manaDownSoundClip, transform, 1f);
        Debug.Log($"[Spend Mana] -{amount} ? {currentMana}/{maxMana}");
    }

    public void GainMana(float amount)
    {
        SetMana(GetMana() + amount);
        SoundFXManager.instance.PlaySoundFXClip(manaUpSoundClip, transform, 1f);
        Debug.Log($"[Gain Mana] +{amount} ? {currentMana}/{maxMana}");
    }

    public float GetMana()
    {
        return currentMana;
    }

    private void SetMana(float amount)
    {
        currentMana = amount;
        manaBar.value = currentMana;
    }
}
