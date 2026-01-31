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
    [SerializeField] private Deck deck;

    /// <summary>
    /// Reference to the shared deck (same as player's).
    /// </summary>
    public Deck Deck => deck;

    /// <summary>
    /// Sets references.
    /// </summary>
    public void Initialize(BoardManager board, Deck sharedDeck)
    {
        enemyBoard = board;
        deck = sharedDeck;
    }

    /// <summary>
    /// Plays random cards on the enemy board.
    /// </summary>
    public void PlayTurn()
    {
        if (enemyBoard == null || deck == null)
        {
            Debug.LogWarning("EnemyAI: Missing board or deck reference!");
            return;
        }

        int toPlay = Mathf.Min(cardsPerTurn, enemyBoard.GetEmptyPlacementSlotCount());
        Debug.Log($"Enemy playing {toPlay} cards (random columns)...");

        for (int i = 0; i < toPlay; i++)
        {
            PlayRandomCardInRandomColumn();
        }
    }

    private void PlayRandomCardInRandomColumn()
    {
        Card cardData = deck.DrawCard();
        if (cardData == null)
            return;

        int col = GetRandomEmptyColumn();
        if (col < 0)
        {
            deck.AddToDiscard(cardData);
            return;
        }

        // Create card visual
        GameObject cardObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cardObj.transform.localScale = new Vector3(1f, 0.1f, 1.4f);
        CardVisual cardVisual = cardObj.AddComponent<CardVisual>();
        cardVisual.Initialize(cardData, null);

        if (enemyBoard.PlaceCard(cardVisual, col))
        {
            SoundManager.Instance?.PlayCardPlaced();
            Debug.Log($"Enemy played: {cardData.CardName} in column {col}");
        }
        else
        {
            Destroy(cardObj);
        }
    }

    private int GetRandomEmptyColumn()
    {
        var empty = new System.Collections.Generic.List<int>();
        int cols = enemyBoard != null ? enemyBoard.ColumnCount : 5;
        for (int c = 0; c < cols; c++)
        {
            if (enemyBoard.IsPlacementSlotEmpty(c))
                empty.Add(c);
        }
        if (empty.Count == 0)
            return -1;
        return empty[UnityEngine.Random.Range(0, empty.Count)];
    }
}
