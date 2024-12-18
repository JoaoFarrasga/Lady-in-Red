using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotionBoard : MonoBehaviour
{
    // Defenir o tamanho do Tabuleiro
    public int width = 6;
    public int height = 8;

    // Defenir o espaço do Tabuleiro
    public float spacingX;
    public float spacingY;

    // Uma referencia dos prefabs das Poções
    public GameObject[] potionPrefabs;

    // Ter uma referencia dos nodes do Tabuleiro e um GameObject
    private Node[ , ] potionBoard;
    public GameObject potionBoardGameObject;

    // LayoutArray
    public ArrayLayout arrayLayout;

    // Public Static do PotionBoard
    public static PotionBoard Instance;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        InitializaBoard();
    }

    void InitializaBoard()
    {
        potionBoard = new Node[width, height];

        spacingX = (float)(width - 1) / 2;
        spacingY = (float)((height - 1) / 2) + 1;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width;  x++)
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
                    potion.GetComponent<Potion>().SetIndicies(x, y);

                    potionBoard[x, y] = new Node(true, potion);
                }
            }
        }

        CheckBoard();
    }

    public bool CheckBoard()
    {
        Debug.Log("CheckingBoard");

        bool hasMatched = false;
        List<Potion> potionsToRemove = new();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Ver se o Node pode ser usado
                if (potionBoard[x, y].isUsable)
                {
                    // Então conseguir a Classe das Poções no Node
                    Potion potion = potionBoard[x, y].potion.GetComponent<Potion>();

                    // Ter a certeza que não tem Match
                    if (!potion.isMatched)
                    {
                        // Fazer a Logica dos Match

                        MatchResult matchedPotions = IsConnected(potion);

                        if (matchedPotions.connectedPotions.Count >= 3)
                        {
                            // Match Complexo

                            potionsToRemove.AddRange(matchedPotions.connectedPotions);

                            foreach (Potion _potion in matchedPotions.connectedPotions)
                                _potion.isMatched = true;

                            hasMatched = true;
                        }
                    }
                }
            }
        }

        return hasMatched;
    }

    MatchResult IsConnected(Potion potion) 
    {
        List<Potion> connectedPotions = new();
        PotionType potionType = potion.potionType;

        connectedPotions.Add(potion);

        // Ver a Direita
        CheckDirection(potion, new Vector2Int(1, 0), connectedPotions);

        // Ver a Esquerda
        CheckDirection(potion, new Vector2Int(-1, 0), connectedPotions);

        // Fizemos um Match de 3 (Horizontal Match)
        if (connectedPotions.Count == 3)
        {
            Debug.Log("Horizontal Match Normal, a cor é: " + connectedPotions[0].potionType);

            return new MatchResult
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection.Horizontal
            };
        }

        // Fizemos um Match de mais do que 3 (LongHorizontal Match)
        else if (connectedPotions.Count > 4)
        {
            Debug.Log("Horizontal Match Longo, a cor é: " + connectedPotions[0].potionType);

            return new MatchResult
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection.LongHorizontal
            };
        }

        // Limpar as ConnectedPotions
        connectedPotions.Clear();

        // Voltar a ler a Posição Inicial
        connectedPotions.Add(potion);

        // Ver em cima
        CheckDirection(potion, new Vector2Int(0, 1), connectedPotions);

        // Ver em baixo
        CheckDirection(potion, new Vector2Int(0, -1), connectedPotions);

        // Fizemos um Match de 3 (Vertical Match)
        if (connectedPotions.Count == 3)
        {
            Debug.Log("Vertical Match Normal, a cor é: " + connectedPotions[0].potionType);

            return new MatchResult
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection.Vertical
            };
        }

        // Fizemos um Match de mais do que 3 (LongVertical Match)
        else if (connectedPotions.Count > 4)
        {
            Debug.Log("Vertical Match Longo, a cor é: " + connectedPotions[0].potionType);

            return new MatchResult
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection.LongVertical
            };
        }
        else
        {
            return new MatchResult
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection.None
            };
        }
    }

    void CheckDirection (Potion potion, Vector2Int direction, List<Potion> connectedPotions)
    {
        PotionType potionType = potion.potionType;

        int x = potion.xIndex + direction.x;
        int y = potion.yIndex + direction.y;

        // Ver se estamos dentro do Board
        while (x >= 0 && x < width && y >= 0 && y < height)
        {
            if (potionBoard[x, y].isUsable)
            {
                Potion neighbourPotion = potionBoard[x, y].potion.GetComponent<Potion>();

                // ver se a Poção Vizinha é do mesmo tempo
                if (!neighbourPotion.isMatched && neighbourPotion.potionType == potionType)
                {
                    connectedPotions.Add(neighbourPotion);

                    x += direction.x;
                    y += direction.y;
                }
                else
                {
                    break;
                }
            }
            else
            {
                break;
            }
        }
    }
}

public class MatchResult
{
    public List<Potion> connectedPotions;
    public MatchDirection direction;
}

public enum MatchDirection
{
    Vertical,
    Horizontal,
    LongVertical,
    LongHorizontal,
    Super,
    None
}
