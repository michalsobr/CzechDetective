using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button), typeof(Image))]
public class NamePromptButtonVisualizer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Sprites")]
    [SerializeField] private Sprite unpressedSprite;
    [SerializeField] private Sprite pressedSprite;

    [Header("References")]
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private TextMeshProUGUI buttonText;

    private Image image;
    private bool isHovering = false;

    private void Awake()
    {
        image = GetComponent<Image>();

        if (nameInputField)
        {
            nameInputField.onValueChanged.AddListener(OnInputChanged);
        }

        // initial state.
        UpdateButtonVisual();
    }

    private void OnDestroy()
    {
        if (nameInputField)
        {
            nameInputField.onValueChanged.RemoveListener(OnInputChanged);
        }
    }

    // gets called on any imput change.
    private void OnInputChanged(string text)
    {
        UpdateButtonVisual();
    }

    // update the visual (sprite and text location) of the button based on input content and mouse location.
    private void UpdateButtonVisual()
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

        string trimmed = nameInputField.text.Trim();
        if (!string.IsNullOrEmpty(trimmed)) ShowPressedButton();

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;

        string trimmed = nameInputField.text.Trim();
        if (!string.IsNullOrEmpty(trimmed)) ShowUnpressedButton();
    }
}
