using UnityEngine;
using System.Collections.Generic;

public class EnemyAI : MonoBehaviour
{
    [Header("AI Settings")]
    [SerializeField] private int cardsPerTurn = 3;
    [SerializeField] private GameObject cardPrefab; // ВАЖНО: Сложи тук префаба на картата в Unity!

    [Header("References")]
    [SerializeField] private BoardManager enemyBoard;
    [SerializeField] private Deck enemyDeck;

    public void Initialize(BoardManager board, Deck deck)
    {
        enemyBoard = board;
        enemyDeck = deck;
    }

    public void PlayTurn()
    {
        if (enemyBoard == null || enemyDeck == null) return;

        int emptySlots = enemyBoard.GetEmptyPlacementSlotCount();
        int toPlay = Mathf.Min(cardsPerTurn, emptySlots);

        Debug.Log($"Enemy playing {toPlay} cards...");

        for (int i = 0; i < toPlay; i++)
        {
            PlayRandomCardInRandomColumn();
        }
    }

    private void PlayRandomCardInRandomColumn()
    {
        Card cardData = enemyDeck.DrawCard();
        if (cardData == null) return;

        int col = GetRandomEmptyColumn();
        
        // Ако няма място, връщаме картата в тестето/сброса, за да не я губим
        if (col < 0)
        {
            enemyDeck.AddToDiscard(cardData);
            return;
        }

        // Създаваме визуална карта от префаба
        GameObject cardObj;
        if (cardPrefab != null)
        {
            cardObj = Instantiate(cardPrefab);
        }
        else
        {
            // Fallback само ако си забравил префаба
            cardObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cardObj.transform.localScale = new Vector3(1f, 0.1f, 1.4f);
        }

        CardVisual cardVisual = cardObj.GetComponent<CardVisual>();
        if (cardVisual == null) cardVisual = cardObj.AddComponent<CardVisual>();
        
        // ВАЖНО: Вторият параметър е null, защото врагът няма "Ръка" (Hand component)
        cardVisual.Initialize(cardData, null);

        if (enemyBoard.PlaceCard(cardVisual, col))
        {
            SoundManager.Instance?.PlayCardPlaced();
        }
        else
        {
            // Ако поставянето се провали, унищожаваме визуалния обект и връщаме картата
            Destroy(cardObj);
            enemyDeck.AddToDiscard(cardData);
        }
    }

    private int GetRandomEmptyColumn()
    {
        var empty = new List<int>();
        int cols = enemyBoard != null ? enemyBoard.ColumnCount : 5;
        for (int c = 0; c < cols; c++)
        {
            if (enemyBoard.IsPlacementSlotEmpty(c))
                empty.Add(c);
        }
        if (empty.Count == 0) return -1;
        return empty[Random.Range(0, empty.Count)];
    }
}