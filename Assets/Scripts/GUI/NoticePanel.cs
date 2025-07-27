using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Controls the confirmation popup for loading or deleting a save slot.
/// Displays the selected save slot details and handles the confirming or canceling actions.
/// </summary>
public class NoticePanel : MonoBehaviour
{
    #region Fields

    [SerializeField] private LoadGameCanvas loadGameCanvas;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button confirmButton;
    [SerializeField] private TextMeshProUGUI noticeText;

    /// <summary>
    /// Indicates whether the current action is a load attempt (<c>true</c>) or delete attempt (<c>false</c>).
    /// </summary>
    private bool isLoadAttempted;
    
    /// <summary>
    /// The save slot index currently selected for loading or deleting.
    /// </summary>
    private int saveSlotIndex = 0;

    #endregion
    #region Unity Lifecycle Methods

    /// <summary>
    /// Called when the script instance is loaded (even if the GameObject is inactive).
    /// Registers button listeners and hides the notice panel by default.
    /// </summary>
    private void Awake()
    {
        if (cancelButton) cancelButton.onClick.AddListener(HideNoticePopup);
        if (confirmButton) confirmButton.onClick.AddListener(ConfirmNotice);

        // Hide the panel by default.
        gameObject.SetActive(false);
    }

    #endregion
    #region Public Methods

    /// <summary>
    /// Displays the notice popup with details of the selected save slot.
    /// </summary>
    /// <param name="isLoadAttempted"><c>true</c> if loading is attempted; <c>false</c> if deleting is attempted.</param>
    /// <param name="saveSlotNumber">The save slot index to display.</param>
    /// <param name="state">The <see cref="GameState"/> associated with the selected save slot.</param>
    public void ShowNoticePopup(bool isLoadAttempted, int saveSlotNumber, GameState state)
    {
        if (state == null) return;

        this.isLoadAttempted = isLoadAttempted;
        this.saveSlotIndex = saveSlotNumber;

        gameObject.SetActive(true);

        string text = isLoadAttempted ? "Load save slot " : "Delete save slot ";

        if (noticeText) noticeText.text = $"{text}{saveSlotNumber}\n{state.playerName} {state.lastSavedTime}?";
    }

    /// <summary>
    /// Hides the notice popup and notifies the <see cref="LoadGameCanvas"/> that it has closed.
    /// </summary>
    public void HideNoticePopup()
    {
        gameObject.SetActive(false);
        if (loadGameCanvas) loadGameCanvas.OnNoticePanelClosed();
    }

    /// <summary>
    /// Executes the confirmed action (load or delete) for the selected save slot.
    /// </summary>
    public void ConfirmNotice()
    {
        if (isLoadAttempted)
        {
            GameManager.Instance.LoadGameState(SaveManager.Instance.Load(saveSlotIndex));
            SceneManager.LoadScene("Initialization");
        }
        else
        {
            SaveManager.Instance.Clear(saveSlotIndex);
            HideNoticePopup();
            loadGameCanvas.RefreshAllSlots();
        }
    }

    #endregion
}
