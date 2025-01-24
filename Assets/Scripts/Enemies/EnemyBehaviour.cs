using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEditor.Progress;

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

    // M�todo para receber dano
    public void TakeDamage(float _damage, Dictionary<OrbType, int> elementMatches,  BattleControler battleControler)
    {

        foreach (var item in elementMatches)
        {
            float damage = _damage * item.Value;

            if (item.Key == enemySO.elementalWeakType) damage = damage * 2; // Dano dobrado contra fraquezas

            else if (item.Key == enemySO.elementalStrongType) damage = -(damage / 2); // Dano reduzido contra resist�ncias

            health -= damage; // Reduz a vida do inimigo
            print("takedamage");
        }

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
        Destroy(gameObject); // Destr�i o inimigo
    }

    public EnemySO GetEnemySO() { return enemySO; }

    public void SetHealthIncrease(float healthPercentage) 
    {
        print("health increase");
        health += maxHealth * healthPercentage;
        enemyHealthText.text = health.ToString();
    }

    public void SetBasicDamageAttackIncrease(float damagePercentage) { basicDamageAttack += basicDamageAttack * damagePercentage; }
}
