using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    public CardCounter player;
    public CardCounter enemy;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        // Нажми ENTER, чтобы закончить ход
        if (Input.GetKeyDown(KeyCode.Return))
        {
            EndTurn();
        }
    }

    public void EndTurn()
    {
        Debug.Log("<color=yellow>--- КОНЕЦ ХОДА ---</color>");

        // 1. Ход врага
        enemy.EnemyAutoTurn();

        // 2. Движение карт вперед
        MoveCardsForward(player);
        MoveCardsForward(enemy);

        // 3. Добор карт
        player.FillCards();
        enemy.FillCards();

        // 4. РИСУЕМ ПОЛЕ В КОНСОЛЬ
        DebugPrintBoard();
    }

    void MoveCardsForward(CardCounter counter)
    {
        List<int> keys = new List<int>(counter.boardcards.Keys);
        keys.Sort();
        keys.Reverse(); // Начинаем с конца (8 -> 0)

        foreach (int oldPos in keys)
        {
            Card card = counter.boardcards[oldPos];
            int newPos = oldPos + 3; // Шаг вперед на 1 ряд

            counter.boardcards.Remove(oldPos);

            if (newPos > 8)
            {
                // Сжигаем
                Debug.Log($"[{counter.name}] Карта {card.name} сгорела на финише!");
                DeckManager.Instance.AddToDiscard(card);
            }
            else
            {
                // Перезаписываем, если занято (упрощенно)
                if (counter.boardcards.ContainsKey(newPos))
                {
                    DeckManager.Instance.AddToDiscard(counter.boardcards[newPos]);
                }
                counter.boardcards.Add(newPos, card);
            }
        }
    }

    // Тот самый метод для визуализации
    void DebugPrintBoard()
    {
        string boardState = "\n--- ТЕКУЩЕЕ ПОЛЕ (P=Игрок, E=Враг) ---\n";
        
        // Ряды: 2 (Финиш), 1 (Центр), 0 (Старт)
        for (int row = 2; row >= 0; row--)
        {
            string line = $"Ряд {row}: ";
            for (int col = 0; col < 3; col++)
            {
                int index = row * 3 + col; // 0..8
                
                string pCard = player.boardcards.ContainsKey(index) ? $"[P:{player.boardcards[index].name.Substring(0,3)}]" : "[ . . ]";
                string eCard = enemy.boardcards.ContainsKey(index) ? $"[E:{enemy.boardcards[index].name.Substring(0,3)}]" : "[ . . ]";
                
                // Показываем карты обоих в одной ячейке (для теста)
                line += $"{pCard}|{eCard}   "; 
            }
            boardState += line + "\n";
        }
        Debug.Log(boardState);
    }
}