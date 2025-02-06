using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{ 
    [Header("Health")]
    [SerializeField] private float maxHealth = 100;
    [SerializeField] private float health;

    [Header("Damage")]
    [SerializeField] private float maxDamageAttack = 10f;
    [SerializeField] private float damageAttack;

    [Header("DamageEffect")]
    [SerializeField] Image damageEffect;

    private void Awake()
    {
        GameManager.OnGameStateChanged += OnGameEnd;
    }

    private void Start()
    {
        health = maxHealth;
        damageAttack = maxDamageAttack;
    }

    public async Task TakeDamage(float damage, BattleControler battleControler)
    {
        health -= damage;
        await TakeDamageEffect();
        //await Task.Delay(5000);
        if (health <= 0)
        {
            Die(battleControler);
            return;
        }
    }

    private async Task TakeDamageEffect()
    {
        Color newColor = damageEffect.color;
        newColor.a = 120f / 255f;             
        damageEffect.color = newColor;

        await Task.Delay(400);

        while (newColor.a > 0f) 
        {
            newColor.a -= 0.01f;
            damageEffect.color = newColor;

            await Task.Delay(100); ;
        }
    }

    public void AttackEnemy(Dictionary<OrbType, int> elementMatches, BattleControler battleControler, int combos)
    {
        EnemyBehaviour focusedEnemy = battleControler.focusedEnemy.GetComponent<EnemyBehaviour>();
        focusedEnemy.TakeDamage(damageAttack, elementMatches, combos, battleControler);
    }

    private void OnGameEnd(GameState gameState) { if (gameState == GameState.ExitBattle) ResetStats(); }  

    private void ResetStats()
    {
        health = maxHealth;
        damageAttack = maxDamageAttack;
    }

    private void Die(BattleControler battleControler) {  battleControler.UpdateBattleState(BattleState.BattleEnd); }

    public float SetHealthIncrease(float healthPercentage, int level)
    { 
        if (GameManager.gameManager.gameLevel == 1) health = maxHealth;
        else health = maxHealth + (maxHealth * (healthPercentage * level));
        print("Health: " + health);
        return health;
    }

    public void SetBasicDamageAttackIncrease(float damagePercentage, int level)
    {
        if (GameManager.gameManager.gameLevel == 1) damageAttack = maxDamageAttack;
        else damageAttack += maxDamageAttack + (maxDamageAttack * (damagePercentage * level));
    }

    public float GetHealth() { return health; }
}
