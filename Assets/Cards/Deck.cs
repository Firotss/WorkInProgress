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

        Ability heal = new Ability("heal");
        Ability maxIncrease = new Ability("max hand increase");

        Sprite redSprite = GetSpriteForType("red");
        Sprite greenSprite = GetSpriteForType("green");
        Sprite dragonSprite = GetSpriteForType("dragon");
        Sprite blueSprite = GetSpriteForType("blue");

        for (int i = 0; i < 5; i++)
            deck.Add(new Card(spell, "Heal Potion", 250, heal, greenSprite));
        for (int i = 0; i < 5; i++)
            deck.Add(new Card(spell, "Bag of Holding", 1, maxIncrease, greenSprite));
        for (int i = 0; i < 10; i++)
            deck.Add(new Card(attacker, "Dragon", 10, null, dragonSprite));
        for (int i = 0; i < 10; i++)
            deck.Add(new Card(attacker, "Wolf", 5, null, redSprite));
        for (int i = 0; i < 10; i++)
            deck.Add(new Card(defender, "Wall", 10, null, blueSprite));

        Debug.Log($"Deck size: {deck.Count}");
        ShuffleDeck();
    }

    private static readonly Dictionary<string, Sprite> SpriteCache = new Dictionary<string, Sprite>();

    private Sprite GetSpriteForType(string color)
    {
        if (color == "red" && redCardSprite != null) return redCardSprite;
        if (color == "green" && greenCardSprite != null) return greenCardSprite;
        if (color == "blue" && blueCardSprite != null) return blueCardSprite;

        string key = color;
        if (SpriteCache.TryGetValue(key, out Sprite cached))
            return cached;

        string name = char.ToUpper(color[0]) + color.Substring(1) + "_Texture";
        Sprite loaded = Resources.Load<Sprite>($"Arts/{name}");
        if (loaded != null)
        {
            SpriteCache[key] = loaded;
            return loaded;
        }

        Texture2D tex = Resources.Load<Texture2D>($"Arts/{name}");
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
