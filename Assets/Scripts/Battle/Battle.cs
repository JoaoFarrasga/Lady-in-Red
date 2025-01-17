using System.Collections.Generic;
using UnityEngine;

public class Battle : BattleManager
{
    //[SerializeField] private List<GameObject> currentEnemies;

    public void AddCurrentEnemy(GameObject enemy)
    {
        currentEnemies.Add(enemy);
    }
}
