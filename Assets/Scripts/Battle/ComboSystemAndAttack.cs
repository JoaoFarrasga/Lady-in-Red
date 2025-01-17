using UnityEngine;

public class ComboSystemAndAttack : MonoBehaviour
{
    [SerializeField] private PotionBoard potionBoard;

    private void PrintMatchStats()
    {
        Debug.Log("Match Stats:");

        foreach (var item in potionBoard.matchCountsByColor)
        {
            Debug.Log($"{item.Key}: {item.Value} matches");
        }
    }
}
