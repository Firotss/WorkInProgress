using UnityEngine;
using System; // Нужно за Action
using System.Collections;

public class TurnManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float turnDelay = 1.0f;

    private GameManager gameManager;
    private BoardManager playerBoard;
    private BoardManager enemyBoard;
    private Player player;
    private Monster monster;
    private Hand hand;
    private EnemyAI enemyAI;

    public int CurrentTurn { get; private set; } = 1;
    public int CurrentRound { get; private set; } = 1;

    // --- ЛИПСВАЩИТЕ СЪБИТИЯ ЗА UIManager ---
    public event Action<int> OnTurnStarted;
    public event Action<int> OnRoundChanged;
    // ---------------------------------------

    public int PlacementsThisTurn { get; private set; }
    public int MaxPlacementsPerTurn => 3;

    public void Initialize(GameManager gm, BoardManager pb, BoardManager eb, Player p, Monster m, Hand h, EnemyAI ai)
    {
        gameManager = gm;
        playerBoard = pb;
        enemyBoard = eb;
        player = p;
        monster = m;
        hand = h;
        enemyAI = ai;
    }

    public void StartTurn()
    {
        Debug.Log($"--- НАЧАЛО ХОДА {CurrentTurn} ---");
        PlacementsThisTurn = 0;
        
        // Обновяваме UI
        OnTurnStarted?.Invoke(CurrentTurn);
        OnRoundChanged?.Invoke(CurrentRound);

        // 1. Обновяваме ръката на играча
        if (hand != null)
        {
            hand.DrawCardsUntilFull(); // Вече съществува в Hand.cs
            hand.RefreshHandVisuals(); // Вече съществува в Hand.cs
        }

        // 2. Предаваме контрол на играча
        if (gameManager != null)
        {
            gameManager.StartNewTurn();
            gameManager.SetGameState(GameState.PlayerTurn);
        }
    }

    public void EndTurn()
    {
        if (gameManager.CurrentState != GameState.PlayerTurn) return;

        Debug.Log("Игрок завершил ход. Переход к противнику...");
        StartCoroutine(ProcessEnemyTurn());
    }

    private IEnumerator ProcessEnemyTurn()
    {
        gameManager.SetGameState(GameState.MonsterTurn);
        yield return new WaitForSeconds(turnDelay);

        if (enemyAI != null)
        {
            enemyAI.PlayTurn();
        }

        yield return new WaitForSeconds(1.5f);

        // Тук ще е логиката за битка и движение на картите
        // MoveCardsAndFight();

        CurrentTurn++;
        StartTurn();
    }

    public void RecordCardPlaced()
    {
        PlacementsThisTurn++;
    }

    public void RecordCardWithdrawn()
    {
        if (PlacementsThisTurn > 0) PlacementsThisTurn--;
    }

    public void Reset()
    {
        CurrentTurn = 1;
        CurrentRound = 1;
    }
}