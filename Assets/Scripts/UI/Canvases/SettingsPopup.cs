using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsPopup : PopupWindow
{
    [SerializeField] private Button qSaveButton;
    [SerializeField] private Button exitButton;

    private void Awake()
    {
        if (qSaveButton) qSaveButton.onClick.AddListener(QuickSave);
        if (exitButton) exitButton.onClick.AddListener(ReturnToMainMenu);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }

    public override void Open()
    {
        base.Open();
    }

    public override void Close()
    {
        base.Close();
    }

    private void QuickSave()
    {
        GameManager.Instance.SaveGameState(GameManager.Instance.CurrentState.currentSaveSlot);
    }

    private void ReturnToMainMenu()
    {
        GameManager.Instance.ClearGame();
        SceneManager.LoadScene("MainMenu");
    }
}
