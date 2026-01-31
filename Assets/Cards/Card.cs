using UnityEngine;

/// <summary>
/// Ability attached to a card (e.g. "heal", "max hand increase").
/// Aligns with reference: Ability has a name.
/// </summary>
[System.Serializable]
public class Ability
{
    public string name;

    public Ability(string name)
    {
        this.name = name;
    }
}

/// <summary>
/// Card data: type, name, score, optional ability, artwork.
/// Aligns with reference: CardType, name, score, Ability, GetCardAbility().
/// </summary>
[System.Serializable]
public class Card
{
    [Header("Card Properties")]
    [SerializeField] private CardType cardType;
    [SerializeField] private string cardName;
    [SerializeField] private int score;
    [SerializeField] private int hp;
    [SerializeField] private Ability ability;
    [SerializeField] private Sprite artwork;

    /// <summary>Name of the card.</summary>
    public string CardName => cardName;

    /// <summary>Card type (attacker, defender, spell).</summary>
    public CardType Type => cardType;

    /// <summary>Score / strength value for the card's effect.</summary>
    public int Score => score;

    public int HP => hp;

    /// <summary>Action points (alias for Score for compatibility).</summary>
    public int ActionPoints => score;

    /// <summary>Optional ability (e.g. heal, max hand increase).</summary>
    public Ability Ability => ability;

    /// <summary>Optional artwork sprite.</summary>
    public Sprite Artwork => artwork;

    /// <summary>
    /// Creates a card with type, name, score, optional ability and artwork.
    /// </summary>
    public Card(CardType type, string name, int score, int hp, Ability ability = null,
        Sprite artwork = null)
    {
        cardType = type;
        cardName = name;
        this.hp = hp;
        this.score = score;
        this.ability = ability;
        this.artwork = artwork;
    }

    /// <summary>Default constructor for serialization.</summary>
    public Card()
    {
        cardType = CardType.Attacker;
        cardName = "Unknown";
        score = 1;
        ability = null;
        artwork = null;
    }

    /// <summary>
    /// Returns the card's ability name, or "none" if no ability.
    /// </summary>
    public string GetCardAbility()
    {
        return ability != null ? ability.name : "none";
    }

    /// <summary>
    /// Gets the color associated with this card's type (from type.color).
    /// </summary>
    public Color GetCardColor()
    {
        if (cardType == null) return Color.white;
        switch (cardType.color?.ToLowerInvariant())
        {
            case "red":
                return new Color(0.9f, 0.2f, 0.2f);
            case "blue":
                return new Color(0.2f, 0.4f, 0.9f);
            case "green":
                return new Color(0.2f, 0.8f, 0.3f);
            default:
                return Color.white;
        }
    }

    // Runs effect by type/ability (attacker/defender/spell + heal etc.)
    public void Execute(Player player, Monster monster)
    {
        string typeName = cardType?.name ?? "";
        string abilityName = GetCardAbility();

        // Spell with ability (e.g. heal)
        if (typeName == "spell")
        {
            if (abilityName == "heal")
            {
                Effects.ApplyHeal(score, player);
                Debug.Log($"Card '{cardName}' executed: heal {score}.");
                return;
            }
            // "max hand increase" is passive (board presence only)
            Debug.Log($"Card '{cardName}' (spell, {abilityName}): no execution.");
            return;
        }

        if (typeName == "attacker")
        {
            Effects.ApplyAttack(score, monster);
            Debug.Log($"Card '{cardName}' executed: attack {score}.");
            return;
        }

        if (typeName == "defender")
        {
            Effects.ApplyDefense(score, player);
            Debug.Log($"Card '{cardName}' executed: defense {score}.");
            return;
        }

        Debug.Log($"Card '{cardName}' executed: unknown type '{typeName}'.");
    }
}
