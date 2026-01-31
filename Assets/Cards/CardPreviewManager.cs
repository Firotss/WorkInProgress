using UnityEngine;
using UnityEngine.UI;

public class CardPreviewManager : MonoBehaviour
{
    public static CardPreviewManager Instance;

    [Header("UI Elements")]
    public GameObject previewPanel; // Объект SpriteShow (или его дочерний объект с картинкой)
    public Image previewImage;      // Компонент Image, куда подставлять арт

    void Awake()
    {
        Instance = this;
        HidePreview(); // Скрываем при старте
    }

    public void ShowPreview(Sprite sprite)
    {
        if (sprite == null) return;

        previewImage.sprite = sprite;
        previewPanel.SetActive(true);
        
        // Дополнительная гарантия прозрачности для кликов
        previewImage.raycastTarget = false; 
    }

    public void HidePreview()
    {
        previewPanel.SetActive(false);
    }
}