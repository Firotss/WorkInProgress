using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance;

    // Список карт в текущей колоде
    [SerializeField]
    private List<Card> cards = new List<Card>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        LoadCards(); 
    }

    public void LoadCards()
    {
        cards.Clear();

        CardType attacker = new CardType("attacker", "red");
        CardType defender = new CardType("defender", "blue");
        CardType spell = new CardType("spell", "green");

        Ability heal = new Ability("heal");
        Ability maxincrease = new Ability("max hand increase");

        for (int i = 0; i < 5; i++) cards.Add(new Card(spell, "Heal Potion", 250, heal));
        for (int i = 0; i < 5; i++) cards.Add(new Card(spell, "Bag of Holding", 1, maxincrease));
        for (int i = 0; i < 10; i++) cards.Add(new Card(attacker, "Bear", 10));
        for (int i = 0; i < 10; i++) cards.Add(new Card(attacker, "Wolf", 5));
        for (int i = 0; i < 10; i++) cards.Add(new Card(defender, "Wall", 10));

        Debug.Log($"Deck loaded: {cards.Count}");
        SortCards();
    }

    public void SortCards()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            Card temp = cards[i];
            int randomIndex = Random.Range(i, cards.Count);
            cards[i] = cards[randomIndex];
            cards[randomIndex] = temp;
        }
    }

    public Card GetFirstCard()
    {
        if (cards.Count == 0) return null;
        Card card = cards[0];
        cards.RemoveAt(0);
        return card;
    }

    public int GenCardsDeckNum()
    {
        return cards.Count;
    }
}