

using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NoticePanel : MonoBehaviour
{
    [SerializeField] private LoadGameCanvas loadGameCanvas;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button confirmButton;
    [SerializeField] private TextMeshProUGUI noticeText;
    private bool isLoadAttempted;
    private int saveSlot = 0;

    // runs immediately when the script is loaded (before the first frame) - even if the GameObject is disabled.
        /// <summary>
    /// runs immediately when the script is loaded (before the first frame) - even if the GameObject is disabled - makes sure only a single instance of this object exists.
    /// </summary>
    private void Awake()
    {
        gameObject.SetActive(false);
    }

    // runs only once - the first time the script is enabled and active in the scene.
    private void Start()
    {
        if (cancelButton) cancelButton.onClick.AddListener(HideNoticePopup);
        if (confirmButton) confirmButton.onClick.AddListener(() => ConfirmNotice(isLoadAttempted, saveSlot));
    }

    public void ShowNoticePopup(bool isLoadAttempted, int saveSlot, GameState state)
    {
        if (state == null) return;

        this.isLoadAttempted = isLoadAttempted;
        this.saveSlot = saveSlot;

        // show the panel.
        gameObject.SetActive(true);

        if (isLoadAttempted) noticeText.text = $"Load save slot {saveSlot}\n{state.playerName} {state.lastSavedTime}?";
        else noticeText.text = $"Delete save slot {saveSlot}\n{state.playerName} {state.lastSavedTime}?";
    }

    /*
    public void ShowNoticePopup(bool isLoadAttempted, int saveSlot, string playerName, string timestamp)
    {
        // show the panel.
        gameObject.SetActive(true);

        this.isLoadAttempted = isLoadAttempted;
        this.saveSlot = saveSlot;

        if (isLoadAttempted) noticeText.text = $"Load save slot {saveSlot}\n{playerName} {timestamp}?";
        else noticeText.text = $"Delete save slot {saveSlot}\n{playerName} {timestamp}?";
    }
    */

    public void HideNoticePopup()
    {
        // hide the panel.
        gameObject.SetActive(false);
        if (loadGameCanvas) loadGameCanvas.OnNoticePanelClosed();
    }

    public void ConfirmNotice(bool isLoadAttempted, int saveSlot)
    {
        if (isLoadAttempted)
        {
            GameManager.Instance.LoadGameState(SaveManager.Instance.Load(saveSlot));
            SceneManager.LoadScene("Initialization");
        }
        else
        {
            SaveManager.Instance.Clear(saveSlot);
            HideNoticePopup();
            loadGameCanvas.RefreshAllSlots();
        }
        // TODO DELTE? hide the panel.
        // gameObject.SetActive(false);
    }

}
