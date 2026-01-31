using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CardCounter : MonoBehaviour
{
    public bool isEnemy = false;
    
    // Рука (0..4)
    public Dictionary<int, Card> handcards = new Dictionary<int, Card>();
    
    // Стол: 3 ряда по 5 колонок = 15 слотов (0..14)
    public Dictionary<int, Card> boardcards = new Dictionary<int, Card>(); 

    // ОГРАНИЧЕНИЕ НА ХОД
    private int cardsPlayedThisTurn = 0;
    private const int MAX_CARDS_PER_TURN = 3;

    void Start()
    {
        FillCards(); 
    }

    void Update()
    {
        if (isEnemy) return;

        // Управление: 1, 2, 3, 4, 5
        if (Input.GetKeyDown(KeyCode.Alpha1)) TryPlayCard(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) TryPlayCard(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) TryPlayCard(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) TryPlayCard(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) TryPlayCard(4);
    }

    // Метод начала нового хода (вызывается из TurnManager)
    public void StartNewTurn()
    {
        cardsPlayedThisTurn = 0; // Сбрасываем счетчик
        FillCards();             // Добираем руку
        Debug.Log($"{(isEnemy ? "Враг" : "Игрок")} готов к ходу. Сыграно: 0/3");
    }

    public void FillCards()
    {
        int maxHandCards = 5; 
        while (handcards.Count < maxHandCards)
        {
            Card newCard = DeckManager.Instance.GetFirstCard();
            if (newCard == null) break;

            int newIndex = 0;
            while (handcards.ContainsKey(newIndex)) newIndex++;
            handcards.Add(newIndex, newCard);
        }
    }

    // Попытка сыграть карту (с проверкой лимита)
    public void TryPlayCard(int laneIndex)
    {
        // 1. Проверка лимита действий (максимум 3)
        if (cardsPlayedThisTurn >= MAX_CARDS_PER_TURN)
        {
            Debug.Log($"{(isEnemy ? "Враг" : "Игрок")}: Лимит ходов исчерпан (3/3)!");
            return;
        }

        // 2. Проверка: занят ли слот
        if (boardcards.ContainsKey(laneIndex))
        {
            Debug.Log("Слот занят!");
            return;
        }

        // 3. Проверка: есть ли карты в руке
        if (handcards.Count == 0) return;

        // ИГРАЕМ КАРТУ
        int handKey = handcards.Keys.First();
        Card cardToPlay = handcards[handKey];

        boardcards.Add(laneIndex, cardToPlay);
        handcards.Remove(handKey);
        
        cardsPlayedThisTurn++; // Увеличиваем счетчик

        Debug.Log($"{(isEnemy?"Враг":"Игрок")} поставил {cardToPlay.name} на линию {laneIndex+1}. ({cardsPlayedThisTurn}/3)");
        
        // Обновляем графику
        if (BoardVisualizer.Instance != null) BoardVisualizer.Instance.UpdateBoardVisuals();
    }

    // Логика бота
    public void EnemyAutoTurn()
    {
        // Враг пытается заполнить случайные линии, но не больше 3
        List<int> availableLanes = new List<int> { 0, 1, 2, 3, 4 };
        
        // Перемешиваем линии, чтобы враг был непредсказуемым
        availableLanes = availableLanes.OrderBy(x => Random.value).ToList();

        foreach (int lane in availableLanes)
        {
            if (cardsPlayedThisTurn >= MAX_CARDS_PER_TURN) break;
            
            if (!boardcards.ContainsKey(lane))
            {
                TryPlayCard(lane);
            }
        }
    }
}