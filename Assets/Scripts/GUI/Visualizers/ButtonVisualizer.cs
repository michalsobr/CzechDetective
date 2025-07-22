using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button), typeof(Image))]
public class ButtonVisualizer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Sprites")]
    [SerializeField] private Sprite unpressedSprite;
    [SerializeField] private Sprite pressedSprite;

    [Header("References")]
    [SerializeField] private TextMeshProUGUI buttonText;

    [Header("Only assign if Continue")]
    [SerializeField] private TMP_InputField nameInputField;

    private Image image;
    private bool isHovering = false;

    // runs immediately when the script is loaded (before the first frame) - even if the GameObject is disabled.
        /// <summary>
    /// runs immediately when the script is loaded (before the first frame) - even if the GameObject is disabled - makes sure only a single instance of this object exists.
    /// </summary>
    private void Awake()
    {
        image = GetComponent<Image>();

        if (nameInputField)
        {
            nameInputField.onValueChanged.AddListener(OnInputChanged);
            // initial state.
            UpdateContinueButtonVisual();
            return;
        }
        UpdateGeneralButtonVisual();
    }

    private void OnDisable()
    {
        ShowUnpressedButton();
    }

    private void OnDestroy()
    {
        if (nameInputField)
        {
            nameInputField.onValueChanged.RemoveListener(OnInputChanged);
        }
    }

    // gets called on any input change.
    private void OnInputChanged(string text)
    {
        UpdateContinueButtonVisual();
    }

    // update the visual (sprite and text location) of the button based on the input content and mouse location.
    private void UpdateContinueButtonVisual()
    {
        string trimmed = nameInputField.text.Trim();

        if (!string.IsNullOrEmpty(trimmed))
        {
            if (isHovering)
            {
                ShowPressedButton();
            }
            else
            {
                ShowUnpressedButton();
            }
        }
        else
        {
            ShowPressedButton();
        }
    }

    // update the visual (sprite and text location) of the button based on the mouse location.
    private void UpdateGeneralButtonVisual()
    {
        if (isHovering)
        {
            ShowPressedButton();
        }
        else
        {
            ShowUnpressedButton();
        }
    }

    private void ShowPressedButton()
    {
        image.sprite = pressedSprite;
        buttonText.rectTransform.anchoredPosition = new Vector2(0f, -24f);
    }

    private void ShowUnpressedButton()
    {
        image.sprite = unpressedSprite;
        buttonText.rectTransform.anchoredPosition = new Vector2(0f, 0f);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;

        if (nameInputField) UpdateContinueButtonVisual();
        else UpdateGeneralButtonVisual();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;

        if (nameInputField) UpdateContinueButtonVisual();
        else UpdateGeneralButtonVisual();
    }
}
