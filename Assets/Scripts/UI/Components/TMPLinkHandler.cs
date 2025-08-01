using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// Detects when the mouse hovers over or exits a TMP link in dialogue text.
/// Displays a translation popup for the hovered word using <see cref="TranslationPopup"/>.
/// </summary>
public class TMPLinkHandler : MonoBehaviour, IPointerExitHandler
{
    #region Fields

    [SerializeField] private TextMeshProUGUI dialogueText;

    /// <summary> The index of the last hovered link. Used to track when the hovered link changes. </summary>
    private int lastLinkIndex = -1;

    #endregion
    #region Unity Lifecycle Methods

    /// <summary>
    /// Called every frame.
    /// Detects whether the mouse is currently hovering over a TMP link in the dialogue text.
    /// If a new link is hovered, displays the translation popup with the translated word at the cursor position.
    /// If hovering the same link, only updates the popup position.
    /// If no link is hovered, hides the popup.
    /// </summary>
    private void Update()
    {
        // Get the current mouse position from the Input System.
        Vector2 mousePos = Mouse.current.position.ReadValue();

        // Determine the index of the link under the mouse, or -1 if none.
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(dialogueText, mousePos, null);

        if (linkIndex != -1) // If mouse is over a link.
        {
            TMP_LinkInfo linkInfo = dialogueText.textInfo.linkInfo[linkIndex];
            string hoveredWord = linkInfo.GetLinkID();

            // Skip translation for quiz answers.
            if (hoveredWord.StartsWith("Answer_")) return; // Don't show translation popup.

            if (linkIndex != lastLinkIndex)
            {
                // The hovered link has changed, update popup text and position.
                TranslationPopup.Instance.Show(hoveredWord, mousePos);
            }
            else
            {
                // Still hovering the same link, only update popup position.
                TranslationPopup.Instance.UpdatePosition(mousePos);
            }

            // Store the current hovered link index
            lastLinkIndex = linkIndex;
        }
        else if (lastLinkIndex != -1) // If mouse left the link
        {
            // Reset state and hide the popup.
            lastLinkIndex = -1;
            TranslationPopup.Instance.Hide();
        }
    }

    #endregion
    #region Event Handlers / Callbacks

    /// <summary>
    /// Invoked when the pointer exits the text area.
    /// Hides the translation popup and resets the hovered link index.
    /// </summary>
    /// <param name="eventData">The pointer event data.</param>
    public void OnPointerExit(PointerEventData eventData)
    {
        TranslationPopup.Instance.Hide();
        lastLinkIndex = -1;
    }

    #endregion
}
