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
            if (CheckBoard(false))
            {
                InitializeBoard();
            }
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

    public bool CheckBoard(bool _takeAction, bool isNewTurn = false)
    {
        bool hasMatched = false;
        List<List<Potion>> allMatches = new List<List<Potion>>();
        List<Potion> lastMatch = new List<Potion>();

        // Reseta o estado 'isMatched' para todas as poções
        foreach (Node node in potionBoard)
        {
            if (node.potion != null)
            {
                node.potion.GetComponent<Potion>().isMatched = false;
            }
        }

        // Detectar matches
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (potionBoard[x, y].isUsable)
                {
                    Potion potion = potionBoard[x, y].potion.GetComponent<Potion>();
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
            foreach (List<Potion> matchGroup in allMatches)
            {
                ProcessMatches(matchGroup);
                lastMatch = matchGroup;
            }
            
            //playerCharacteristics.Attack(lastMatch, battleControler.GetLevelEnemies(), matchCountsByColor);
            RemoveAndRefill(allMatches.SelectMany(m => m).ToList());

            // Reseta o estado 'isMatched' após processar todos os matches
            foreach (var matchGroup in allMatches)
            {
                foreach (Potion potion in matchGroup)
                {
                    potion.isMatched = false;
                }
            }

            if (CheckBoard(false))
            {
                CheckBoard(true);
            }
        }

        if (_takeAction && isNewTurn && hasMatched)
        {
            print("LastMatch: " + lastMatch[0].potionType);
            foreach (var item in matchCountsByColor)
            {
                //Debug.Log($"{item.Key}: {item.Value} matches");
                //matchCountsByColor[item.Key] *= totalCombos;
            }
            player.Attack(lastMatch, matchCountsByColor, battleControler, totalCombos);
            totalTurns++; // Incrementa os turnos apenas no início do processamento de uma nova jogada
            totalCombos = 0;
            matchCountsByColor.Clear();
            if (totalTurns == battleControler.maxPlayerTurns)
            {
                battleControler.UpdateBattleState(BattleState.EnemyTurn);
                battleControler.maxEnemyTurns = 1;
                totalTurns = 0;
            }
            //PrintMatchStats();  // Imprime estatísticas apenas uma vez após processar todos os matches
        }

        return hasMatched;
    }

    private void ProcessMatches(List<Potion> matchedPotions)
    {
        if (matchedPotions.Count == 0) return;

        // Determina se é um SuperMatch ou um Match Normal e incrementa os contadores apropriadamente
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

        // Incrementa a contagem para cada cor única encontrada no match
        foreach (OrbType type in uniqueColors)
        {
            if (!matchCountsByColor.ContainsKey(type))
            {
                matchCountsByColor[type] = 0;
            }
            matchCountsByColor[type]++;  // Incrementa por 1, independente do número de poções do mesmo tipo no match
        }
    }




    private void PrintMatchStats()
    {
        Debug.Log("Match Stats:");
        Debug.Log($"Total Turnos: {totalTurns}");
        Debug.Log($"Total Combos: {totalCombos}");
        foreach (var item in matchCountsByColor)
        {
            Debug.Log($"{item.Key}: {item.Value} matches");
        }
    }


    public enum MatchType
    {
        Normal,    // Para 3 poções alinhadas
        SuperMatch // Para 4 poções alinhadas
    }

    private void RemoveAndRefill(List<Potion> _potionsToRemove)
    {
        //removing the potion and clearing the board at that location
        foreach(Potion potion in _potionsToRemove)
        {
            //getting it´s x and y indicates and storing them
            int _xIndex = potion.xIndex;
            int _yIndex = potion.yIndex;

            //Destroy the potion
            Destroy(potion.gameObject);

            //create a blank node on the potion board
            potionBoard[_xIndex, _yIndex] = new Node(true, null);
        }
        for (int x=0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (potionBoard[x, y].potion == null)
                {
                //Debug.Log("The location X:" + x + "Y" + y + "is empty, attepting to refill it");
                RefillPotion(x, y);
                }
            }

        }
    }

    private void RefillPotion(int x, int y)
    {
        //y offset
        int yOffset = 1;

        //while the cell above our current cell is null and we´re below the height of the board
        while (y + yOffset < height && potionBoard[x,y + yOffset].potion == null)
        {
            //Debug.Log("The potion above me is null, but im not at the top of the board yet, so add to my yOffset and try again, Current Offset is: " + yOffset + "Im about do add 1");
            yOffset++;
        }
        //we´ve either hit the top of the board or we found the potion
        if (y + yOffset < height && potionBoard[x, y + yOffset].potion != null)
        { 
            //we´ve founf the potion
            Potion potionAbove = potionBoard[x,y + yOffset].potion.GetComponent<Potion>();
            
            //move it to the current location
            Vector3 targetPos = new Vector3(x - spacingX, y - spacingY, potionAbove.transform.position.z);
            //Debug.Log("I´ve found a potion when reflling the board and it was in the location: [" + x + "," + (y + yOffset) + "] we have moved it to the location:[" + x + "," + y + "]");
            //Move to location
            potionAbove.MoveToTarget(targetPos);
            //update incidies
            potionAbove.SetIndicies(x, y);
            //update our potionboard
            potionBoard[x, y] = potionBoard[x,y + yOffset];
            //set the location the potion came from to null
            potionBoard[x, y + yOffset] = new Node(true, null);
        }
        //if we´ve hit the top of the board without finding a potion
        if (y + yOffset == height)
        {
            //Debug.Log("I´ve reached the top of the board without finding a potion");
            SpawnPotionAtTop(x);
        }
            

    }

    private void SpawnPotionAtTop(int x)
    {
        int index = FindIndexOflowestNull(x);
        int locationToMoveTo = 8 - index;
        //Debug.Log("About to spawn a potion, ideally i´d like to put it in the index of " + index);
        //get a random potion
        int randomIndex = Random.Range(0, potionPrefabs.Length);
        GameObject newPotion = Instantiate(potionPrefabs[randomIndex], new Vector2(x - spacingX, height - spacingY), Quaternion.identity);
        newPotion.transform.SetParent(potionParent.transform);
        //set indicies
        newPotion.GetComponent<Potion>().SetIndicies(x, index);
        //set it on the potion board
        potionBoard[x,index] = new Node(true, newPotion);
        //move it to that location
        Vector3 targetPostion = new Vector3(newPotion.transform.position.x, newPotion.transform.position.y - locationToMoveTo, newPotion.transform.position.z);
        newPotion.GetComponent<Potion>().MoveToTarget(targetPostion);
    }


    private int FindIndexOflowestNull(int x)
    {
        int lowestNull = 99;
        for (int y = 7; y >= 0; y--) 
        {
            if (potionBoard[x,y].potion == null)
            {
                lowestNull = y;
            }
        }
        return lowestNull;
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
                CheckDirection(pot, new Vector2Int(0,1), extraConnectedPotions);
                CheckDirection(pot, new Vector2Int(0, -1), extraConnectedPotions);

                if(extraConnectedPotions.Count >= 2)
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
                CheckDirection(pot, new Vector2Int(1,0), extraConnectedPotions);
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
        else if(selectedPotion != _potion) 
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
        yield return new WaitForSeconds(0.2f); // Atraso para animação, se necessário
        bool hasMatch = CheckBoard(true, true); // Passa true para isNewTurn

        if (!hasMatch)
        {
            // Se não houve match, desfaz a troca
            DoSwap(_currentPotion, _targetPotion);
        }

        isProcessingMove = false;
    }

    private void DoSwap(Potion _currentPotion, Potion _targetPotion)
    {
        GameObject temp = potionBoard[_currentPotion.xIndex,_currentPotion.yIndex].potion;

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

    private IEnumerator ProcessMatches(Potion _currentPotion, Potion _targetPotion)
    {
        yield return new WaitForSeconds(0.2f);
        bool hasMatch = CheckBoard(true);

        if (!hasMatch)
        {
            DoSwap(_currentPotion, _targetPotion);
        }

        isProcessingMove = false;
    }

    #endregion

    #region Cascade Potions




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
