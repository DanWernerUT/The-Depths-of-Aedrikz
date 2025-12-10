using UnityEngine;
using UnityEngine.UI;

public class PlayerStatsTester : MonoBehaviour
{
    public ManaScript player;
    public HealthScript health;
    public Button resetHealthBtn;
    public Button resetManaBtn;
    public Button loseHealthBtn;
    public Button gainHealthBtn;
    public Button spendManaBtn;
    public Button gainManaBtn;

    void Start()
    {
        resetHealthBtn.onClick.AddListener(health.ResetHealth);
        loseHealthBtn.onClick.AddListener(() => health.LoseHealth(10f));
        gainHealthBtn.onClick.AddListener(() => health.GainHealth(10f));
        resetManaBtn.onClick.AddListener(player.ResetMana);
        spendManaBtn.onClick.AddListener(() => player.SpendMana(10f));
        gainManaBtn.onClick.AddListener(() => player.GainMana(10f));
    }
}
