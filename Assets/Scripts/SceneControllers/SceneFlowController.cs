using UnityEngine;

// base class for all scene-specific controllers. Contains virtual methods for scene-specific logic like triggering dialogue and reacting to completed dialogue events.
public class SceneFlowController : MonoBehaviour
{
    [Header("Common Scene Elements")]
    [SerializeField] protected GameObject backgroundImage;
    [SerializeField] protected GameObject interactableCanvas;

    // override in subclasses to trigger dialogue when the scene starts.
    public virtual void Start()
    {
#if UNITY_EDITOR
        InstantiateIfMissing<GameManager>("Prefabs/GameManager");
        InstantiateIfMissing<SaveManager>("Prefabs/SaveManager");
        InstantiateIfMissing<UIManager>("Prefabs/UIManager");
        InstantiateIfMissing<DialogueManager>("Prefabs/DialogueManager");

        DialogueDatabase.Instance.Reload();
#endif

        if (backgroundImage)
            backgroundImage.SetActive(true);

        if (interactableCanvas)
            interactableCanvas.SetActive(false);
    }

    // called by DialogueManager when a dialogue with the given ID finishes.
    public virtual void OnDialogueComplete(string id)
    {
        // update the GameState by adding the completed dialogue to it.
        GameManager.Instance.CurrentState.MarkDialogueComplete(id);
    }

    public virtual void ShowSceneEntryDialogue(GameState state) { }

#if UNITY_EDITOR
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

}

