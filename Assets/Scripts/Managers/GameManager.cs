using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager gameManager;
    public GameState State;
    public static event Action<GameState> OnGameStateChanged;
    public int gameLevel = 1;

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
        State = GameState.InitialScreen;
    }

    public void UpdateGameState(GameState newState)
    {
        State = newState;

        switch (newState)
        {
            case GameState.InitialScreen:
                break;
            case GameState.InBattle:
                break;
            case GameState.ExitBattle:
                break;
            case GameState.Pause:
                break;
            default:
                break;
        }

        OnGameStateChanged?.Invoke(newState);
    }

    public void OnEnemyDeath(EnemyBehaviour enemy)
    {
        Debug.Log($"Enemy defeated: {enemy.name}");

        // Lógica adicional para lidar com a morte do inimigo, como verificar se a batalha terminou
        CheckBattleEnd();
    }

    private void CheckBattleEnd()
    {
        EnemyBehaviour[] enemies = FindObjectsOfType<EnemyBehaviour>();
        if (enemies.Length == 0)
        {
            Debug.Log("All enemies defeated!");
            UpdateGameState(GameState.ExitBattle);
        }
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
