using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    [Header("EnemyStats")]
    [SerializeField] private float health;
    [SerializeField] private float maxHealth;
    [SerializeField] private float maxBasicDamageAttack;
    [SerializeField] private float basicDamageAttack;
    private float heavyDamageAttack;

    [SerializeField] private TMP_Text enemyHealthText;

    [SerializeField] private EnemySO enemySO;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        enemyHealthText = GetComponentInChildren<TMP_Text>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    private void Start()
    {
        spriteRenderer.sprite = enemySO.enemyAppearence;
        //health = maxHealth;
        //basicDamageAttack = enemySO.maxBasicDamageAttack;
        //enemyHealthText.text = health.ToString();
    }

    public void SetEnemySO(EnemySO enemySO)
    {
        this.enemySO = enemySO;
        maxHealth = enemySO.maxHealth;
        maxBasicDamageAttack = enemySO.maxBasicDamageAttack;
    }

    public void AttackPlayer(GameObject target, BattleControler battleControler)
    {
        target.GetComponent<Player>().TakeDamage(basicDamageAttack, battleControler);
    }

    // M�todo para receber dano
    public void TakeDamage(float _damage, Dictionary<OrbType, int> elementMatches, int combos, BattleControler battleControler)
    {
        float totalDamage = 0f;
        foreach (var item in elementMatches)
        {
            float damage = _damage * item.Value;
            //print("damage: " + damage);

            if (item.Key == enemySO.elementalWeakType) damage = damage * 2; // Dano dobrado contra fraquezas

            else if (item.Key == enemySO.elementalStrongType) damage = -(damage / 2); // Dano reduzido contra resist�ncias

            totalDamage += damage;
        }
        health -= totalDamage * combos; // Reduz a vida do inimigo
        print("takedamage");

        if (health <= 0)
        {
            Die(battleControler);
            return;
        }
        enemyHealthText.text = health.ToString();
    }

    private void Die(BattleControler battleControler)
    {
        battleControler.GetLevelEnemies().Remove(this.gameObject);
        battleControler.CheckNumEnemies();
        Destroy(gameObject); // Destr�i o inimigo
    }

    public EnemySO GetEnemySO() { return enemySO; }

    public void SetHealthIncrease(float healthPercentage) 
    {
        print("Maxhealth: " + maxHealth);
        if (GameManager.gameManager.gameLevel == 1) health = maxHealth;
        else health = maxHealth + (maxHealth * healthPercentage);
        
        print("Health: " + health);
        enemyHealthText.text = health.ToString();
    }

    public void SetBasicDamageAttackIncrease(float damagePercentage) 
    {
        if (GameManager.gameManager.gameLevel == 1) basicDamageAttack = enemySO.maxBasicDamageAttack;
        else basicDamageAttack += maxBasicDamageAttack + (maxBasicDamageAttack * damagePercentage); 
    }
}
