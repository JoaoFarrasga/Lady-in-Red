using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BattleControler : MonoBehaviour
{
    [Header("InputActions")]
    private PlayerInputActions inputActions;
    private Camera mainCamera;

    [Header("BattleState")]
    private BattleState battleState;

    [Header("EnemyInfo")]
    [SerializeField] GameObject enemyPrefab;
    [SerializeField] public GameObject focusedEnemy { get; set; }
    [SerializeField] List<GameObject> levelEnemies;
    private float increaseHealthPercentage = 0.5f;
    private float increaseDamagePercentage = 0.15f;
    public int maxEnemyTurns { get; set; } = 1;

    [Header("PlayerInfo")]
    [SerializeField] Player player;
    public int maxPlayerTurns { get; set; } = 3;

    [Header("ReferencePoint")]
    [SerializeField] private GameObject referencePoint;
    [SerializeField] private float totalEnemySpace;

    [Header("BattleGeneratorInfo")]
    [SerializeField] private BattleGenerator battleGenerator;


    private void Awake()
    {
        mainCamera = Camera.main;
        inputActions = new PlayerInputActions();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        inputActions.Enable();
        inputActions.Gameplay.Click.performed += OnClickEnemyPerformed;
        SetUpBattle();
    }

    private void OnDisable()
    {
        inputActions.Gameplay.Click.performed -= OnClickEnemyPerformed;
        inputActions.Disable();
    }

    private void OnClickEnemyPerformed(InputAction.CallbackContext context)
    {
        //if (isProcessingMove) return;

        var rayHit = Physics2D.GetRayIntersection(mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue()));

        if (!rayHit.collider) return;

        var enemy = rayHit.collider.gameObject.GetComponent<EnemyBehaviour>();
        //print("focusedEnemy: " +  enemy);
        if (enemy != null)
        {
            focusedEnemy = enemy.gameObject;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void UpdateBattleState(BattleState newbattleState)
    {
        battleState = newbattleState;

        switch (battleState)
        {
            case BattleState.BattleInit:
                SetUpBattle();
                break;
            case BattleState.PlayerTurn:
                //CheckNumEnemies();
                // Manage Player Turn
                break;
            case BattleState.EnemyTurn:
                EnemyTurnAttack();
                // Manage Enemy Turn
                break;
            case BattleState.BattleEnd:
                BattleEnd(player);
                break;
            default:
                break;
        }
    }

    private void SetUpBattle()
    {
        battleGenerator.CreateBattles();
        PlaceEnemies(GameManager.gameManager.gameLevel);
        UpdateBattleState(BattleState.PlayerTurn);
    }

    private void BattleEnd(Player player)
    {
        if (player.GetHealth() < 0 || GameManager.gameManager.gameLevel >= 10)
        {
            GameManager.gameManager.UpdateGameState(GameState.ExitBattle);
            increaseHealthPercentage = 0.5f;
            increaseDamagePercentage = 0.15f;
            return;
        }
        else if (GameManager.gameManager.gameLevel < 10)
        {
            Debug.Log("All enemies defeated!");
            GameManager.gameManager.gameLevel++;
            increaseHealthPercentage += 0.15f;
            increaseDamagePercentage += 0.03f;
            UpdateBattleState(BattleState.BattleInit);
        }
    }

    private void PlaceEnemies(int level)
    {
        if(levelEnemies.Count != 0) levelEnemies.Clear();
        bool isPlacingOnRightSide = true;
        int offsetMultiplier = 1;

        float enemyPlaceDivision = (totalEnemySpace * 2) / (battleGenerator.Battles()[level - 1].Count + 1);

        for (int i = 0; i < battleGenerator.Battles()[level - 1].Count; i++)
        {
            GameObject go = Instantiate(enemyPrefab, transform);
            go.AddComponent<EnemyBehaviour>().SetEnemySO(battleGenerator.Battles()[level - 1][i]);
            if(level > 1)
            {
                go.GetComponent<EnemyBehaviour>().SetHealthIncrease(increaseHealthPercentage);
                go.GetComponent<EnemyBehaviour>().SetBasicDamageAttackIncrease(increaseDamagePercentage);
            }

            levelEnemies.Add(go);

            if (isPlacingOnRightSide)
            {
                float enemySpot = totalEnemySpace - (offsetMultiplier * enemyPlaceDivision);
                go.transform.localPosition += new Vector3(enemySpot, 0, 0);
                isPlacingOnRightSide = !isPlacingOnRightSide;
            }
            else
            {
                float enemySpot = (offsetMultiplier * enemyPlaceDivision) - totalEnemySpace;
                go.transform.localPosition += new Vector3(enemySpot, 0, 0);
                isPlacingOnRightSide = !isPlacingOnRightSide;
                offsetMultiplier += 1;
            }
        }

        focusedEnemy = levelEnemies[0];
    }

    private void EnemyTurnAttack()
    {
        print("EnemyAttacking");
        foreach (GameObject enemy in levelEnemies)
        {
            enemy.GetComponent<EnemyBehaviour>().AttackPlayer(player.gameObject, this);
        }
        UpdateBattleState(BattleState.PlayerTurn);
    }

    public void CheckNumEnemies()
    {
        print("enemy count: " + levelEnemies.Count);
        if (levelEnemies.Count == 0) UpdateBattleState(BattleState.BattleEnd);
        else focusedEnemy = levelEnemies[0];
    }

    public BattleState GetBattleState() { return battleState; }

    public List<GameObject> GetLevelEnemies() { return levelEnemies; }
}


public enum BattleState
{
    BattleInit,
    PlayerTurn,
    EnemyTurn,
    BattleEnd
}
