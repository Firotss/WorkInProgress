using UnityEngine;

public class InputHandler : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private LayerMask cardLayerMask = -1;
    [SerializeField] private LayerMask boardLayerMask = -1;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            HandleClick();
        HandleKeyboardInput();
    }

    private void HandleClick()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return;
        }

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f))
        {
            CardVisual card = hit.collider.GetComponent<CardVisual>();
            if (card != null)
            {
                if (card.IsOnBoard && GameManager.Instance != null &&
                    GameManager.Instance.IsCardOnPlayerRow0(card))
                {
                    GameManager.Instance.SetSelectedBoardCardForWithdraw(card);
                }
                return;
            }

            if (GameManager.Instance != null &&
                GameManager.Instance.CurrentState == GameState.PlayerTurn &&
                GameManager.Instance.PlayerHand != null &&
                GameManager.Instance.PlayerHand.SelectedCard != null)
            {
                TryPlaceCardAtClick(hit.point);
            }
        }
    }

    private void TryPlaceCardAtClick(Vector3 clickPosition)
    {
        if (GameManager.Instance == null) return;

        BoardManager board = GameManager.Instance.Board;
        if (board == null) return;

        int column = board.GetColumnFromWorldPosition(clickPosition);
        GameManager.Instance.TryPlaceCardInColumn(column);
    }

    private void HandleKeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnEndTurnClicked();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (GameManager.Instance != null)
                GameManager.Instance.TryPlaceSelectedCard();
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (GameManager.Instance != null)
                GameManager.Instance.TryWithdrawCard();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            if (GameManager.Instance != null &&
                (GameManager.Instance.CurrentState == GameState.Victory ||
                 GameManager.Instance.CurrentState == GameState.GameOver))
            {
                GameManager.Instance.RestartGame();
            }
        }

        for (int i = 0; i < 5; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                SelectCardByIndex(i);
        }
    }

    private void SelectCardByIndex(int index)
    {
        if (GameManager.Instance == null) return;

        Hand hand = GameManager.Instance.PlayerHand;
        if (hand == null) return;

        var cards = hand.GetCardsInHand();
        if (index >= 0 && index < cards.Count)
            hand.SelectCard(cards[index]);
    }
}
