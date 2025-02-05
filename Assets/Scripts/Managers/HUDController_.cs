using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HUDController_ : MonoBehaviour
{
    public TMP_Text comboCounter;
    public PotionBoard potionBoard;

    [SerializeField] private BattleControler battleControler;
    [SerializeField] private Player player;
    [SerializeField] private Slider playerLife;

    void Update()
    {
        if (potionBoard != null)
        {
            comboCounter.text = potionBoard.totalCombos.ToString();
            playerLife.value = player.GetHealth();
        }
    }

    public Slider GetPlayerLifeSlider() { return playerLife; }
}