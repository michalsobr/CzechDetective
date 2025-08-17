using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections.Generic;

/// <summary>
/// Represents an interactable UI image that responds to clicks only on its visible (non-transparent) pixels.
/// </summary>
[RequireComponent(typeof(Image))]
public class InteractableImage : MonoBehaviour, IPointerClickHandler
{
    [Header("UI References")]
    [SerializeField] private Image interactableImage;

    [Header("Interactable Settings")]
    public string interactableID;
    public string dialogueID;
    public string buttonText;
    public List<string> unlockedWords;

    [Header("Alpha Detection")]
    [SerializeField] private Texture2D sourceTexture;
    [SerializeField, Range(0f, 1f)] private float alphaThreshold = 0.1f;

    private void Awake()
    {
        interactableImage.alphaHitTestMinimumThreshold = 0.1f;
    }

    /// <summary>
    /// Called by InteractableManager to verify if the click was on a visible area.
    /// </summary>
    public bool WasClickedOn(PointerEventData eventData)
    {
        return IsClickOnOpaquePixel(eventData);
    }

    /// <summary>
    /// Unity UI click handler — only triggers if click lands on visible pixels.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"Interactable {interactableID} clicked.");

        if (!IsClickOnOpaquePixel(eventData)) return;

        InteractableManager.Instance.ShowButton(this);
    }

    /// <summary>
    /// Checks whether the clicked pixel on the image is sufficiently opaque.
    /// Prevents clicks on transparent areas from triggering the interaction.
    /// </summary>
    private bool IsClickOnOpaquePixel(PointerEventData eventData)
    {
        // If no source texture is assigned, assume it's not clickable.
        if (sourceTexture == null) return false;

        Vector2 localCursor;

        // Convert the screen click position to local coordinates within the image's RectTransform.
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            interactableImage.rectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out localCursor);

        // Get the image's pixel rect to calculate normalized (0–1) position inside it.
        Rect rect = interactableImage.rectTransform.rect;

        // Normalize local click position (0 to 1 range across width/height).
        float normX = (localCursor.x - rect.x) / rect.width;
        float normY = (localCursor.y - rect.y) / rect.height;

        // Convert normalized coordinates to pixel coordinates in the texture.
        int texX = Mathf.FloorToInt(normX * sourceTexture.width);
        int texY = Mathf.FloorToInt(normY * sourceTexture.height);

        // Ensure the pixel coordinates are within the texture bounds.
        if (texX < 0 || texY < 0 || texX >= sourceTexture.width || texY >= sourceTexture.height)
            return false;

        // Read the pixel color at the calculated position.
        Color pixel = sourceTexture.GetPixel(texX, texY);

        // Return true if the pixel's alpha is above the defined threshold.
        return pixel.a > alphaThreshold;
    }

    /// <summary>
    /// Called when the interaction button is pressed.
    /// Triggers the dialogue and hides the button.
    /// </summary>
    public void TriggerInteraction()
    {
        InteractableManager.Instance.HideButton();

        if (unlockedWords != null && unlockedWords.Count > 0)
            TranslationManager.Instance.UnlockWords(unlockedWords.ToArray());

        DialogueManager.Instance.ShowDialogue(dialogueID, this);
    }

    /// <summary>
    /// Called by DialogueManager after the final dialogue line is shown.
    /// Marks this interactable as completed.
    /// </summary>
    public void OnInteractionComplete()
    {
        if (!GameManager.Instance.CurrentState.completedInteractables.Contains(interactableID))
            GameManager.Instance.CurrentState.completedInteractables.Add(interactableID);
    }
}
