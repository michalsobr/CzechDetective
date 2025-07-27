using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Handles main menu button logic for starting a new game, loading a saved game, and exiting the application.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    #region Fields

    [Header("UI References")]
    [SerializeField] private NamePromptCanvas namePromptCanvas;
    [SerializeField] private LoadGameCanvas loadGameCanvas;

    [Header("Main Menu Elements")]
    [SerializeField] private Button startButton;
    [SerializeField] private MainMenuVisualizer startVisualizer;
    [SerializeField] private Button loadButton;
    [SerializeField] private MainMenuVisualizer loadVisualizer;
    [SerializeField] private Button exitButton;

    #endregion
    #region Unity Lifecycle Methods

    /// <summary>
    /// Called when the script instance is loaded (even if the GameObject is inactive).
    /// Registers button click listeners for the main menu buttons.
    /// </summary>
    private void Awake()
    {
        if (startButton) startButton.onClick.AddListener(OnStartClicked);
        if (loadButton) loadButton.onClick.AddListener(OnLoadClicked);
        if (exitButton) exitButton.onClick.AddListener(OnExitClicked);
    }

    #endregion
    #region Public Methods

    /// <summary>
    /// Sets the interactability of all main menu buttons.
    /// </summary>
    /// <param name="state"><c>true</c> to enable buttons; <c>false</c> to disable them.</param>
    public void SetButtonInteractability(bool state)
    {
        if (startButton) startButton.interactable = state;
        if (loadButton) loadButton.interactable = state;
        if (exitButton) exitButton.interactable = state;
    }

    #endregion
    #region Event Handlers / Callbacks

    /// <summary>
    /// Handles the "New Game" button click.
    /// Disables all main menu buttons, shows the name prompt panel, resets the start button visual to its default state, and subscribes to the callback that will create and load the new game once the player confirms their name.
    /// </summary>
    private void OnStartClicked()
    {
        if (namePromptCanvas)
        {
            SetButtonInteractability(false);
            if (startVisualizer) startVisualizer.DisableGradient();

            namePromptCanvas.Show();

            // Clear any previous callbacks to prevent duplicate subscriptions
            namePromptCanvas.OnNameChosenCallback = null;
            namePromptCanvas.OnNameChosenCallback += OnNameChosen;
        }
    }

    /// <summary>
    /// Disables all main menu buttons, shows the load game popup, and resets the load button visual to its default state.
    /// </summary>
    private void OnLoadClicked()
    {
        SetButtonInteractability(false);
        if (loadVisualizer) loadVisualizer.DisableGradient();

        if (loadGameCanvas) loadGameCanvas.ShowCanvas();
    }

    /// <summary>
    /// Quits the application. Logs a message when running in the editor.
    /// </summary>
    private void OnExitClicked()
    {
        Debug.Log("Exit clicked");
        Application.Quit();
    }

    /// <summary>
    /// Called when the player has chosen a valid name.
    /// Creates a new game state, saves it, and loads the Initialization scene.
    /// </summary>
    private void OnNameChosen(string playerName)
    {
        // Create and save a new game state
        GameManager.Instance.CreateNewGame(playerName);
        GameManager.Instance.SaveGameState();

        // Load the initialization scene
        SceneManager.LoadScene("Initialization");
    }

    #endregion
}
