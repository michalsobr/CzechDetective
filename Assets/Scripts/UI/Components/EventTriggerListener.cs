using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// Listens for hover and click on TMP <link> elements inside a TextMeshProUGUI.
/// We only handle quiz answer links here (with IDs like "a:0", etc.).
/// Word translation links ("w:...") are ignored by this script.
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class EventTriggerListener : MonoBehaviour, IPointerExitHandler
{
    #region Fields

    [Header("References")]
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Canvas parentCanvas;

    /// <summary> Index of the currently hovered answer link, -1 if none. </summary>
    private int lastHoveredAnswerIndex = -1;

    #endregion

    #region Unity

    private void Update()
    {
        if (!dialogueText || Mouse.current == null) return;

        Vector2 mousePos = Mouse.current.position.ReadValue();

        // Correct camera, overlay -> null, otherwise canvas.worldCamera (shouldn't be necessary)
        Camera cam = (parentCanvas && parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            ? null
            : parentCanvas ? parentCanvas.worldCamera : null;

        // Find which link (if any) the mouse is over.
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(dialogueText, mousePos, cam);

        if (linkIndex == -1)
        {
            // Not over any link -> clear previous hover if we had one.
            UnhighlightCurrent();
            return;
        }

        // We have a link under the mouse.
        var linkInfo = dialogueText.textInfo.linkInfo[linkIndex];
        string linkId = linkInfo.GetLinkID();

        // Only react to quiz answers ("a:"). Ignore word links ("w:...") here.
        if (!linkId.StartsWith("a:"))
        {
            UnhighlightCurrent();
            return;
        }

        // If we just moved to a different answer link.
        if (linkIndex != lastHoveredAnswerIndex)
        {
            // Unhighlight the previous answer, if any.
            UnhighlightCurrent();

            // Highlight the new one.
            QuizManager.Instance?.UpdateAnswerHighlight(linkId, true);
            lastHoveredAnswerIndex = linkIndex;
        }
        else // Same link as last frame -> do nothing (but clicks below are still allowed)

        // Handle click on this answer
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            QuizManager.Instance?.OnAnswerClicked(linkId);
            // After a click, the dialogue changes -> clear local hover state
            lastHoveredAnswerIndex = -1;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Mouse left the text area.
        UnhighlightCurrent();
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Removes hover highlight from the currently hovered answer (if any).
    /// </summary>
    private void UnhighlightCurrent()
    {
        if (lastHoveredAnswerIndex == -1 || dialogueText == null) return;

        if (lastHoveredAnswerIndex >= 0 && lastHoveredAnswerIndex < dialogueText.textInfo.linkInfo.Length)
        {
            var prevLink = dialogueText.textInfo.linkInfo[lastHoveredAnswerIndex];
            string prevId = prevLink.GetLinkID();

            if (prevId.StartsWith("a:"))
            {
                QuizManager.Instance?.UpdateAnswerHighlight(prevId, false);
            }
        }

        lastHoveredAnswerIndex = -1;
    }

    #endregion
}
