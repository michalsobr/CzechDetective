using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Handles the main menu logic, including starting a new game, loading a saved game, and exiting the application.
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
    /// Invoked when the script instance is loaded, even if the GameObject is inactive.
    /// Registers click listeners for all main menu buttons.
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
    /// Enables or disables interaction for all main menu buttons.
    /// </summary>
    /// <param name="state">
    /// <c>true</c> to enable buttons; 
    /// <c>false</c> to disable them.
    /// </param>
    public void SetButtonInteractability(bool state)
    {
        if (startButton) startButton.interactable = state;
        if (loadButton) loadButton.interactable = state;
        if (exitButton) exitButton.interactable = state;
    }

    #endregion
    #region Event Handlers / Callbacks

    /// <summary>
    /// Invoked when the "New Game" button is clicked.
    /// Disables main menu buttons, shows the name prompt panel, resets the start button visual, and subscribes to the callback that will create and load a new game when the player confirms their name.
    /// </summary>
    private void OnStartClicked()
    {
        if (namePromptCanvas)
        {
            if (startVisualizer) startVisualizer.DisableGradient();

            namePromptCanvas.Show();

            // Clear any previous callbacks to prevent duplicate subscriptions
            namePromptCanvas.OnNameChosenCallback = null;
            namePromptCanvas.OnNameChosenCallback += OnNameChosen;
        }
    }

    /// <summary>
    /// Invoked when the "Load Game" button is clicked.
    /// Disables main menu buttons, resets the load button visual, and displays the load game popup.
    /// </summary>
    private void OnLoadClicked()
    {
        SetButtonInteractability(false);
        if (loadVisualizer) loadVisualizer.DisableGradient();

        if (loadGameCanvas) loadGameCanvas.ShowCanvas();
    }

    /// <summary>
    /// Invoked when the "Exit" button is clicked.
    /// Quits the application.
    /// </summary>
    private void OnExitClicked()
    {
        Debug.Log("Exit clicked");
        Application.Quit();
    }

    /// <summary>
    /// Invoked after the player chooses a valid name.
    /// Creates a new game state, saves it, initializes required managers, and loads the Base scene.
    /// </summary>
    private void OnNameChosen(string playerName)
    {
        // Create and save a new game state
        GameManager.Instance.CreateNewGame(playerName);
        GameManager.Instance.SaveGameState();

        GameManager.Instance.InitializeManagers();

        // Load the Base scene directly
        SceneManager.LoadScene("Base");
    }

    #endregion
}
