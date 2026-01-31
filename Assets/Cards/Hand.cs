using UnityEngine;
using System.Collections.Generic;

public class Hand : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float cardSpacing = 2.0f;
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private int maxHandSize = 5; // Максимален брой карти

    [Header("References")]
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform handCenter;

    private List<CardVisual> cards = new List<CardVisual>();
    private Deck currentDeck;
    private BoardManager playerBoard;
    
    public CardVisual SelectedCard { get; private set; }

    // --- Инициализация ---

    public void SetDeck(Deck deck)
    {
        currentDeck = deck;
    }

    public void SetPlayerBoard(BoardManager board)
    {
        playerBoard = board;
    }

    // --- Основни методи (Draw / Remove) ---

    public void DrawInitialCards()
    {
        DrawCardsUntilFull();
    }

    // Този метод липсваше и предизвикваше грешката
    public void DrawCardsUntilFull()
    {
        if (currentDeck == null) return;

        int cardsNeeded = maxHandSize - cards.Count;
        for (int i = 0; i < cardsNeeded; i++)
        {
            Card cardData = currentDeck.DrawCard();
            if (cardData != null)
            {
                CreateCardVisual(cardData);
            }
        }
        ArrangeCards();
    }

    private void CreateCardVisual(Card cardData)
    {
        if (cardPrefab == null) return;

        GameObject cardObj = Instantiate(cardPrefab, handCenter.position, Quaternion.identity, transform);
        CardVisual visual = cardObj.GetComponent<CardVisual>();
        
        if (visual != null)
        {
            visual.Initialize(cardData, this);
            cards.Add(visual);
        }
    }

    public void RemoveCard(CardVisual card)
    {
        if (cards.Contains(card))
        {
            cards.Remove(card);
            ArrangeCards();
        }
    }

    public void AddCardBack(CardVisual card)
    {
        if (!cards.Contains(card))
        {
            card.transform.SetParent(transform);
            card.IsOnBoard = false; // Важно: маркираме, че вече не е на масата
            cards.Add(card);
            ArrangeCards();
        }
    }

    public void ClearHand()
    {
        foreach (var card in cards)
        {
            if (card != null) Destroy(card.gameObject);
        }
        cards.Clear();
    }

    // --- Визуализация и Подредба ---

    // Този метод липсваше (или се наричаше по друг начин)
    public void RefreshHandVisuals()
    {
        ArrangeCards();
    }

    public void ArrangeCards()
    {
        if (cards.Count == 0) return;

        float totalWidth = (cards.Count - 1) * cardSpacing;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i] == null) continue;

            Vector3 targetPos = handCenter.position + new Vector3(startX + (i * cardSpacing), 0, 0);
            
            // Ако картата е избрана, я вдигаме леко нагоре
            if (cards[i] == SelectedCard)
            {
                targetPos += Vector3.up * 0.5f;
            }

            cards[i].MoveTo(targetPos, moveSpeed);
            cards[i].transform.rotation = Quaternion.identity; // Изправяме картата
        }
    }

    // --- Селекция ---

    public void SelectCard(CardVisual card)
    {
        if (SelectedCard != null && SelectedCard != card)
        {
            SelectedCard.SetSelected(false);
        }

        SelectedCard = card;
        if (SelectedCard != null)
        {
            SelectedCard.SetSelected(true);
        }
        ArrangeCards();
    }

    public void DeselectCard()
    {
        if (SelectedCard != null)
        {
            SelectedCard.SetSelected(false);
            SelectedCard = null;
            ArrangeCards();
        }
    }
}