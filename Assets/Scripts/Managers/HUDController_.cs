using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HUDController_ : MonoBehaviour
{
    public TMP_Text turnCounter;
    public TMP_Text comboCounter;
    public PotionBoard potionBoard;

    [SerializeField] private BattleControler battleControler;
    [SerializeField] private Player player;
    [SerializeField] private Slider playerLife;

    void Update()
    {
        if (potionBoard != null)
        {
            turnCounter.text = "Turnos: " + potionBoard.totalTurns;
            comboCounter.text = "Combos: " + potionBoard.totalCombos;
            playerLife.value = player.GetHealth();
        }
    }
}