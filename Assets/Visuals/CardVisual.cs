using UnityEngine;
using TMPro;
using System.Collections;

public class CardVisual : MonoBehaviour
{
    [Header("Card Data")]
    [SerializeField] private Card cardData;

    [Header("Visual Settings")]
    [SerializeField] private Color selectedTintColor = new Color(1f, 1f, 0.7f);

    public Card CardData => cardData;
    private Hand ownerHand;
    public bool IsSelected { get; private set; }
    public bool IsOnBoard { get; private set; }

    private Renderer cardRenderer;
    private Color originalColor;
    private TextMeshPro cardText;

    private void Awake()
    {
        cardRenderer = GetComponent<Renderer>();
        TextMeshPro existingText = GetComponentInChildren<TextMeshPro>();
        if (existingText != null)
            cardText = existingText;
    }

    public void Initialize(Card data, Hand hand = null)
    {
        cardData = data;
        ownerHand = hand;
        IsOnBoard = false;
        IsSelected = false;
        UpdateVisuals();
    }

    private Coroutine previewTimer;

    private void OnMouseEnter()
    {
        previewTimer = StartCoroutine(WaitAndShow());
    }

    private void OnMouseExit()
    {
        if (previewTimer != null) StopCoroutine(previewTimer);
        UIManager.Instance.HidePreview();
    }

    private IEnumerator WaitAndShow()
    {
        yield return new WaitForSeconds(0.5f);
        
        if (cardRenderer != null && cardRenderer.material.mainTexture != null)
        {
            UIManager.Instance.ShowPreview(cardRenderer.material.mainTexture);
        }
    }

    public void UpdateVisuals()
    {
        if (cardData == null) return;

        if (cardRenderer == null)
            cardRenderer = GetComponent<Renderer>();

        if (cardRenderer != null)
        {
            if (cardData.Artwork != null && cardData.Artwork.texture != null)
            {
                Shader unlit = Shader.Find("Unlit/Texture");
                if (unlit != null)
                    cardRenderer.material.shader = unlit;
                cardRenderer.material.mainTexture = cardData.Artwork.texture;
                cardRenderer.material.color = Color.white;
                originalColor = Color.white;
            }
            else
            {
                cardRenderer.material.mainTexture = null;
                originalColor = cardData.GetCardColor();
                cardRenderer.material.color = originalColor;
            }
        }

        if (cardText != null)
            cardText.text = $"{cardData.CardName}\n{cardData.Score}";

        string typeName = cardData.Type?.name ?? "unknown";
        gameObject.name = $"Card_{typeName}_{cardData.Score}";
    }

    public void SetSelected(bool selected)
    {
        if (IsOnBoard) return;
        IsSelected = selected;
        if (cardRenderer != null)
            cardRenderer.material.color = selected ? originalColor * selectedTintColor : originalColor;
    }

    public void SetOnBoard(bool onBoard)
    {
        IsOnBoard = onBoard;
        if (onBoard)
        {
            IsSelected = false;
            ownerHand = null;
            if (cardRenderer != null)
                cardRenderer.material.color = originalColor;
        }
    }

    public void ReturnToHand(Hand hand)
    {
        if (hand == null) return;
        IsOnBoard = false;
        ownerHand = hand;
        transform.SetParent(hand.transform);
        if (cardRenderer != null)
            cardRenderer.material.color = originalColor;
    }

    private void OnMouseDown()
    {
        HandleClick();
    }

    public void HandleClick()
    {
        if (IsOnBoard) return;
        if (ownerHand != null)
            ownerHand.SelectCard(this);
    }

    public CardType GetCardType()
    {
        return cardData?.Type ?? CardType.Attacker;
    }

    public int GetActionPoints()
    {
        return cardData?.Score ?? 0;
    }
}
