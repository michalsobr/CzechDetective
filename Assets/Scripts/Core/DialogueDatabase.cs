using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DialogueDatabase : MonoBehaviour
{
    #region Fields

    // singleton instance.
    public static DialogueDatabase Instance { get; private set; }

    private Dictionary<string, DialogueEntry> dialogueDictionary = new();

    #endregion
    #region Unity Lifecycle Methods

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
    }

    /// <summary>
    /// gets triggered every time this object is enabled -  listens for scene changes.
    /// </summary>
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    /// <summary>
    /// gets triggered every time this object is disabled -  stops listening for scene changes.
    /// </summary>
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    #endregion
    #region Public Methods

    /// <summary>
    /// get dialogue entry with this ID.
    /// </summary>
    /// <param name="id"></param>
    /// <returns> dialogue entry with that ID / NULL if not found.</returns>
    public DialogueEntry Get(string id)
    {
        if (dialogueDictionary.TryGetValue(id, out var entry)) return entry;

        // else.
        Debug.LogWarning($"[DialogueDatabase] Dialogue ID not found: {id}");
        return null;
    }

    /// <summary>
    /// method for forced manual database reload.
    /// </summary>
    public void Reload()
    {
        LoadCurrentSceneDialogue();
    }

    #endregion
    #region Private Methods

    /// <summary>
    /// gets the scene-specific .json dialogue file based on the current scene and loads it into the database / dialogue dictionary.
    /// </summary>
    private void LoadCurrentSceneDialogue()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        string resourcePath = $"Dialogues/{sceneName}";

        // loads the .json file from the generated resource path.
        TextAsset jsonFile = Resources.Load<TextAsset>(resourcePath);

        // if there was no file at that path and the text asset is therefore empty - output a warning in the log and return.
        if (!jsonFile)
        {
            Debug.LogWarning($"[DialogueDatabase] No dialogue file found at Resources/{resourcePath}.json");
            return;
        }

        // wrap the file in a wrapper so that we can convert it from .json to an object.
        DialogueListWrapper wrapper = JsonUtility.FromJson<DialogueListWrapper>(jsonFile.text);

        // clear the previous scene's dialogue (if there was one).
        dialogueDictionary.Clear();

        // load seperate entries into the dialogue dictionary (for quicker lookup).
        foreach (DialogueEntry entry in wrapper.entries)
            if (!string.IsNullOrEmpty(entry.id)) dialogueDictionary[entry.id] = entry;

        // testing debug.
        Debug.Log($"[DialogueDatabase] Loaded {dialogueDictionary.Count} dialogues for scene '{sceneName}'.");
    }

    #endregion
    #region Event Handlers / Callbacks

    /// <summary>
    /// gets triggered when scene changed - loads the current scene's specific-scene dialogue.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // skip if in the Initialization scene.
        if (scene.name == "Initialization") return;

        LoadCurrentSceneDialogue();
    }

    #endregion
    #region Nested Classes

    /// <summary>
    /// wrapper class for all dialogue entries (used to convert from .json file to object).
    /// </summary>
    [Serializable]
    private class DialogueListWrapper
    {
        public List<DialogueEntry> entries;
    }

    #endregion
}
