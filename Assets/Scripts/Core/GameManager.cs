using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the overall game state and provides a centralized interface for creating, loading, saving, and clearing game data. 
/// Implements a singleton pattern and persists across scenes.
/// /// </summary>
public class GameManager : MonoBehaviour
{
    #region Fields
    // Singleton instance.
    public static GameManager Instance { get; private set; }
    public GameState CurrentState { get; private set; } = null;

    #endregion
    #region Unity Lifecycle Methods

    /// <summary>
    /// Called when the script instance is loaded (even if the GameObject is inactive).
    /// Ensures a single instance and sets up persistent state across scenes.
    /// </summary>
    private void Awake()
    {
        // If another instance already exists, destroy this one.
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Prevent this object from being destroyed when loading new scenes.
        DontDestroyOnLoad(gameObject);
    }

    #endregion
    #region Public Methods

    /// <summary>
    /// Clears the current <see cref="GameState"/> (if any) and creates a new one using the specified player name and the currently active scene as the starting scene.
    /// </summary>
    /// <param name="playerName">The name of the player for the new game state.</param>
    public void CreateNewGame(string playerName)
    {
        ClearGame();
        CurrentState = GameState.NewGame(playerName, SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Loads an existing <see cref="GameState"/> into the game manager.
    /// </summary>
    /// <param name="loadedState">The game state to load as the current state.</param>

    public void LoadGameState(GameState loadedState)
    {
        CurrentState = loadedState;
    }

    /// <summary>
    /// Saves the current <see cref="GameState"/> to a save slot.
    /// If no slot number is specified, it defaults to 0 which saves to the next available or last slot.
    /// </summary>
    /// <param name="slotNum"> The save slot number. Defaults to 0, which automatically selects the next available or last slot.</param>
    public void SaveGameState(int slotNum = 0)
    {
        // Safety check: prevent saving if no game state is loaded.
        if (!IsGameLoaded)
        {
            Debug.LogError("[GameManager] Cannot save, GameState is null.");
            return;
        }

        if (slotNum == 0) SaveManager.Instance.Save(CurrentState);
        else SaveManager.Instance.Save(CurrentState, slotNum);
    }

    /// <summary>
    /// Clears the current <see cref="GameState"/> by setting it to <c>null</c>.
    /// Typically used before returning to the main menu or starting a new game.
    /// </summary>
    public void ClearGame()
    {
        CurrentState = null;
        Debug.Log("[GameManager] Game state cleared successfully (null).");
    }

    #endregion
    #region Helper Methods (Validation / Checks)

    /// <summary>
    /// Indicates whether a <see cref="GameState"/> is currently loaded.
    /// </summary>
    public bool IsGameLoaded => CurrentState != null;

    #endregion
}
