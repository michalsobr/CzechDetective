using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DialogueDatabase : MonoBehaviour
{
    public static DialogueDatabase Instance { get; private set; }

    private Dictionary<string, DialogueEntry> dialogueDict = new();

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

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // when a new scene is loaded, load the database with scene-specific dialogue.
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // skip if in the Init scene.
        if (scene.name == "Init") return;

        LoadCurrentSceneDialogue();
    }

    /// returns the dialogue entry with the given ID, or null if not found.
    public DialogueEntry Get(string id)
    {
        if (dialogueDict.TryGetValue(id, out var entry))
        {
            return entry;
        }
        // else.
        Debug.LogWarning($"[DialogueDatabase] Dialogue ID not found: {id}");
        return null;
    }

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

        dialogueDict.Clear();

        foreach (DialogueEntry entry in wrapper.entries)
        {
            if (!string.IsNullOrEmpty(entry.id))
            {
                dialogueDict[entry.id] = entry;
            }
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
