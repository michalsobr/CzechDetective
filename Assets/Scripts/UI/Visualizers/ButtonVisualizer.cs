using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// Handles the visual state of a UI button, including its sprite and text margin, based on hover state, forced pressed state, and (optionally) the content of a linked input field.
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

    /// <summary> The original bottom margin of the button text. </summary>
    private float defaultMargin;

    /// <summary> Whether the pointer is currently hovering over the button. </summary>
    private bool isHovering = false;

    /// <summary> Whether the button should remain in a forced pressed state. </summary>
    private bool forcePressed = false;

    #endregion
    #region Unity Lifecycle Methods

    /// <summary>
    /// Invoked when the script instance is loaded, even if the GameObject is inactive.
    /// Registers the input field listener, if assigned, and stores the default text margin.
    /// </summary>
    private void Awake()
    {
        if (nameInputField) nameInputField.onValueChanged.AddListener(OnInputChanged);
        if (buttonText) defaultMargin = buttonText.margin.w;
    }

    /// <summary>
    /// Invoked when the GameObject becomes enabled. 
    /// Initializes the button state depending on whether an input field is assigned.
    /// </summary>
    private void OnEnable()
    {
        if (nameInputField) ShowPressedButton();
        else ShowUnpressedButton();
    }

    /// <summary>
    /// Invoked when the GameObject becomes disabled.
    /// Resets the button to its unpressed state.
    /// </summary>
    private void OnDisable() => ShowUnpressedButton();


    /// <summary>
    /// Invoked when the object is destroyed.
    /// Unregisters the input field listener, if assigned.
    /// </summary>
    private void OnDestroy()
    {
        if (nameInputField) nameInputField.onValueChanged.RemoveListener(OnInputChanged);
    }

    #endregion
    #region Public Methods

    /// <summary>
    /// Sets the button to its unpressed state (sprite + default margin).
    /// </summary>
    public void ShowUnpressedButton()
    {
        if (!buttonImage || !buttonText) return;

        buttonImage.sprite = unpressedSprite;

        buttonText.margin = new Vector4(0f, 0f, 0f, defaultMargin);
    }

    /// <summary>
    /// Sets the button to its pressed state (sprite + margin shifted).
    /// </summary>
    public void ShowPressedButton()
    {
        if (!buttonImage || !buttonText) return;

        buttonImage.sprite = pressedSprite;

        buttonText.margin = new Vector4(0f, defaultMargin, 0f, 0f);
    }

    /// <summary>
    /// Forces the button to remain in the pressed state or revert to its normal hover-based behavior.
    /// </summary>
    /// <param name="pressed">
    /// <c>true</c> to lock the button in the pressed state; 
    /// <c>false</c> to restore normal hover-based behavior.
    /// </param>
    public void ForcePressed(bool pressed)
    {
        forcePressed = pressed;

        if (pressed || isHovering) ShowPressedButton();
        else ShowUnpressedButton();
    }

    #endregion
    #region Private Methods

    /// <summary>
    /// Updates the button appearance for the name prompt popup.
    /// Pressed when the input field is empty or the button is hovered.
    /// </summary>
    private void UpdateNamePromptButtonVisual()
    {
        if (string.IsNullOrWhiteSpace(nameInputField.text) || isHovering) ShowPressedButton();
        else ShowUnpressedButton();
    }

    #endregion
    #region Event Handlers / Callbacks

    /// <summary>
    /// Invoked when the pointer enters the button area.
    /// Sets hover state and updates the appearance unless forcePressed is enabled.
    /// </summary>
    /// <param name="eventData">The pointer event data.</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (forcePressed) return;

        isHovering = true;

        if (nameInputField) UpdateNamePromptButtonVisual();
        else ShowPressedButton();
    }

    /// <summary>
    /// Invoked when the pointer exits the button area.
    /// Clears hover state and updates the appearance unless forcePressed is enabled.
    /// </summary>
    /// <param name="eventData">The pointer event data.</param>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (forcePressed) return;

        isHovering = false;

        if (nameInputField) UpdateNamePromptButtonVisual();
        else ShowUnpressedButton();
    }

    /// <summary>
    /// Invoked when the linked input field value changes.
    /// Updates the button appearance for the name prompt popup.
    /// </summary>
    /// <param name="text">The current text in the input field.</param>
    private void OnInputChanged(string text) => UpdateNamePromptButtonVisual();

    #endregion
}
