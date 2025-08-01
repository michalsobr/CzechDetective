using UnityEngine;
using TMPro;


/// <summary>
/// Handles displaying a translation popup when hovering over linked words in dialogue text.
/// Implements a singleton pattern.
/// </summary>
public class TranslationPopup : MonoBehaviour
{
    #region Fields

    // Singleton instance.
    public static TranslationPopup Instance;

    [SerializeField] private RectTransform translationRT;
    [SerializeField] private TextMeshProUGUI translationText;

    #endregion
    #region Unity Lifecycle Methods

    /// <summary>
    /// Invoked when the script instance is loaded, even if the GameObject is inactive.
    /// Implements a singleton pattern and hides the popup by default.
    /// </summary>
    private void Awake()
    {
        // If another instance already exists, destroy this one.
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // This object persists across scenes as part of the Dialogue Manager.

        // Hide popup by default.
        gameObject.SetActive(false);
    }

    #endregion
    #region Public Methods

    /// <summary>
    /// Displays the translation popup near the given screen position with the translated word.
    /// Dynamically resizes the popup based on the translated text length.
    /// </summary>
    /// <param name="word">The word to translate and display.</param>
    /// <param name="screenPos">The screen position where the popup should appear.</param>
    public void Show(string word, Vector2 screenPos)
    {
        // Make the popup visible.
        gameObject.SetActive(true);

        // Update the text content first so its size can be measured.
        translationText.text = GetTranslation(word);
        translationText.ForceMeshUpdate();

        // Determine the preferred size of the popup based on text width/height.
        Vector2 preferredSize = new Vector2(
            translationText.preferredWidth + (translationText.preferredWidth / 5),
            2 * translationText.preferredHeight
        );

        translationRT.sizeDelta = preferredSize;

        UpdatePosition(screenPos);
    }

    /// <summary>
    /// Hides the translation popup.
    /// </summary>
    public void Hide() => gameObject.SetActive(false);

    /// <summary>
    /// Updates the position of the translation popup relative to the given screen position.
    /// Does not modify the popup text or size – only repositions it to follow the cursor.
    /// </summary>
    /// <param name="screenPos">The current screen position of the mouse cursor.</param>
    public void UpdatePosition(Vector2 screenPos)
    {
        // Convert the screen position to a local position relative to the popup's parent (canvas).
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            translationRT.parent as RectTransform,
            screenPos,
            null,
            out Vector2 localPos
        );

        // Slightly offset the popup upwards so it does not overlap with the cursor.
        localPos.y += 15f;

        // Slightly offset the popup upwards so it does not overlap with the cursor.
        translationRT.localPosition = localPos;
    }

    #endregion
    #region Private Methods

    /// <summary>
    /// Retrieves a translated string for the given word.
    /// </summary>
    /// <param name="word">The word to translate.</param>
    /// <returns>The translated word, if available; otherwise, a placeholder text.</returns>
    private string GetTranslation(string id)
    {
        // Load full JSON once in TranslationManager
        return TranslationManager.Instance.GetTranslationById(id);
    }

    #endregion
}
