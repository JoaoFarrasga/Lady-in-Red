using JetBrains.Annotations;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

public class BattleControler : MonoBehaviour
{
    [Header("InputActions")]
    private PlayerInputActions inputActions;
    private Camera mainCamera;

    [Header("BattleState")]
    [SerializeField] private BattleState battleState;

    [Header("EnemyInfo")]
    [SerializeField] GameObject enemyPrefab;
    public GameObject focusedEnemy { get; set; }
    [SerializeField] List<GameObject> levelEnemies;
    private float increaseEnemyHealthPercentage = 0.5f;
    private float increaseEnemyDamagePercentage = 0.15f;
    public int maxEnemyTurns { get; set; } = 1;

    [Header("PlayerInfo")]
    [SerializeField] Player player;
    private float increasePlayerHealthPercentage = 0.2f;
    private float increasePlayerDamagePercentage = 0.05f;

    public int maxPlayerTurns { get; set; } = 3;

    [Header("ReferencePoint")]
    [SerializeField] private GameObject referencePoint;
    [SerializeField] private float totalEnemySpace;

    [Header("BattleGeneratorInfo")]
    [SerializeField] private BattleGenerator battleGenerator;

    [Header("HUD Controller")]
    [SerializeField] HUDController_ hudController;

    [Header("Dead VFX")]
    [SerializeField] GameObject deadVFX;


    private void Awake()
    {
        mainCamera = Camera.main;
        inputActions = new PlayerInputActions();
    }

    private void Start()
    {
        hudController = GetComponent<HUDController_>();
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
            FocusEnemy(enemy.gameObject);
        }
    }


    public async void UpdateBattleState(BattleState newbattleState)
    {
        battleState = newbattleState;

        switch (battleState)
        {
            case BattleState.BattleInit:
                SetUpBattle();
                break;
            case BattleState.PlayerTurn:
                print("EnemyTurn");
                break;
            case BattleState.EnemyTurn:
                await GameManager.gameManager.MessagePOP_UP("EnemyTurn");
                await EnemyTurnAttack();
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
        PlaceEnemies(GameManager.gameManager.gameLevel);
        IncreasePlayerStats();
        UpdateBattleState(BattleState.PlayerTurn);
    }

    private async void BattleEnd(Player player)
    {
        if (player.GetHealth() < 0)
        {
            await GameManager.gameManager.MessagePOP_UP("YOU DIED\n(skill issue)", () => { GameManager.gameManager.UpdateGameState(GameState.ExitBattle); });
            //GameManager.gameManager.UpdateGameState(GameState.ExitBattle);
            DestroyLevelEnemies();
            increaseEnemyHealthPercentage = 0.5f;
            increaseEnemyDamagePercentage = 0.15f;
            return;
        }
        else if(GameManager.gameManager.gameLevel >= 10)
        {
            await GameManager.gameManager.MessagePOP_UP("YOU WON", () => { GameManager.gameManager.UpdateGameState(GameState.ExitBattle); });
            //GameManager.gameManager.UpdateGameState(GameState.ExitBattle);
            increaseEnemyHealthPercentage = 0.5f;
            increaseEnemyDamagePercentage = 0.15f;
            return;
        }
        Debug.Log("All enemies defeated!");
        GameManager.gameManager.gameLevel++;
        increaseEnemyHealthPercentage += 0.15f;
        increaseEnemyDamagePercentage += 0.03f;
        await GameManager.gameManager.MessagePOP_UP("ENEMIES DEFEATED", () => { UpdateBattleState(BattleState.BattleInit); });
        //UpdateBattleState(BattleState.BattleInit);
    }

    private void DestroyLevelEnemies(){ foreach(GameObject enemy in levelEnemies) Destroy(enemy); }

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
            go.GetComponent<EnemyBehaviour>().SetDeadVFX(deadVFX);

            if (go.GetComponent<EnemyBehaviour>().GetEnemySO().enemyType == "Minion")
            {
                go.GetComponent<EnemyBehaviour>().SetHealthIncrease(increaseEnemyHealthPercentage);
                go.GetComponent<EnemyBehaviour>().SetBasicDamageAttackIncrease(increaseEnemyDamagePercentage);
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

        FocusEnemy(levelEnemies[0]);
    }

    private void IncreasePlayerStats()
    {
        float playerHealthIncrease = player.SetHealthIncrease(increasePlayerHealthPercentage, GameManager.gameManager.gameLevel);
        player.SetBasicDamageAttackIncrease(increasePlayerDamagePercentage, GameManager.gameManager.gameLevel);

        hudController.GetPlayerLifeSlider().maxValue = playerHealthIncrease;
    }

    private async Task EnemyTurnAttack()
    {
        foreach (GameObject enemy in levelEnemies)
        {
            if (player.GetHealth() <= 0) return;
            await enemy.GetComponent<EnemyBehaviour>().AttackPlayer(player.gameObject, this);
            //await Task.Delay(1500);
        }
        if( battleState != BattleState.BattleEnd) await GameManager.gameManager.MessagePOP_UP("Player Turn", () => { UpdateBattleState(BattleState.PlayerTurn); });
    }

    public void CheckNumEnemies()
    {
        print("enemy count: " + levelEnemies.Count);
        if (levelEnemies.Count == 0) UpdateBattleState(BattleState.BattleEnd);
        else FocusEnemy(levelEnemies[0]);
    }

    public BattleState GetBattleState() { return battleState; }

    public List<GameObject> GetLevelEnemies() { return levelEnemies; }

    private void FocusEnemy(GameObject focus)
    {
        if (focusedEnemy != null)
        {
            focusedEnemy.GetComponent<EnemyBehaviour>().DefocusEnemy();
        }

        focusedEnemy = focus;

        focusedEnemy.GetComponent<EnemyBehaviour>().FocusEnemy();
    }
}


public enum BattleState
{
    BattleInit,
    PlayerTurn,
    EnemyTurn,
    BattleEnd
}
