using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

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

    // Dicionário de contagem de match por cor
    public Dictionary<OrbType, int> matchCountsByColor = new Dictionary<OrbType, int>();
    // Dicionário de contagem de match por tipo (se quiser usar)
    public Dictionary<MatchType, int> matchCountsByType = new Dictionary<MatchType, int>();

    // Total de combos dentro de um único turno
    public int totalCombos = 0;

    // Total de turnos do jogador (para controlar o limite, etc.)
    public int totalTurns = 0;
    private int gameLevel = 0;
    private int currentGameLevel = 0;

    public bool firstTurn = true;

    // Damage VFX
    [SerializeField] private DamageVFX damageVFX;

    [Header("SFX")]
    [SerializeField] AudioManager audioManager;

    private void Awake()
    {
        Instance = this;
        mainCamera = Camera.main;
        inputActions = new PlayerInputActions();
    }

    void Start()
    {
        InitializeBoard();
        currentGameLevel = GameManager.gameManager.gameLevel;
    }

    private void Update()
    {
        gameLevel = GameManager.gameManager.gameLevel;

        if (gameLevel != currentGameLevel)
        {
            currentGameLevel = gameLevel;
            totalTurns = 0;
        }
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

        if (firstTurn && GameManager.gameManager.State == GameState.InBattle)
        {
            firstTurn = false;
        }

        var rayHit = Physics2D.GetRayIntersection(mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue()));

        if (!rayHit.collider) return;

        var potion = rayHit.collider.gameObject.GetComponent<Potion>();

        if (potion != null && battleControler.GetBattleState() == BattleState.PlayerTurn)
        {
            EnableAndDisableChildSprite(potion.transform);
            SelectPotion(potion);
            audioManager.SFXClip(audioManager.clickSound);
        }
    }

    void EnableAndDisableChildSprite(Transform _transform)
    {
        SpriteRenderer sr = _transform.Find("Luz").GetComponent<SpriteRenderer>();

        if (sr.enabled)
        {
            sr.enabled = false;
        } 
        else
        {
            sr.enabled = true;
        }
    }

    #endregion

    #region Initialize Board

    void InitializeBoard()
    {
        DestroyPotions();
        potionBoard = new Node[width, height];

        spacingX = (float)(((width - 1) / 2) - 3.25);
        spacingY = (float)(((height - 1) / 2) + 1.75);

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                Vector2 position = new Vector2(j - spacingX, i - spacingY);

                if (arrayLayout.rows[i].row[j])
                {
                    potionBoard[j, i] = new Node(false, null); // Espaço "vazio" ou não utilizável
                }
                else
                {
                    int randomIndex = Random.Range(0, potionPrefabs.Length);
                    GameObject potion = Instantiate(potionPrefabs[randomIndex], position, Quaternion.identity);
                    potion.transform.SetParent(potionParent.transform);

                    potion.GetComponent<Potion>().SetIndicies(j, i);

                    potionBoard[j, i] = new Node(true, potion);
                    potionsToDestroy.Add(potion);
                }
            }
        }

        if (potionBoard != null)
        {
            // Checa se já existe algum match inicial (opcional):
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

        // Reseta o estado de match para todas as poções
        foreach (Node node in potionBoard)
        {
            if (node.potion != null)
            {
                node.potion.GetComponent<Potion>().isMatched = false;
            }
        }

        // Detecta matches
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (potionBoard[x, y].isUsable && potionBoard[x, y].potion != null)
                {
                    Potion potion = potionBoard[x, y].potion.GetComponent<Potion>();

                    // Se não está marcado como matched, testamos.
                    if (potion != null && !potion.isMatched)
                    {
                        MatchResult matchResult = IsConnected(potion);

                        // Se há 3 ou mais conectados, é um match.
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
                                // Aqui poderia agrupar todos os matches.
                                // Mas, para simplificar, vamos guardar apenas o último encontrado.
                                allMatches.Clear();
                                allMatches.Add(currentMatch);
                                hasMatched = true;
                            }
                        }
                    }
                }
            }
        }

        // Se detectamos match e queremos tomar ação (remover etc.)
        if (_takeAction && hasMatched)
        {
            if (allMatches.Count > 0)
            {
                List<Potion> firstMatchGroup = allMatches[0];

                // Processa os matches: soma combos, registra tipo do primeiro match, etc.
                ProcessMatches(firstMatchGroup, firstTurn);

                // Remove e faz a cascata
                RemoveAndRefill(firstMatchGroup);

                // Chama corrotina para aguardar settle e checar novamente
                StartCoroutine(WaitAndCheckMatches(_takeAction, firstTurn, isNewTurn));
            }
        }

        return hasMatched;
    }

    private IEnumerator WaitAndCheckMatches(bool _takeAction, bool firstTurn, bool isNewTurn)
    {
        // Espera todas as poções pararem de se mover
        yield return StartCoroutine(WaitForAllPotionsToSettle());

        // Agora checa se ainda há matches
        bool hasMatch = CheckBoard(false, firstTurn);

        if (hasMatch)
        {
            CheckBoard(true, firstTurn, isNewTurn);
        }
        else
        {
            if (_takeAction && !firstTurn && isNewTurn)
            {
                if (battleControler.GetLevelEnemies().Count != 0)
                {   
                    var firstOrbType = matchCountsByColor.Keys.First();
                    damageVFX.DamgeVFXStart(firstOrbType);

                    yield return new WaitForSeconds(1f);

                    player.AttackEnemy(matchCountsByColor, battleControler, totalCombos);
                }

                totalTurns++;

                // Limpamos as variáveis para o próximo turno.
                totalCombos = 0;
                matchCountsByColor.Clear();

                // Se atingimos limite de turnos do jogador
                if (totalTurns == battleControler.maxPlayerTurns)
                {
                    if(battleControler.GetLevelEnemies().Count != 0) battleControler.UpdateBattleState(BattleState.EnemyTurn);
                    battleControler.maxEnemyTurns = 1;
                    totalTurns = 0;
                }
            }
        }
    }

    private void ProcessMatches(List<Potion> matchedPotions, bool firstTurn = false)
    {
        if (matchedPotions.Count == 0 || firstTurn) return;

        // Verifica quantas poções foram combinadas para definir o tipo do match
        // e incrementa combos.
        MatchType matchType;

        if (matchedPotions.Count >= 4)
        {
            matchType = MatchType.SuperMatch;
            totalCombos += 2;  // Exemplo: +2 combos para um SuperMatch (4 peças)
            audioManager.SFXClip(audioManager.comboSound);
        }
        else
        {
            matchType = MatchType.Normal;
            totalCombos += 1;  // Exemplo: +1 combo para um match normal (3 peças)
            audioManager.SFXClip(audioManager.comboSound);
        }

        HashSet<OrbType> uniqueColors = new HashSet<OrbType>();
        foreach (Potion potion in matchedPotions)
        {
            uniqueColors.Add(potion.potionType);
        }

        // Incrementa 1 para cada cor distinta
        foreach (OrbType type in uniqueColors)
        {
            if (!matchCountsByColor.ContainsKey(type))
            {
                matchCountsByColor[type] = 0;
            }
            matchCountsByColor[type]++;
        }
    }

    MatchResult IsConnected(Potion potion)
    {
        List<Potion> connectedPotions = new();
        OrbType potionType = potion.potionType;

        connectedPotions.Add(potion);

        // Checa horizontalmente (esquerda e direita)
        CheckDirection(potion, new Vector2Int(1, 0), connectedPotions);
        CheckDirection(potion, new Vector2Int(-1, 0), connectedPotions);

        if (connectedPotions.Count >= 3)
        {
            return new MatchResult
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection.Horizontal
            };
        }

        // Se não achou 3 na horizontal, checa vertical
        connectedPotions.Clear();
        connectedPotions.Add(potion);

        CheckDirection(potion, new Vector2Int(0, 1), connectedPotions);
        CheckDirection(potion, new Vector2Int(0, -1), connectedPotions);

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
        // Só podemos trocar se forem adjacentes
        if (!IsAdjacent(_currentPotion, _targetPotion))
        {
            EnableAndDisableChildSprite(_currentPotion.transform);
            EnableAndDisableChildSprite(_targetPotion.transform);
            return;
        }

        // Realiza a troca e então processa matches
        DoSwap(_currentPotion, _targetPotion);
        isProcessingMove = true;

        // Reseta as variáveis de contagem, pois é início de um novo turno do jogador
        totalCombos = 0;
        matchCountsByColor.Clear();

        StartCoroutine(ProcessMatchesAfterSwap(_currentPotion, _targetPotion));
    }

    private IEnumerator ProcessMatchesAfterSwap(Potion _currentPotion, Potion _targetPotion)
    {
        // Espera um pouco para a animação de swap
        yield return new WaitForSeconds(0.2f);

        // Espera todas as poções terminarem movimentos
        yield return StartCoroutine(WaitForAllPotionsToSettle());

        // Checa se houve match
        bool hasMatch = CheckBoard(true, false, true);
        // Aqui passamos isNewTurn = true para, ao final das cascatas, disparar o AttackEnemy se tiver combos.

        if (!hasMatch)
        {
            // Se não houve match, desfaz a troca
            DoSwap(_currentPotion, _targetPotion);
            EnableAndDisableChildSprite(_currentPotion.transform);
            EnableAndDisableChildSprite(_targetPotion.transform);
            yield return StartForAllPotionsSettleAgain();
        }

        DisableAllChieldSprite();
        isProcessingMove = false;
    }

    private void DisableAllChieldSprite()
    {
        // Percorre todo o tabuleiro
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Verifica se o Node é utilizável e se há uma poção
                if (potionBoard[x, y] != null && potionBoard[x, y].potion != null)
                {
                    // Acessa o componente Potion
                    Potion potion = potionBoard[x, y].potion.GetComponent<Potion>();

                    // Localiza o filho chamado "Luz"
                    Transform luzTransform = potion.transform.Find("Luz");
                    if (luzTransform != null)
                    {
                        // Desabilita o SpriteRenderer
                        SpriteRenderer sr = luzTransform.GetComponent<SpriteRenderer>();
                        if (sr != null)
                        {
                            sr.enabled = false;
                        }
                    }
                }
            }
        }
    }


    private IEnumerator StartForAllPotionsSettleAgain()
    {
        yield return new WaitForSeconds(0.2f);
        yield return StartCoroutine(WaitForAllPotionsToSettle());
    }

    private void DoSwap(Potion _currentPotion, Potion _targetPotion)
    {
        GameObject temp = potionBoard[_currentPotion.xIndex, _currentPotion.yIndex].potion;

        potionBoard[_currentPotion.xIndex, _currentPotion.yIndex].potion =
            potionBoard[_targetPotion.xIndex, _targetPotion.yIndex].potion;
        potionBoard[_targetPotion.xIndex, _targetPotion.yIndex].potion = temp;

        int tempXIndex = _currentPotion.xIndex;
        int tempYIndex = _currentPotion.yIndex;

        _currentPotion.xIndex = _targetPotion.xIndex;
        _currentPotion.yIndex = _targetPotion.yIndex;

        _targetPotion.xIndex = tempXIndex;
        _targetPotion.yIndex = tempYIndex;

        _currentPotion.MoveToTarget(
            potionBoard[_targetPotion.xIndex, _targetPotion.yIndex].potion.transform.position);
        _targetPotion.MoveToTarget(
            potionBoard[_currentPotion.xIndex, _currentPotion.yIndex].potion.transform.position);
    }

    private bool IsAdjacent(Potion _currentPotion, Potion _targetPotion)
    {
        return Mathf.Abs(_currentPotion.xIndex - _targetPotion.xIndex) +
               Mathf.Abs(_currentPotion.yIndex - _targetPotion.yIndex) == 1;
    }

    #endregion

    #region Cascade Potions

    private void RemoveAndRefill(List<Potion> _potionsToRemove)
    {
        foreach (Potion potion in _potionsToRemove)
        {
            int _xIndex = potion.xIndex;
            int _yIndex = potion.yIndex;

            // Animação de destruir (se tiver)
            if (potion.animator != null)
            {
                potion.animator.SetTrigger("Destroyed");
            }
            if(!firstTurn) audioManager.SFXClip(audioManager.piecesdestroySound);
            Destroy(potion.gameObject);

            potionBoard[_xIndex, _yIndex] = new Node(true, null);
        }

        // "cair" as poções acima
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
        // yOffset para buscar a próxima poção acima
        int yOffset = 1;

        while (y + yOffset < height && potionBoard[x, y + yOffset].potion == null)
        {
            yOffset++;
        }

        // Se encontramos uma poção acima
        if (y + yOffset < height && potionBoard[x, y + yOffset].potion != null)
        {
            Potion potionAbove = potionBoard[x, y + yOffset].potion.GetComponent<Potion>();

            Vector3 targetPos = new Vector3(x - spacingX, y - spacingY, potionAbove.transform.position.z);
            potionAbove.MoveToTarget(targetPos);
            potionAbove.SetIndicies(x, y);

            potionBoard[x, y] = potionBoard[x, y + yOffset];
            potionBoard[x, y + yOffset] = new Node(true, null);
        }
        else if (y + yOffset >= height)
        {
            // Se chegamos no topo sem encontrar nenhuma poção, spawnamos uma nova
            SpawnPotionAtTop(x, y);
        }
    }

    private void SpawnPotionAtTop(int x, int y)
    {
        int randomIndex = Random.Range(0, potionPrefabs.Length);
        GameObject newPotion = Instantiate(potionPrefabs[randomIndex], new Vector2(x - spacingX, height - spacingY), Quaternion.identity);
        newPotion.transform.SetParent(potionParent.transform);

        newPotion.GetComponent<Potion>().SetIndicies(x, y);
        potionBoard[x, y] = new Node(true, newPotion);

        // Move até a posição correta
        Vector3 targetPostion = new Vector3(x - spacingX, y - spacingY, newPotion.transform.position.z);
        newPotion.GetComponent<Potion>().MoveToTarget(targetPostion);
    }

    private IEnumerator WaitForAllPotionsToSettle()
    {
        bool anyMoving = true;
        while (anyMoving)
        {
            anyMoving = false;
            // Percorre o tabuleiro para ver se alguma poção está em movimento
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
                if (anyMoving) break;
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

public enum MatchType
{
    Normal,    // 3 poções
    SuperMatch, // 4 ou mais poções
    None       // Caso não haja match (uso interno)
}
