using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class BattleManager : MonoBehaviour
{
    [Header("BattleState")]
    private BattleState battleState;
    //public static event Action<BattleState> OnBattleStateChanged;

    [Header("EnemyInfo")]
    private int currentBattleEnemyCount;
    [SerializeField] private List<EnemySO> listOfEnemiesSO;
    [SerializeField] protected List<GameObject> currentEnemies;
    [SerializeField] GameObject enemyPrefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        BattleStart();
        //UpdateBattleState();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void UpdateBattleState(BattleState newbattleState) // colocar em battle
    {
        battleState = newbattleState;

        switch (battleState)
        {
            case BattleState.BattleInit:
                BattleStart();
                break;
            case BattleState.PlayerTurn:
                //Manage Player Turn
                break;
            case BattleState.EnemyTurn:
                //Manage Enemy Turn
                break;
            case BattleState.BattleEnd:
                BattleEnd();
                break;
            default:
                break;
        }
    }

    private void BattleStart()
    {
        //StartBattleLogic
        currentBattleEnemyCount = (int)(Random.Range(1.0f, 3.9f));
        print(currentBattleEnemyCount);
        ChoseEnemies();
        UpdateBattleState(BattleState.PlayerTurn);
    }

    private void BattleEnd()
    {
        //EndBattleLogic
    }

    private void ChoseEnemies()
    {
        float enemySpotTransform = 14f / (currentBattleEnemyCount + 1);
        print(enemySpotTransform);
        for (int i = 0; i < currentBattleEnemyCount; i++)
        {
            GameObject go = Instantiate(enemyPrefab, transform);
            go.transform.position += new Vector3((enemySpotTransform * (i + 1)) - 7, 4, -3);
            print(go.transform.position);
            go.AddComponent<EnemyBehaviour>().SetEnemySO(listOfEnemiesSO[Random.Range(0, currentBattleEnemyCount)]);
            currentEnemies.Add(go);
        }
    }
}

public enum BattleState
{
    BattleInit,
    PlayerTurn,
    EnemyTurn,
    BattleEnd
}
