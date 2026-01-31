using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq; // Важно для поиска скриптов

public class BoardVisualizer : MonoBehaviour
{
    public static BoardVisualizer Instance;

    [Header("Настройки")]
    public Transform playerGridTransform; 
    public Transform enemyGridTransform;
    public GameObject slotPrefab;       
    public GameObject cardVisualPrefab; 

    // Списки для хранения ссылок на слоты и карты
    private List<Transform> playerSlots = new List<Transform>();
    private List<Transform> enemySlots = new List<Transform>();
    private Dictionary<int, GameObject> playerCardObjs = new Dictionary<int, GameObject>();
    private Dictionary<int, GameObject> enemyCardObjs = new Dictionary<int, GameObject>();

    void Awake() { Instance = this; }

    void Start()
    {
        // Генерируем сетку при старте
        GenerateGrid(playerGridTransform, playerSlots);
        GenerateGrid(enemyGridTransform, enemySlots);
        
        // Первая отрисовка (если игра началась сразу)
        Invoke("UpdateBoardVisuals", 0.1f);
    }

    void GenerateGrid(Transform gridParent, List<Transform> slotList)
    {
        foreach(Transform child in gridParent) Destroy(child.gameObject);
        slotList.Clear();

        // Добавляем Grid Layout программно
        GridLayoutGroup grid = gridParent.GetComponent<GridLayoutGroup>();
        if(grid == null) grid = gridParent.gameObject.AddComponent<GridLayoutGroup>();
        
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 5; // 5 КОЛОНОК
        grid.cellSize = new Vector2(180, 240); // Подгоните под свой размер
        grid.spacing = new Vector2(20, 20);

        // Создаем 15 слотов
        for (int i = 0; i < 15; i++)
        {
            GameObject newSlot = Instantiate(slotPrefab, gridParent);
            newSlot.name = $"Slot_{i}";
            slotList.Add(newSlot.transform);
            
            // Сразу делаем слот полупрозрачным, если это Image
            var img = newSlot.GetComponent<Image>();
            if(img) {
                var c = img.color;
                c.a = 0.3f; // Прозрачность
                img.color = c;
            }
        }
    }

    public void UpdateBoardVisuals()
    {
        // 1. Удаляем старые картинки карт
        foreach (var obj in playerCardObjs.Values) Destroy(obj);
        foreach (var obj in enemyCardObjs.Values) Destroy(obj);
        playerCardObjs.Clear();
        enemyCardObjs.Clear();

        // 2. Ищем данные
        var counters = FindObjectsOfType<CardCounter>();
        var player = counters.FirstOrDefault(x => !x.isEnemy);
        var enemy = counters.FirstOrDefault(x => x.isEnemy);

        // 3. Рисуем карты
        if(player != null)
            foreach (var entry in player.boardcards)
                SpawnCard(entry.Key, entry.Value, playerSlots, playerCardObjs, Color.green);

        if(enemy != null)
            foreach (var entry in enemy.boardcards)
                SpawnCard(entry.Key, entry.Value, enemySlots, enemyCardObjs, Color.red);
    }

    void SpawnCard(int index, Card data, List<Transform> slots, Dictionary<int, GameObject> tracker, Color debugColor)
    {
        if (index >= slots.Count) return;
        
        GameObject cardObj = Instantiate(cardVisualPrefab, slots[index]);
        
        // Для теста красим карту, чтобы видеть чья она
        var img = cardObj.GetComponent<Image>();
        if(img) img.color = debugColor;

        tracker.Add(index, cardObj);
    }
}