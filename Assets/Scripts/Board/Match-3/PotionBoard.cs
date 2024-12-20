using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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

    public List<GameObject> potionsToDestroy = new();
    public GameObject potionParent;

    [SerializeField] private Potion selectedPotion;
    [SerializeField] private bool isProcessingMove;

    // LayoutArray
    public ArrayLayout arrayLayout;

    // Public Static do PotionBoard
    public static PotionBoard Instance;

    private Camera mainCamera;
    private PlayerInputActions inputActions;

    private void Awake()
    {
        Instance = this;

        mainCamera = Camera.main;
        inputActions = new PlayerInputActions();
    }

    void Start()
    {
        InitializeBoard();
    }

    #region Click

    private void OnEnable()
    {
        inputActions.Enable();
        inputActions.Gameplay.Click.performed += OnClickPerformed;
    }

    private void OnDisable()
    {
        inputActions.Gameplay.Click.performed -= OnClickPerformed;
        inputActions.Disable();
    }

    private void OnClickPerformed(InputAction.CallbackContext context)
    {
        if (isProcessingMove) return;

        var rayHit = Physics2D.GetRayIntersection(mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue()));

        if (!rayHit.collider) return;

        var potion = rayHit.collider.gameObject.GetComponent<Potion>();
        if (potion != null)
        {
            SelectPotion(potion);
        }
    }

    #endregion

    #region Criar o Board

    void InitializeBoard()
    {
        DestroyPotions();
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
                    potion.transform.SetParent(potionParent.transform);

                    potion.GetComponent<Potion>().SetIndicies(x, y);

                    potionBoard[x, y] = new Node(true, potion);
                    potionsToDestroy.Add(potion);
                }
            }
        }

        if (CheckBoard(false))
        {
            InitializeBoard();
        }
    }

    private void DestroyPotions()
    {
        if (potionsToDestroy != null)
        {
            foreach (GameObject potion in potionsToDestroy)
            {
                Destroy(potion);
            }
            potionsToDestroy.Clear();
        }
    }

    #endregion

    #region Ver os Matches

    public bool CheckBoard(bool _takeAction)
    {      
        bool hasMatched = false;
        List<Potion> potionsToRemove = new();

        foreach (Node nodePotion in potionBoard)
        {
            if (nodePotion != null && nodePotion.potion != null)
            {
                nodePotion.potion.GetComponent<Potion>().isMatched = false;
            }
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (potionBoard[x, y].isUsable)
                {
                    Potion potion = potionBoard[x, y].potion?.GetComponent<Potion>();
                    if (potion != null && !potion.isMatched)
                    {
                        MatchResult matchedPotions = IsConnected(potion);

                        if (matchedPotions.connectedPotions.Count >= 3)
                        {
                            MatchResult superMatchPotions = SuperMatch(matchedPotions);

                            potionsToRemove.AddRange(superMatchPotions.connectedPotions);

                            foreach (Potion _potion in superMatchPotions.connectedPotions)
                                _potion.isMatched = true;

                            hasMatched = true;
                        }
                    }
                }
            }
        }

        // Remover as poções marcadas como 'matched' quando necessário
        if (_takeAction)
        {
            foreach (Potion potionToRemove in potionsToRemove)
            {
                potionToRemove.isMatched = false;
            }

            RemoveAndReffil(potionsToRemove); // Certifique-se que o RemoveAndReffil está sendo chamado corretamente

            // Verifique se há mais matches após a remoção das poções
            if (CheckBoard(false))
            {
                CheckBoard(true);
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
            return new MatchResult
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection.Horizontal
            };
        }

        // Fizemos um Match de mais do que 3 (LongHorizontal Match)
        else if (connectedPotions.Count > 4)
        {
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
            return new MatchResult
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection.Vertical
            };
        }

        // Fizemos um Match de mais do que 3 (LongVertical Match)
        else if (connectedPotions.Count > 4)
        {
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
            if (potionBoard[x, y].isUsable && potionBoard[x, y].potion != null)
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

    private MatchResult SuperMatch(MatchResult _matchedResult)
    {
        // Se tivermos um Horizontal Match ver se existe um Vertical Match em todos eles
        if (_matchedResult.direction == MatchDirection.Horizontal || _matchedResult.direction == MatchDirection.LongHorizontal)
        {
            // Para cada Poção...
            foreach (Potion potion in _matchedResult.connectedPotions)
            {
                List<Potion> extraConnectedPotions = new();

                // Ver em Cima
                CheckDirection(potion, new Vector2Int(0, 1), extraConnectedPotions);

                // Ver em Baixo
                CheckDirection(potion, new Vector2Int(0, -1), extraConnectedPotions);

                // Temos 2 ou mais poções que foram encontradas em Match com este Node
                if (extraConnectedPotions.Count >= 2)
                {
                    extraConnectedPotions.AddRange(_matchedResult.connectedPotions);

                    // Return Super Match
                    return new MatchResult
                    {
                        connectedPotions = extraConnectedPotions,
                        direction = MatchDirection.Super
                    };
                }
            }
       
            // Não existe Super Match
            return new MatchResult
            {
                connectedPotions = _matchedResult.connectedPotions,
                direction = _matchedResult.direction,
            };
        }

        // Se tivermos um Vertical Match ver se existe um Horizontal Match em todos eles
        else if (_matchedResult.direction == MatchDirection.Vertical || _matchedResult.direction == MatchDirection.LongVertical)
        {
            // Para cada Poção...
            foreach (Potion potion in _matchedResult.connectedPotions)
            {
                List<Potion> extraConnectedPotions = new();

                // Ver para a Direita
                CheckDirection(potion, new Vector2Int(1, 0), extraConnectedPotions);

                // Ver para a Esquerda
                CheckDirection(potion, new Vector2Int(-1, 0), extraConnectedPotions);

                // Temos 2 ou mais poções que foram encontradas em Match com este Node
                if (extraConnectedPotions.Count >= 2)
                {
                    extraConnectedPotions.AddRange(_matchedResult.connectedPotions);

                    // Return Super Match
                    return new MatchResult
                    {
                        connectedPotions = extraConnectedPotions,
                        direction = MatchDirection.Super
                    };
                }
            }

            // Não existe Super Match
            return new MatchResult
            {
                connectedPotions = _matchedResult.connectedPotions,
                direction = _matchedResult.direction,
            };
        }

        return null;
    }

    #endregion

    #region Trocar os Nodes

    // Selecionar uma Poção
    public void SelectPotion(Potion _potion)
    {
        // Se não tivermos selecionado uma poção, selecionar esta poção
        if (selectedPotion == null)
        {
            selectedPotion = _potion;
        }

        // Se for selecionada a mesma poção outra vez, retirar essa poção da Seleção
        else if (selectedPotion == _potion)
        {
            selectedPotion = null;
        }

        // Se a Poção Selecionada não é null e não for a poção que está selecionada, Trocar poções
        // Por as Selected Potions como Null
        else if (selectedPotion != _potion)
        {
            SwapPosition(selectedPotion, _potion);
            selectedPotion = null;
        }
    }

    // Trocar a Poção com outra
    private void SwapPosition(Potion _currentPotion, Potion _targetPotion)
    {
        if (!IsAdjacacent(_currentPotion, _targetPotion))
        {
            return;
        }

        DoSwap(_currentPotion, _targetPotion);

        isProcessingMove = true;

        StartCoroutine(ProcessMatches(_currentPotion, _targetPotion));
    }

    // DoSwap
    private void DoSwap(Potion _currentPotion, Potion _targetPotion)
    {
        // Trocar as posições no tabuleiro
        var currentNode = potionBoard[_currentPotion.xIndex, _currentPotion.yIndex];
        var targetNode = potionBoard[_targetPotion.xIndex, _targetPotion.yIndex];

        potionBoard[_currentPotion.xIndex, _currentPotion.yIndex] = targetNode;
        potionBoard[_targetPotion.xIndex, _targetPotion.yIndex] = currentNode;

        // Atualizar os índices de ambas as poções
        int tempXIndex = _currentPotion.xIndex;
        int tempYIndex = _currentPotion.yIndex;

        _currentPotion.xIndex = _targetPotion.xIndex;
        _currentPotion.yIndex = _targetPotion.yIndex;
        _targetPotion.xIndex = tempXIndex;
        _targetPotion.yIndex = tempYIndex;

        // Mover as poções para suas novas posições visuais
        _currentPotion.MoveToTarget(new Vector2(_targetPotion.xIndex - spacingX, _targetPotion.yIndex - spacingY));
        _targetPotion.MoveToTarget(new Vector2(_currentPotion.xIndex - spacingX, _currentPotion.yIndex - spacingY));
    }


    // IsAdjacent
    private bool IsAdjacacent(Potion _currentPotion, Potion _targetPotion)
    {
        return Mathf.Abs(_currentPotion.xIndex - _targetPotion.xIndex) + Mathf.Abs(_currentPotion.yIndex - _targetPotion.yIndex) == 1;
    }

    // ProcessMatches
    private IEnumerator ProcessMatches(Potion _currentPotion, Potion _targetPotion)
    {
        yield return new WaitForSeconds(0.2f);

        bool hasMatch = CheckBoard(true);

        if (!hasMatch)
        {
            DoSwap(_targetPotion, _currentPotion);
        }

        isProcessingMove = false;
    }


    #endregion

    #region Nodes a Cair

    private void RemoveAndReffil(List<Potion> _potionsToRemove)
    {
        Debug.Log($"Removendo {_potionsToRemove.Count} poções");

        // Remover os Nodes e Limpar essa localização
        foreach (Potion potion in _potionsToRemove)
        {
            Debug.Log($"Removendo poção {potion.gameObject.name}");
            // Ter a posição do Node e Armazená-los
            int _xIndex = potion.xIndex;
            int _yIndex = potion.yIndex;

            // Destuir o Node
            Destroy(potion.gameObject);

            // Criar um Null no Tabuleiro
            potionBoard[_xIndex, _yIndex] = new Node(true, null);
        }

        // Preencher novamente os espaços vazios
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (potionBoard[x, y].potion == null)
                {
                    RefillPotion(x, y);
                }
            }
        }
    }


    private void RefillPotion(int x, int y)
    {
        int yOffset = 1;

        while (y + yOffset < height && potionBoard[x, y + yOffset].potion == null)
        {
            yOffset++;
        }

        if (y + yOffset < height && potionBoard[x, y + yOffset].potion != null)
        {
            Potion potionAbove = potionBoard[x, y + yOffset].potion.GetComponent<Potion>();

            Vector3 targetPos = new Vector3(x - spacingX, y - spacingY, potionAbove.transform.position.z);

            potionAbove.MoveToTarget(targetPos);

            potionAbove.SetIndicies(x, y);

            potionBoard[x, y] = potionBoard[x, yOffset];

            potionBoard[x, yOffset] = new Node(true, null);
        }

        if (y + yOffset == height)
        {
            SpawnPotionAtTop(x);
        }
    }

    private void SpawnPotionAtTop(int x)
    {
        int index = FindIndexOfLowestNUll(x);
        int locationToMoveTo = 8 - index;

        int randomIndex = Random.Range(0, potionPrefabs.Length);
        GameObject newPotion = Instantiate(potionPrefabs[randomIndex], new Vector2(x - spacingX, height - spacingY), Quaternion.identity);
        newPotion.transform.SetParent(potionParent.transform);

        newPotion.GetComponent<Potion>().SetIndicies(x, index);

        potionBoard[x, index] = new Node(true, newPotion);

        Vector3 targetPosition = new Vector3(newPotion.transform.position.x, newPotion.transform.position.y - locationToMoveTo, newPotion.transform.position.z);
        newPotion.GetComponent<Potion>().MoveToTarget(targetPosition);
    }

    private int FindIndexOfLowestNUll(int x)
    {
        int lowestNull = 99;

        for (int y = 7; y >= 0; y--)
        {
            if (potionBoard[x, y].potion == null)
            {
                lowestNull = y;
            }
        }

        return lowestNull;
    }

    #endregion
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
