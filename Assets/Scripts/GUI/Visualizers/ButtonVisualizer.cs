using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// Controls the visual appearance of a UI button based on hover state and, if assigned, the content of a name input field. Updates the button sprite and text position to reflect its current state.
/// </summary>
public class ButtonVisualizer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    #region Fields

    [Header("Sprites")]
    [SerializeField] private Sprite unpressedSprite;
    [SerializeField] private Sprite pressedSprite;

    [Header("UI References")]
    [SerializeField] private Image buttonImage;
    [SerializeField] private TextMeshProUGUI buttonText;

    [Header("*Only for Name Prompt Popup")]
    [SerializeField] private TMP_InputField nameInputField;

    private bool isHovering = false;

    #endregion
    #region Unity Lifecycle Methods

    /// <summary>
    /// Called when the script instance is loaded (even if the GameObject is inactive).
    /// Registers listeners for the input field, if assigned, and updates the button's default visual state.
    /// </summary>
    private void Awake()
    {
        // Register a listener if the input field is assigned.
        // Apply the default visual state for the button.
        if (nameInputField)
        {
            nameInputField.onValueChanged.AddListener(OnInputChanged);

            ShowPressedButton();
        }
        else ShowUnpressedButton();
    }

    /// <summary>
    /// Called each time the object becomes disabled. 
    /// Resets the button to its unpressed state.
    /// </summary>
    private void OnDisable()
    {
        ShowUnpressedButton();
    }

    /// <summary>
    /// Called when the object is destroyed. 
    /// Removes the input field listener, if assigned.
    /// </summary>
    private void OnDestroy()
    {
        if (nameInputField) nameInputField.onValueChanged.RemoveListener(OnInputChanged);
    }

    #endregion
    #region Private Methods

    /// <summary>
    /// Sets the button sprite to the unpressed state and resets the button text position.
    /// </summary>
    private void ShowUnpressedButton()
    {
        buttonImage.sprite = unpressedSprite;
        buttonText.rectTransform.anchoredPosition = new Vector2(0f, 0f);
    }

    /// <summary>
    /// Sets the button sprite to the pressed state and shifts the button text downward.
    /// </summary>
    private void ShowPressedButton()
    {
        buttonImage.sprite = pressedSprite;
        buttonText.rectTransform.anchoredPosition = new Vector2(0f, -24f);
    }

    /// <summary>
    /// Updates the button's appearance based on the input field content and hover state.
    /// </summary>
    private void UpdateNamePromptButtonVisual()
    {
        if (string.IsNullOrEmpty(nameInputField.text.Trim()) || isHovering) ShowPressedButton();
        else ShowUnpressedButton();
    }

    #endregion
    #region Event Handlers

    /// <summary>
    /// Called when the pointer enters the button area. Marks the button as hovered and updates its visual state.
    /// </summary>
    /// <param name="eventData">The pointer event data.</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        if (nameInputField) UpdateNamePromptButtonVisual();
        else ShowPressedButton();
    }

    /// <summary>
    /// Called when the pointer exits the button area. Marks the button as no longer hovered and updates its visual state.
    /// </summary>
    /// <param name="eventData">The pointer event data.</param>
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        if (nameInputField) UpdateNamePromptButtonVisual();
        else ShowUnpressedButton();
    }

    /// <summary>
    /// Called whenever the linked input field value changes. Updates the button's visual state accordingly.
    /// </summary>
    /// <param name="text">The current text in the input field.</param>
    private void OnInputChanged(string text)
    {
        UpdateNamePromptButtonVisual();
    }

    #endregion
}
