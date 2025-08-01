using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// Detects mouse hover and click events on TMP links inside a TextMeshProUGUI component.
/// Used to make quiz answers (or other links) clickable and hoverable.
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class EventTriggerListener : MonoBehaviour, IPointerExitHandler
{
    #region Fields

    private TextMeshProUGUI tmpText;

    /// <summary> The index of the currently hovered link. </summary>
    private int lastLinkIndex = -1;

    #endregion
    #region Unity Lifecycle Methods

    private void Awake()
    {
        tmpText = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();

        // Check if the mouse is over a TMP link
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(tmpText, mousePos, null);

        if (linkIndex != -1)
        {
            TMP_LinkInfo linkInfo = tmpText.textInfo.linkInfo[linkIndex];
            string linkId = linkInfo.GetLinkID();

            // Only handle links that are quiz answers
            if (linkId.StartsWith("Answer_"))
            {
                // Hover started or moved to a new link
                if (linkIndex != lastLinkIndex)
                {
                    lastLinkIndex = linkIndex;
                    HighlightLink(linkInfo.GetLinkID(), true); // Apply hover color
                }

                // Detect click on the link
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    QuizManager.Instance.OnAnswerClicked(linkInfo.GetLinkID());
                }
            }
        }
        else if (lastLinkIndex != -1)
        {
            // Mouse left the link area
            TMP_LinkInfo linkInfo = tmpText.textInfo.linkInfo[lastLinkIndex];
            HighlightLink(linkInfo.GetLinkID(), false); // Remove hover color
            lastLinkIndex = -1;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (lastLinkIndex != -1)
        {
            TMP_LinkInfo linkInfo = tmpText.textInfo.linkInfo[lastLinkIndex];
            string linkId = linkInfo.GetLinkID();

            // Only reset highlight for quiz answers.
            if (linkId.StartsWith("Answer_"))
            {
                QuizManager.Instance.UpdateAnswerHighlight(linkId, false);
            }

            lastLinkIndex = -1;
        }
    }

    #endregion
    #region Private Methods

    /// <summary>
    /// Changes the color of a specific link ID when hovered or unhovered.
    /// </summary>
    /// <param name="linkId">The ID of the link (quiz answer index).</param>
    /// <param name="hover">Whether the link is being hovered.</param>
    private void HighlightLink(string linkId, bool hover)
    {
        // Rebuild the text with updated colors
        QuizManager.Instance.UpdateAnswerHighlight(linkId, hover);
    }

    #endregion
}
