using UnityEngine;

public class BattleControler : MonoBehaviour
{
    [Header("BattleState")]
    private BattleState battleState;

    [Header("EnemyInfo")]
    [SerializeField] GameObject enemyPrefab;

    [Header("ReferencePoint")]
    [SerializeField] private GameObject referencePoint;
    [SerializeField] private float totalEnemySpace;

    [Header("BattleGeneratorInfo")]
    [SerializeField] private BattleGenerator battleGenerator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        SetUpBattle();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void UpdateBattleState(BattleState newbattleState)
    {
        battleState = newbattleState;

        switch (battleState)
        {
            case BattleState.BattleInit:
                SetUpBattle();
                break;
            case BattleState.PlayerTurn:
                // Manage Player Turn
                break;
            case BattleState.EnemyTurn:
                // Manage Enemy Turn
                break;
            case BattleState.BattleEnd:
                BattleEnd();
                break;
            default:
                break;
        }
    }

    private void SetUpBattle()
    {
        PlaceEnemies(1);
        UpdateBattleState(BattleState.PlayerTurn);
    }

    private void BattleEnd()
    {
        // Logic for ending the battle
    }

    private void PlaceEnemies(int level)
    {
        bool isPlacingOnRightSide = true;
        int offsetMultiplier = 1;

        float enemyPlaceDivision = (totalEnemySpace * 2) / (battleGenerator.Battles()[level - 1].Count + 1);

        for (int i = 0; i < battleGenerator.Battles()[level - 1].Count; i++)
        {
            GameObject go = Instantiate(enemyPrefab, transform);
            go.AddComponent<EnemyBehaviour>().SetEnemySO(battleGenerator.Battles()[level - 1][i]);

            if (isPlacingOnRightSide)
            {
                float enemySpot = totalEnemySpace - (offsetMultiplier * enemyPlaceDivision);
                go.transform.localPosition += new Vector3(enemySpot, 0, 0);
                isPlacingOnRightSide = !isPlacingOnRightSide;
            }
            else
            {
                float enemySpot = (offsetMultiplier * enemyPlaceDivision) - totalEnemySpace;
                go.transform.localPosition += new Vector3(enemySpot, 0, 0);
                isPlacingOnRightSide = !isPlacingOnRightSide;
                offsetMultiplier += 1;
            }
        }
    }
}


public enum BattleState
{
    BattleInit,
    PlayerTurn,
    EnemyTurn,
    BattleEnd
}
