using UnityEngine;

/// <summary>
/// Base class for all scene-specific controllers, providing virtual methods for scene logic such as triggering dialogue and handling completed dialogue events.
/// </summary>
public class SceneFlowController : MonoBehaviour
{
    #region Fields

    [Header("Common Scene Elements")]
    [SerializeField] protected GameObject backgroundImage;
    [SerializeField] protected GameObject interactableCanvas;

    #endregion
    #region Unity Lifecycle Methods

    /// <summary>
    /// Invoked on the first frame when the script is enabled and active.
    /// Sets up the default state of common scene elements and, in the Unity Editor, instantiates core managers if missing and reloads the dialogue database.
    /// </summary>
    public virtual void Start()
    {
        if (backgroundImage) backgroundImage.SetActive(true);
        if (interactableCanvas) interactableCanvas.SetActive(false);

        // Force a manual reload of the dialogue database for the current scene.
        DialogueDatabase.Instance.Reload();
    }

    #endregion
    #region Public Methods

    /// <summary>
    /// Invoked by the <see cref="DialogueManager"/> when a dialogue with the given ID finishes.
    /// Marks the dialogue as completed in the current <see cref="GameState"/>.
    /// </summary>
    /// <param name="id">The ID of the completed dialogue.</param>
    public virtual void OnDialogueComplete(string id)
    {
        GameManager.Instance.CurrentState.MarkDialogueComplete(id);
        TranslationManager.Instance.UnlockTranslations();
    }

    /// <summary>
    /// Displays the scene's entry dialogue when the scene starts.
    /// Intended to be overridden by derived classes for scene-specific behavior.
    /// </summary>
    /// <param name="state">The current <see cref="GameState"/>.</param>
    public virtual void ShowSceneEntryDialogue(GameState state) { }

    #endregion
}
