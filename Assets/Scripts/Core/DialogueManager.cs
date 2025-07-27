using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Manages the display and progression of dialogue sequences in the game.
/// Handles loading and showing dialogue lines, controlling speaker portraits and names, and processing input to advance or skip dialogue using a typewriter effect.
/// Implements a singleton pattern and persists across scenes.
/// Runs before other scripts by default.
/// </summary>
[DefaultExecutionOrder(-100)]
public class DialogueManager : MonoBehaviour
{
    #region Fields

    // Singleton instance.
    public static DialogueManager Instance { get; private set; }

    [Header("Dialogue Panel")]
    [SerializeField] private GameObject dialogueCanvas;
    [SerializeField] private RectTransform panelTransform;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private RectTransform textTransform;

    [Header("Dialogue Speakers")]
    [SerializeField] private GameObject leftSpeaker;
    [SerializeField] private GameObject rightSpeaker;
    [SerializeField] private RectTransform leftSpeakerTransform;
    [SerializeField] private RectTransform rightSpeakerTransform;
    [SerializeField] private Image leftSpeakerImage;
    [SerializeField] private Image rightSpeakerImage;
    [SerializeField] private TextMeshProUGUI leftSpeakerText;
    [SerializeField] private TextMeshProUGUI rightSpeakerText;

    [Header("Typewriter Settings")]
    // Characters per second.
    [SerializeField] private float typingSpeed;

    private SceneFlowController controller;

    private DialogueEntry currentEntry;
    private List<string> currentLines;
    private int currentLineIndex = 0;
    private string currentSpeaker;
    private string currentSpeakerSide;
    private int currentLineCount;

    private Coroutine typingCoroutine;
    private bool isTyping = false;
    private bool isFullyTyped = false;

    private InputSystem_Actions inputActions;
    // prevents input if the line is animating/skipped
    private bool advanceBlocked = false;

    #endregion
    #region Unity Lifecycle Methods

    /// <summary>
    /// Called when the script instance is loaded (even if the GameObject is inactive).
    /// Ensures a single instance, sets up persistent state across scenes, initializes input actions, and configures the dialogue canvas to be hidden by default.
    /// </summary>
    private void Awake()
    {
        // If another instance already exists, destroy this one.
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Prevent this object from being destroyed when loading new scenes.
        DontDestroyOnLoad(gameObject);

        // Initialize input actions.
        inputActions = new InputSystem_Actions();

        // Hide the dialogue canvas by default.
        if (dialogueCanvas) dialogueCanvas.SetActive(false);
    }

    /// <summary>
    /// Called each time the object becomes enabled.
    /// Enables input actions and registers listeners for UI input and scene load events.
    /// </summary>
    private void OnEnable()
    {
        // Enable UI input actions.
        inputActions.UI.Enable();

        // Register input listeners.
        inputActions.UI.Click.performed += OnClickPerformed;
        inputActions.UI.Navigate.performed += OnNavigatePerformed;

        // Register scene load listener.
        SceneManager.sceneLoaded += OnSceneLoaded;

        // If the scene was already loaded (and the controller is still null), assign it immediately.
        if (!controller) controller = FindFirstObjectByType<SceneFlowController>();
    }

