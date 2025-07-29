using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Handles the hover visual effect for main menu buttons by applying or removing a color gradient on the button text.
/// </summary>
public class MainMenuVisualizer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    #region Fields

    /// <summary> The TextMeshProUGUI component whose color changes on hover. </summary>
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Button button;

    private readonly Color normalColor = Color.white;
    private readonly Color32 topLeft = new Color32(0x00, 0x5B, 0xBF, 255);
    private readonly Color32 topRight = new Color32(0x00, 0x45, 0xA5, 255);
    private readonly Color32 bottomLeft = new Color32(0xFF, 0xFF, 0xFF, 255);
    private readonly Color32 bottomRight = new Color32(0xEF, 0x33, 0x40, 255);

    #endregion
    #region Unity Lifecycle Methods

    /// <summary>
    /// Invoked when the script instance is loaded, even if the GameObject is inactive.
    /// Logs a warning if the TextMeshProUGUI reference is missing.
    /// </summary>
    private void Awake()
    {
        if (text == null) Debug.LogWarning("[MainMenuVisualizer] The reference for the TextMeshProUGUI component is missing.");
    }

    #endregion
    #region Public Methods

    /// <summary>
    /// Enables the gradient hover effect on the button text.
    /// </summary>
    public void EnableGradient()
    {
        text.enableVertexGradient = true;
        text.colorGradient = new VertexGradient(topLeft, topRight, bottomLeft, bottomRight);
    }

    /// <summary>
    /// Disables the gradient hover effect on the button text and restores the default color.
    /// </summary>
    public void DisableGradient()
    {
        text.enableVertexGradient = false;
        text.color = normalColor;
    }

    #endregion
    #region Event Handlers / Callbacks

    /// <summary>
    /// Invoked when the pointer enters the button area.
    /// Enables the gradient hover effect if the button is interactable.
    /// </summary>
    /// <param name="eventData">The pointer event data.</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button && button.interactable) EnableGradient();
    }

    /// <summary>
    /// Invoked when the pointer exits the button area.
    /// Disables the gradient hover effect if the button is interactable.
    /// </summary>
    /// <param name="eventData">The pointer event data.</param>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (button && button.interactable) DisableGradient();
    }

    #endregion
}
