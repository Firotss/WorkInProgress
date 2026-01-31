using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "NewCard", menuName = "Card Game/Card Data")]
public class CardData : ScriptableObject
{
    public string cardName;
    public string cardType; 
    public int score;
    public string ability;
    public Sprite artwork;
}

[System.Serializable]
public class Ability
{
    public string name;
    public Ability(string name) { this.name = name; }
}

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
}

[System.Serializable]
public class Card
{
    public CardType cardType;
    public string name;
    public Ability ability;
    public int score;
    public Sprite artwork; 

    public Card(CardType cardType, string name, int score, Ability ability = null, Sprite artwork = null)
    {
        this.cardType = cardType;
        this.name = name;
        this.score = score;
        this.ability = ability;
        this.artwork = artwork;
    }

    public string GetCardAbility()
    {
        return ability != null ? ability.name : "none";
    }
}