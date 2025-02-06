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
        // Desabilita todos antes de configurar o que precisa ficar ativo
        Fundo1.SetActive(false);
        Fundo2.SetActive(false);
        Fundo3.SetActive(false);

        // Verifica em qual faixa de níveis o gameLevel está
        if (gameLevel >= 1 && gameLevel <= 4)
        {
            Fundo1.SetActive(true);
        }
        else if (gameLevel >= 5 && gameLevel <= 8)
        {
            Fundo2.SetActive(true);
        }
        else if (gameLevel >= 9 && gameLevel <= 12)
        {
            Fundo3.SetActive(true);
        }
        // Caso tenha mais níveis, você pode continuar a lógica conforme necessário.
    }
}
