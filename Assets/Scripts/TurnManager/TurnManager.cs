using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;
    public CardCounter player;
    public CardCounter enemy;

    const int COLUMNS = 5; 
    const int TOTAL_SLOTS = 15; // 3 ряда * 5

    void Awake() { Instance = this; }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return)) EndTurn();
    }

    public void EndTurn()
    {
        Debug.Log("\n<color=yellow>=== ЗАВЕРШЕНИЕ ХОДА ===</color>");

        // 1. Враг делает свои действия (до 3 карт)
        enemy.EnemyAutoTurn();

        // 2. Двигаем карты вперед
        MoveCardsForward(player);
        MoveCardsForward(enemy);

        // 3. Начинаем новый ход (сброс счетчиков и добор карт)
        player.StartNewTurn();
        enemy.StartNewTurn();
        
        // 4. Обновляем визуал
        if (BoardVisualizer.Instance != null) BoardVisualizer.Instance.UpdateBoardVisuals();
    }

    void MoveCardsForward(CardCounter counter)
    {
        // Сортируем ключи с конца, чтобы карты не наехали друг на друга
        List<int> keys = new List<int>(counter.boardcards.Keys);
        keys.Sort();
        keys.Reverse(); 

        foreach (int oldPos in keys)
        {
            Card card = counter.boardcards[oldPos];
            int newPos = oldPos + COLUMNS; // +5 слотов вперед

            counter.boardcards.Remove(oldPos);

            // Если карта ушла за пределы 15 слотов -> Сброс
            if (newPos >= TOTAL_SLOTS)
            {
                Debug.Log($"Карта {card.name} сгорела!");
                DeckManager.Instance.AddToDiscard(card);
            }
            else
            {
                // Если клетка впереди занята (баг или наезд) - сбрасываем старую
                if (counter.boardcards.ContainsKey(newPos))
                    DeckManager.Instance.AddToDiscard(counter.boardcards[newPos]);
                
                counter.boardcards.Add(newPos, card);
            }
        }
    }
}