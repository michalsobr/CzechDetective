using UnityEngine;
using TMPro;

/// <summary>
/// Small tooltip shown near the cursor.
/// </summary>
public class TranslationPopup : MonoBehaviour
{
    #region Fields

    public static TranslationPopup Instance;

    [SerializeField] private RectTransform panel;
    [SerializeField] private TextMeshProUGUI label;

    private const float WidthPad = 0.25f;  // 25% width padding
    private const float HeightFactor = 2f; // two lines tall
    private const float OffsetY = 15f;     // lift above cursor
    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        gameObject.SetActive(false);
    }

    #endregion
    #region Public Methods

    public void Show(string text, Vector2 screenPos)
    {
        if (!panel || !label || string.IsNullOrEmpty(text)) return;

        gameObject.SetActive(true);

        label.text = text;
        label.ForceMeshUpdate();

        Vector2 size = new(label.preferredWidth * (1f + WidthPad),
                           label.preferredHeight * HeightFactor);

        panel.sizeDelta = size;
        UpdatePosition(screenPos);
    }

    public void Hide()
    {
        if (gameObject.activeSelf) gameObject.SetActive(false);
    }

    public void UpdatePosition(Vector2 screenPos)
    {
        if (!panel || !panel.parent) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            panel.parent as RectTransform, screenPos, null, out var local);

        local.y += OffsetY;
        panel.localPosition = local;
    }
    
    #endregion
}
