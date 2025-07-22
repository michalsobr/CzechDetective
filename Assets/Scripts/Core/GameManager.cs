using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // singleton instance..
    public static GameManager Instance { get; private set; }
    public GameState CurrentState { get; private set; } = null;

    // runs immediately when the script is loaded (before the first frame) - even if the GameObject is disabled.
        /// <summary>
    /// runs immediately when the script is loaded (before the first frame) - even if the GameObject is disabled - makes sure only a single instance of this object exists.
    /// </summary>
    private void Awake()
    {
        // safety check, if single instance already exists.
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        // persist across scenes.
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // creates a new GameState with player name and the starting scene.
    public void CreateNewGame(string playerName)
    {
        CurrentState = GameState.NewGame(playerName, SceneManager.GetActiveScene().name);
    }

    // loads an existing GameState.
    public void LoadGameState(GameState loadedState)
    {
        CurrentState = loadedState;
    }

    // saves the current game state, if given no paremeters defaults to "slot 0" which saves at the next available (or last) save slot OR if given a slot number - save in that slot.
    public void SaveGameState(int slotNum = 0)
    {
        // safety check.
        if (!IsGameLoaded)
        {
            Debug.LogError("[GameManager] Cannot save, GameState is null.");
            return;
        }

        if (slotNum == 0) SaveManager.Instance.Save(CurrentState);
        else SaveManager.Instance.Save(CurrentState, slotNum);
    }

    // for future use - returning to main menu.
    public void ClearGame()
    {
        CurrentState = null;
        Debug.Log("[GameManager] Game state cleared sucessfully (null).");
    }

    public bool IsGameLoaded => CurrentState != null;
}
