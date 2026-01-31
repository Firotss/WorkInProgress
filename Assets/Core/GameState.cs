/// <summary>
/// Enumeration of possible game states.
/// </summary>
public enum GameState
{
    /// <summary>
    /// Game not yet started.
    /// </summary>
    NotStarted,
    
    /// <summary>
    /// Player's turn - can place cards.
    /// </summary>
    PlayerTurn,
    
    /// <summary>
    /// Processing end of turn (card effects, movement).
    /// </summary>
    ProcessingTurn,
    
    /// <summary>
    /// Monster's turn - monster attacks.
    /// </summary>
    MonsterTurn,
    
    /// <summary>
    /// Game over - player won.
    /// </summary>
    Victory,
    
    /// <summary>
    /// Game over - player lost.
    /// </summary>
    GameOver
}
