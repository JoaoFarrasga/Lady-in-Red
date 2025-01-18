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
        // print("PlayerHealth: " + health);
        if (health <= 0) Die();
    }

    private void AttackEnemy(GameObject enemy, Dictionary<OrbType, int> elementMatches)
    {
        foreach(var item in elementMatches)
        {
            enemy.GetComponent<EnemyBehaviour>().TakeDamage(damageAttack, item.Key, item.Value);
        }
    }

    public void Attack(List<Potion> potion, List<GameObject> enemies, Dictionary<OrbType, int> elementMatches)
    {
        foreach (GameObject go in enemies) {
            if (go.GetComponent<EnemyBehaviour>().GetEnemySO().elementalType == potion[0].potionType)
            {
                AttackEnemy(go, elementMatches);
                return;
            }
        }
        AttackEnemy(enemies[0], elementMatches);
    }

    private void Die() { }


}
