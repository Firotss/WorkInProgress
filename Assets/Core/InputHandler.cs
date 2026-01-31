using UnityEngine;

public class InputHandler : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private LayerMask cardLayerMask; // Увери се, че не е -1 в Инспектора!
    [SerializeField] private LayerMask boardLayerMask; 
    [SerializeField] private float dragHeightOffset = 1.0f;

    private Camera mainCamera;
    private CardVisual draggingCard = null;
    private Vector3 startDragPosition;
    private Plane dragPlane;
    private bool isDragging = false;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) HandleMouseDown();
        if (isDragging && draggingCard != null) HandleMouseDrag();
        if (Input.GetMouseButtonUp(0)) HandleMouseUp();
        
        HandleKeyboardInput();
    }

    private void HandleMouseDown()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f)) // Тук може да добавиш cardLayerMask, ако е настроен
        {
            CardVisual card = hit.collider.GetComponent<CardVisual>();

            if (card != null)
            {
                // 1. КАРТА НА МАСАТА
                if (card.IsOnBoard)
                {
                    if (GameManager.Instance.IsCardOnPlayerRow0(card))
                    {
                        // ЛОГИКА: Първи клик = Избор, Втори клик = Прибиране
                        if (GameManager.Instance.SelectedBoardCard == card)
                        {
                            GameManager.Instance.TryWithdrawCard(); // Втори клик
                        }
                        else
                        {
                            GameManager.Instance.SetSelectedBoardCardForWithdraw(card); // Първи клик
                        }
                    }
                    return;
                }

                // 2. КАРТА В РЪКАТА (Започваме влачене)
                if (GameManager.Instance.PlayerHand != null && !card.IsOnBoard)
                {
                    GameManager.Instance.PlayerHand.SelectCard(card);
                    
                    draggingCard = card;
                    startDragPosition = card.transform.position;
                    isDragging = true;
                    dragPlane = new Plane(Vector3.up, new Vector3(0, card.transform.position.y, 0));
                    
                    Collider col = draggingCard.GetComponent<Collider>();
                    if (col) col.enabled = false;
                }
            }
            else
            {
                // Кликнато е нещо, което не е карта (напр. масата) - махаме селекцията
                DeselectAll();
            }
        }
        else
        {
            // Кликнато е в празно пространство
            DeselectAll();
        }
    }

    private void DeselectAll()
    {
        if (GameManager.Instance == null) return;
        
        GameManager.Instance.DeselectBoardCard();
        // За ръката: Може да решим да не махаме селекцията веднага, за да е по-удобно
        // но ако искаш пълен деселект:
        // GameManager.Instance.PlayerHand.DeselectCard();
    }

    private void HandleMouseDrag()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        float enter;
        if (dragPlane.Raycast(ray, out enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            hitPoint.y = startDragPosition.y + 0.5f; // Леко повдигаме картата
            draggingCard.transform.position = hitPoint;
        }
    }

    private void HandleMouseUp()
    {
        if (!isDragging || draggingCard == null) return;

        Collider col = draggingCard.GetComponent<Collider>();
        if (col) col.enabled = true;

        bool cardPlaced = false;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Търсим масата под мишката
        if (Physics.Raycast(ray, out hit, 100f, boardLayerMask)) 
        {
            if (GameManager.Instance != null && GameManager.Instance.Board != null)
            {
                int column = GameManager.Instance.Board.GetColumnFromWorldPosition(hit.point);
                GameManager.Instance.TryPlaceCardInColumn(column);
                
                // Проверяваме дали картата е успешно поставена (IsOnBoard се задава в BoardManager)
                if (draggingCard.IsOnBoard) cardPlaced = true;
            }
        }

        if (!cardPlaced)
        {
            // Връщаме картата на мястото ѝ в ръката
            draggingCard.transform.position = startDragPosition;
            if (GameManager.Instance?.PlayerHand != null)
            {
                GameManager.Instance.PlayerHand.ArrangeCards(); // Подреждаме ръката отново
            }
        }

        draggingCard = null;
        isDragging = false;
    }
    
    private void HandleKeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
             GameManager.Instance?.OnEndTurnClicked();
    }
}