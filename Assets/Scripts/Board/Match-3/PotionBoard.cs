using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PotionBoard : MonoBehaviour
{
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

        if (potionBoard == null)
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

                            if (superMatchPotions.connectedPotions.Count > 0)
                            {
                                potionsToRemove.AddRange(superMatchPotions.connectedPotions);
                                foreach (Potion matchedPotion in superMatchPotions.connectedPotions)
                                {
                                    matchedPotion.isMatched = true;
                                }
                                hasMatched = true;
                            }
                        }
                    }
                }
            }
        }

        if (_takeAction)
        {
            foreach (Potion potionToRemove in potionsToRemove)
            {
                potionToRemove.isMatched = false;
            }

            RemoveAndRefill(potionsToRemove);

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
        PotionType potionType = potion.potionType;

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
        List<Potion> allMatchedPotions = new List<Potion>(_matchedResult.connectedPotions);

        if (_matchedResult.direction == MatchDirection.Horizontal || _matchedResult.direction == MatchDirection.LongHorizontal)
        {
            foreach (Potion potion in _matchedResult.connectedPotions)
            {
                List<Potion> extraConnectedPotions = new();
                CheckDirection(potion, new Vector2Int(0, 1), extraConnectedPotions); // Up
                CheckDirection(potion, new Vector2Int(0, -1), extraConnectedPotions); // Down

                if (extraConnectedPotions.Count >= 2)
                {
                    allMatchedPotions.AddRange(extraConnectedPotions);
                }
            }
        }

        else if (_matchedResult.direction == MatchDirection.Vertical || _matchedResult.direction == MatchDirection.LongVertical)
        {
            foreach (Potion potion in _matchedResult.connectedPotions)
            {
                List<Potion> extraConnectedPotions = new();
                CheckDirection(potion, new Vector2Int(1, 0), extraConnectedPotions); // Right
                CheckDirection(potion, new Vector2Int(-1, 0), extraConnectedPotions); // Left

                if (extraConnectedPotions.Count >= 2)
                {
                    allMatchedPotions.AddRange(extraConnectedPotions);
                }
            }
        }

        return new MatchResult
        {
            connectedPotions = allMatchedPotions,
            direction = _matchedResult.direction
        };
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
        else
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
        StartCoroutine(ProcessMatches(_currentPotion, _targetPotion));
    }

    private void DoSwap(Potion _currentPotion, Potion _targetPotion)
    {
        var currentNode = potionBoard[_currentPotion.xIndex, _currentPotion.yIndex];
        var targetNode = potionBoard[_targetPotion.xIndex, _targetPotion.yIndex];

        potionBoard[_currentPotion.xIndex, _currentPotion.yIndex] = targetNode;
        potionBoard[_targetPotion.xIndex, _targetPotion.yIndex] = currentNode;

        int tempXIndex = _currentPotion.xIndex;
        int tempYIndex = _currentPotion.yIndex;

        _currentPotion.xIndex = _targetPotion.xIndex;
        _currentPotion.yIndex = _targetPotion.yIndex;
        _targetPotion.xIndex = tempXIndex;
        _targetPotion.yIndex = tempYIndex;

        _currentPotion.MoveToTarget(new Vector2(_targetPotion.xIndex - spacingX, _targetPotion.yIndex - spacingY));
        _targetPotion.MoveToTarget(new Vector2(_currentPotion.xIndex - spacingX, _currentPotion.yIndex - spacingY));
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
            DoSwap(_targetPotion, _currentPotion);
        }

        isProcessingMove = false;
    }

    #endregion

    #region Cascade Potions

    private void RemoveAndRefill(List<Potion> _potionsToRemove)
    {
        // Remove matched potions and clear their positions on the board
        foreach (Potion potion in _potionsToRemove)
        {
            int _xIndex = potion.xIndex;
            int _yIndex = potion.yIndex;

            Destroy(potion.gameObject);
            potionBoard[_xIndex, _yIndex] = new Node(true, null);
        }

        // Cascading: Move potions down to fill empty spaces
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (potionBoard[x, y].potion == null)
                {
                    CascadePotionDown(x, y);
                }
            }
        }
    }

    private void CascadePotionDown(int x, int y)
    {
        // Move potions down to fill the gap
        for (int yOffset = y + 1; yOffset < height; yOffset++)
        {
            if (potionBoard[x, yOffset].potion != null)
            {
                Potion potionAbove = potionBoard[x, yOffset].potion.GetComponent<Potion>();
                potionAbove.MoveToTarget(new Vector3(x - spacingX, y - spacingY, potionAbove.transform.position.z));
                potionAbove.SetIndicies(x, y);

                potionBoard[x, y] = potionBoard[x, yOffset];
                potionBoard[x, yOffset] = new Node(true, null);
                break;
            }
        }

        // Refill the gap with a new potion at the top
        if (potionBoard[x, y].potion == null)
        {
            RefillPotion(x, y);
        }
    }

    private void RefillPotion(int x, int y)
    {
        int randomIndex = Random.Range(0, potionPrefabs.Length);
        GameObject newPotion = Instantiate(potionPrefabs[randomIndex], new Vector2(x - spacingX, height - spacingY), Quaternion.identity);
        newPotion.transform.SetParent(potionParent.transform);
        newPotion.GetComponent<Potion>().SetIndicies(x, y);
        potionBoard[x, y] = new Node(true, newPotion);

        newPotion.GetComponent<Potion>().MoveToTarget(new Vector3(x - spacingX, y - spacingY, newPotion.transform.position.z));
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
