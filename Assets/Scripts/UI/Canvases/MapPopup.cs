using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Map popup with a dropdown of unlocked locations (excluding the current scene).
/// Loads the selected scene when the user confirms.
/// </summary>
public class MapPopup : PopupWindow
{
    #region Fields

    [Header("UI")]
    [SerializeField] private TMP_Dropdown dropdown;
    [SerializeField] private Button changeLocationButton;

    private string currentSceneName;

    #endregion

    protected override void OnEnable()
    {
        base.OnEnable();

        currentSceneName = SceneManager.GetActiveScene().name;
        if (changeLocationButton) changeLocationButton.onClick.AddListener(OnConfirm);
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        currentSceneName = null;
        if (changeLocationButton) changeLocationButton.onClick.RemoveListener(OnConfirm);
    }

    public override void Open()
    {
        base.Open();

        RebuildDropdown();
    }

    public override void Close()
    {
        base.Close();
    }

    private void RebuildDropdown()
    {
        if (!dropdown) return;

        dropdown.options.Clear();

        // TODO remove, only for testing
        // dropdown.options.Add(new TMP_Dropdown.OptionData("Test Location"));

        if (currentSceneName != "Base")
            dropdown.options.Add(new TMP_Dropdown.OptionData("Tichovice Town Square"));
        if (currentSceneName != "VillaOutside" && GameManager.Instance.CurrentState.completedDialogues.Contains("base.letter.thirteen"))
            dropdown.options.Add(new TMP_Dropdown.OptionData("Novák's Villa"));
        // TODO placeholder condition - replace
        if (currentSceneName != "VillaDiningRoom" && GameManager.Instance.CurrentState.completedDialogues.Contains("villaoutside.teta.fib_correct2"))
            dropdown.options.Add(new TMP_Dropdown.OptionData("Novák's Villa - Dining Room"));

        // Reset dropdown to have selected the first option by default.
        dropdown.value = 0;
        dropdown.RefreshShownValue();
    }

    private void OnConfirm()
    {
        if (!dropdown) return;

        string sceneToLoad = null;

        if (dropdown.options[dropdown.value].text == "Tichovice Town Square") sceneToLoad = "Base";
        else if (dropdown.options[dropdown.value].text == "Novák's Villa") sceneToLoad = "VillaOutside";
        else if (dropdown.options[dropdown.value].text == "Novák's Villa - Dining Room") sceneToLoad = "VillaDiningRoom";

        Close();

        if (!string.IsNullOrEmpty(sceneToLoad)) SceneManager.LoadScene(sceneToLoad);
    }

}
