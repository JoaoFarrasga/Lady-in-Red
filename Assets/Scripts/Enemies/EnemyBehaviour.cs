using Unity.VisualScripting;
using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    [Header("EnemyStats")]
    private float health;
    private float basicDamageAttack;
    private float HeavyDamageAttack;

    [SerializeField] private EnemySO enemySO;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = enemySO.enemyAppearence;
    }
    public void SetEnemySO(EnemySO enemySO) 
    {
        this.enemySO = enemySO;
    }
}
