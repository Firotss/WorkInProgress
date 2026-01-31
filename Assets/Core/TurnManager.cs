using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class TurnManager : MonoBehaviour
{
    [Header("Turn Settings")]
    [SerializeField] private float turnProcessingDelay = 0.5f;
    
    public int CurrentTurn { get; private set; }
    public int CurrentRound { get; private set; }
    
    public event Action<int> OnTurnStarted;
    public event Action<int> OnTurnEnded;
    public event Action<int> OnRoundChanged;

    private GameManager gameManager;
    private BoardManager playerBoard;
    private BoardManager enemyBoard;
    private Player player;
    private Monster monster;
    private Hand hand;
    private EnemyAI enemyAI;

    public void Initialize(GameManager gm, BoardManager pBoard, BoardManager eBoard, 
        Player p, Monster m, Hand h, EnemyAI ai)
    {
        gameManager = gm;
        playerBoard = pBoard;
        enemyBoard = eBoard;
        player = p;
        monster = m;
        hand = h;
        enemyAI = ai;
        
        CurrentTurn = 0;
        CurrentRound = 1;
    }

    public void StartTurn()
    {
        CurrentTurn++;
        Debug.Log($"=== Turn {CurrentTurn} Started (Round {CurrentRound}) ===");
        
        OnTurnStarted?.Invoke(CurrentTurn);
        
        gameManager.SetGameState(GameState.PlayerTurn);
    }

    public void EndTurn()
    {
        if (gameManager.CurrentState != GameState.PlayerTurn)
        {
            Debug.LogWarning("Cannot end turn - not player's turn!");
            return;
        }
        
        StartCoroutine(ProcessTurnCoroutine());
    }

    private IEnumerator ProcessTurnCoroutine()
    {
        gameManager.SetGameState(GameState.ProcessingTurn);
        Debug.Log("Processing turn...");
        
        // Deselect any selected card
        hand.DeselectCard();
        
        // Step 1: Advance player cards (row 2 cards activate against monster)
        yield return new WaitForSeconds(turnProcessingDelay);
        Debug.Log("--- Player cards activating ---");
        List<Card> playerActivatedCards = playerBoard.AdvanceCards(player, monster);
        Debug.Log($"Player activated {playerActivatedCards.Count} cards.");
        
        // Check if monster was defeated
        if (monster.IsDefeated)
        {
            gameManager.SetGameState(GameState.Victory);
            yield break;
        }
        
        // Check if monster stage changed (round change)
        if (monster.CurrentStage > CurrentRound)
        {
            CurrentRound = monster.CurrentStage;
            OnRoundChanged?.Invoke(CurrentRound);
            Debug.Log($"=== Round {CurrentRound} Begins! ===");
        }
        
        // Step 2: Advance enemy cards (row 2 cards activate against player)
        yield return new WaitForSeconds(turnProcessingDelay);
        Debug.Log("--- Enemy cards activating ---");
        List<Card> enemyActivatedCards = enemyBoard.AdvanceCards(player, monster);
        Debug.Log($"Enemy activated {enemyActivatedCards.Count} cards.");
        
        // Check if player died from enemy cards
        if (player.Health <= 0)
        {
            gameManager.SetGameState(GameState.GameOver);
            yield break;
        }
        
        // Step 3: Enemy plays new cards
        yield return new WaitForSeconds(turnProcessingDelay);
        gameManager.SetGameState(GameState.MonsterTurn);
        Debug.Log("--- Enemy playing cards ---");
        enemyAI.PlayTurn();
        
        // Step 4: Draw cards for player
        yield return new WaitForSeconds(turnProcessingDelay);
        Debug.Log("--- Drawing cards ---");
        hand.DrawTurnCards(); // Draw 3 cards
        
        OnTurnEnded?.Invoke(CurrentTurn);
        
        // Start new turn
        StartTurn();
    }

    public void Reset()
    {
        CurrentTurn = 0;
        CurrentRound = 1;
    }
}
