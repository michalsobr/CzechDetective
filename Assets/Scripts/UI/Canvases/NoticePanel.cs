using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Manages the confirmation popup for loading or deleting a save slot, showing the selected slot details and handling confirm or cancel actions.
/// </summary>
public class NoticePanel : MonoBehaviour
{
    #region Fields

    [SerializeField] private LoadGameCanvas loadGameCanvas;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button confirmButton;
    [SerializeField] private TextMeshProUGUI noticeText;

    /// <summary> Whether the current action is a load attempt (<c>true</c>) or a delete attempt (<c>false</c>). </summary>
    private bool isLoadAttempted;

    /// <summary> The index of the save slot currently selected for loading or deleting. </summary>
    private int saveSlotIndex = 0;

    #endregion
    #region Unity Lifecycle Methods

    /// <summary>
    /// Invoked when the script instance is loaded, even if the GameObject is inactive.
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
    /// Displays the notice popup with the details of the selected save slot.
    /// </summary>
    /// <param name="isLoadAttempted">
    /// <c>true</c> if a load attempt is made; 
    /// <c>false</c> if a delete attempt is made.
    /// </param>
    /// <param name="saveSlotNumber">The index of the save slot being acted on.</param>
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
    /// Hides the notice popup and notifies the <see cref="LoadGameCanvas"/> that the panel has closed.
    /// </summary>
    public void HideNoticePopup()
    {
        gameObject.SetActive(false);
        if (loadGameCanvas) loadGameCanvas.OnNoticePanelClosed();
    }

    /// <summary>
    /// Executes the confirmed action (load or delete) for the selected save slot and updates the UI or scene accordingly.
    /// </summary>
    public void ConfirmNotice()
    {
        if (isLoadAttempted)
        {
            GameManager.Instance.LoadGameState(SaveManager.Instance.Load(saveSlotIndex));

            GameManager.Instance.InitializeManagers();

            SceneManager.LoadScene(GameManager.Instance.CurrentState.currentScene);
        }
        else
        {
            SaveManager.Instance.Clear(saveSlotIndex);
            HideNoticePopup();
        }
    }

    #endregion
}
