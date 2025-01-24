using System.Collections.Generic;
using UnityEngine;

public class PotionBoardInitializer : MonoBehaviour
{
    [Header("Potion Board Size")]
    [SerializeField] private int width = 6;
    [SerializeField] private int height = 8;

    [Header("Potion Board Spacing")]
    [SerializeField] public float spacingX;
    [SerializeField] public float spacingY;

    [Header("GameObjects for Potion Board")]
    [SerializeField] public GameObject[] potionPrefabs;
    [SerializeField] private GameObject potionParent;

    [Header("Array Layout")]
    [SerializeField] private ArrayLayout arrayLayout;

    public Node[,] InitializeBoard(ref List<GameObject> potionsToDestroy)
    {
        Node[,] potionBoard = new Node[width, height];

        spacingX = (float)(width - 1) / 2;
        spacingY = (float)((height - 1) / 2) + 1;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector2 position = new Vector2(x - spacingX, y - spacingY);

                if (arrayLayout.rows[y].row[x])
                {
                    potionBoard[x, y] = new Node(false, null);
                }
                else
                {
                    int randomIndex = Random.Range(0, potionPrefabs.Length);
                    GameObject potion = Instantiate(potionPrefabs[randomIndex], position, Quaternion.identity);
                    potion.transform.SetParent(potionParent.transform);

                    potion.GetComponent<Potion>().SetIndicies(x, y);

                    potionBoard[x, y] = new Node(true, potion);
                    potionsToDestroy.Add(potion);
                }
            }
        }

        return potionBoard;
    }

    public void DestroyPotions(List<GameObject> potionsToDestroy)
    {
        foreach (GameObject potion in potionsToDestroy)
        {
            Destroy(potion);
        }
        potionsToDestroy.Clear();
    }
}
