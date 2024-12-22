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
            default:
                break;
        }

        OnGameStateChanged?.Invoke(newState);
    }

    private void Update()
    {

    }

}

//States of the game
public enum GameState
{
    InitialScreen,
    GameStart,
    PlayerTurn,
    Enemyturn,
    GameEnd,
    Pause
}
