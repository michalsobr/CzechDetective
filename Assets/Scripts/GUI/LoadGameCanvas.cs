using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the Load Game popup in the main menu. 
/// Handles displaying save slots, toggling pages, and opening the confirmation panel for loading or deleting saves.
/// </summary>
public class LoadGameCanvas : MonoBehaviour
{
    #region Fields

    [Header("UI References")]
    [SerializeField] private MainMenuController mainMenuController;

    [Header("Popup Elements")]
    [SerializeField] private GameObject loadGamePanel;
    [SerializeField] private NoticePanel noticePanel;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button navigationButton;
    [SerializeField] private Image navigationButtonImage;
    [SerializeField] private Sprite upIcon;
    [SerializeField] private Sprite downIcon;

    [Header("Save Slot Groups")]
    [SerializeField] private List<SaveSlotGroup> saveSlotGroups = new();

    /// <summary>
    /// The currently active page index (0 = save slots 1–4, 1 = save slots 5–8).
    /// </summary>
    private int currentPage = 0;

    #endregion
    #region Unity Lifecycle Methods

    /// <summary>
    /// Called when the script instance is loaded (even if the GameObject is inactive).
    /// Sets up button listeners, hides all delete buttons by default, and ensures the canvas is hidden by default.
    /// </summary>
    private void Awake()
    {
        if (closeButton) closeButton.onClick.AddListener(HideLoadGame);
        if (navigationButton) navigationButton.onClick.AddListener(TogglePage);

        int slotNumber = 1;

        foreach (var group in saveSlotGroups)
        {
            // Safety check: validate that all required references for this SaveSlotGroup are assigned.
            // If any reference is missing, log a warning and abort the entire setup to avoid null reference errors.
            if (group == null || group.slotButton == null || group.deleteButton == null || group.deleteSlotButton == null || group.slotVisualizer == null)
            {
                Debug.LogWarning("[LoadGameCanvas] One or more SaveSlotGroup references are missing. Aborting setup.");
                return; // Abort setup.
            }

            int uiSlotNumber = slotNumber;

            group.slotButton.onClick.AddListener(() =>
                OnNoticePanelOpen(true, uiSlotNumber, group.slotVisualizer.gameStateCopy));
            group.deleteButton.onClick.AddListener(() =>
                OnNoticePanelOpen(false, uiSlotNumber, group.slotVisualizer.gameStateCopy));

            // Hide delete button by default.
            group.deleteSlotButton.SetActive(false);

            slotNumber++;
        }

        // Hide the canvas by default.
        gameObject.SetActive(false);
    }

    #endregion
    #region Public Methods

    /// <summary>
    /// Shows the Load Game canvas (and the Load Game panel, if disabled).
    /// </summary>
    public void ShowLoadGame()
    {
        gameObject.SetActive(true);
        ShowLoadGamePanel();
    }

    /// <summary>
    /// Hides the Load Game canvas and re-enables the main menu buttons.
    /// </summary>
    public void HideLoadGame()
    {
        gameObject.SetActive(false);
        mainMenuController.SetButtonInteractability(true);
    }

    /// <summary>
    /// Shows the Load Game panel, resets to the first page, and refreshes all slots.
    /// </summary>
    public void ShowLoadGamePanel()
    {
        loadGamePanel.SetActive(true);

        currentPage = 0;
        UpdateNavigation();
        RefreshAllSlots();
    }

    /// <summary>
    /// Hides the Load Game panel.
    /// </summary>
    public void HideLoadGamePanel()
    {
        loadGamePanel.SetActive(false);
    }

    /// <summary>
    /// Refreshes all visible slots for the current page.
    /// </summary>
    public void RefreshAllSlots()
    {
        int saveSlotNumber = (currentPage * 4) + 1;

        foreach (var group in saveSlotGroups)
        {
            // Load the GameState for the current save slot.
            GameState state = SaveManager.Instance.Load(saveSlotNumber);

            // Update the visual data for the save slot based on the loaded state.
            group.slotVisualizer.SetSlotData(saveSlotNumber, state);

            bool hasSave = group.slotVisualizer.hasSave;

            // Enable or disable interaction based on if a save exists in this save slot.
            group.slotButton.interactable = hasSave;
            group.deleteSlotButton.SetActive(hasSave);

            saveSlotNumber++;
        }
    }

    #endregion
    #region Private Methods

    /// <summary>
    /// Toggles between the two pages of save slots (1–4 and 5–8), updates the navigation icon, and refreshes all slots.
    /// </summary>
    private void TogglePage()
    {
        currentPage = 1 - currentPage;
        UpdateNavigation();
        RefreshAllSlots();
    }

    /// <summary>
    /// Updates the navigation button icon based on the current page.
    /// </summary>
    private void UpdateNavigation()
    {
        if (navigationButtonImage) navigationButtonImage.sprite = currentPage == 0 ? downIcon : upIcon;
    }

    #endregion
    #region Event Handlers / Callbacks

    /// <summary>
    /// Opens the confirmation panel for loading or deleting a save file.
    /// </summary>
    /// <param name="isLoadAttempted"><c>true</c> if loading is attempted; <c>false</c> if deleting is attempted.</param>
    /// <param name="uiSlotNumber">The UI slot index.</param>
    /// <param name="state">The associated <see cref="GameState"/> for this save slot.</param>
    private void OnNoticePanelOpen(bool isLoadAttempted, int uiSlotNumber, GameState state)
    {
        HideLoadGamePanel();
        noticePanel.ShowNoticePopup(isLoadAttempted, (currentPage * 4) + uiSlotNumber, state);
    }

    /// <summary>
    /// Called when the confirmation panel is closed. Re-shows the Load Game panel.
    /// </summary>
    public void OnNoticePanelClosed()
    {
        ShowLoadGamePanel();
    }

    #endregion
}
