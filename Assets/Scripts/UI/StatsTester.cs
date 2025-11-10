using UnityEngine;
using UnityEngine.UI;

public class PlayerStatsTester : MonoBehaviour
{
    public PlayerStats player;
    public Button resetHealthBtn;
    public Button resetManaBtn;
    public Button loseHealthBtn;
    public Button gainHealthBtn;
    public Button spendManaBtn;
    public Button gainManaBtn;

    void Start()
    {
        resetHealthBtn.onClick.AddListener(player.ResetHealth);
        resetManaBtn.onClick.AddListener(player.ResetMana);
        loseHealthBtn.onClick.AddListener(() => player.LooseHealth(10f));
        gainHealthBtn.onClick.AddListener(() => player.GainHealth(10f));
        spendManaBtn.onClick.AddListener(() => player.SpendMana(10f));
        gainManaBtn.onClick.AddListener(() => player.GainMana(10f));
    }
}
