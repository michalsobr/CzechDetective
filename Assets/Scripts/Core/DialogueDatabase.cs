using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Centralized manager for loading and storing dialogue entries for the current scene.
/// Uses a singleton pattern and loads scene-specific dialogue data from JSON files in the Resources folder.
/// </summary>
public class DialogueDatabase : MonoBehaviour
{
    #region Fields

    // Singleton instance.
    public static DialogueDatabase Instance { get; private set; }

    private Dictionary<string, DialogueEntry> dialogueDictionary = new();

    #endregion
    #region Unity Lifecycle Methods

    /// <summary>
    /// Called when the script instance is loaded (even if the GameObject is inactive).
    /// Ensures a single instance of this object exists (singleton pattern).
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
        // This object persists across scenes as part of the Dialogue Manager.
    }

    /// <summary>
    /// Called each time the object becomes enabled.
    /// Registers a listener for scene load events.
    /// </summary>
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    /// <summary>
    /// Called each time the object becomes disabled.
    /// Unregisters the scene load event listener.
    /// </summary>
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    #endregion
    #region Public Methods

    /// <summary>
    /// Retrieves a dialogue entry by its unique ID.
    /// </summary>
    /// <param name="id">The unique identifier of the dialogue entry.</param>
    /// <returns>The matching <see cref="DialogueEntry"/>, or <c>null</c> if not found.</returns>
    public DialogueEntry Get(string id)
    {
        if (dialogueDictionary.TryGetValue(id, out var entry)) return entry;

        // If not found, log a warning and return null.
        Debug.LogWarning($"[DialogueDatabase] Dialogue ID not found: {id}");
        return null;
    }

    /// <summary>
    /// Forces a manual reload of the dialogue database for the current scene.
    /// </summary>
    public void Reload()
    {
        LoadCurrentSceneDialogue();
    }

    #endregion
    #region Private Methods

    /// <summary>
    /// Loads the scene-specific JSON dialogue file from the Resources folder and populates the dialogue dictionary with its entries.
    /// </summary>
    private void LoadCurrentSceneDialogue()
    {
        // Get the current scene name and construct the dialogue resource path.
        string sceneName = SceneManager.GetActiveScene().name;
        string resourcePath = $"Dialogues/{sceneName}";

        // Load the JSON file from the Resources folder.
        TextAsset jsonFile = Resources.Load<TextAsset>(resourcePath);

        // If the file was not found, log a warning and exit early.
        if (!jsonFile)
        {
            Debug.LogWarning($"[DialogueDatabase] No dialogue file found at Resources/{resourcePath}.json");
            return;
        }

        // Deserialize the JSON file into a wrapper object.
        DialogueListWrapper wrapper = JsonUtility.FromJson<DialogueListWrapper>(jsonFile.text);

        // Clear any previously loaded dialogue entries.
        dialogueDictionary.Clear();

        // Add each valid entry to the dialogue dictionary for quick lookup.
        foreach (DialogueEntry entry in wrapper.entries)
            if (!string.IsNullOrEmpty(entry.id)) dialogueDictionary[entry.id] = entry;

        // Log how many entries were successfully loaded (for debugging).
        Debug.Log($"[DialogueDatabase] Loaded {dialogueDictionary.Count} dialogues for scene '{sceneName}'.");
    }

    #endregion
    #region Event Handlers / Callbacks

    /// <summary>
    /// Called when a new scene is loaded. Loads the dialogue data for the current scene.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Skip dialogue loading for the Initialization scene.
        if (scene.name == "Initialization") return;

        LoadCurrentSceneDialogue();
    }

    #endregion
    #region Nested Classes

    /// <summary>
    /// Wrapper class used to deserialize a list of dialogue entries from a JSON file.
    /// </summary>
    [Serializable]
    private class DialogueListWrapper
    {
        public List<DialogueEntry> entries;
    }

    #endregion
}
