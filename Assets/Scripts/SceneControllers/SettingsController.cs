using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections;

// handles input and validation for name prompt.
public class SettingsController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject namePromptPanel;
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private Button continueButton;

    public Action<string> OnNameChosenCallback;

    private void Start()
    {
        // show the name prompt window.
        if (namePromptPanel)
        {
            namePromptPanel.SetActive(true);
            nameInputField.Select();
            nameInputField.ActivateInputField();
        }

        // listen for continue button clicks.
        if (continueButton)
            continueButton.onClick.AddListener(OnContinueClicked);
    }

    private void OnContinueClicked()
    {
        // get rid off leading or trailing whitespace.
        string playerName = nameInputField.text.Trim();

        // if the chosen name remained empty, trigger the shake animation.
        if (string.IsNullOrEmpty(playerName))
        {
            StartCoroutine(Shake(nameInputField.transform));
            return;
        }

        // call back to BaseController.
        OnNameChosenCallback?.Invoke(playerName);
    }

    // shake animation used on the input field if it remains empty (to signal something is wrong).
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
}
