using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Android;

public class Player : MonoBehaviour
{ 
    [Header("Health")]
    [SerializeField] private float maxHealth = 100;
    private float health;

    [Header("Damage")]
    [SerializeField] private float maxDamageAttack = 10f;
    private float damageAttack;

    private void Awake()
    {
        GameManager.OnGameStateChanged += OnGameEnd;
    }

    private void Start()
    {
        health = maxHealth;
        damageAttack = maxDamageAttack;
    }

    public void TakeDamage(float damage, BattleControler battleControler)
    {
        health -= damage;
        if (health <= 0) Die(battleControler);
    }

    public void AttackEnemy(Dictionary<OrbType, int> elementMatches, BattleControler battleControler, int combos)
    {
        EnemyBehaviour focusedEnemy = battleControler.focusedEnemy.GetComponent<EnemyBehaviour>();
        focusedEnemy.TakeDamage(damageAttack, elementMatches, combos, battleControler);
    }

    private void OnGameEnd(GameState gameState)
    {
        if (gameState == GameState.ExitBattle) { ResetStats(); }
    }

    private void ResetStats()
    {
        health = maxHealth;
        damageAttack = maxDamageAttack;
    }

    private void Die(BattleControler battleControler) {  battleControler.UpdateBattleState(BattleState.BattleEnd); }

    public float GetHealth() { return health; }
}
