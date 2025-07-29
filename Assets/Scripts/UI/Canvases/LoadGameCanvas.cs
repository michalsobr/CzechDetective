using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the Load Game popup in the main menu, handling save slot display, page navigation, and confirmation panel for loading or deleting saves.
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

    /// <summary> The currently active page index (0 = save slots 1–4, 1 = save slots 5–8). </summary>
    private int currentPage = 0;

    #endregion
    #region Unity Lifecycle Methods

    /// <summary>
    /// Invoked when the script instance is loaded, even if the GameObject is inactive.
    /// Sets up button listeners, initializes save slot groups, and hides the canvas by default.
    /// Logs a warning and aborts setup if any required references are missing.
    /// </summary>
    private void Awake()
    {
        if (closeButton) closeButton.onClick.AddListener(HideCanvas);
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

        // Hide the popup by default.
        gameObject.SetActive(false);
    }

    #endregion
    #region Public Methods

    /// <summary>
    /// Displays the Load Game popup and opens the Load Game panel.
    /// </summary>
    public void ShowCanvas()
    {
        gameObject.SetActive(true);
        Show();
    }

    /// <summary>
    /// Hides the Load Game popup and re-enables main menu button interaction.
    /// </summary>
    public void HideCanvas()
    {
        gameObject.SetActive(false);
        if (mainMenuController) mainMenuController.SetButtonInteractability(true);
    }

    /// <summary>
    /// Displays the Load Game panel, resets to the first page, and refreshes all slot visuals.
    /// </summary>
    public void Show()
    {
        loadGamePanel.SetActive(true);

        currentPage = 0;
        UpdateNavigation();
        RefreshAllSlots();
    }

    /// <summary>
    /// Hides the Load Game panel without disabling the entire canvas.
    /// </summary>
    public void Hide()
    {
        loadGamePanel.SetActive(false);
    }

    /// <summary>
    /// Updates all visible save slots on the current page with the latest game state data.
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
    /// Switches between the two pages of save slots (save slots 1–4 and 5–8), updates the navigation icon, and refreshes all slot visuals.
    /// </summary>
    private void TogglePage()
    {
        currentPage = 1 - currentPage;
        UpdateNavigation();
        RefreshAllSlots();
    }

    /// <summary>
    /// Updates the navigation button icon to reflect the current page.
    /// </summary>
    private void UpdateNavigation()
    {
        if (navigationButtonImage) navigationButtonImage.sprite = currentPage == 0 ? downIcon : upIcon;
    }

    #endregion
    #region Event Handlers / Callbacks

    /// <summary>
    /// Opens the confirmation panel for loading or deleting a save file, and hides the Load Game panel while the confirmation panel is open.
    /// </summary>
    /// <param name="isLoadAttempted">
    /// <c>true</c> if a load attempt is made; 
    /// <c>false</c> if a delete attempt is made.
    /// </param>
    /// <param name="uiSlotNumber">The index of the UI save slot.</param>
    /// <param name="state">The <see cref="GameState"/> associated with the selected save slot.</param>
    private void OnNoticePanelOpen(bool isLoadAttempted, int uiSlotNumber, GameState state)
    {
        Hide();
        noticePanel.ShowNoticePopup(isLoadAttempted, (currentPage * 4) + uiSlotNumber, state);
    }

    /// <summary>
    /// Invoked when the confirmation panel is closed.
    /// Reopens the Load Game panel.
    /// </summary>
    public void OnNoticePanelClosed()
    {
        Show();
    }

    #endregion
}
