using UnityEngine;
using System;
using System.Collections;
using TMPro;
using System.Threading.Tasks;

public class GameManager : MonoBehaviour
{
    public static GameManager gameManager;
    public GameState State;
    public static event Action<GameState> OnGameStateChanged;
    public int gameLevel = 1;

    [Header("UIManager")]
    [SerializeField] MenuManagerFrancisco menuManager;
    [SerializeField] GameObject messagePopUp;

    private void Awake()
    {
        if (gameManager == null)
        {
            gameManager = this;
            DontDestroyOnLoad(gameObject); // Não destrua este objeto ao carregar novas cenas
        }
        else
        {
            // Se uma instância já existir, destrua este objeto
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        menuManager = GetComponent<MenuManagerFrancisco>();
        State = GameState.InitialScreen;
    }

    public void UpdateGameState(GameState newState)
    {
        State = newState;

        switch (newState)
        {
            case GameState.InitialScreen:
                gameLevel = 1;
                break;
            case GameState.InBattle:
                break;
            case GameState.ExitBattle:
                menuManager.GameEnd();
                UpdateGameState(GameState.InitialScreen);
                break;
            case GameState.Pause:
                break;
            default:
                break;
        }

        OnGameStateChanged?.Invoke(newState);
    }

    public async Task MessagePOP_UP(string message, System.Action onComplete = null)
    {
        messagePopUp.SetActive(true);
        messagePopUp.transform.GetComponentInChildren<TMP_Text>().text = message;
        //print("Animation duration: " + (double) messagePopUp.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length);
        await Task.Delay(1500);
        messagePopUp.SetActive(false);
        onComplete?.Invoke(); // Call the callback when done

    }
}


//States of the game
public enum GameState
{
    InitialScreen,
    InBattle,
    ExitBattle,
    Pause
}
