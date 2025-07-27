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

    [Header("Main Menu Buttons")]
    [SerializeField] private MainMenuController startText;
    [SerializeField] private MainMenuController loadText;
    [SerializeField] private MainMenuController exitText;

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
    /// The currently active page index (0 = slots 1–4, 1 = slots 5–8).
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
            int saveSlotNumber = slotNumber;

            group.slotButton.onClick.AddListener(() =>
                OnNoticePanelOpen(true, saveSlotNumber, group.slotVisualizer.gameStateCopy));
            group.deleteButton.onClick.AddListener(() =>
                OnNoticePanelOpen(false, saveSlotNumber, group.slotVisualizer.gameStateCopy));

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
    /// Shows the Load Game canvas (and the Load Game panel, if disabled) and disables the main menu buttons.
    /// </summary>
    public void ShowLoadGame()
    {
        SetMainMenuInteractable(false);

        gameObject.SetActive(true);
        ShowLoadGamePanel();
    }

    /// <summary>
    /// Hides the Load Game canvas and re-enables the main menu buttons.
    /// </summary>
    public void HideLoadGame()
    {
        gameObject.SetActive(false);

        SetMainMenuInteractable(true);
    }

    /// <summary>
    /// Shows the Load Game panel, resets to the first page, and refreshes all save slots.
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
    /// Refreshes all visible save slots for the current page.
    /// </summary>
    public void RefreshAllSlots()
    {
        int slotNumber = (currentPage * 4) + 1;

        foreach (var group in saveSlotGroups)
        {
            // Load the GameState for the current slot.
            GameState state = SaveManager.Instance.Load(slotNumber);

            // Update the visual data for the slot based on the loaded state.
            group.slotVisualizer.SetSlotData(slotNumber, state);

            bool hasSave = group.slotVisualizer.hasSave;

            // Enable or disable interaction based on if a save exists in this slot.
            group.slotButton.interactable = hasSave;
            group.deleteSlotButton.SetActive(hasSave);

            slotNumber++;
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

    /// <summary>
    /// Sets the interactability of the main menu buttons.
    /// </summary>
    /// <param name="state"><c>true</c> to enable buttons; <c>false</c> to disable them.</param>
    private void SetMainMenuInteractable(bool state)
    {
        if (startText) startText.interactable = state;
        if (loadText) loadText.interactable = state;
        if (exitText) exitText.interactable = state;
    }

    #endregion
    #region Event Handlers / Callbacks

    /// <summary>
    /// Opens the confirmation panel for loading or deleting a save file.
    /// </summary>
    /// <param name="isLoadAttempted"><c>true</c> if loading is attempted; <c>false</c> if deleting is attempted.</param>
    /// <param name="saveSlot">The save slot index.</param>
    /// <param name="state">The associated <see cref="GameState"/> for this slot.</param>
    private void OnNoticePanelOpen(bool isLoadAttempted, int saveSlot, GameState state)
    {
        HideLoadGamePanel();
        noticePanel.ShowNoticePopup(isLoadAttempted, (currentPage * 4) + saveSlot, state);
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
