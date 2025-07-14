using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // save in the next available slot OR in the last slot (8) if all are full.
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
        // update last save time - current time and currect scene.
        state.lastSavedTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        state.currentScene = SceneManager.GetActiveScene().name;

        // converts GameState object into Json string.
        string json = JsonUtility.ToJson(state, true);
        // writes the contents of json string into file at the SavePath location.
        File.WriteAllText(GetSavePath(slotNum), json);

        // for testing.
        Debug.Log($"[SaveManager] Game saved to {GetSavePath(slotNum)}");
    }

    private static string GetSavePath(int slotNum)
    {
        return Path.Combine(Application.persistentDataPath, $"save_slot{slotNum}.json");
    }

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

    // will be used in the future for deleting saves.
    public void Clear(int slotNum)
    {
        if (SaveExists(slotNum))
        {
            File.Delete(GetSavePath(slotNum));

            Debug.Log($"[SaveManager] Save file at {GetSavePath(slotNum)} deleted.");
        }
    }

    public bool SaveExists(int slotNum)
    {
        return File.Exists(GetSavePath(slotNum));
    }
}
