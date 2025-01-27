using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager gameManager;
    public GameState State;
    public static event Action<GameState> OnGameStateChanged;
    public int gameLevel = 1;

    [Header("UIManager")]
    [SerializeField] MenuManagerFrancisco menuManager;

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
}


//States of the game
public enum GameState
{
    InitialScreen,
    InBattle,
    ExitBattle,
    Pause
}
