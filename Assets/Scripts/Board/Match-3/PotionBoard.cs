using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;


public class PotionBoard : MonoBehaviour
{

    [Header("Battle Controller")]
    [SerializeField] private BattleControler battleControler;
    [SerializeField] private Player player;
    public int width = 6;
    public int height = 8;
    public float spacingX;
    public float spacingY;
    public GameObject[] potionPrefabs;
    private Node[,] potionBoard;
    public GameObject potionBoardGameObject;
    public List<GameObject> potionsToDestroy = new();
    public GameObject potionParent;
    [SerializeField] private Potion selectedPotion;
    [SerializeField] private bool isProcessingMove;
    public ArrayLayout arrayLayout;
    public static PotionBoard Instance;
    private Camera mainCamera;
    private PlayerInputActions inputActions;
    public Dictionary<OrbType, int> matchCountsByColor = new Dictionary<OrbType, int>();
    public Dictionary<MatchType, int> matchCountsByType = new Dictionary<MatchType, int>();
    public int totalCombos = 0;
    public int totalTurns = 0;

    public bool firstTurn = true;


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

        if (firstTurn && GameManager.gameManager.State == GameState.InBattle) { firstTurn = false; }

        var rayHit = Physics2D.GetRayIntersection(mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue()));

        if (!rayHit.collider) return;

        var potion = rayHit.collider.gameObject.GetComponent<Potion>();

        if (potion != null && battleControler.GetBattleState() == BattleState.PlayerTurn)
        {
            SelectPotion(potion);
        }
    }

    #endregion

    #region Initialize Board

    void InitializeBoard()
    {
        DestroyPotions();
        potionBoard = new Node[width, height];

        spacingX = (float)(width - 1) / 2;
        spacingY = (float)((height - 1) / 2) + 1;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector2 position = new Vector2(x - spacingX, y - spacingY);

                if (arrayLayout.rows[y].row[x])
                {
                    potionBoard[x, y] = new Node(false, null); // Ensure Node is initialized
                }
                else
                {
                    int randomIndex = Random.Range(0, potionPrefabs.Length);
                    GameObject potion = Instantiate(potionPrefabs[randomIndex], position, Quaternion.identity);
                    potion.transform.SetParent(potionParent.transform);

                    potion.GetComponent<Potion>().SetIndicies(x, y);

                    potionBoard[x, y] = new Node(true, potion); // Properly assign the potion reference
                    potionsToDestroy.Add(potion);
                }
            }
        }

        if (potionBoard != null)
        {
            CheckBoard(true, firstTurn);
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

    #region Check Matches

    public bool CheckBoard(bool _takeAction, bool firstTurn, bool isNewTurn = false)
    {
        bool hasMatched = false;
        List<List<Potion>> allMatches = new List<List<Potion>>();

        // Resetando o estado de match para todas as po��es...
        foreach (Node node in potionBoard)
        {
            if (node.potion != null)
            {
                node.potion.GetComponent<Potion>().isMatched = false;
            }
        }

        // Detecta matches...
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (potionBoard[x, y].isUsable)
                {
                    Potion potion = potionBoard[x, y].potion?.GetComponent<Potion>();
                    if (potion != null && !potion.isMatched)
                    {
                        MatchResult matchResult = IsConnected(potion);
                        if (matchResult.connectedPotions.Count >= 3)
                        {
                            List<Potion> currentMatch = new List<Potion>();
                            foreach (Potion matchedPotion in matchResult.connectedPotions)
                            {
                                if (!matchedPotion.isMatched)
                                {
                                    matchedPotion.isMatched = true;
                                    currentMatch.Add(matchedPotion);
                                }
                            }
                            if (currentMatch.Count > 0)
                            {
                                allMatches.Clear();
                                allMatches.Add(currentMatch);
                                hasMatched = true;
                            }
                        }
                    }
                }
            }
        }

        if (_takeAction && hasMatched)
        {
            if (allMatches.Count > 0)
            {
                List<Potion> firstMatchGroup = allMatches[0];
                ProcessMatches(firstMatchGroup, firstTurn);

                RemoveAndRefill(firstMatchGroup);

                // Em vez de chamar WaitOneSecond(), chamamos a espera pelo fim dos movimentos:
                StartCoroutine(WaitAndCheckMatches());
            }
            
            if (_takeAction && !firstTurn && hasMatched && isNewTurn)
            {
                if (battleControler.GetLevelEnemies().Count != 0) player.AttackEnemy(matchCountsByColor, battleControler, totalCombos);
                totalTurns++; // Incrementa os turnos
                totalCombos = 0;
                matchCountsByColor.Clear();
                if (totalTurns == battleControler.maxPlayerTurns)
                {
                    battleControler.UpdateBattleState(BattleState.EnemyTurn);
                    battleControler.maxEnemyTurns = 1;
                    totalTurns = 0;
                }
            }
        }

        return hasMatched;
    }

    private IEnumerator WaitAndCheckMatches()
    {
        // Aguarda que todas as po��es concluam seus movimentos (cascade, spawn, etc.)
        yield return StartCoroutine(WaitForAllPotionsToSettle());
        // Agora, depois que todas as po��es est�o fixas, checa os matches novamente.
        if (CheckBoard(false, firstTurn))
        {
            CheckBoard(true, firstTurn);
        }
    }

    private void ProcessMatches(List<Potion> matchedPotions, bool firstTurn = false)
    {
        if (matchedPotions.Count == 0 || firstTurn) return;

        // Determina se � um SuperMatch ou um Match Normal e incrementa os contadores apropriadamente
        if (matchedPotions.Count == 4)
        {
            totalCombos += 2;  // Adiciona 2 ao contador de combos para SuperMatch
        }
        else if (matchedPotions.Count == 3)
        {
            totalCombos += 1;  // Adiciona 1 ao contador de combos para Match Normal
        }

        // Utiliza um HashSet para garantir que cada cor seja contada apenas uma vez por match
        HashSet<OrbType> uniqueColors = new HashSet<OrbType>();
        foreach (Potion potion in matchedPotions)
        {
            uniqueColors.Add(potion.potionType);
        }

        // Incrementa a contagem para cada cor �nica encontrada no match
        foreach (OrbType type in uniqueColors)
        {
            if (!matchCountsByColor.ContainsKey(type))
            {
                matchCountsByColor[type] = 0;
            }
            matchCountsByColor[type]++;  // Incrementa por 1, independente do n�mero de po��es do mesmo tipo no match
        }
    }

    public enum MatchType
    {
        Normal,    // Para 3 po��es alinhadas
        SuperMatch // Para 4 po��es alinhadas
    }

    MatchResult IsConnected(Potion potion)
    {
        List<Potion> connectedPotions = new();
        OrbType potionType = potion.potionType;

        connectedPotions.Add(potion);

        CheckDirection(potion, new Vector2Int(1, 0), connectedPotions); // Right
        CheckDirection(potion, new Vector2Int(-1, 0), connectedPotions); // Left

        if (connectedPotions.Count >= 3)
        {
            return new MatchResult
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection.Horizontal
            };
        }

        connectedPotions.Clear();
        connectedPotions.Add(potion);

        CheckDirection(potion, new Vector2Int(0, 1), connectedPotions); // Up
        CheckDirection(potion, new Vector2Int(0, -1), connectedPotions); // Down

        if (connectedPotions.Count >= 3)
        {
            return new MatchResult
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection.Vertical
            };
        }

        return new MatchResult { connectedPotions = connectedPotions, direction = MatchDirection.None };
    }

    void CheckDirection(Potion potion, Vector2Int direction, List<Potion> connectedPotions)
    {
        OrbType potionType = potion.potionType;

        int x = potion.xIndex + direction.x;
        int y = potion.yIndex + direction.y;

        while (x >= 0 && x < width && y >= 0 && y < height)
        {
            if (potionBoard[x, y].isUsable && potionBoard[x, y].potion != null)
            {
                Potion neighbourPotion = potionBoard[x, y].potion.GetComponent<Potion>();

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
        if (_matchedResult.direction == MatchDirection.Horizontal || _matchedResult.direction == MatchDirection.LongHorizontal)
        {
            foreach (Potion pot in _matchedResult.connectedPotions)
            {
                List<Potion> extraConnectedPotions = new();
                CheckDirection(pot, new Vector2Int(0, 1), extraConnectedPotions);
                CheckDirection(pot, new Vector2Int(0, -1), extraConnectedPotions);

                if (extraConnectedPotions.Count >= 2)
                {
                    extraConnectedPotions.AddRange(_matchedResult.connectedPotions);

                    return new MatchResult
                    {
                        connectedPotions = extraConnectedPotions,
                        direction = MatchDirection.Super
                    };
                }


            }
            return new MatchResult
            {
                connectedPotions = _matchedResult.connectedPotions,
                direction = _matchedResult.direction

            };
        }
        else if (_matchedResult.direction == MatchDirection.Vertical || _matchedResult.direction == MatchDirection.LongVertical)
        {
            foreach (Potion pot in _matchedResult.connectedPotions)
            {
                List<Potion> extraConnectedPotions = new();
                CheckDirection(pot, new Vector2Int(1, 0), extraConnectedPotions);
                CheckDirection(pot, new Vector2Int(-1, 0), extraConnectedPotions);

                if (extraConnectedPotions.Count >= 2)
                {
                    Debug.Log("I have a super Vertical match");
                    extraConnectedPotions.AddRange(_matchedResult.connectedPotions);

                    return new MatchResult
                    {
                        connectedPotions = extraConnectedPotions,
                        direction = MatchDirection.Super
                    };
                }


            }
            return new MatchResult
            {
                connectedPotions = _matchedResult.connectedPotions,
                direction = _matchedResult.direction

            };
        }
        return null;
    }

    #endregion

    #region Swap Potions

    public void SelectPotion(Potion _potion)
    {
        if (selectedPotion == null)
        {
            selectedPotion = _potion;
        }
        else if (selectedPotion == _potion)
        {
            selectedPotion = null;
        }
        else if (selectedPotion != _potion)
        {
            SwapPosition(selectedPotion, _potion);
            selectedPotion = null;
        }
    }

    private void SwapPosition(Potion _currentPotion, Potion _targetPotion)
    {
        if (!IsAdjacacent(_currentPotion, _targetPotion))
        {
            return;
        }

        DoSwap(_currentPotion, _targetPotion);
        isProcessingMove = true;
        StartCoroutine(ProcessMatchesAfterSwap(_currentPotion, _targetPotion));
    }

    private IEnumerator ProcessMatchesAfterSwap(Potion _currentPotion, Potion _targetPotion)
    {
        // Aguarda um curto per�odo para que a anima��o de swap se inicie.
        yield return new WaitForSeconds(0.2f);

        // Aguarda que todas as po��es terminem suas anima��es (movimentos, cascade, spawn, etc.)
        yield return StartCoroutine(WaitForAllPotionsToSettle());

        bool hasMatch = CheckBoard(true, false, true); // Passa true para isNewTurn

        if (!hasMatch)
        {
            // Se n�o houve match, desfaz a troca
            DoSwap(_currentPotion, _targetPotion);
            // Aguarda novamente para que o swap de desfazer termine (se houver anima��o)
            yield return StartCoroutine(WaitForAllPotionsToSettle());
        }

        isProcessingMove = false;
    }

    private void DoSwap(Potion _currentPotion, Potion _targetPotion)
    {
        GameObject temp = potionBoard[_currentPotion.xIndex, _currentPotion.yIndex].potion;

        potionBoard[_currentPotion.xIndex, _currentPotion.yIndex].potion = potionBoard[_targetPotion.xIndex, _targetPotion.yIndex].potion;
        potionBoard[_targetPotion.xIndex, _targetPotion.yIndex].potion = temp;

        int tempXIndex = _currentPotion.xIndex;
        int tempYIndex = _currentPotion.yIndex;

        _currentPotion.xIndex = _targetPotion.xIndex;
        _currentPotion.yIndex = _targetPotion.yIndex;

        _targetPotion.xIndex = tempXIndex;
        _targetPotion.yIndex = tempYIndex;

        _currentPotion.MoveToTarget(potionBoard[_targetPotion.xIndex, _targetPotion.yIndex].potion.transform.position);
        _targetPotion.MoveToTarget(potionBoard[_currentPotion.xIndex, _currentPotion.yIndex].potion.transform.position);
    }

    private bool IsAdjacacent(Potion _currentPotion, Potion _targetPotion)
    {
        return Mathf.Abs(_currentPotion.xIndex - _targetPotion.xIndex) + Mathf.Abs(_currentPotion.yIndex - _targetPotion.yIndex) == 1;
    }

    #endregion

    #region Cascade Potions

    private void RemoveAndRefill(List<Potion> _potionsToRemove)
    {
        foreach (Potion potion in _potionsToRemove)
        {
            int _xIndex = potion.xIndex;
            int _yIndex = potion.yIndex;

            potion.animator.SetTrigger("Destroyed");

            Destroy(potion.gameObject);

            potionBoard[_xIndex, _yIndex] = new Node(true, null);
        }
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
        //y offset
        int yOffset = 1;

        while (y + yOffset < height && potionBoard[x, y + yOffset].potion == null)
        {
            // Debug.Log("The potion above me is null, but im not at the top of the board yet, so add to my yOffset and try again, Current Offset is: " + yOffset + "Im about do add 1");
            yOffset++;
        }

        if (y + yOffset < height && potionBoard[x, y + yOffset].potion != null)
        {
            Potion potionAbove = potionBoard[x, y + yOffset].potion.GetComponent<Potion>();

            Vector3 targetPos = new Vector3(x - spacingX, y - spacingY, potionAbove.transform.position.z);
            //Debug.Log("I�ve found a potion when reflling the board and it was in the location: [" + x + "," + (y + yOffset) + "] we have moved it to the location:[" + x + "," + y + "]");
            potionAbove.MoveToTarget(targetPos);
            potionAbove.SetIndicies(x, y);
            potionBoard[x, y] = potionBoard[x, y + yOffset];
            potionBoard[x, y + yOffset] = new Node(true, null);
        }

        if (y + yOffset == height)
        {
            //Debug.Log("I�ve reached the top of the board without finding a potion");
            SpawnPotionAtTop(x);
        }


    }

    private void SpawnPotionAtTop(int x)
    {
        int index = FindIndexOflowestNull(x);
        int locationToMoveTo = 8 - index;
        //Debug.Log("About to spawn a potion, ideally i�d like to put it in the index of " + index);
        //get a random potion
        int randomIndex = Random.Range(0, potionPrefabs.Length);
        GameObject newPotion = Instantiate(potionPrefabs[randomIndex], new Vector2(x - spacingX, height - spacingY), Quaternion.identity);
        newPotion.transform.SetParent(potionParent.transform);
        //set indicies
        newPotion.GetComponent<Potion>().SetIndicies(x, index);
        //set it on the potion board
        potionBoard[x, index] = new Node(true, newPotion);
        //move it to that location
        Vector3 targetPostion = new Vector3(newPotion.transform.position.x, newPotion.transform.position.y - locationToMoveTo, newPotion.transform.position.z);
        newPotion.GetComponent<Potion>().MoveToTarget(targetPostion);
    }

    private int FindIndexOflowestNull(int x)
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

    private IEnumerator WaitForAllPotionsToSettle()
    {
        bool anyMoving = true;
        while (anyMoving)
        {
            anyMoving = false;
            // Percorre o tabuleiro para ver se alguma po��o est� em movimento.
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (potionBoard[x, y].potion != null)
                    {
                        Potion potion = potionBoard[x, y].potion.GetComponent<Potion>();
                        if (potion != null && potion.isMoving)
                        {
                            anyMoving = true;
                            break;
                        }
                    }
                }
                if (anyMoving)
                    break;
            }
            yield return null;
        }
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