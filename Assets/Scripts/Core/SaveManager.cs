using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

// runs before other scripts by deafault.
[DefaultExecutionOrder(-100)]
public class SaveManager : MonoBehaviour
{
    // singleton instance..
    public static SaveManager Instance { get; private set; }

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
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // determine the next available saving slot OR default to saving in the last slot (8) if all the other slots are full.
    public void Save(GameState state)
    {
        int chosenSlot;

        // try slots 1 - 8.
        for (int i = 1; i <= 8; i++)
        {
            // if save doesn't exist at this slot - slot is free.
            if (!SaveExists(i))
            {
                chosenSlot = i;
                Save(state, chosenSlot);
                return;
            }
        }
        // otherwise overwrite the last slot (8).
        chosenSlot = 8;
        Save(state, chosenSlot);
    }

    // save in a specific slot.
    public void Save(GameState state, int slotNum)
    {
        // update the last save time to current time, the current scene name and the current save slot.
        state.lastSavedTime = System.DateTime.Now.ToString("d/M/yy HH:mm");
        state.currentScene = SceneManager.GetActiveScene().name;
        state.currentSaveSlot = slotNum;

        // converts GameState object into Json string.
        string json = JsonUtility.ToJson(state, true);
        // writes the contents of json string into file at the SavePath location.
        File.WriteAllText(GetSavePath(slotNum), json);

        Debug.Log($"[SaveManager] Game saved to {GetSavePath(slotNum)}");
    }

    // get the save file data path given a slot number.
    private static string GetSavePath(int slotNum)
    {
        return Path.Combine(Application.persistentDataPath, $"save_slot{slotNum}.json");
    }

    // reads the .json file at the given slot number and returns it as a GameState.
    public GameState Load(int slotNum)
    {
        if (!SaveExists(slotNum))
        {
            Debug.LogWarning($"[SaveManager] No save file found at {GetSavePath(slotNum)}");
            return null;
        }
        // else
        string json = File.ReadAllText(GetSavePath(slotNum));
        GameState loaded = JsonUtility.FromJson<GameState>(json);

        Debug.Log($"[SaveManager] Game loaded from {GetSavePath(slotNum)}");

        return loaded;
    }

    // for future use - deleting a save file at the given slot number.
    public void Clear(int slotNum)
    {
        if (SaveExists(slotNum))
        {
            File.Delete(GetSavePath(slotNum));

            Debug.Log($"[SaveManager] Save file at {GetSavePath(slotNum)} deleted.");
        }
    }

    // checks if a path to a save file given a slot number exists.
    public bool SaveExists(int slotNum)
    {
        return File.Exists(GetSavePath(slotNum));
    }
}
