using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DialogueDatabase : MonoBehaviour
{
    // singleton
    public static DialogueDatabase Instance { get; private set; }

    private Dictionary<string, DialogueEntry> dialogueDict = new();

    // runs immediately when the script is loaded (before the first frame) - even if the GameObject is disabled.
    private void Awake()
    {
        // safety check, if single instance already exists.
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // gets triggered when this object is enabled.
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // gets triggered when this object is disabled.
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // gets triggered when scenes get change - load the database with scene-specific dialogue.
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // skip if in the Initialization scene.
        if (scene.name == "Initialization") return;

        LoadCurrentSceneDialogue();
    }

    // returns the dialogue entry with the given ID OR null if not found.
    public DialogueEntry Get(string id)
    {
        if (dialogueDict.TryGetValue(id, out var entry)) return entry;

        // else.
        Debug.LogWarning($"[DialogueDatabase] Dialogue ID not found: {id}");
        return null;
    }

    // gets the .json file based on the current scene and loads it into the database.
    private void LoadCurrentSceneDialogue()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        string resourcePath = $"Dialogues/{sceneName}";

        TextAsset jsonFile = Resources.Load<TextAsset>(resourcePath);

        // if no such dialogue json file is found.
        if (!jsonFile)
        {
            Debug.LogWarning($"[DialogueDatabase] No dialogue file found at Resources/{resourcePath}.json");
            return;
        }

        DialogueListWrapper wrapper = JsonUtility.FromJson<DialogueListWrapper>(jsonFile.text);

        // clear the previous scene's dialogue.
        dialogueDict.Clear();

        foreach (DialogueEntry entry in wrapper.entries)
        {
            if (!string.IsNullOrEmpty(entry.id)) dialogueDict[entry.id] = entry;
        }

        Debug.Log($"[DialogueDatabase] Loaded {dialogueDict.Count} dialogues for scene '{sceneName}'.");
    }

    // JsonUtility requires a wrapper class.
    [Serializable]
    private class DialogueListWrapper
    {
        public List<DialogueEntry> entries;
    }

    // manual database load.
    public void Reload()
    {
        LoadCurrentSceneDialogue();
    }
}
