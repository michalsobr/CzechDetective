using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the overall game state and provides a centralized interface for creating, loading, saving, and clearing game data.
/// Implements a singleton pattern and persists across scenes.
/// Runs before other scripts by default.
/// </summary>
[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour
{
    #region Fields
    // Singleton instance.
    public static GameManager Instance { get; private set; }

    public GameState CurrentState { get; private set; } = null;

    #endregion
    #region Unity Lifecycle Methods

    /// <summary>
    /// Invoked when the script instance is loaded, even if the GameObject is inactive.
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
    /// Creates and assigns a new <see cref="GameState"/> for a new game using the specified player name.
    /// Clears any previously loaded game state.
    /// </summary>
    /// <param name="playerName">The name of the player for the new game.</param>
    public void CreateNewGame(string playerName)
    {
        ClearGame();
        CurrentState = GameState.NewGame(playerName);
    }

    /// <summary>
    /// Loads an existing <see cref="GameState"/> into the manager.
    /// </summary>
    /// <param name="loadedState">The game state to load.</param>
    public void LoadGameState(GameState loadedState)
    {
        CurrentState = loadedState;
    }

    /// <summary>
    /// Saves the current <see cref="GameState"/> to a save slot.  
    /// If <paramref name="saveSlotNum"/> is <c>0</c>, slot selection is handled by <see cref="SaveManager"/>.
    /// </summary>
    /// <param name="saveSlotNum">The save slot number. If <c>0</c>, <see cref="SaveManager"/> decides the slot automatically.</param>
    public void SaveGameState(int saveSlotNum = 0)
    {
        // Safety check: prevent saving if no game state is loaded.
        if (!IsGameLoaded)
        {
            Debug.LogError("[GameManager] Cannot save, GameState is null.");
            return;
        }

        if (saveSlotNum == 0) SaveManager.Instance.Save(CurrentState);
        else SaveManager.Instance.Save(CurrentState, saveSlotNum);
    }

    /// <summary>
    /// Clears the current <see cref="GameState"/> by setting it to <c>null</c>.  
    /// Used when returning to the main menu or starting a new game.
    /// </summary>
    public void ClearGame()
    {
        CurrentState = null;
        Debug.Log("[GameManager] Game state cleared successfully (null).");
    }

    /// <summary>
    /// Ensures that all required persistent managers exist in the scene, instantiating them from prefabs if missing.
    /// </summary>
    public void InitializeManagers()
    {
        if (FindFirstObjectByType<UIManager>() != null)
            DestroyImmediate(FindFirstObjectByType<UIManager>().gameObject);

        if (FindFirstObjectByType<DialogueManager>() != null)
            DestroyImmediate(FindFirstObjectByType<DialogueManager>().gameObject);

        if (FindFirstObjectByType<EventSystemDDOL>() == null)
            Instantiate(Resources.Load("Prefabs/EventSystemDDOL"));

        if (FindFirstObjectByType<GameManager>() == null)
            Instantiate(Resources.Load("Prefabs/GameManager"));

        if (FindFirstObjectByType<SaveManager>() == null)
            Instantiate(Resources.Load("Prefabs/SaveManager"));

        if (FindFirstObjectByType<UIManager>() == null)
            Instantiate(Resources.Load("Prefabs/UIManager"));

        if (FindFirstObjectByType<DialogueManager>() == null)
            Instantiate(Resources.Load("Prefabs/DialogueManager"));
    }

    #endregion
    #region Helper Methods (Validation / Checks)

    /// <summary>
    /// Indicates whether a <see cref="GameState"/> is currently loaded.
    /// </summary>
    public bool IsGameLoaded => CurrentState != null;

    #endregion
}
