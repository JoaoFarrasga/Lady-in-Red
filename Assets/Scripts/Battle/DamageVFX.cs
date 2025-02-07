using UnityEngine;
using System.Collections;

public class DamageVFX : MonoBehaviour
{
    [SerializeField] private BattleControler battleControler;

    [Header("Prefabs dos Ataques")]
    [SerializeField] private GameObject ataque_Agua;
    [SerializeField] private GameObject ataque_Fogo;
    [SerializeField] private GameObject ataque_Folha;
    [SerializeField] private GameObject ataque_Normal;

    [Header("Refer�ncias")]
    [SerializeField] private GameObject Livro;

    private GameObject _focusedEnemy;

    [Header("AudioManager")]
    [SerializeField] AudioManager audioManager;

    private void Update()
    {
        _focusedEnemy = battleControler.focusedEnemy;
    }

    public void DamgeVFXStart(OrbType ataqueType)
    {
        if (_focusedEnemy == null) return;

        GameObject prefabToSpawn = null;

        switch (ataqueType)
        {
            case OrbType.Red:
                prefabToSpawn = ataque_Fogo;
                break;

            case OrbType.Blue:
                prefabToSpawn = ataque_Agua;
                break;

            case OrbType.Green:
                prefabToSpawn = ataque_Folha;
                break;

            case OrbType.Purple:
                prefabToSpawn = ataque_Normal;
                break;

            // Se for outra cor, n�o faz nada
            default:
                return;
        }

        // Instancia o prefab na posi��o do Livro
        if (prefabToSpawn != null)
        {
            // Instancia na posi��o do livro
            GameObject projectile = Instantiate(
                prefabToSpawn,
                Livro.transform.position,
                Quaternion.identity);

            // Inicia a coroutine para mover o projetil at� o inimigo
            StartCoroutine(MoveProjectileToEnemy(projectile, _focusedEnemy.transform.position));
        }
    }

    private IEnumerator MoveProjectileToEnemy(GameObject projectile, Vector3 targetPos, float duration = 1.0f)
    {
        float timeElapsed = 0f;
        Vector3 startPos = projectile.transform.position;

        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            float t = timeElapsed / duration;

            // Interpola entre a posi��o inicial (startPos) e a posi��o do inimigo (targetPos)
            projectile.transform.position = Vector3.Lerp(startPos, targetPos, t);

            yield return null;
        }

        // Garantir que ao final esteja exato na posi��o do inimigo
        projectile.transform.position = targetPos;
        audioManager.SFXClip(audioManager.attackSound);

        // Se quiser, podemos destruir o projetil depois de chegar
        // ou disparar alguma anima��o adicional, etc.
        Destroy(projectile);
    }
}
