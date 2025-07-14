using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InitController : MonoBehaviour
{
    // default scene used for both new or loaded games.
    public static string SceneToLoad = "Base";
    // stays null if it is a new game.
    public static GameState GameStateToLoad = null;

    void Awake()
    {
        // checks to not create duplicates instances.
        if (FindFirstObjectByType<GameManager>() == null)
            Instantiate(Resources.Load("Prefabs/GameManager"));

        if (FindFirstObjectByType<SaveManager>() == null)
            Instantiate(Resources.Load("Prefabs/SaveManager"));

        if (FindFirstObjectByType<UIManager>() == null)
            Instantiate(Resources.Load("Prefabs/UIManager"));

        if (FindFirstObjectByType<DialogueManager>() == null)
            Instantiate(Resources.Load("Prefabs/DialogueManager"));

        StartCoroutine(LoadTargetScene());
    }

    private IEnumerator LoadTargetScene()
    {
        // delay execution by one frame (to make sure everything from Awake() has been fully initialized).
        yield return null;

        // if the game was loaded.
        if (GameStateToLoad != null)
        {
            // load game using the save file's GameState.
            GameManager.Instance.LoadGameState(GameStateToLoad);
            SceneToLoad = GameManager.Instance.CurrentState.currentScene;
        }
        // else - leave GameState null (it will be initialized after the name entry in the Base scene, and load the correct scene.
        SceneManager.LoadScene(SceneToLoad);
    }
}
