using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CardCounter : MonoBehaviour
{
    public bool isEnemy = false;
    
    // Рука (ключи 0..4)
    public Dictionary<int, Card> handcards = new Dictionary<int, Card>();
    
    // Стол (ключи 0..8, где 0-2 это первый ряд)
    public Dictionary<int, Card> boardcards = new Dictionary<int, Card>(); 

    void Start()
    {
        FillCards(); 
    }

    void Update()
    {
        if (isEnemy) return;

        // Игрок выбирает, в какой слот положить карту (1, 2 или 3 на клавиатуре)
        // Это соответствует индексам 0, 1, 2 на поле
        if (Input.GetKeyDown(KeyCode.Alpha1)) PlayCardToLane(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) PlayCardToLane(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) PlayCardToLane(2);
    }

    public void FillCards()
    {
        int maxHandCards = 5; // Можно вернуть логику с пассивками позже
        while (handcards.Count < maxHandCards)
        {
            Card newCard = DeckManager.Instance.GetFirstCard();
            if (newCard == null) break;

            int newIndex = 0;
            while (handcards.ContainsKey(newIndex)) newIndex++;
            handcards.Add(newIndex, newCard);
        }
    }

    // Игрок кладет карту в конкретную линию (laneIndex = 0, 1 или 2)
    public void PlayCardToLane(int laneIndex)
    {
        // 1. Проверка: Есть ли место в этом слоте? (Мы можем класть только в начало - x=0)
        if (boardcards.ContainsKey(laneIndex))
        {
            Debug.Log("Этот слот уже занят!");
            return;
        }

        // 2. Проверка: Есть ли карты в руке?
        if (handcards.Count == 0) return;

        // Берем первую карту из руки (для простоты)
        int handKey = handcards.Keys.First();
        Card cardToPlay = handcards[handKey];

        // 3. Перемещаем
        boardcards.Add(laneIndex, cardToPlay);
        handcards.Remove(handKey);

        Debug.Log($"{(isEnemy ? "Враг" : "Игрок")} поставил {cardToPlay.name} на линию {laneIndex + 1}");
    }

    // --- ЛОГИКА ВРАГА ---
    public void EnemyAutoTurn()
    {
        // Враг тупой: пытается заполнить все 3 стартовых слота (0, 1, 2)
        // но не больше 3 карт за ход
        int cardsPlayed = 0;

        for (int i = 0; i < 3; i++) // Проходим по слотам 0, 1, 2
        {
            if (cardsPlayed >= 3) break; // Лимит хода

            // Если слот свободен
            if (!boardcards.ContainsKey(i))
            {
                PlayCardToLane(i);
                cardsPlayed++;
            }
        }
    }
}