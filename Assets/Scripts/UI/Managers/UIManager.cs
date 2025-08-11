using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the game's main UI functionality, including popup canvases and the highlight button.
/// Implements a singleton pattern and persists across scenes.
/// </summary>
public class UIManager : MonoBehaviour
{
    #region Fields

    // Singleton instance.
    public static UIManager Instance { get; private set; }

    [Header("UI Buttons")]
    [SerializeField] public Button settingsButton;
    [SerializeField] public Button journalButton;
    [SerializeField] public Button mapButton;
    [SerializeField] public Button highlightButton;

    [Header("UI Button Group")]
    /// <summary>
    /// The list of UI button groups managed by this UI manager.
    /// Each group links a UI button to its associated popup canvas and controlling script.
    /// </summary>
    [SerializeField] private List<UIButtonGroup> buttonGroups = new();

    private bool isPopupOpen = false;
    private bool isJournalUnlocked = false;
    private bool isMapUnlocked = false;
    private bool isHighlightUnlocked = false;

    #endregion
    #region Unity Lifecycle Methods

    /// <summary>
    /// Invoked when the script instance is loaded, even if the GameObject is inactive.
    /// Ensures a single instance, sets up all button listeners, and configures the popup canvases to be hidden by default.
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

        // Hide all popup canvases by default and register button click listeners.
        foreach (var group in buttonGroups)
        {
            // Safety check: validate that all required references for this UIButtonGroup are assigned.
            // If any reference is missing, log a warning and abort the entire setup to avoid null reference errors.
            if (group == null || group.button == null || group.canvas == null || group.popupScript == null)
            {
                Debug.LogWarning("[UIManager] One or more UIButtonGroup references are missing. Aborting setup.");
                return; // Abort setup.
            }

            group.canvas.SetActive(false); // Ensure canvases start hidden.

            group.button.onClick.AddListener(() => ShowPopupCanvas(group));

            if (GameManager.Instance.CurrentState.completedDialogues.Contains("base.journal.one"))
                UnlockJournal();
            if (GameManager.Instance.CurrentState.completedDialogues.Contains("base.location.one"))
                UnlockMapAndHighlight();
        }

        // Register highlight button listener, if assigned.
        if (highlightButton) highlightButton.onClick.AddListener(HighlightInteractables);

        // Set all UI buttons to be interactable by default.
        SetAllUIButtonsActive(true);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    #endregion
    #region Public Methods

    /// <summary>
    /// Displays the popup canvas associated with the specified button group.
    /// </summary>
    /// <param name="target">The UI button group whose popup should be opened.</param>
    public void ShowPopupCanvas(UIButtonGroup target)
    {
        isPopupOpen = true;

        InteractableManager.Instance.SetAllInteractablesActive(false);
        SetAllUIButtonsActive(false);

        target.popupScript.Open();
    }

    /// <summary>
    /// Enables or disables all UI buttons.
    /// </summary>
    /// <param name="state"><c>true</c> to make UI buttons interactable; otherwise, <c>false</c>.</param>
    public void SetAllUIButtonsActive(bool state)
    {
        if (settingsButton) settingsButton.interactable = state;

        if (journalButton) journalButton.interactable = isJournalUnlocked ? state : false;
        if (mapButton) mapButton.interactable = isMapUnlocked ? state : false;
        if (highlightButton) highlightButton.interactable = isHighlightUnlocked && !DialogueManager.Instance.IsDialogueOpen ? state : false;
    }

    /// <summary>
    /// Sets the interactability state of a specific UI button managed by the UIManager.
    /// </summary>
    /// <param name="targetButton">The button whose state should be changed.</param>
    /// <param name="state"><c>true</c> to make it interactable; otherwise, <c>false</c>.</param>
    public void SetUIButtonInteractable(Button targetButton, bool state)
    {
        targetButton.interactable = state;
    }

    /// <summary>
    /// Determines whether any popup canvas is currently open.
    /// </summary>
    /// <returns><c>true</c> if a popup canvas is open; otherwise, <c>false</c>.</returns>
    public bool IsPopupOpen()
    {
        return isPopupOpen;
    }

    /// <summary>
    /// Marks that no popup canvas is currently open.
    /// </summary>
    public void ClosePopup()
    {
        isPopupOpen = false;

        SetAllUIButtonsActive(true);

        if (!DialogueManager.Instance.IsDialogueOpen)
            InteractableManager.Instance.SetAllInteractablesActive(true);
        else highlightButton.interactable = false;
    }

    public void UnlockJournal()
    {
        isJournalUnlocked = true;
        if (journalButton) journalButton.interactable = true;
    }

    public void UnlockMapAndHighlight()
    {
        isMapUnlocked = true;
        mapButton.interactable = true;

        isHighlightUnlocked = true;
        if (!DialogueManager.Instance.IsDialogueOpen) highlightButton.interactable = true;
    }

    #endregion
    #region Private Methods

    /// <summary>
    /// TODO Highlights interactable objects in the scene.
    /// </summary>
    private void HighlightInteractables()
    {
    }

    #endregion
    #region Event Handlers / Callbacks

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Hide all popup canvases by default and register button click listeners.
        foreach (var group in buttonGroups)
        {
            // Safety check: validate that all required references for this UIButtonGroup are assigned.
            // If any reference is missing, log a warning and abort the entire setup to avoid null reference errors.
            if (group == null || group.button == null || group.canvas == null || group.popupScript == null)
            {
                Debug.LogWarning("[UIManager] One or more UIButtonGroup references are missing. Aborting setup.");
                return; // Abort setup.
            }

            group.canvas.SetActive(false); // Ensure canvases are hidden.
        }

        SetAllUIButtonsActive(true);
        ClosePopup();
    }

    #endregion
}
