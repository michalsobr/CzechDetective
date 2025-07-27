using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles the transition from the Initialization scene to the appropriate gameplay scene.
/// Ensures that required managers are present and loads the correct scene based on the current game state.
/// </summary>
public class InitializationController : MonoBehaviour
{
    #region Unity Lifecycle Methods

    /// <summary>
    /// Called when the script instance is loaded (even if the GameObject is inactive).
    /// Ensures that required managers exist and starts loading the target scene.
    /// </summary>
    private void Awake()
    {
        // Instantiate managers, if they are not already present in the scene.
        if (FindFirstObjectByType<EventSystemDDOL>() == null)
            Instantiate(Resources.Load("Prefabs/EventSystemDDOL"));

        if (FindFirstObjectByType<GameManager>() == null)
            Instantiate(Resources.Load("Prefabs/GameManager"));

        if (FindFirstObjectByType<SaveManager>() == null)
            Instantiate(Resources.Load("Prefabs/SaveManager"));

        if (FindFirstObjectByType<UIManager>() == null)
            Instantiate(Resources.Load("Prefabs/UIManager"));

        if (FindFirstObjectByType<DialogueManager>() == null)
            Instantiate(Resources.Load("Prefabs/DialogueManager"));

        // Begin scene loading.
        StartCoroutine(LoadTargetScene());
    }

    #endregion
    #region Coroutines

    /// <summary>
    /// Loads the appropriate scene based on the current game state.
    /// </summary>
    private IEnumerator LoadTargetScene()
    {
        // Wait one frame to ensure managers created in Awake() are fully initialized.
        yield return null;

        Debug.Log($"[InitializationController] Loading scene: {GameManager.Instance.CurrentState.currentScene}");

        SceneManager.LoadScene(GameManager.Instance.CurrentState.currentScene);
    }

    #endregion
}
