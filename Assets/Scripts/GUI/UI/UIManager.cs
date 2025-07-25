using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the game's main UI functionality, including popup canvases and the highlight button.
/// Implements a singleton pattern and persists across scenes.
/// </summary>
public class UIManager : MonoBehaviour
{
    #region Fields

    // Singleton instance.
    public static UIManager Instance { get; private set; }

    [Header("UI Button Group")]
    /// <summary>
    /// The list of UI button groups managed by this UI manager.
    /// Each group links a UI button to its associated popup canvas and controlling script.
    /// </summary>
    [SerializeField] private List<UIButtonGroup> buttonGroups = new();

    [Header("Highlight Button")]
    [SerializeField] private Button highlightButton;

    private bool isInteractable = true;
    private bool isPopupOpen = false;

    #endregion
    #region Unity Lifecycle Methods

    /// <summary>
    /// Called when the script instance is loaded (even if the GameObject is inactive).
    /// Ensures a single instance, sets up persistent state across scenes, sets up all button listeners, and configures the popup canvases to be hidden by default.
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
            if (group.button && group.canvas && group.popupScript)
            {
                group.canvas.SetActive(false); // Ensure canvases start hidden.

                group.button.onClick.AddListener(() => ShowPopupCanvas(group));
            }
        }

        // Register highlight button listener, if assigned.
        highlightButton?.onClick.AddListener(HighlightInteractables);

        // Apply the initial interactability state to all UI buttons.
        SetInteractable(isInteractable);
    }

    #endregion
    #region Public Methods

    /// <summary>
    /// Displays the popup canvas associated with the specified button group.
    /// </summary>
    /// <param name="target">The UI button group whose popup should be opened.</param>
    public void ShowPopupCanvas(UIButtonGroup target)
    {
        target.popupScript.Open();
        isPopupOpen = true;
        SetInteractable(false);
    }

    /// <summary>
    /// Enables or disables all UI buttons.
    /// </summary>
    /// <param name="state"><c>true</c> to make UI buttons interactable; otherwise, <c>false</c>.</param>
    public void SetInteractable(bool state)
    {
        isInteractable = state;

        foreach (var group in buttonGroups)
        {
            if (group.button != null) group.button.interactable = state;
        }
        if (highlightButton) highlightButton.interactable = state;
    }

    /// <summary>
    /// Determines whether UI buttons are currently interactable.
    /// </summary>
    /// <returns><c>true</c> if the UI is interactable; otherwise, <c>false</c>.</returns>
    public bool IsInteractable()
    {
        return isInteractable;
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
}
