using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager gameManager;
    public GameState State;
    public static event Action<GameState> OnGameStateChanged;

    private void Awake()
    {
        if (gameManager == null)
        {
            gameManager = this;
            DontDestroyOnLoad(gameObject); // Don't destroy this object when loading new scenes
        }
        else
        {
            // If an instance already exists, destroy this one
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
            case GameState.GameStart:
                //Manage GameStart
                break;
            case GameState.GameEnd:
                //Manage Enemy Turn
                break;
            case GameState.Pause:
                break;
            default:
                break;
        }

        OnGameStateChanged?.Invoke(newState);
    }

    public void Damage(PotionType type, int combo)
    {
        int damage = 3 * combo; // Calcula o dano baseado no combo
        Debug.Log($"Aplicando {damage} de dano aos inimigos.");

        // Envia o dano para todos os inimigos
        EnemyBehaviour[] enemies = FindObjectsOfType<EnemyBehaviour>(); // Obtém todos os inimigos na cena
        foreach (EnemyBehaviour enemy in enemies)
        {
            enemy.TakeDamage(damage);
        }
    }
}

//States of the game
public enum GameState
{
    InitialScreen,
    GameStart,
    GameEnd,
    Pause
}