    /// <summary>
    /// Called each time the object becomes disabled.
    /// Unregisters input and scene load event listeners and disables input actions.
    /// </summary>
    private void OnDisable()
    {
        // Unregister input listeners.
        inputActions.UI.Click.performed -= OnClickPerformed;
        inputActions.UI.Navigate.performed -= OnNavigatePerformed;

        // Disable UI input actions.
        inputActions.UI.Disable();

        // Unregister scene load listener.
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    #endregion
    #region Public Methods

    /// <summary>
    /// Starts the dialogue sequence using the dialogue entry with the specified ID.
    /// </summary>
    /// <param name="id">The unique identifier of the dialogue entry to show.</param>
    public void ShowDialogue(string id)
    {
        DialogueEntry entry = DialogueDatabase.Instance.Get(id);

        // If a dialogue entry with the given ID exists, show it.
        // *If not, DialogueDatabase will log a warning.
        if (entry != null) ShowDialogue(entry);
    }

    #endregion
    #region Private Methods (Core Logic)

    /// <summary>
    /// Begins displaying dialogue using the specified dialogue entry.
    /// </summary>
    /// <param name="entry">The dialogue entry containing the speaker, side, and lines of dialogue.</param>
    private void ShowDialogue(DialogueEntry entry)
    {
        // Safety check: ensure the entry and its lines are valid.
        if (entry == null || entry.lines == null || entry.lines.Count == 0)
        {
            Debug.LogWarning("[DialogueManager] Received null or empty DialogueEntry.");
            return;
        }


        // Show the dialogue canvas if it exists.
        if (dialogueCanvas) dialogueCanvas.SetActive(true);

        // Set up dialogue state.
        currentEntry = entry;
        currentLines = entry.lines;
        currentSpeaker = entry.speaker;
        currentSpeakerSide = entry.speakerSide;
        currentLineIndex = 0;
        isTyping = false;
        isFullyTyped = false;

        /// Update the speaker's image and name based on the current dialogue entry.
        UpdateDialogueSpeakers();

        // Begin displaying the first dialogue line.
        DisplayLine();
    }

    /// <summary>
    /// Displays the current dialogue line by starting the typewriter animation.
    /// Also adjusts the dialogue UI based on the number of lines the text will occupy as well as speaker name, and speaker side.
    /// </summary>
    private void DisplayLine()
    {
        // Calculate how many lines the current dialogue line will occupy and adjust the UI accordingly.
        currentLineCount = GetLineCount(currentLines[currentLineIndex]);
        AdjustDialogueUI();

        // Start the typewriter animation for the current line.
        typingCoroutine = StartCoroutine(TypeLine(currentLines[currentLineIndex]));
    }

    /// <summary>
    /// Handles advancing the dialogue. 
    /// If the line is still typing, it instantly completes it. 
    /// If the line is fully typed, it proceeds to the next line of dialgoue or ends the dialogue.
    /// </summary>
    private void AdvanceDialogue()
    {
        // If the current line is still typing, finish it instantly.
        if (isTyping)
        {
            StopCoroutine(typingCoroutine);
            dialogueText.text = currentLines[currentLineIndex];

            isTyping = false;
            isFullyTyped = true;

            // Prevent rapid multiple advances.
            StartCoroutine(AdvanceDialogueCooldown());
            return;
        }

        // If the current line is fully displayed, move to the next one or end the dialogue.
        if (isFullyTyped)
        {
            currentLineIndex++;

            // If there are more lines to be shown.
            if (currentLineIndex < currentLines.Count)
                DisplayLine(); // Show the next line
            else
                EndDialogue(); // End the dialogue

            // Prevent rapid multiple advances.
            StartCoroutine(AdvanceDialogueCooldown());
        }
    }

    /// <summary>
    /// Finalizes the dialogue sequence by hiding the dialogue UI, resetting the dialogue state, and notifying the current scene's controller that the dialogue has ended.
    /// </summary>
    private void EndDialogue()
    {
        // Hide the dialogue UI.
        dialogueCanvas.SetActive(false);

        string id = currentEntry.id;

        // Reset dialogue state.
        currentEntry = null;
        currentLines = null;

        // Notify the scene controller that the dialogue has finished.
        if (controller)
        {
            Debug.Log($"[DialogueManager] Calling {controller.name}.OnDialogueComplete({id})");
            controller.OnDialogueComplete(id);
        }
        else Debug.Log($"[DialogueManager] Controller is null.)");
    }

    #endregion
    #region Private Methods (UI Updates)

    /// <summary>
    /// Updates speaker visibility, image, name, side, and position based on the entry data,
    /// </summary>
    private void UpdateDialogueSpeakers()
    {
        // Hide both speakers by default.
        leftSpeaker.SetActive(false);
        rightSpeaker.SetActive(false);

        // Show and update the left speaker if active.
        if (currentSpeakerSide == "left")
        {
            leftSpeaker.SetActive(true);
            leftSpeakerImage.sprite = LoadSpeakerSprite();
            leftSpeakerText.text = UpdateSpeakerText();
        }

        // Show and update the right speaker if active.
        if (currentSpeakerSide == "right")
        {
            rightSpeaker.SetActive(true);
            rightSpeakerImage.sprite = LoadSpeakerSprite();
            rightSpeakerText.text = UpdateSpeakerText();
        }
    }

    /// <summary>
    /// Loads the speaker's portrait sprite from the Resources folder based on the current speaker's name.
    /// </summary>
    /// <returns>The speaker's portrait sprite, or <c>null</c> if not found.</returns>
    private Sprite LoadSpeakerSprite()
    {
        Sprite sprite = Resources.Load<Sprite>($"Sprites/Characters/{currentSpeaker}");

        if (sprite == null) Debug.LogWarning($"[DialogueManager] Portrait not found for: {currentSpeaker}");

        return sprite;
    }

    /// <summary>
    /// Determines the display name of the current speaker based on the speaker's ID.
    /// </summary>
    /// <returns> A more user-friendly name if a match is found; otherwise - the raw speaker ID.</returns>
    private string UpdateSpeakerText()
    {
        string speakerName = "";

        // Map specific speaker IDs to display names.
        // Example: currentSpeaker could be "detective_init", "detective_happy", "detective_angry", etc.
        if (currentSpeaker.Contains("detective")) speakerName = "Tobiáš";
        else if (currentSpeaker == "letterman") speakerName = "???";
        else if (currentSpeaker == "aunt_unknown") speakerName = "Růžena Nováková";
        else if (currentSpeaker == "aunt_known") speakerName = "teta Růžena";

        return string.IsNullOrEmpty(speakerName) ? currentSpeaker : speakerName;
    }

    /// <summary>
    /// Adjusts the dialogue UI layout based on the number of visible lines.
    /// </summary>
    private void AdjustDialogueUI()
    {
        // Safety check: do nothing if there are no lines to display.
        if (currentLineCount <= 0) return;

        UpdateDialoguePanel();
        UpdateSpeakerPosition();
    }

    /// <summary>
    /// Updates the dialogue panel's size and text padding based on the current line count.
    /// </summary>
    private void UpdateDialoguePanel()
    {
        // Calculate the new height of the dialogue panel based on line count.
        float baseHeight = 72f;
        float newHeight = baseHeight * currentLineCount;

        // Apply the new size to the panel.
        panelTransform.sizeDelta = new Vector2(panelTransform.sizeDelta.x, newHeight);

        // Calculate text padding based on line count.
        float constPadding = 100f;
        float newPadding = 18f + ((currentLineCount - 1) * 12f);

        // Apply the new size and padding to the text container.
        textTransform.sizeDelta = new Vector2(textTransform.sizeDelta.x, newHeight);
        dialogueText.margin = new Vector4(constPadding, newPadding, constPadding, newPadding);
    }

    /// <summary>
    /// Updates the position of the active speaker based on their side and adjusts the vertical Y position according to the current dialogue line count.
    /// </summary>
    private void UpdateSpeakerPosition()
    {
        // Define the constant X and base Y positions and calculate the adjusted Y position.
        float constXSpeaker = 175f;
        float baseYSpeaker = 85f;
        float newYSpeaker = baseYSpeaker + ((currentLineCount - 1) * 68f);

        // Update the position of the active speaker based on their side.
        if (currentSpeakerSide == "left") leftSpeakerTransform.anchoredPosition = new Vector3(constXSpeaker, newYSpeaker, 0f);
        if (currentSpeakerSide == "right") rightSpeakerTransform.anchoredPosition = new Vector3(-constXSpeaker, newYSpeaker, 0f);
    }

    /// <summary>
    /// Calculates how many lines the given dialogue text will occupy on screen.
    /// </summary>
    /// <param name="line">The dialogue text to measure.</param>
    /// <returns>The number of visible lines it will occupy.</returns>
    private int GetLineCount(string line)
    {
        dialogueText.text = line;

        // Force a mesh update to get an accurate line count info.
        dialogueText.ForceMeshUpdate();

        return dialogueText.textInfo.lineCount;
    }

    #endregion
    #region Coroutines

    /// <summary>
    /// Coroutine that simulates a typewriter effect by displaying the dialogue text character by character.
    /// </summary>
    /// <param name="line">The dialogue line to display with the typing animation.</param>
    /// <returns>An enumerator for the coroutine execution.</returns>
    private IEnumerator TypeLine(string line)
    {
        isTyping = true;
        isFullyTyped = false;

        dialogueText.text = "";

        // Display each character one by one with a short delay to create a typewriter effect.
        foreach (char c in line)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        isFullyTyped = true;
    }

    /// <summary>
    /// Coroutine that temporarily blocks dialogue advancement to prevent accidental multi-clicks or fast skipping.
    /// </summary>
    /// <returns>An enumerator for the coroutine execution.</returns>
    private IEnumerator AdvanceDialogueCooldown()
    {
        advanceBlocked = true;
        // Wait for 0.3 seconds before allowing dialogue advancement again.
        yield return new WaitForSeconds(0.3f);
        advanceBlocked = false;
    }

    #endregion
    #region Event Handlers / Callbacks

    /// <summary>
    /// Handles click input during dialogue. 
    /// Depending on the current state, either finishes typing the current line or advances the dialogue.
    /// </summary>
    /// <param name="context">The input action context for the click event.</param>

    private void OnClickPerformed(InputAction.CallbackContext context)
    {
        // Block input if advancement is currently disabled or the click is not valid.
        if (advanceBlocked || !IsValidClick(context)) return;

        AdvanceDialogue();
    }

    /// <summary>
    /// Handles spacebar (navigate) input during dialogue. 
    /// Functions the same as <see cref="OnClickPerformed"/> but is triggered by the navigate action.
    /// </summary>
    /// <param name="context">The input action context for the navigate event.</param>

    private void OnNavigatePerformed(InputAction.CallbackContext context)
    {
        // Block input if advancement is currently disabled or dialogue cannot be advanced.
        if (advanceBlocked || !CanAdvanceDialogue()) return;

        AdvanceDialogue();
    }

    /// <summary>
    /// Called when a new scene is loaded. Updates the reference to the scene-specific <see cref="SceneFlowController"/>.
    /// </summary>
    /// <param name="scene">The scene that was loaded.</param>
    /// <param name="mode">The scene loading mode.</param>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Find and update the reference to the scene's controller.
        controller = FindFirstObjectByType<SceneFlowController>();
    }

