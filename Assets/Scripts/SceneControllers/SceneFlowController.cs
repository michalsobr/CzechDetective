using UnityEngine;

/// <summary>
/// Serves as the base class for all scene-specific controllers.
/// Provides virtual methods for scene-specific logic, such as triggering dialogue and reacting to completed dialogue events.
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
    /// Called only once, on the first frame when the script is enabled and active.
    /// Initializes core managers (in the Unity Editor), if missing, and sets the default state of common scene elements.
    /// </summary>
    public virtual void Start()
    {
        // Instantiate required core managers if the Initialization scene was skipped.
#if UNITY_EDITOR
        InstantiateIfMissing<GameManager>("Prefabs/GameManager");
        InstantiateIfMissing<SaveManager>("Prefabs/SaveManager");
        InstantiateIfMissing<UIManager>("Prefabs/UIManager");
        InstantiateIfMissing<DialogueManager>("Prefabs/DialogueManager");

        // Force a manual reload of the dialogue database for the current scene.
        DialogueDatabase.Instance.Reload();
#endif

        if (backgroundImage) backgroundImage.SetActive(true);
        if (interactableCanvas) interactableCanvas.SetActive(false);
    }

    #endregion
    #region Public Methods

    /// <summary>
    /// Called by the <see cref="DialogueManager"/> when a dialogue with the given ID finishes.
    /// Updates the current <see cref="GameState"/> by marking the dialogue as completed.
    /// </summary>
    /// <param name="id">The ID of the completed dialogue.</param>
    public virtual void OnDialogueComplete(string id)
    {
        GameManager.Instance.CurrentState.MarkDialogueComplete(id);
    }

    /// <summary>
    /// Displays the scene's entry dialogue when the scene starts.
    /// Intended to be overridden by derived classes for scene-specific behavior.
    /// </summary>
    /// <param name="state">The current <see cref="GameState"/>.</param>
    public virtual void ShowSceneEntryDialogue(GameState state) { }

    #endregion
    #region Private Methods (Editor Only)

#if UNITY_EDITOR

    /// <summary>
    /// Instantiates the specified prefab from the Resources folder if the object is missing in the scene.
    /// Used in the Unity Editor when the Initialization scene is skipped.
    /// </summary>
    /// <typeparam name="T">The type of the component to find or instantiate.</typeparam>
    /// <param name="prefabPath">The path to the prefab inside the Resources folder.</param>
    private void InstantiateIfMissing<T>(string prefabPath) where T : MonoBehaviour
    {
        if (FindFirstObjectByType<T>() == null)
        {
            GameObject prefab = Resources.Load<GameObject>(prefabPath);
            if (prefab != null)
            {
                Instantiate(prefab);
                Debug.Log($"[SceneFlowController] Auto-spawned {typeof(T).Name} from {prefabPath}");
            }
            else
            {
                Debug.LogWarning($"[SceneFlowController] Could not find prefab at Resources/{prefabPath}");
            }
        }
    }

#endif

    #endregion
}
