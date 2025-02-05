using UnityEngine;

public class DamageVFX : MonoBehaviour
{
    [SerializeField] private BattleControler battleControler;

    [SerializeField] private GameObject ataque_Agua;
    [SerializeField] private GameObject ataque_Fogo;
    [SerializeField] private GameObject ataque_Folha;
    [SerializeField] private GameObject ataque_Normal;

    private GameObject _focusedEnemy;

    public void Update()
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

            // OrbType.White ou qualquer outro fica ignorado
            default:
                return;
        }

        // Instancia o prefab selecionado na posição do inimigo focado
        if (prefabToSpawn != null)
        {
            Instantiate(prefabToSpawn, _focusedEnemy.transform.position, Quaternion.identity);
        }
    }
}