    #endregion
    #region Helper Methods (Validation / Checks)

    /// <summary>
    /// Determines whether a click input is valid for advancing the dialogue.
    /// A click is valid only if dialogue can be advanced and the click occurred inside the dialogue panel.
    /// </summary>
    /// <param name="context">The input action context for the click event.</param>
    /// <returns><c>true</c> if the click is valid for advancing the dialogue; otherwise, <c>false</c>.</returns>
    private bool IsValidClick(InputAction.CallbackContext context)
    {
        if (!CanAdvanceDialogue()) return false;

        // Only accept clicks that occur inside the dialogue panel.
        Vector2 clickPos = Mouse.current.position.ReadValue();
        return RectTransformUtility.RectangleContainsScreenPoint(panelTransform, clickPos);
    }

    /// <summary>
    /// Determines whether the dialogue can currently be advanced.
    /// Checks if there is an active dialogue, lines remaining, and that no popup is blocking the input.
    /// </summary>
    /// <returns><c>true</c> if the dialogue can be advanced; otherwise, <c>false</c>.</returns>
    private bool CanAdvanceDialogue()
    {
        // Block input if any popup is currently open.
        if (UIManager.Instance != null && UIManager.Instance.IsPopupOpen()) return false;

        // Safety check: allow advancing only if there are lines remaining and the dialogue UI is active.
        return currentLines != null && currentLines.Count > 0 && dialogueCanvas.activeSelf;
    }

    #endregion
}
