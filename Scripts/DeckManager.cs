using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance;

    [SerializeField]
    private List<Card> deck = new List<Card>();

    [SerializeField]
    private List<Card> discardPile = new List<Card>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        LoadCards();
    }

    public void LoadCards()
    {
        deck.Clear();
        discardPile.Clear();

        CardType attacker = new CardType("attacker", "red");
        CardType defender = new CardType("defender", "blue");
        CardType spell = new CardType("spell", "green");

        Ability heal = new Ability("heal");
        Ability maxincrease = new Ability("max hand increase");

        for (int i = 0; i < 5; i++) deck.Add(new Card(spell, "Heal Potion", 250, heal));
        for (int i = 0; i < 5; i++) deck.Add(new Card(spell, "Bag of Holding", 1, maxincrease));
        for (int i = 0; i < 10; i++) deck.Add(new Card(attacker, "Bear", 10));
        for (int i = 0; i < 10; i++) deck.Add(new Card(attacker, "Wolf", 5));
        for (int i = 0; i < 10; i++) deck.Add(new Card(defender, "Wall", 10));

        Debug.Log($"Deck size: {deck.Count}");
        ShuffleDeck();
    }

    public void ShuffleDeck()
    {
        for (int i = 0; i < deck.Count; i++)
        {
            Card temp = deck[i];
            int randomIndex = Random.Range(i, deck.Count);
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }
    }

    public Card GetFirstCard()
    {
        if (deck.Count == 0)
        {
            if (discardPile.Count > 0)
            {
                ReshuffleDiscardPile();
            }
            else
            {
                return null;
            }
        }

        Card card = deck[0];
        deck.RemoveAt(0);
        return card;
    }

    public void AddToDiscard(Card card)
    {
        discardPile.Add(card);
        Debug.Log($"Card {card.name} was added to discard pile: {discardPile.Count}");
    }

    private void ReshuffleDiscardPile()
    {
        Debug.Log("Cards were reshuffled");
        
        deck.AddRange(discardPile);
        discardPile.Clear();
        
        ShuffleDeck();
    }

    public int GetDeckSize() { return deck.Count; }
    public int GetDiscardSize() { return discardPile.Count; }
}