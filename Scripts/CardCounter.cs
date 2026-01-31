using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CardCounter : MonoBehaviour
{
    public Dictionary<int, Card> handcards = new Dictionary<int, Card>();
    public Dictionary<int, Card> boardcards = new Dictionary<int, Card>(); 

    void Start()
    {
        FillCards(); 
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("--- Filling cards... ---");
            FillCards();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            PlayCardFromHandToBoard();
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            BurnCardFromBoard();
        }
    }

    public void FillCards()
    {
        int maxHandCards = CountMaxHandCards();

        while (handcards.Count < maxHandCards)
        {
            Card newCard = DeckManager.Instance.GetFirstCard();
            
            if (newCard == null) 
            {
                break;
            }

            int newIndex = 0;
            while (handcards.ContainsKey(newIndex)) newIndex++;

            handcards.Add(newIndex, newCard);
            Debug.Log($"[ADD] Card added to hand: {newCard.name}. (Hand size: {handcards.Count}/{maxHandCards})");
        }
    }

    public int CountMaxHandCards()
    {
        int maxHandCards = 5;
        foreach (var entry in boardcards)
        {
            if (entry.Value != null && entry.Value.GetCardAbility() == "max hand increase")
            {
                maxHandCards += 1;
            }
        }
        return maxHandCards;
    }

    private void PlayCardFromHandToBoard()
    {
        if (handcards.Count == 0) return;

        int handKey = handcards.Keys.First(); 
        Card cardToPlay = handcards[handKey];

        int boardKey = 0;
        while (boardcards.ContainsKey(boardKey)) boardKey++;

        boardcards.Add(boardKey, cardToPlay); 
        handcards.Remove(handKey);            

        Debug.Log($"[PLAY] Card {cardToPlay.name} played to board (board key: {boardKey}).");
    }

    private void BurnCardFromBoard()
    {
        if (boardcards.Count == 0) return;

        int boardKey = boardcards.Keys.First();
        Card cardToBurn = boardcards[boardKey];

        DeckManager.Instance.AddToDiscard(cardToBurn);

        boardcards.Remove(boardKey);

        Debug.Log($"[BURN] Card {cardToBurn.name} burned.");
    }
}