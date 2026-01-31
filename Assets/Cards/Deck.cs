using UnityEngine;
using System.Collections.Generic;

public class Deck : MonoBehaviour
{
    private List<Card> deck;
    private List<Card> discardPile;
    [SerializeField] private bool useLoadedDeck = true;

    [Header("Card Sprites (Red / Green / Blue textures)")]
    [SerializeField] private Sprite redCardSprite;
    [SerializeField] private Sprite greenCardSprite;
    [SerializeField] private Sprite blueCardSprite;

    public int CardsRemaining => deck?.Count ?? 0;
    public int DiscardCount => discardPile?.Count ?? 0;

    private void Awake()
    {
        deck = new List<Card>();
        discardPile = new List<Card>();

        if (useLoadedDeck)
        {
            LoadCards();
        }
    }

    /// <summary>
    /// Populates deck with reference composition: Heal Potion, Bag of Holding, Bear, Wolf, Wall.
    /// Assigns Red/Green/Blue texture sprites by card type (attacker=red, spell=green, defender=blue).
    /// </summary>
    public void LoadCards()
    {
        deck.Clear();
        discardPile.Clear();

        CardType attacker = new CardType("attacker", "red");
        CardType defender = new CardType("defender", "blue");
        CardType spell = new CardType("spell", "green");

        Ability crowd_control = new Ability("crowd control");
        Ability creature_damage_increase = new Ability("creature damage increase");
        Ability knight_ultimate = new Ability("knight ultimate");
        Ability points_steal = new Ability("points steal");
        Ability point_increase = new Ability("point increase");

        Ability heal = new Ability("heal");
        Ability maxIncrease = new Ability("max hand increase");

        Sprite babyForestDragonSprite = GetSpriteForType("Baby_Forest_Dragon");
        Sprite bloodFiendSprite = GetSpriteForType("Blood_Fiend");
        Sprite chaliceofVigorSprite = GetSpriteForType("Chalice_of_Vigor");
        Sprite chasingthegooseSprite = GetSpriteForType("Chasing_the_goose");
        Sprite fiendFeedingSprite = GetSpriteForType("Fiend_Feeding");
        Sprite forestDragonSprite = GetSpriteForType("Forest_Dragon");
        Sprite quintessenceSprite = GetSpriteForType("Quintessence");
        Sprite sweetKittenSprite = GetSpriteForType("Sweet_Kitten");
        Sprite theDecrepitKnightSprite = GetSpriteForType("The_Decrepit_Knight");

        for (int i = 0; i < 10; i++)
            deck.Add(new Card(attacker, "Baby Forest Dragon", 3, 15, creature_damage_increase, babyForestDragonSprite));
        for (int i = 0; i < 10; i++)
            deck.Add(new Card(attacker, "Blood Fiend", 2, 20, points_steal, bloodFiendSprite));
        for (int i = 0; i < 5; i++)
            deck.Add(new Card(attacker, "Forest Dragon", 7, 10, crowd_control, forestDragonSprite));
        for (int i = 0; i < 3; i++)
            deck.Add(new Card(attacker, "The Decrepit Knight", 3, 30, knight_ultimate, theDecrepitKnightSprite));
        for (int i = 0; i < 15; i++)
            deck.Add(new Card(attacker, "Sweet Kitten", 5, 15, point_increase, sweetKittenSprite));

        for (int i = 0; i < 15; i++)
            deck.Add(new Card(defender, "Chalice of Vigor", 3, 30, null, chaliceofVigorSprite));
        for (int i = 0; i < 10; i++)
            deck.Add(new Card(defender, "Chasing the Goose", 5, 50, null, chasingthegooseSprite));

        for (int i = 0; i < 10; i++)
            deck.Add(new Card(spell, "Fiend Feeding", 1, 20, maxIncrease, fiendFeedingSprite));
        for (int i = 0; i < 5; i++)
            deck.Add(new Card(spell, "Quintessence", 1, 20, heal, quintessenceSprite));

        Debug.Log($"Deck size: {deck.Count}");
        ShuffleDeck();
    }

    private static readonly Dictionary<string, Sprite> SpriteCache = new Dictionary<string, Sprite>();

    private Sprite GetSpriteForType(string name)
    {
        string key = name;
        if (SpriteCache.TryGetValue(key, out Sprite cached))
            return cached;

        Debug.Log($"Loading sprite: {name}");
        Sprite loaded = Resources.Load<Sprite>($"Arts/Cards/{name}");
        if (loaded != null)
        {
            SpriteCache[key] = loaded;
            return loaded;
        }

        Texture2D tex = Resources.Load<Texture2D>($"Arts/Cards/{name}");
        if (tex != null)
        {
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            SpriteCache[key] = sprite;
            return sprite;
        }

        return null;
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

    public Card DrawCard()
    {
        if (deck.Count == 0)
        {
            if (discardPile.Count > 0)
            {
                ReshuffleDiscardPile();
            }
            else
            {
                Debug.LogWarning("No cards left to draw!");
                return null;
            }
        }

        if (deck.Count == 0)
            return null;

        Card card = deck[0];
        deck.RemoveAt(0);
        Debug.Log($"Drew card: {card.CardName}");
        return card;
    }

    public void AddToDiscard(Card card)
    {
        if (card == null) return;
        discardPile.Add(card);
        Debug.Log($"Card {card.CardName} was added to discard pile: {discardPile.Count}");
    }

    public void DiscardCard(Card card)
    {
        AddToDiscard(card);
    }

    private void ReshuffleDiscardPile()
    {
        Debug.Log("Cards were reshuffled");
        deck.AddRange(discardPile);
        discardPile.Clear();
        ShuffleDeck();
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayShuffle();
    }

    public void ResetDeck()
    {
        if (useLoadedDeck)
        {
            LoadCards();
        }
        else
        {
            deck.Clear();
            discardPile.Clear();
        }
    }

    public int GetDeckSize() => deck?.Count ?? 0;
    public int GetDiscardSize() => discardPile?.Count ?? 0;
}
