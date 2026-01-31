using UnityEngine;

[System.Serializable]
public class BoardSlot
{
    public int Row { get; private set; }
    public int Column { get; private set; }
    public CardVisual PlacedCard { get; private set; }
    public Vector3 WorldPosition { get; set; }
    public bool HasCard => PlacedCard != null;

    public BoardSlot(int row, int col)
    {
        Row = row;
        Column = col;
        PlacedCard = null;
    }

    public bool PlaceCard(CardVisual card)
    {
        if (HasCard)
        {
            Debug.LogWarning($"Slot [{Row},{Column}] is already occupied!");
            return false;
        }
        PlacedCard = card;
        return true;
    }

    public CardVisual RemoveCard()
    {
        CardVisual card = PlacedCard;
        PlacedCard = null;
        return card;
    }

    public void Clear()
    {
        PlacedCard = null;
    }
}
