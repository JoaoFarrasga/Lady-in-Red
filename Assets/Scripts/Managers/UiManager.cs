using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public TMP_Text turnCounter;
    public TMP_Text comboCounter;
    public PotionBoard potionBoard; 

    void Update()
    {
        if (potionBoard != null)
        {
            turnCounter.text = "Turnos: " + potionBoard.totalTurns;
            comboCounter.text = "Combos: " + potionBoard.totalCombos;
        }
    }
}
