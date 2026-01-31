using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Simple AI that controls the enemy (monster) card plays.
/// Plays random cards each turn.
/// </summary>
public class EnemyAI : MonoBehaviour
{
    [Header("AI Settings")]
    [SerializeField] private int cardsPerTurn = 3;
    
    [Header("References")]
    [SerializeField] private BoardManager enemyBoard;
    [SerializeField] private Deck enemyDeck;
    
    /// <summary>
    /// Reference to the enemy's deck.
    /// </summary>
    public Deck EnemyDeck => enemyDeck;

    /// <summary>
    /// Sets references.
    /// </summary>
    public void Initialize(BoardManager board, Deck deck)
    {
        enemyBoard = board;
        enemyDeck = deck;
    }

    /// <summary>
    /// Plays random cards on the enemy board.
    /// </summary>
    public void PlayTurn()
    {
        if (enemyBoard == null || enemyDeck == null)
        {
            Debug.LogWarning("EnemyAI: Missing board or deck reference!");
            return;
        }
        
        int cardsToPlay = Mathf.Min(cardsPerTurn, enemyBoard.GetEmptyPlacementSlotCount());
        
        Debug.Log($"Enemy playing {cardsToPlay} cards...");
        
        for (int i = 0; i < cardsToPlay; i++)
        {
            PlayRandomCard();
        }
    }

    /// <summary>
    /// Plays a single random card on an empty slot.
    /// </summary>
    private void PlayRandomCard()
    {
        // Draw a card
        Card cardData = enemyDeck.DrawCard();
        if (cardData == null)
        {
            Debug.Log("Enemy deck is empty!");
            return;
        }
        
        // Find empty slot
        int emptySlots = enemyBoard.GetEmptyPlacementSlotCount();
        if (emptySlots == 0)
        {
            Debug.Log("No empty slots for enemy card!");
            return;
        }
        
        // Create card visual
        GameObject cardObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cardObj.transform.localScale = new Vector3(1f, 0.1f, 1.4f);
        
        CardVisual cardVisual = cardObj.AddComponent<CardVisual>();
        cardVisual.Initialize(cardData, null);
        
        // Place on board
        if (enemyBoard.PlaceCardAuto(cardVisual))
        {
            Debug.Log($"Enemy played: {cardData.CardName}");
        }
        else
        {
            Destroy(cardObj);
        }
    }
}
