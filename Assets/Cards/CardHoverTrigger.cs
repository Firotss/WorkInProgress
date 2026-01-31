using UnityEngine;

public class CardHoverTrigger : MonoBehaviour
{
    [Header("Settings")]
    public float timeToWait = 3.0f; // Время ожидания в секундах

    private float hoverTimer = 0f;
    private bool isHovering = false;
    private bool isPreviewShown = false;
    
    private CardVisual cardVisual;

    void Start()
    {
        cardVisual = GetComponent<CardVisual>();
    }

    // Этот метод Unity вызывает сама, когда мышка находится над коллайдером
    private void OnMouseOver()
    {
        // Если зажата левая кнопка (мы тащим карту), таймер не должен идти
        if (Input.GetMouseButton(0))
        {
            ResetTimer();
            return;
        }

        // Если превью уже показано, ничего не делаем
        if (isPreviewShown) return;

        // Накапливаем время
        hoverTimer += Time.deltaTime;

        // Если время вышло
        if (hoverTimer >= timeToWait)
        {
            ShowCardBig();
        }
    }

    // Этот метод вызывается, когда мышка уходит с карты
    private void OnMouseExit()
    {
        ResetTimer();
    }

    private void ResetTimer()
    {
        hoverTimer = 0f;
        isPreviewShown = false;
        
        // Скрываем превью, если оно было открыто
        if (CardPreviewManager.Instance != null)
        {
            CardPreviewManager.Instance.HidePreview();
        }
    }

    private void ShowCardBig()
    {
        if (CardPreviewManager.Instance != null && cardVisual != null)
        {
            // Здесь берем картинку карты.
            // Убедитесь, что cardVisual.CardData.Artwork существует!
            // Если у вас картинка хранится по-другому, поправьте эту строку.
            if (cardVisual.CardData != null)
            {
                CardPreviewManager.Instance.ShowPreview(cardVisual.CardData.Artwork);
                isPreviewShown = true;
            }
        }
    }
}