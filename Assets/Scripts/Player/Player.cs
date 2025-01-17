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

    public void TakeDamage(float damage)
    {
        health -= damage;
        //print("PlayerHealth: " + health);
        if (health <= 0) Die();
    }

    private void AttackEnemy(GameObject enemy, Dictionary<OrbType, int> elementMatches, BattleControler battleControler)
    {
        foreach(var item in elementMatches)
        {
            enemy.GetComponent<EnemyBehaviour>().TakeDamage(damageAttack, item.Key, item.Value, battleControler);
        }
    }

    public void Attack(List<Potion> potion, Dictionary<OrbType, int> elementMatches, BattleControler battleControler)
    {
        foreach (GameObject go in battleControler.GetLevelEnemies()) {
            if (go.GetComponent<EnemyBehaviour>().GetEnemySO().elementalType == potion[0].potionType)
            {
                AttackEnemy(go, elementMatches, battleControler);
                return;
            }
        }
        AttackEnemy(battleControler.GetLevelEnemies()[0], elementMatches, battleControler);
    }

    private void Die() { }

    public float GetHealth() { return health; }


}
