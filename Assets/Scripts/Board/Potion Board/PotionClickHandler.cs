using UnityEngine;
using UnityEngine.InputSystem;

public class PotionClickHandler : MonoBehaviour
{
    private Camera mainCamera;
    private PlayerInputActions inputActions;
    private PotionBoard potionBoard;
    private BattleControler battleControler;

    private void Awake()
    {
        mainCamera = Camera.main;
        inputActions = new PlayerInputActions();
        potionBoard = GetComponent<PotionBoard>();
        battleControler = FindObjectOfType<BattleControler>();

        Debug.Log("Awaked");
    }

    private void OnEnable()
    {
        Debug.Log("Enabled");

        inputActions.Enable();
        inputActions.Gameplay.Click.performed += OnClickPerformed;
    }

    private void OnDisable()
    {
        Debug.Log("Disabled");

        inputActions.Gameplay.Click.performed -= OnClickPerformed;
        inputActions.Disable();
    }

    private void OnClickPerformed(InputAction.CallbackContext context)
    {
        Debug.Log("ClickPerformed");

        if (potionBoard == null || battleControler == null || potionBoard.isProcessingMove) return;

        if (potionBoard.firstTurn && GameManager.gameManager.State == GameState.InBattle)
        {
            potionBoard.firstTurn = false;
        }

        var rayHit = Physics2D.GetRayIntersection(mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue()));

        if (!rayHit.collider) return;

        var potion = rayHit.collider.gameObject.GetComponent<Potion>();

        if (potion != null && battleControler.GetBattleState() == BattleState.PlayerTurn)
        {
            potionBoard.SelectPotion(potion);
        }
    }
}
