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

        // Instantiate required core managers if the Initialization scene was skipped.
#if UNITY_EDITOR
        InstantiateIfMissing<EventSystemDDOL>("Prefabs/EventSystemDDOL");
        InstantiateIfMissing<GameManager>("Prefabs/GameManager");
        InstantiateIfMissing<SaveManager>("Prefabs/SaveManager");
        InstantiateIfMissing<UIManager>("Prefabs/UIManager");
        InstantiateIfMissing<DialogueManager>("Prefabs/DialogueManager");

        // Create and assign a fresh GameState to the Game Manager.
        GameManager.Instance.CreateNewGame("Editor");

        // Force a manual reload of the dialogue database for the current scene.
        DialogueDatabase.Instance.Reload();
#endif
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
    /// Instantiates a prefab from the Resources folder if an object of the specified type is not found in the scene.
    /// Used in the Unity Editor when the initialization step in Main Menu scene is skipped.
    /// </summary>
    /// <typeparam name="T">The type of component to find or instantiate.</typeparam>
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
