using UnityEngine;
using System.Collections.Generic;
using System;

public class Hand : MonoBehaviour
{
    [Header("Hand Settings")]
    [SerializeField] private int baseMaxHandSize = 5;
    [SerializeField] private int initialDrawCount = 6;
    [SerializeField] private int drawPerTurn = 3;
    [SerializeField] private float cardSpacing = 1.2f;
    [SerializeField] private Vector3 handPosition = new Vector3(0, 0.2f, -6f);
    
    [Header("References")]
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Deck deck;
    [SerializeField] private BoardManager playerBoard;
    
    private List<CardVisual> cardsInHand;
    public CardVisual SelectedCard { get; private set; }
    public int CardCount => cardsInHand?.Count ?? 0;
    public int MaxHandSize => CountMaxHandCards();
    public int DrawPerTurn => drawPerTurn;
    public event Action<CardVisual> OnCardSelected;
    public event Action<int> OnHandUpdated;

    private void Awake()
    {
        cardsInHand = new List<CardVisual>();
    }

    public void SetDeck(Deck deckRef) { deck = deckRef; }
    public void SetPlayerBoard(BoardManager board) { playerBoard = board; }

    public int CountMaxHandCards()
    {
        int max = baseMaxHandSize;
        if (playerBoard != null)
        {
            int greenBonus = playerBoard.GetCountOfCardsWithAbility("max hand increase");
            if (playerBoard.HasRow0ColorCombo("green"))
                greenBonus *= 2;
            max += greenBonus;
        }
        return max;
    }

    public void FillCards()
    {
        int maxHandCards = CountMaxHandCards();
        while (cardsInHand.Count < maxHandCards)
        {
            Card newCard = deck != null ? deck.DrawCard() : null;
            if (newCard == null)
                break;
            CardVisual cardVisual = CreateCardVisual(newCard);
            cardsInHand.Add(cardVisual);
            Debug.Log($"[ADD] Card added to hand: {newCard.CardName}. (Hand size: {cardsInHand.Count}/{maxHandCards})");
        }
        ArrangeCards();
        OnHandUpdated?.Invoke(CardCount);
    }

    public void DrawInitialCards()
    {
        FillCards();
    }

    public void DrawTurnCards()
    {
        DrawCards(drawPerTurn);
    }

    public void DrawCards(int count)
    {
        int maxHand = CountMaxHandCards();
        int cardsToDraw = Mathf.Min(count, maxHand - cardsInHand.Count);

        for (int i = 0; i < cardsToDraw; i++)
        {
            DrawCard();
        }

        ArrangeCards();
        OnHandUpdated?.Invoke(CardCount);
    }

    public void DrawToFull()
    {
        FillCards();
    }

    public bool DrawCard()
    {
        if (cardsInHand.Count >= CountMaxHandCards())
        {
            Debug.LogWarning("Hand is full!");
            return false;
        }
        
        if (deck == null)
        {
            Debug.LogError("No deck assigned to hand!");
            return false;
        }
        
        Card cardData = deck.DrawCard();
        if (cardData == null)
        {
            return false;
        }
        
        CardVisual cardVisual = CreateCardVisual(cardData);
        cardsInHand.Add(cardVisual);
        
        return true;
    }

    private CardVisual CreateCardVisual(Card cardData)
    {
        GameObject cardObj;
        
        if (cardPrefab != null)
        {
            cardObj = Instantiate(cardPrefab, transform);
        }
        else
        {
            cardObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cardObj.transform.parent = transform;
            cardObj.transform.localScale = new Vector3(1f, 0.1f, 1.4f);
        }
        
        CardVisual cardVisual = cardObj.GetComponent<CardVisual>();
        if (cardVisual == null)
        {
            cardVisual = cardObj.AddComponent<CardVisual>();
        }
        
        cardVisual.Initialize(cardData, this);
        
        return cardVisual;
    }

    public void ArrangeCards()
    {
        if (cardsInHand.Count == 0) return;
        
        float totalWidth = (cardsInHand.Count - 1) * cardSpacing;
        float startX = -totalWidth / 2f;
        
        // Use the Hand object's position as base
        Vector3 basePosition = transform.position;
        
        for (int i = 0; i < cardsInHand.Count; i++)
        {
            if (cardsInHand[i] == null) continue;
            
            Vector3 cardPosition = basePosition;
            cardPosition.x = startX + (i * cardSpacing);
            cardPosition.y = 0.2f; // Slightly above ground
            
            // Keep selected card raised
            if (cardsInHand[i] == SelectedCard && SelectedCard != null)
            {
                cardPosition.y += 0.5f;
            }
            
            cardsInHand[i].transform.position = cardPosition;
        }
    }

    public void SelectCard(CardVisual card)
    {
        // --- ИЗМЕНЕНИЕ: УБИРАЕМ ЛОГИКУ ДЕСЕЛЕКТА ПРИ ПОВТОРНОМ КЛИКЕ ---
        // Если мы кликаем по уже выбранной карте, мы хотим продолжить её держать/тащить,
        // а не сбрасывать.
        if (SelectedCard == card)
        {
            // Можно просто выйти, ничего не меняя, чтобы не пересчитывать позицию лишний раз
            return; 
        }
        // ---------------------------------------------------------------

        // Снимаем выделение с предыдущей карты (если была другая)
        if (SelectedCard != null)
        {
            SelectedCard.SetSelected(false);
        }

        // Выбираем новую
        SelectedCard = card;
        SelectedCard.SetSelected(true);
        
        ArrangeCards(); // Пересчитываем позицию (поднимаем карту)
        
        OnCardSelected?.Invoke(card);
        Debug.Log($"Selected card: {SelectedCard.CardData.CardName}");
    }

    // Внутри скрипта Hand.cs

    public void DeselectCard()
    {
        if (SelectedCard != null)
        {
            SelectedCard.SetSelected(false); // Визуально выключаем подсветку
            
            // Возвращаем карту на место (если она была приподнята)
            // Если у вас есть метод ArrangeCards(), вызовите его
            // ArrangeCards(); 
            
            SelectedCard = null; // Обнуляем ссылку
        }
    }

    public void RemoveCard(CardVisual card)
    {
        if (cardsInHand.Contains(card))
        {
            cardsInHand.Remove(card);

            if (SelectedCard == card)
            {
                SelectedCard = null;
            }

            ArrangeCards();
            OnHandUpdated?.Invoke(CardCount);
            Debug.Log($"Card removed from hand. Cards remaining: {CardCount}");
        }
    }

    public void AddCardBack(CardVisual card)
    {
        if (card == null) return;
        if (cardsInHand.Contains(card)) return;
        card.ReturnToHand(this);
        cardsInHand.Add(card);
        ArrangeCards();
        OnHandUpdated?.Invoke(CardCount);
        Debug.Log($"Card returned to hand: {card.CardData.CardName}. Hand size: {CardCount}");
    }

    public void ClearHand()
    {
        foreach (CardVisual card in cardsInHand)
        {
            if (card != null)
            {
                Destroy(card.gameObject);
            }
        }
        
        cardsInHand.Clear();
        SelectedCard = null;
        OnHandUpdated?.Invoke(0);
    }

    public List<CardVisual> GetCardsInHand()
    {
        return new List<CardVisual>(cardsInHand);
    }
}
