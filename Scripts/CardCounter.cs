using System.Collections.Generic;
using UnityEngine;

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
            Card newCard = DeckManager.Instance.GetFirstCard();
            if (newCard != null)
            {
                int nextSlot = handcards.Count;
                handcards.Add(nextSlot, newCard);
                Debug.Log($"Player get: {newCard.name}. Count in hand: {handcards.Count}");
            }
            else
            {
                Debug.Log("Deck is empty");
            }
        }
    }

    public void FillCards()
    {
        int maxHandCards = CountMaxHandCards();
        while (handcards.Count < maxHandCards)
        {
            Card newCard = DeckManager.Instance.GetFirstCard();
            if (newCard == null) break;

            int newIndex = handcards.Count; 
            handcards.Add(newIndex, newCard);
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
}