using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections;

/// <summary>
/// Controls the name prompt popup in the main menu.
/// Handles input validation and invokes a callback when the player confirms their name.
/// </summary>
public class NamePromptCanvas : MonoBehaviour
{
    #region Fields

    /// <summary>
    /// Callback invoked when the player has confirmed their name.
    /// </summary>
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
    /// Called when the script instance is loaded (even if the GameObject is inactive).
    /// Registers the continue button click listener and hides the panel by default.
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
    /// Displays the name prompt panel and focuses the input field for typing.
    /// </summary>
    public void Show()
    {
        if (!namePromptPanel) return;

        namePromptPanel.SetActive(true);
        nameInputField.text = string.Empty;
        nameInputField.Select();
        nameInputField.ActivateInputField();
    }

    /// <summary>
    /// Hides the name prompt panel.
    /// </summary>
    public void Hide()
    {
        if (namePromptPanel) namePromptPanel.SetActive(false);
        if (mainMenuController) mainMenuController.SetButtonInteractability(true);
    }

    #endregion
    #region Coroutines


    /// <summary>
    /// Plays a shake animation on the input field to indicate invalid input.
    /// </summary>
    /// <param name="target">The transform of the input field to shake.</param>
    /// <param name="duration">How long the shake lasts.</param>
    /// <param name="magnitude">How strong the shake effect is.</param>
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
    /// Called when the continue button is clicked.
    /// Validates the input and invokes the callback if a valid name is provided.
    /// </summary>
    private void OnContinueClicked()
    {
        string playerName = nameInputField.text.Trim();

        if (string.IsNullOrEmpty(playerName))
        {
            StartCoroutine(Shake(nameInputField.transform));
            return;
        }

        Hide();
        OnNameChosenCallback?.Invoke(playerName);
    }

    #endregion
}
