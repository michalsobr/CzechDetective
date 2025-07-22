using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InitializationController : MonoBehaviour
{
    // default scene used for both new or loaded games.
    public static string SceneToLoad = "Base";

    // runs immediately when the script is loaded (before the first frame) - even if the GameObject is disabled.
    private void Awake()
    {
        if (FindFirstObjectByType<UIManager>() == null)
            Instantiate(Resources.Load("Prefabs/UIManager"));

        if (FindFirstObjectByType<DialogueManager>() == null)
            Instantiate(Resources.Load("Prefabs/DialogueManager"));

        StartCoroutine(LoadTargetScene());
    }

    // changes scenes based on if a new game was started or a game was loaded.
    private IEnumerator LoadTargetScene()
    {
        // delay execution by one frame (to make sure everything from Awake() has been fully initialized).
        yield return null;

        // if the game was loaded.
        if (GameManager.Instance.CurrentState != null) SceneToLoad = GameManager.Instance.CurrentState.currentScene;

        // SceneToLoad will remain default "Base" if "New Game" was chose OR will be replaced with the value from the loaded save file.
        SceneManager.LoadScene(SceneToLoad);
    }
}
