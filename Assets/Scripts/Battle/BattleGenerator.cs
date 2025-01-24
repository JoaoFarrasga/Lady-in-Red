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

    public void CreateBattles()
    {
        battles.Clear();
        for (int i = 0; i < numOfLevels; i++) 
        {
            int numOfEnemies = NumberOfEnemysInLevel();
            List<EnemySO> enemies = new();
            for (int j = 0; j < numOfEnemies; j++)
            { 
                enemies.Add(listOfEnemiesSO[Random.Range(0, listOfEnemiesSO.Count)]); 
            }
            battles.Add(enemies);
        }
    }

    private int NumberOfEnemysInLevel()
    {
        //StartBattleLogic
        int currentBattleEnemyCount = (int)Random.Range(1.0f, 3.9f);
        return currentBattleEnemyCount;
    }

    public List<List<EnemySO>> Battles() { return battles; }

}
