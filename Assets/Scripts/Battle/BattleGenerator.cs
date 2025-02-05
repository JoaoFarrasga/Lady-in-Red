using JetBrains.Annotations;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

public class BattleGenerator : MonoBehaviour
{
    [Header("GameInfos")]
    [SerializeField] private int numOfLevels = 12;

    [Header("Enemies")]
    //private List<EnemySO> enemies = new();
    [SerializeField] private List<EnemySO> listOfMinionsEnemiesSO, listOfBossEnemiesSO;
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
            if((i + 1) % 4 == 0)
            {
                enemies.Add(listOfBossEnemiesSO[((i + 1) / 4) - 1]);
            }
            else
            {
                for (int j = 0; j < numOfEnemies; j++)
                {
                    enemies.Add(listOfMinionsEnemiesSO[Random.Range(0, listOfMinionsEnemiesSO.Count)]);
                }
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
