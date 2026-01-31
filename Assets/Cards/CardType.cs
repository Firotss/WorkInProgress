using UnityEngine;

[System.Serializable]
public class CardType
{
    public string name;
    public string color;

    public CardType(string name, string color)
    {
        this.name = name;
        this.color = color;
    }

    public static readonly CardType Attacker = new CardType("attacker", "red");
    public static readonly CardType Defender = new CardType("defender", "blue");
    public static readonly CardType Spell = new CardType("spell", "green");
}
