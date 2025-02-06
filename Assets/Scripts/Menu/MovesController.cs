using UnityEngine;

public class MovesController : MonoBehaviour
{
    [SerializeField] private PotionBoard potionBoard;

    // Referências para cada um dos objetos
    [SerializeField] private GameObject move1;
    [SerializeField] private GameObject move1_painted;
    [SerializeField] private GameObject move2;
    [SerializeField] private GameObject move2_painted;
    [SerializeField] private GameObject move3;
    [SerializeField] private GameObject move3_painted;

    private int _turns;

    private void Update()
    {
        _turns = potionBoard.totalTurns;

        switch (_turns)
        {
            case 0:
                // move1_painted, move2, move3
                move1.SetActive(false);
                move1_painted.SetActive(true);

                move2.SetActive(true);
                move2_painted.SetActive(false);

                move3.SetActive(true);
                move3_painted.SetActive(false);
                break;

            case 1:
                // move1_painted, move2_painted, move3
                move1.SetActive(false);
                move1_painted.SetActive(true);

                move2.SetActive(false);
                move2_painted.SetActive(true);

                move3.SetActive(true);
                move3_painted.SetActive(false);
                break;

            case 2:
                // move1_painted, move2_painted, move3_painted
                move1.SetActive(false);
                move1_painted.SetActive(true);

                move2.SetActive(false);
                move2_painted.SetActive(true);

                move3.SetActive(false);
                move3_painted.SetActive(true);
                break;

            default:
                Debug.LogWarning("Valor de turno inválido!");
                break;
        }
    }
}
