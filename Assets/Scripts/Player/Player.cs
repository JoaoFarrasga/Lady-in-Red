using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{ 
    [Header("Health")]
    private float maxHealth = 100;
    private float health;

    [Header("Damage")]
    private float damageAttack;

    private void Start()
    {
        health = maxHealth;
        damageAttack = 10f;
    }

    public void TakeDamage(float damage, BattleControler battleControler)
    {
        health -= damage;
        if (health <= 0) Die(battleControler);
    }

    public void AttackEnemy(Dictionary<OrbType, int> elementMatches, BattleControler battleControler, int combos)
    {
        foreach(var item in elementMatches)
        {
            battleControler.focusedEnemy.GetComponent<EnemyBehaviour>().TakeDamage(damageAttack * combos, item.Key, item.Value, battleControler);
            print("focusedEnemy: " + battleControler.focusedEnemy.GetComponent<EnemyBehaviour>().GetEnemySO().enemyName);
        }
    }

    private void Die(BattleControler battleControler) 
    {
        battleControler.UpdateBattleState(BattleState.BattleEnd);
    }

    public float GetHealth() { return health; }


}
