using Unity.VisualScripting;
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

    private GameManager gameManager;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = enemySO.enemyAppearence;
        maxHealth = enemySO.maxHealth;
        health = maxHealth;
    }

    public void SetEnemySO(EnemySO enemySO)
    {
        this.enemySO = enemySO;
    }

    public void SetGameManager(GameManager _gameManager)
    {
        this.gameManager = _gameManager;
    }

    // Método para receber dano
    public void TakeDamage(int damage)
    {
        health -= damage; // Reduz a vida do inimigo

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}
