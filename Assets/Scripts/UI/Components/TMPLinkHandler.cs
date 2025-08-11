using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// Shows a popup when the mouse is over a TMP <link>.
/// Expects link IDs like "w:Key".
/// </summary>
public class TMPLinkHandler : MonoBehaviour, IPointerExitHandler
{
    #region Fields

    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Canvas parentCanvas;

    private int lastLinkIndex = -1;

    #endregion
    #region Unity

    private void Update()
    {
        if (!dialogueText || Mouse.current == null) return;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Camera cam = (parentCanvas && parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : parentCanvas.worldCamera;

        int linkIndex = TMP_TextUtilities.FindIntersectingLink(dialogueText, mousePos, cam);
        if (linkIndex == -1)
        {
            HidePopup();
            return;
        }

        if (linkIndex != lastLinkIndex)
        {
            var info = dialogueText.textInfo.linkInfo[linkIndex];
            string id = info.GetLinkID(); // "w:Key"
            string key = id.StartsWith("w:") ? id.Substring(2) : id;

            if (TranslationManager.Instance != null &&
                TranslationManager.Instance.TryGetPopupLabel(key, out string label) &&
                !string.IsNullOrEmpty(label)) TranslationPopup.Instance?.Show(label, mousePos);
            else TranslationPopup.Instance?.Hide();

            lastLinkIndex = linkIndex;
        }
        else TranslationPopup.Instance?.UpdatePosition(mousePos);
    }

    public void OnPointerExit(PointerEventData eventData) => HidePopup();

    private void HidePopup()
    {
        if (lastLinkIndex != -1)
        {
            lastLinkIndex = -1;
            TranslationPopup.Instance?.Hide();
        }
    }
    
    #endregion
}
