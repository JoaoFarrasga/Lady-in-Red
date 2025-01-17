using JetBrains.Annotations;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

public class BattleGenerator : MonoBehaviour
{
    [Header("GameInfos")]
    [SerializeField] private int numOfLevels = 10;

    [Header("Enemies")]
    //private List<EnemySO> enemies = new();
    [SerializeField] private List<EnemySO> listOfEnemiesSO;
    [SerializeField] private List<List<EnemySO>> battles = new();

    [Header("BattleController")]
    [SerializeField] private BattleControler battleControler;

    private void Start()
    {
        //print("CreatingBattles");
        CreateBattles();
    }

    public void CreateBattles()
    {
        for (int i = 0; i < numOfLevels; i++) 
        {
            int numOfEnemies = NumberOfEnemysInLevel();
            List<EnemySO> enemies = new();
            for (int j = 0; j < numOfEnemies; j++)
            {
                
                enemies.Add(listOfEnemiesSO[Random.Range(0, listOfEnemiesSO.Count)]); 
            }
            //print(enemies.Count);
            battles.Add(enemies);
            //print("BattleCount: " + battles[i].Count);
            //enemies.Clear();
        }
        //print("Num of Battles: " + battles.Count);
        //print("Num of enemies in Level1: " + battles[0].Count);
        battleControler.UpdateBattleState(BattleState.BattleInit); 
    }

    private int NumberOfEnemysInLevel()
    {
        //StartBattleLogic
        int currentBattleEnemyCount = (int)Random.Range(1.0f, 3.9f);
        //print(currentBattleEnemyCount);
        return currentBattleEnemyCount;
    }

    public List<List<EnemySO>> Battles() { return battles; }

}
