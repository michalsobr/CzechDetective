using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages saving, loading, and deleting <see cref="GameState"/> data.
/// Implements a singleton pattern and persists across scenes.
/// Runs before other scripts by default.
/// </summary>
[DefaultExecutionOrder(-100)]
public class SaveManager : MonoBehaviour
{
    #region Fields
    
    // Singleton instance.
    public static SaveManager Instance { get; private set; }

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
    /// Saves the specified <see cref="GameState"/> to the next available save slot.
    /// If all slots (1–8) are full, it overwrites the last slot (8).
    /// </summary>
    /// <param name="state">The game state to save.</param>
    public void Save(GameState state)
    {
        int chosenSlot;

        // Try slots 1–8 and find the first available slot.
        for (int i = 1; i <= 8; i++)
        {
            // If the slot is free, save to it and return.
            if (!SaveExists(i))
            {
                chosenSlot = i;
                Save(state, chosenSlot);
                return;
            }
        }

        // If all slots are full, overwrite the last slot (8).
        chosenSlot = 8;
        Save(state, chosenSlot);
    }

    /// <summary>
    /// Saves the specified <see cref="GameState"/> in the given save slot.
    /// Updates the save time, current scene, and slot number before writing the state as JSON.
    /// </summary>
    /// <param name="state">The game state to save.</param>
    /// <param name="slotNum">The slot number in which to save the game state.</param>
    public void Save(GameState state, int slotNum)
    {
        // Update the last save time, current scene, and save slot.
        state.lastSavedTime = System.DateTime.Now.ToString("d/M/yy HH:mm");
        state.currentSaveSlot = slotNum;

        // Only update the current scene if this is not the initial save from the main menu.
        if (SceneManager.GetActiveScene().name != "MainMenu")
            state.currentScene = SceneManager.GetActiveScene().name;

        // Convert the GameState object to a JSON string.
        string json = JsonUtility.ToJson(state, true);

        // Write the JSON string to the save file path.
        File.WriteAllText(GetSavePath(slotNum), json);

        Debug.Log($"[SaveManager] Game saved to {GetSavePath(slotNum)}");
    }

    /// <summary>
    /// Loads the <see cref="GameState"/> from the specified save slot.
    /// Reads the JSON file from disk, deserializes it, and returns the resulting game state.
    /// </summary>
    /// <param name="slotNum">The slot number from which to load from.</param>
    /// <returns>
    /// The deserialized <see cref="GameState"/> if the save file exists; otherwise, <c>null</c>.
    /// </returns>
    public GameState Load(int slotNum)
    {
        // Safety check: return null if the save file does not exist.
        if (!SaveExists(slotNum))
        {
            Debug.LogWarning($"[SaveManager] No save file found at {GetSavePath(slotNum)}");
            return null;
        }

        // Read the JSON file from the save path.
        string json = File.ReadAllText(GetSavePath(slotNum));

        // Deserialize the JSON into a GameState object.
        GameState loaded = JsonUtility.FromJson<GameState>(json);

        Debug.Log($"[SaveManager] Game loaded from {GetSavePath(slotNum)}");

        return loaded;
    }

    /// <summary>
    /// Deletes the save file at the specified slot number, if it exists.
    /// </summary>
    /// <param name="slotNum">The slot number of the save file to delete.</param>
    public void Clear(int slotNum)
    {
        // Only delete if the save file exists in this slot.
        if (SaveExists(slotNum))
        {
            File.Delete(GetSavePath(slotNum));

            Debug.Log($"[SaveManager] Save file at {GetSavePath(slotNum)} deleted.");
        }
    }

    /// <summary>
    /// Checks whether a save file exists for the specified slot number.
    /// </summary>
    /// <param name="slotNum">The slot number to check.</param>
    /// <returns><c>true</c> if a save file exists for the given slot; otherwise, <c>false</c>.</returns>
    public bool SaveExists(int slotNum)
    {
        return File.Exists(GetSavePath(slotNum));
    }

    #endregion
    #region Private Methods

    /// <summary>
    /// Generates the full file path for the save file associated with the specified slot number.
    /// </summary>
    /// <param name="slotNum">The slot number for which to generate the save file path.</param>
    /// <returns>The absolute file path to the save file.</returns>
    private static string GetSavePath(int slotNum)
    {
        return Path.Combine(Application.persistentDataPath, $"save_slot{slotNum}.json");
    }

    #endregion
}
