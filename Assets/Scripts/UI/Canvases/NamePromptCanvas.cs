using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections;

/// <summary>
/// Manages the name prompt popup in the main menu, handling input validation and invoking a callback when the player confirms their name.
/// </summary>
public class NamePromptCanvas : MonoBehaviour
{
    #region Fields

    /// <summary> Callback invoked when the player confirms their name. </summary>
    public Action<string> OnNameChosenCallback;

    [Header("UI References")]
    [SerializeField] private MainMenuController mainMenuController;
    [SerializeField] private GameObject namePromptPanel;
    [SerializeField] private Button closeButton;
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private Button continueButton;

    #endregion
    #region Unity Lifecycle Methods 

    /// <summary>
    /// Invoked when the script instance is loaded, even if the GameObject is inactive.
    /// Registers button listeners and hides the name prompt panel by default.
    /// </summary>
    private void Awake()
    {
        if (continueButton) continueButton.onClick.AddListener(OnContinueClicked);
        if (closeButton) closeButton.onClick.AddListener(Hide);

        // Hides the panel by default.
        if (namePromptPanel) namePromptPanel.SetActive(false);
    }

    #endregion
    #region Public Methods

    /// <summary>
    /// Displays the name prompt panel, disables main menu button interaction, and focuses the input field for text entry.
    /// </summary>
    public void Show()
    {
        if (!namePromptPanel) return;

        if (mainMenuController) mainMenuController.SetButtonInteractability(false);

        namePromptPanel.SetActive(true);
        nameInputField.text = string.Empty;
        nameInputField.Select();
        nameInputField.ActivateInputField();
    }

    /// <summary>
    /// Hides the name prompt panel and re-enables main menu button interaction.
    /// </summary>
    public void Hide()
    {
        if (namePromptPanel) namePromptPanel.SetActive(false);
        if (mainMenuController) mainMenuController.SetButtonInteractability(true);
    }

    #endregion
    #region Coroutines

    /// <summary>
    /// Plays a horizontal shake animation on the input field to indicate invalid input.
    /// </summary>
    /// <param name="target">The transform of the input field to shake.</param>
    /// <param name="duration">The duration of the shake effect in seconds.</param>
    /// <param name="magnitude">The strength of the shake offset.</param>
    private IEnumerator Shake(Transform target, float duration = 0.4f, float magnitude = 10f)
    {
        Vector3 originalPos = target.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = UnityEngine.Random.Range(-1f, 1f) * magnitude;
            target.localPosition = originalPos + new Vector3(x, 0f, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        target.localPosition = originalPos;
    }

    #endregion
    #region Event Handlers / Callbacks

    /// <summary>
    /// Invoked when the continue button is clicked.
    /// Validates the input and invokes the callback if a valid name is entered.
    /// Plays a shake animation if the input is empty.
    /// </summary>
    private void OnContinueClicked()
    {
        if (string.IsNullOrWhiteSpace(nameInputField.text))
        {
            StartCoroutine(Shake(nameInputField.transform));
            return;
        }

        Hide();
        OnNameChosenCallback?.Invoke(nameInputField.text);
    }

    #endregion
}
