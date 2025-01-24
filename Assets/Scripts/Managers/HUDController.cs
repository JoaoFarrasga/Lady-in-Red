using UnityEngine;
using TMPro;

public class HUDController : MonoBehaviour
{
    public TMP_Text turnCounter;
    public TMP_Text comboCounter;
    public PotionBoard potionBoard;

    [SerializeField] private BattleControler battleControler;
    [SerializeField] private Player player; 
    [SerializeField] private TMP_Text playerLife;

    void Update()
    {
        if (potionBoard != null)
        {
            turnCounter.text = "Turnos: " + potionBoard.totalTurns;
            comboCounter.text = "Combos: " + potionBoard.totalCombos;
            playerLife.text = "Life: " + player.GetHealth();
        }
    }
}
