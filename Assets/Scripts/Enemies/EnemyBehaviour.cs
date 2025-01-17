using JetBrains.Annotations;
using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    [Header("EnemyStats")]
    [SerializeField] private float health;
    [SerializeField] private float maxHealth;
    private float basicDamageAttack;
    private float heavyDamageAttack;

    [SerializeField] private EnemySO enemySO;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = enemySO.enemyAppearence;
        maxHealth = enemySO.maxHealth;
        health = maxHealth;
        basicDamageAttack = enemySO.maxBasicDamageAttack;
    }

    public void SetEnemySO(EnemySO enemySO)
    {
        this.enemySO = enemySO;
    }

    public void AttackPlayer(GameObject target)
    {
        target.GetComponent<Player>().TakeDamage(basicDamageAttack);
    }

    // Método para receber dano
    public void TakeDamage(float _damage, OrbType element, int elementCount)
    {
        float damage = _damage * elementCount;

        if (element == enemySO.elementalWeakType)
        {
            damage = damage * 2; // Dano dobrado contra fraquezas
        }
        else if (element == enemySO.elementalStrongType)
        {
            damage = -(damage / 2); // Dano reduzido contra resistências
        }

        health -= damage; // Reduz a vida do inimigo

        if (health <= 0)
        {
            Die();
        }

        print("enemyHealth: " + health);
        print("enemyName: " + enemySO.name);
    }

    private void Die()
    {
        // Notificar o GameManager sobre a morte do inimigo
        GameManager.gameManager.OnEnemyDeath(this);
        Destroy(gameObject); // Destrói o inimigo
    }

    public EnemySO GetEnemySO() { return enemySO; }
}
