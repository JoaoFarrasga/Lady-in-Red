using UnityEngine;
using UnityEngine.UI;

public class MenuManagerFrancisco : MonoBehaviour
{
    [Header("Battle")]
    [SerializeField] BattleControler battleControler;
    [SerializeField] BattleGenerator battleGenerator;

    [Header("Canvas")]
    [SerializeField] private Button startBtn;
    [SerializeField] private Button optionsBtn;
    [SerializeField] private Button exitBtn;
    [SerializeField] private GameObject hud, menu, options;
    private GameObject currentGOActive;

    private void Start()
    {
        startBtn.onClick.AddListener(StartGame);
        optionsBtn.onClick.AddListener(Options);
        exitBtn.onClick.AddListener(Exit);
        currentGOActive = menu;
    }

    private void StartGame()
    {
        battleGenerator.CreateBattles();
        battleControler.UpdateBattleState(BattleState.BattleInit);
        GameManager.gameManager.UpdateGameState(GameState.InBattle);
        currentGOActive.SetActive(false);
        hud.SetActive(true);
        currentGOActive = hud;
    }

    private void Options()
    {
        currentGOActive.SetActive(false);
        options.SetActive(true);
        currentGOActive = options;
    }

    private void Exit()
    {
        Application.Quit();
    }

    public void GameEnd()
    {
        currentGOActive.SetActive(false);
        menu.SetActive(true);
        currentGOActive = menu;
    }

    public GameObject GetCurrentGOActive() { return currentGOActive; }
}
