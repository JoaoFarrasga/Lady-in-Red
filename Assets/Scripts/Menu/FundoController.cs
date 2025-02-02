using UnityEngine;

public class FundoController : MonoBehaviour
{
    private int gameLevel;

    [Header("Fundos")]
    [SerializeField] private GameObject Fundo1;
    [SerializeField] private GameObject Fundo2;
    [SerializeField] private GameObject Fundo3;

    void Start()
    {
        gameLevel = GameManager.gameManager.gameLevel;
        AtualizarFundo();
    }

    void Update()
    {
        if (gameLevel != GameManager.gameManager.gameLevel)
        {
            gameLevel = GameManager.gameManager.gameLevel;
            AtualizarFundo();
        }
    }

    void AtualizarFundo()
    {
        Fundo1.SetActive(false);
        Fundo2.SetActive(false);
        Fundo3.SetActive(false);

        int fundoIndex = (gameLevel - 1) % 3;

        switch (fundoIndex)
        {
            case 0:
                Fundo1.SetActive(true);
                break;
            case 1:
                Fundo2.SetActive(true);
                break;
            case 2:
                Fundo3.SetActive(true);
                break;
        }
    }
}
