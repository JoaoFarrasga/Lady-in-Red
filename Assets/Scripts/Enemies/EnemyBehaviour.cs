using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyBehaviour : MonoBehaviour
{
    [Header("EnemyStats")]
    [SerializeField] private float health;
    [SerializeField] private float maxHealth;
    [SerializeField] private float maxBasicDamageAttack;
    [SerializeField] private float basicDamageAttack;
    private float heavyDamageAttack;

    [Header("EnemyHealthText")]
    [SerializeField] private Slider enemyHealthText;

    [Header("EnemySO")]
    [SerializeField] private EnemySO enemySO;

    [Header("EnemyAppearence")]
    //[SerializeField] private GameObject body, face, particles;
    private SpriteRenderer bodySpriteRenderer, faceSpriteRenderer, particleSpriteRenderer;

    [Header("EnemyAnimator")]
    private Animator animator;
    private string currentAnimation;
    private float savedTime;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        enemyHealthText = GetComponentInChildren<Slider>();
        bodySpriteRenderer = transform.Find("Body").GetComponent<SpriteRenderer>();
        faceSpriteRenderer = transform.Find("Face").GetComponent<SpriteRenderer>();
        particleSpriteRenderer = transform.Find("Particle").GetComponent<SpriteRenderer>();
    }
    private void Start()
    {
        bodySpriteRenderer.sprite = enemySO.bodySprite;
        faceSpriteRenderer.sprite = enemySO.faceSprite;
        if (enemySO.particleSprite != null) particleSpriteRenderer.sprite = enemySO.particleSprite;
    }

    public void SetEnemySO(EnemySO enemySO)
    {
        this.enemySO = enemySO;
        maxHealth = enemySO.maxHealth;
        enemyHealthText.maxValue = maxHealth;
        enemyHealthText.value = maxHealth;
        maxBasicDamageAttack = enemySO.maxBasicDamageAttack;
    }

    public async Task AttackPlayer(GameObject target, BattleControler battleControler)
    {
        PauseAnimation();
        await target.GetComponent<Player>().TakeDamage(basicDamageAttack, battleControler);
        //await Task.Delay(500);
        ResumeAnimation();
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
        enemyHealthText.value = health;
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
        //print("Maxhealth: " + maxHealth);
        if (GameManager.gameManager.gameLevel == 1) health = maxHealth;
        else health = maxHealth + (maxHealth * (healthPercentage));
        
        //print("Health: " + health);
        enemyHealthText.maxValue = health;
    }

    public void SetBasicDamageAttackIncrease(float damagePercentage) 
    {
        if (GameManager.gameManager.gameLevel == 1) basicDamageAttack = enemySO.maxBasicDamageAttack;
        else basicDamageAttack += maxBasicDamageAttack + (maxBasicDamageAttack * (damagePercentage)); 
    }

    void PauseAnimation()
    {
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        currentAnimation = state.IsName("EnemyIdle") ? "EnemyIdle" : state.fullPathHash.ToString(); // Store current animation name
        savedTime = state.normalizedTime; // Store current time (normalized)
        animator.speed = 0;
    }

    void ResumeAnimation()
    {
        animator.speed = 1;
        animator.Play(currentAnimation, 0, savedTime); // Resume from saved time
    }
}
