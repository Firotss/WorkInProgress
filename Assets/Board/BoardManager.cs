using UnityEngine;
using System;
using System.Collections.Generic;

public class BoardManager : MonoBehaviour
{
    [Header("Board Settings")]
    [SerializeField] private int rows = 3;
    [SerializeField] private int columns = 5;
    [SerializeField] private float slotSpacing = 1.2f;
    [SerializeField] private Vector3 boardOffset = new Vector3(-2.4f, 0, 0);
    [SerializeField] private bool isPlayerBoard = true;

    [Header("Visual Settings")]
    [SerializeField] private GameObject slotMarkerPrefab;
    [SerializeField] private Color placementRowColor = new Color(0.3f, 0.3f, 0.8f, 1f);
    [SerializeField] private Color middleRowColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);
    [SerializeField] private Color activationRowColor = new Color(0.8f, 0.3f, 0.3f, 0.5f);

    [Header("References")]
    [SerializeField] private Deck deck;

    private BoardSlot[,] slots;
    private GameObject[,] slotMarkers;
    public bool IsPlayerBoard => isPlayerBoard;
    public int ColumnCount => columns;
    public event Action<Card> OnCardActivated;
    public event Action OnCardsAdvanced;

    private void Awake()
    {
        InitializeBoard();
    }

    public void InitializeBoard()
    {
        slots = new BoardSlot[rows, columns];
        slotMarkers = new GameObject[rows, columns];
        
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                // Create slot data
                slots[row, col] = new BoardSlot(row, col);
                
                // Calculate world position
                Vector3 position = transform.position + boardOffset;
                position.x += col * slotSpacing;
                
                // For player board: row 0 (placement) closest to player, row 2 toward enemy
                // For enemy board: row 0 (placement) closest to enemy, row 2 toward player
                if (isPlayerBoard)
                {
                    position.z += row * slotSpacing; // Row 0 at base, cards advance toward enemy (+Z)
                }
                else
                {
                    position.z -= row * slotSpacing; // Row 0 at base, cards advance toward player (-Z)
                }
                
                slots[row, col].WorldPosition = position;
                
                // Create visual marker
                CreateSlotMarker(row, col, position);
            }
        }
        
        Debug.Log($"{(isPlayerBoard ? "Player" : "Enemy")} Board initialized: {rows}x{columns} grid");
    }

    private void CreateSlotMarker(int row, int col, Vector3 position)
    {
        GameObject marker;
        
        if (slotMarkerPrefab != null)
        {
            marker = Instantiate(slotMarkerPrefab, position, Quaternion.identity, transform);
        }
        else
        {
            // Create a simple quad as marker
            marker = GameObject.CreatePrimitive(PrimitiveType.Quad);
            marker.transform.position = position + Vector3.up * 0.01f;
            marker.transform.rotation = Quaternion.Euler(90, 0, 0);
            marker.transform.localScale = new Vector3(slotSpacing * 0.9f, slotSpacing * 0.9f, 1);
            marker.transform.parent = transform;
            
            // Remove collider from marker
            Collider markerCollider = marker.GetComponent<Collider>();
            if (markerCollider != null)
            {
                Destroy(markerCollider);
            }
        }

        // Set color based on row
        Renderer renderer = marker.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.SetFloat("_Mode", 3);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;
            
            Color color = row switch
            {
                0 => placementRowColor,
                1 => middleRowColor,
                2 => activationRowColor,
                _ => Color.gray
            };
            
            // Tint enemy board slightly different
            if (!isPlayerBoard)
            {
                color = new Color(color.r * 0.8f, color.g * 0.6f, color.b * 0.6f, color.a);
            }
            
            mat.color = color;
            renderer.material = mat;
        }
        
        marker.name = $"Slot_{row}_{col}";
        slotMarkers[row, col] = marker;
    }

    public bool PlaceCard(CardVisual card, int column)
    {
        if (column < 0 || column >= columns)
        {
            Debug.LogWarning($"Invalid column: {column}");
            return false;
        }
        
        BoardSlot slot = slots[0, column];
        
        if (slot.HasCard)
        {
            Debug.LogWarning($"Slot [0,{column}] is already occupied!");
            return false;
        }
        
        if (slot.PlaceCard(card))
        {
            card.transform.position = slot.WorldPosition + Vector3.up * 0.1f;
            card.SetOnBoard(true);
            Debug.Log($"Card placed at slot [0,{column}] on {(isPlayerBoard ? "player" : "enemy")} board");
            return true;
        }
        
        return false;
    }

    public bool PlaceCardAuto(CardVisual card)
    {
        for (int col = 0; col < columns; col++)
        {
            if (!slots[0, col].HasCard)
            {
                return PlaceCard(card, col);
            }
        }

        Debug.LogWarning("No empty slots in placement row!");
        return false;
    }

    public int GetCardRow(CardVisual card)
    {
        if (card == null || slots == null) return -1;
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                if (slots[row, col] != null && slots[row, col].PlacedCard == card)
                    return row;
            }
        }
        return -1;
    }

    public bool TryRemoveCardFromRow0(CardVisual card)
    {
        if (card == null || slots == null) return false;
        for (int col = 0; col < columns; col++)
        {
            if (slots[0, col].PlacedCard == card)
            {
                slots[0, col].RemoveCard();
                card.SetOnBoard(false);
                return true;
            }
        }
        return false;
    }

    public bool HasRow0ColorCombo(string color)
    {
        if (string.IsNullOrEmpty(color) || slots == null) return false;
        for (int startCol = 0; startCol <= columns - 3; startCol++)
        {
            bool allMatch = true;
            for (int c = startCol; c < startCol + 3; c++)
            {
                if (!slots[0, c].HasCard)
                {
                    allMatch = false;
                    break;
                }
                Card card = slots[0, c].PlacedCard?.CardData;
                if (card == null || card.Type == null ||
                    !string.Equals(card.Type.color, color, StringComparison.OrdinalIgnoreCase))
                {
                    allMatch = false;
                    break;
                }
            }
            if (allMatch) return true;
        }
        return false;
    }

    public int GetTotalScoreByColor(string color)
    {
        if (string.IsNullOrEmpty(color) || slots == null) return 0;
        int total = 0;
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                if (slots[row, col] != null && slots[row, col].HasCard)
                {
                    Card card = slots[row, col].PlacedCard?.CardData;
                    if (card != null && card.Type != null &&
                        string.Equals(card.Type.color, color, StringComparison.OrdinalIgnoreCase))
                    {
                        total += card.Score;
                    }
                }
            }
        }
        return total;
    }

    public int GetCountOfCardsByColor(string color)
    {
        if (string.IsNullOrEmpty(color) || slots == null) return 0;
        int count = 0;
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                if (slots[row, col] != null && slots[row, col].HasCard)
                {
                    Card card = slots[row, col].PlacedCard?.CardData;
                    if (card != null && card.Type != null &&
                        string.Equals(card.Type.color, color, StringComparison.OrdinalIgnoreCase))
                    {
                        count++;
                    }
                }
            }
        }
        return count;
    }

    public List<Card> AdvanceCards()
    {
        List<Card> discarded = new List<Card>();

        for (int col = 0; col < columns; col++)
        {
            BoardSlot activationSlot = slots[2, col];
            if (activationSlot.HasCard)
            {
                CardVisual cardVisual = activationSlot.RemoveCard();
                Card cardData = cardVisual.CardData;
                discarded.Add(cardData);
                OnCardActivated?.Invoke(cardData);
                if (deck != null)
                    deck.AddToDiscard(cardData);
                Destroy(cardVisual.gameObject);
            }
        }

        for (int col = 0; col < columns; col++)
        {
            BoardSlot sourceSlot = slots[1, col];
            BoardSlot targetSlot = slots[2, col];
            if (sourceSlot.HasCard)
            {
                CardVisual card = sourceSlot.RemoveCard();
                targetSlot.PlaceCard(card);
                card.transform.position = targetSlot.WorldPosition + Vector3.up * 0.1f;
            }
        }

        for (int col = 0; col < columns; col++)
        {
            BoardSlot sourceSlot = slots[0, col];
            BoardSlot targetSlot = slots[1, col];
            if (sourceSlot.HasCard)
            {
                CardVisual card = sourceSlot.RemoveCard();
                targetSlot.PlaceCard(card);
                card.transform.position = targetSlot.WorldPosition + Vector3.up * 0.1f;
            }
        }

        OnCardsAdvanced?.Invoke();
        return discarded;
    }

    private void ExecuteEnemyCard(Card card, Player player)
    {
        string typeName = card.Type?.name ?? "";
        int score = card.Score;

        if (typeName == "attacker")
        {
            player.TakeDamage(score);
            Debug.Log($"Enemy attack: {score} damage to player");
            return;
        }
        if (typeName == "defender")
        {
            Debug.Log($"Enemy defense: {score}");
            return;
        }
        if (typeName == "spell")
        {
            if (card.GetCardAbility() == "heal")
                Debug.Log($"Enemy spell (heal): {score}");
            return;
        }
        player.TakeDamage(score);
    }

    public int GetCountOfCardsWithAbility(string abilityName)
    {
        if (string.IsNullOrEmpty(abilityName) || slots == null) return 0;
        int count = 0;
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                if (slots[row, col] != null && slots[row, col].HasCard)
                {
                    Card card = slots[row, col].PlacedCard?.CardData;
                    if (card != null && card.GetCardAbility() == abilityName)
                        count++;
                }
            }
        }
        return count;
    }

    public void SetDeck(Deck deckRef)
    {
        deck = deckRef;
    }

    public bool HasEmptyPlacementSlot()
    {
        for (int col = 0; col < columns; col++)
        {
            if (!slots[0, col].HasCard)
            {
                return true;
            }
        }
        return false;
    }

    public bool IsPlacementSlotEmpty(int column)
    {
        if (column < 0 || column >= columns || slots == null)
            return false;
        return !slots[0, column].HasCard;
    }

    public int GetEmptyPlacementSlotCount()
    {
        int count = 0;
        for (int col = 0; col < columns; col++)
        {
            if (!slots[0, col].HasCard)
                count++;
        }
        return count;
    }

    public Vector3 GetSlotPosition(int row, int col)
    {
        if (row >= 0 && row < rows && col >= 0 && col < columns)
        {
            return slots[row, col].WorldPosition;
        }
        return Vector3.zero;
    }

    public int GetColumnFromWorldPosition(Vector3 worldPos)
    {
        float localX = worldPos.x - transform.position.x - boardOffset.x;
        int col = Mathf.RoundToInt(localX / slotSpacing);
        return Mathf.Clamp(col, 0, columns - 1);
    }

    public void ClearBoard()
    {
        if (slots == null) return;
        
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                if (slots[row, col] != null && slots[row, col].HasCard)
                {
                    CardVisual card = slots[row, col].RemoveCard();
                    if (card != null)
                    {
                        Destroy(card.gameObject);
                    }
                }
            }
        }
        Debug.Log($"{(isPlayerBoard ? "Player" : "Enemy")} board cleared.");
    }

    public List<CardVisual> GetAllCardsOnBoard()
    {
        List<CardVisual> cards = new List<CardVisual>();
        
        if (slots == null) return cards;
        
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                if (slots[row, col] != null && slots[row, col].HasCard)
                {
                    cards.Add(slots[row, col].PlacedCard);
                }
            }
        }
        
        return cards;
    }
}
