using JetBrains.Annotations;
using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using TMPro.EditorUtilities;
using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    [Header("EnemyStats")]
    [SerializeField] private float health;
    [SerializeField] private float maxHealth;
    private float basicDamageAttack;
    private float heavyDamageAttack;

    [SerializeField] private TMP_Text enemyHealthText;

    [SerializeField] private EnemySO enemySO;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        enemyHealthText = GetComponentInChildren<TMP_Text>();
        spriteRenderer.sprite = enemySO.enemyAppearence;
        maxHealth = enemySO.maxHealth;
        health = maxHealth;
        basicDamageAttack = enemySO.maxBasicDamageAttack;
        enemyHealthText.text = health.ToString();
    }

    public void SetEnemySO(EnemySO enemySO)
    {
        this.enemySO = enemySO;
    }

    public void AttackPlayer(GameObject target, BattleControler battleControler)
    {
        target.GetComponent<Player>().TakeDamage(basicDamageAttack, battleControler);
    }

    // Método para receber dano
    public void TakeDamage(float _damage, OrbType element, int elementCount, BattleControler battleControler)
    {
        float damage = _damage * elementCount;

        if (element == enemySO.elementalWeakType) damage = damage * 2; // Dano dobrado contra fraquezas

        else if (element == enemySO.elementalStrongType) damage = -(damage / 2); // Dano reduzido contra resistências

        health -= damage; // Reduz a vida do inimigo
        print("takedamage");
        enemyHealthText.text = health.ToString();

        if (health <= 0)
        {
            Die(battleControler);
        }
    }

    private void Die(BattleControler battleControler)
    {
        // Notificar o GameManager sobre a morte do inimigo
        //GameManager.gameManager.OnEnemyDeath(this);
        battleControler.GetLevelEnemies().Remove(this.gameObject);
        battleControler.CheckNumEnemies();
        //battleControler.focusedEnemy = battleControler.GetLevelEnemies()[0];
        Destroy(gameObject); // Destrói o inimigo
    }

    public EnemySO GetEnemySO() { return enemySO; }

    public void SetHealthIncrease(float healthPercentage) { health += maxHealth * healthPercentage ; }

    public void SetBasicDamageAttackIncrease(float damagePercentage) { basicDamageAttack += basicDamageAttack * damagePercentage; }
}
