using UnityEngine;

public class InputHandler : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private LayerMask cardLayerMask = -1;
    [SerializeField] private LayerMask boardLayerMask = -1;
    [SerializeField] private float dragHeightOffset = 1.0f; // Height to lift card during drag

    private Camera mainCamera;
    
    // Drag & Drop Variables
    private CardVisual draggingCard = null;
    private Vector3 startDragPosition;
    private Plane dragPlane; // Plane for dragging the card
    private bool isDragging = false;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        // 1. Mouse Down (Start Drag or Select Card on Board)
        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseDown();
        }

        // 2. Mouse Drag (Move Card)
        if (isDragging && draggingCard != null)
        {
            HandleMouseDrag();
        }

        // 3. Mouse Up (Attempt to Play Card)
        if (Input.GetMouseButtonUp(0))
        {
            HandleMouseUp();
        }

        HandleKeyboardInput();
    }

    private void HandleMouseDown()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f))
        {
            CardVisual card = hit.collider.GetComponent<CardVisual>();

            // --- ВАРИАНТ 1: Кликнули по КАРТЕ ---
            if (card != null)
            {
                // А. КАРТА НА СТОЛЕ (Возврат в руку)
                if (card.IsOnBoard)
                {
                    if (GameManager.Instance != null)
                    {
                        // Проверяем, наш ли это ряд (нельзя забирать карты врага или те, что уже уехали вперед)
                        if (GameManager.Instance.IsCardOnPlayerRow0(card))
                        {
                            GameManager.Instance.SetSelectedBoardCardForWithdraw(card); // Выбрали
                            GameManager.Instance.TryWithdrawCard(); // ЗАБРАЛИ
                        }
                    }
                    return; // Выходим, чтобы не сработала логика драг-н-дропа
                }

                // Б. КАРТА В РУКЕ (Начинаем перетаскивать)
                if (GameManager.Instance != null && GameManager.Instance.PlayerHand != null)
                {
                    GameManager.Instance.PlayerHand.SelectCard(card);

                    draggingCard = card;
                    startDragPosition = card.transform.position;
                    isDragging = true;
                    dragPlane = new Plane(Vector3.up, new Vector3(0, card.transform.position.y + dragHeightOffset, 0));
                    
                    Collider col = draggingCard.GetComponent<Collider>();
                    if (col) col.enabled = false;
                }
            }
            // --- ВАРИАНТ 2: Кликнули мимо карты ---
            else
            {
                DeselectAll();
            }
        }
        // --- ВАРИАНТ 3: Кликнули в пустоту ---
        else
        {
            DeselectAll();
        }
    }
    // Helper method to clear all selections
    private void DeselectAll()
    {
        // Проверка на null обязательна
        if (GameManager.Instance != null)
        {
            bool wasSelected = false;

            // 1. Снимаем выделение в РУКЕ
            if (GameManager.Instance.PlayerHand != null)
            {
                if (GameManager.Instance.PlayerHand.SelectedCard != null)
                {
                    GameManager.Instance.PlayerHand.DeselectCard();
                    wasSelected = true;
                }
            }
                
            // 2. Снимаем выделение на СТОЛЕ
            if (GameManager.Instance.SelectedBoardCard != null) // Используем свойство из GameManager
            {
                GameManager.Instance.DeselectBoardCard();
                wasSelected = true;
            }

            if (wasSelected) Debug.Log("--- DeselectAll сработал: выделение снято ---");
        }
    }

    private void HandleMouseDrag()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        float enter = 0.0f;

        // Intersect mouse ray with the drag plane
        if (dragPlane.Raycast(ray, out enter))
        {
            // Get point on plane
            Vector3 hitPoint = ray.GetPoint(enter);
            
            // Move card to this point
            draggingCard.transform.position = hitPoint;
        }
    }

    private void HandleMouseUp()
    {
        if (!isDragging || draggingCard == null) return;

        // Re-enable collider
        Collider col = draggingCard.GetComponent<Collider>();
        if (col) col.enabled = true;

        bool cardPlaced = false;

        // Raycast down to find the board
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Look for Board using layer mask
        if (Physics.Raycast(ray, out hit, 100f, boardLayerMask)) 
        {
            if (GameManager.Instance != null)
            {
                BoardManager board = GameManager.Instance.Board;
                if (board != null)
                {
                    int column = board.GetColumnFromWorldPosition(hit.point);
                    
                    // Try to place the card. 
                    // GameManager checks logic (is turn valid? is slot free? card limit reached?)
                    GameManager.Instance.TryPlaceCardInColumn(column);
                    
                    // Check if placement was successful by checking card state
                    if (draggingCard.IsOnBoard) 
                    {
                        cardPlaced = true;
                    }
                }
            }
        }

        // If card was NOT placed (dragged off board, invalid move, etc.)
        if (!cardPlaced)
        {
            // Return card to start position
            draggingCard.transform.position = startDragPosition;
            
            // Optional: You might want to keep it selected or deselect it here depending on preference.
            // keeping it selected is usually better UX so they can try again or see info.
        }

        // Reset variables
        draggingCard = null;
        isDragging = false;
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

        if (Input.GetKeyDown(KeyCode.R))
        {
            if (GameManager.Instance != null &&
                (GameManager.Instance.CurrentState == GameState.Victory ||
                 GameManager.Instance.CurrentState == GameState.GameOver))
            {
                GameManager.Instance.RestartGame();
            }
        }
    }
}