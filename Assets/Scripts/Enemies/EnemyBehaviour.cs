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
    [SerializeField] private SpriteRenderer healthSprite;

    [Header("EnemySO")]
    [SerializeField] private EnemySO enemySO;

    [Header("EnemyAppearence")]
    //[SerializeField] private GameObject body, face, particles;
    private SpriteRenderer bodySpriteRenderer, faceSpriteRenderer, particleSpriteRenderer;

    [Header("EnemyAnimator")]
    private Animator animator;
    private string currentAnimation;
    private float savedTime;

    [Header("Dead VFX")]
    [SerializeField] private GameObject deadVFX;

    [Header("SFX")]
    [SerializeField] AudioManager audioManager;

    private bool amIFocused = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        enemyHealthText = GetComponentInChildren<Slider>();
        bodySpriteRenderer = transform.Find("Body").GetComponent<SpriteRenderer>();
        faceSpriteRenderer = transform.Find("Face").GetComponent<SpriteRenderer>();
        particleSpriteRenderer = transform.Find("Particle").GetComponent<SpriteRenderer>();
        healthSprite = transform.Find("HealthSprite").GetComponent<SpriteRenderer>();
    }
    private void Start()
    {
        bodySpriteRenderer.sprite = enemySO.bodySprite;
        faceSpriteRenderer.sprite = enemySO.normalFaceSprite;
        if (enemySO.particleSprite != null) particleSpriteRenderer.sprite = enemySO.particleSprite;
        healthSprite.sprite = enemySO.healthSprite;
    }

    public void SetEnemySO(EnemySO enemySO)
    {
        this.enemySO = enemySO;
        maxHealth = enemySO.maxHealth;
        enemyHealthText.maxValue = maxHealth;
        enemyHealthText.value = maxHealth;
        maxBasicDamageAttack = enemySO.maxBasicDamageAttack;

        if (enemySO.enemyType == "Boss")
        {
            // Ajusta a vida e o dano para o Boss
            health = maxHealth;
            basicDamageAttack = maxBasicDamageAttack;

            // Diminui a escala (tamanho) dos sprites pela metade
            bodySpriteRenderer.transform.localScale *= 0.4f;
            faceSpriteRenderer.transform.localScale *= 0.4f;

            // Verifica se existe um particleSprite antes de alterar
            if (particleSpriteRenderer != null)
            {
                particleSpriteRenderer.transform.localScale *= 0.4f;
            }
        }
    }

    public async Task AttackPlayer(GameObject target, BattleControler battleControler)
    {
        PauseAnimation();
        faceSpriteRenderer.sprite = enemySO.madFaceSprite;
        await target.GetComponent<Player>().TakeDamage(basicDamageAttack, battleControler);
        //await Task.Delay(500);
        ResumeAnimation();
        faceSpriteRenderer.sprite = enemySO.normalFaceSprite;
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

            else if (item.Key == enemySO.elementalStrongType)
            {
                damage = -(damage / 2);
                if(health > maxHealth) health = maxHealth;
            }// Dano reduzido contra resist�ncias

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

        if (deadVFX != null)
        {
            Instantiate(deadVFX, transform.position, Quaternion.identity);
        }

        audioManager.SFXClip(audioManager.enemyDeathSound);
        Destroy(gameObject); // Destr�i o inimigo
    }

    public EnemySO GetEnemySO() { return enemySO; }

    public void SetHealthIncrease(float healthPercentage) 
    {
        //print("Maxhealth: " + maxHealth);
        if (GameManager.gameManager.gameLevel == 1) health = maxHealth;
        else
        {
            maxHealth += maxHealth * (healthPercentage);
            health = maxHealth;
        }
        
        //print("Health: " + health);
        enemyHealthText.maxValue = maxHealth;
        enemyHealthText.value = maxHealth;
    }

    public void SetBasicDamageAttackIncrease(float damagePercentage) 
    {
        if (GameManager.gameManager.gameLevel == 1) basicDamageAttack = enemySO.maxBasicDamageAttack;
        else
        {
            maxBasicDamageAttack += maxBasicDamageAttack * (damagePercentage);
            basicDamageAttack = maxBasicDamageAttack;
        }
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
        speedAnimation();
        animator.Play(currentAnimation, 0, savedTime); // Resume from saved time
    }

    void speedAnimation()
    {
        animator.speed = amIFocused ? 2 : 1;
    }

    public void FocusEnemy()
    {
        if (!amIFocused)
        {
            amIFocused = true;
        }

        speedAnimation();
    }

    public void DefocusEnemy()
    {
        if (amIFocused)
        {
            amIFocused = false;
        }

        speedAnimation();
    }

    public void SetDeadVFX(GameObject _deadVFX)
    {
        deadVFX = _deadVFX;
    }

    public void SetAudioManager(AudioManager _audioManager) { audioManager = _audioManager; }
}
