using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class TurnManager : MonoBehaviour
{
    [Header("Turn Settings")]
    [SerializeField] private float turnProcessingDelay = 0.5f;
    [SerializeField] private int basePlacementsPerTurn = 3;

    [Header("Stage Change Dialogue")]
    [SerializeField] private string stage2Dialogue = "That power... I've felt it before. It wasn't their own—it came from something they had.";
    [SerializeField] private string stage3Dialogue = "Yes. You remember. You lost to me once. That is why you woke here—in this desolate place.";
    [SerializeField] private string stage2DialogueSpeaker = "Player";
    [SerializeField] private string stage3DialogueSpeaker = "Masked Entity";

    public int CurrentTurn { get; private set; }
    public int CurrentRound { get; private set; }
    public int PlacementsThisTurn { get; private set; }

    public int MaxPlacementsPerTurn =>
        basePlacementsPerTurn + (playerBoard != null ? playerBoard.GetCountOfCardsByColor("green") : 0);

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
        PlacementsThisTurn = 0;
        Debug.Log($"=== Turn {CurrentTurn} Started (Round {CurrentRound}) ===");

        OnTurnStarted?.Invoke(CurrentTurn);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartNewTurn();
        }
        
        gameManager.SetGameState(GameState.PlayerTurn);
    }

    public bool CanPlaceCard()
    {
        return gameManager != null && gameManager.CurrentState == GameState.PlayerTurn &&
               PlacementsThisTurn < MaxPlacementsPerTurn;
    }

    public void RecordCardPlaced()
    {
        if (PlacementsThisTurn < MaxPlacementsPerTurn)
            PlacementsThisTurn++;
    }

    public void RecordCardWithdrawn()
    {
        PlacementsThisTurn = Mathf.Max(0, PlacementsThisTurn - 1);
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
        int playerRed = playerBoard.GetTotalScoreByColor("red");
        int enemyBlue = enemyBoard.GetTotalScoreByColor("blue");
        int baseDamageToMonster = Mathf.Max(0, playerRed - enemyBlue);
        float monsterMult = 1f;
        if (playerBoard.HasRow0ColorCombo("red"))
        {
            monsterMult *= 2f;
            Debug.Log("Combo: 3 red on player row 0 -> +100% damage dealt.");
        }
        if (enemyBoard.HasRow0ColorCombo("blue"))
        {
            monsterMult *= 0.5f;
            Debug.Log("Combo: 3 blue on enemy row 0 -> -50% damage received.");
        }
        int damageToMonster = Mathf.RoundToInt(baseDamageToMonster * monsterMult);
        if (damageToMonster > 0)
        {
            monster.TakeDamage(damageToMonster);
            Debug.Log($"Player damage: base {baseDamageToMonster} x{monsterMult} = {damageToMonster} to monster.");
        }

        if (monster.IsDefeated)
        {
            // Victory is handled by GameManager.OnMonsterDefeated() and the final boss cutscene.
            yield break;
        }

        // If monster stage changed, show one-line dialogue and wait for player to dismiss
        if (monster.CurrentStage > CurrentRound)
        {
            DialogueManager dialogueManager = UnityEngine.Object.FindObjectOfType<DialogueManager>(true);
            if (dialogueManager != null)
            {
                string lineText = monster.CurrentStage == 2 ? stage2Dialogue : stage3Dialogue;
                string speaker = monster.CurrentStage == 2 ? stage2DialogueSpeaker : stage3DialogueSpeaker;
                bool isPlayer = monster.CurrentStage == 2;
                var lines = new List<DialogueManager.DialogueLine>
                {
                    new DialogueManager.DialogueLine(speaker, lineText, isPlayer)
                };
                bool dialogueDone = false;
                void OnStageDialogueDone() => dialogueDone = true;
                dialogueManager.OnDialogueCompleted += OnStageDialogueDone;
                dialogueManager.StartDialogueWithLines(lines);
                while (!dialogueDone)
                    yield return null;
                dialogueManager.OnDialogueCompleted -= OnStageDialogueDone;
            }
            CurrentRound = monster.CurrentStage;
            OnRoundChanged?.Invoke(CurrentRound);
        }

        yield return new WaitForSeconds(turnProcessingDelay);
        playerBoard.AdvanceCards();

        yield return new WaitForSeconds(turnProcessingDelay);
        gameManager.SetGameState(GameState.MonsterTurn);
        enemyAI.PlayTurn();

        yield return new WaitForSeconds(turnProcessingDelay);
        int enemyRed = enemyBoard.GetTotalScoreByColor("red");
        int playerBlue = playerBoard.GetTotalScoreByColor("blue");
        int baseDamageToPlayer = Mathf.Max(0, enemyRed - playerBlue);
        float playerDamageMult = 1f;
        if (enemyBoard.HasRow0ColorCombo("red"))
        {
            playerDamageMult *= 2f;
            Debug.Log("Combo: 3 red on enemy row 0 -> +100% damage dealt to player.");
        }
        if (playerBoard.HasRow0ColorCombo("blue"))
        {
            playerDamageMult *= 0.5f;
            Debug.Log("Combo: 3 blue on player row 0 -> -50% damage received.");
        }
        int damageToPlayer = Mathf.RoundToInt(baseDamageToPlayer * playerDamageMult);
        if (damageToPlayer > 0)
        {
            player.TakeDamage(damageToPlayer);
            Debug.Log($"Enemy damage: base {baseDamageToPlayer} x{playerDamageMult} = {damageToPlayer} to player.");
        }

        if (player.Health <= 0)
        {
            gameManager.SetGameState(GameState.GameOver);
            yield break;
        }

        // Step 3: Enemy plays new cards
        yield return new WaitForSeconds(turnProcessingDelay);
        enemyBoard.AdvanceCards();

        yield return new WaitForSeconds(turnProcessingDelay);
        hand.FillCards();

        OnTurnEnded?.Invoke(CurrentTurn);
        StartTurn();
    }

    public void Reset()
    {
        CurrentTurn = 0;
        CurrentRound = 1;
    }
}
