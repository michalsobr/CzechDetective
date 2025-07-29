using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Manages displaying and progressing dialogue sequences in the game.
/// Handles loading dialogue lines, showing speakers, and processing input to advance or skip dialogue using a typewriter effect.
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
    [SerializeField] private Button advanceButton;
    [SerializeField] private ButtonVisualizer advanceVisualizer;
    [SerializeField] private TextMeshProUGUI advanceText;
    [SerializeField] private TMPLinkHandler tmpLinkHandler;

    [Header("Dialogue Speakers")]
    [SerializeField] private GameObject leftSpeaker;
    [SerializeField] private RectTransform leftSpeakerTransform;
    [SerializeField] private Image leftSpeakerImage;
    [SerializeField] private TextMeshProUGUI leftSpeakerText;
    [SerializeField] private GameObject rightSpeaker;
    [SerializeField] private RectTransform rightSpeakerTransform;
    [SerializeField] private Image rightSpeakerImage;
    [SerializeField] private TextMeshProUGUI rightSpeakerText;

    [Header("Typewriter Settings")]
    [SerializeField] private float typingSpeed; // Characters per second.

    private SceneFlowController controller;

    private DialogueEntry currentEntry;
    private List<string> currentLines;
    private int currentLineIndex = 0;
    private string currentSpeaker;
    private string currentSpeakerSide;
    private int currentLineCount;
    private bool isTyping = false;
    private bool isFullyTyped = false;

    /// <summary> The active coroutine running the typewriter effect. </summary>
    private Coroutine typingCoroutine;

    /// <summary> Input actions used to handle UI input (advance/skip dialogue). </summary>
    private InputSystem_Actions inputActions;

    /// <summary> Whether advancing is temporarily blocked to prevent rapid skipping. </summary>
    private bool advanceBlocked = false;

    #endregion
    #region Unity Lifecycle Methods

    /// <summary>
    /// Invoked when the script instance is loaded, even if the GameObject is inactive.
    /// Ensures a single instance, makes the GameObject persistent across scenes, initializes input actions, and hides the dialogue canvas by default.
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

        if (advanceButton) advanceButton.onClick.AddListener(OnAdvance);

        // Hide the dialogue canvas by default.
        if (dialogueCanvas) dialogueCanvas.SetActive(false);
    }

    /// <summary>
    /// Invoked when the object becomes enabled.
    /// Enables input actions, subscribes to input events, and registers the scene load listener.
    /// </summary>
    private void OnEnable()
    {
        // Enable UI input actions.
        inputActions.UI.Enable();

        // Register input and scene load listeners.
        inputActions.UI.Advance.performed += OnAdvancePerformed;
        SceneManager.sceneLoaded += OnSceneLoaded;

        // If the scene was already loaded (and the controller is still null), assign it immediately.
        if (!controller) controller = FindFirstObjectByType<SceneFlowController>();
    }

    /// <summary>
    /// Invoked when the object becomes disabled.
    /// Disables input actions and unsubscribes from input and scene load events.
    /// </summary>
    private void OnDisable()
    {
        // Unregister input and scene load listeners.
        inputActions.UI.Advance.performed -= OnAdvancePerformed;
        SceneManager.sceneLoaded -= OnSceneLoaded;

        // Disable UI input actions.
        inputActions.UI.Disable();
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
    /// Handles advancing the dialogue sequence.
    /// If a line is still typing, it instantly completes it.
    /// If the line is fully typed, it proceeds to the next line or ends the dialogue.
    /// </summary>
    private void AdvanceDialogue()
    {

        // If the current line is still typing, finish it instantly.
        if (isTyping)
        {
            StopCoroutine(typingCoroutine);
            dialogueText.text = AddUnderlineToLinks(currentLines[currentLineIndex]);

            isTyping = false;
            isFullyTyped = true;

            // Button: "Next", unpressed, interactable
            UpdateAdvanceButton(true, "Next", false);

            // Prevent rapid multiple advances.
            StartCoroutine(AdvanceDialogueCooldown());
            return;
        }

        // If the current line is fully displayed, move to the next one or end the dialogue.
        if (isFullyTyped)
        {
            currentLineIndex++;

            if (currentLineIndex < currentLines.Count) // If there are more lines to be shown.
                DisplayLine(); // Show the next line
            else
                EndDialogue(); // End the dialogue

            // Prevent rapid multiple advances.
            StartCoroutine(AdvanceDialogueCooldown());
        }
    }

    /// <summary>
    /// Finalizes the dialogue sequence by hiding the UI, clearing dialogue state, and notifying the current <see cref="SceneFlowController"/> that the dialogue has ended.
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
        if (controller) controller.OnDialogueComplete(id);
    }

    #endregion
    #region Private Methods (UI Updates)

    /// <summary>
    /// Updates the visibility, image, name, and side of dialogue speakers based on the current entry data.
    /// </summary>
    private void UpdateDialogueSpeakers()
    {
        // Hide both speakers by default.
        leftSpeaker.SetActive(false);
        rightSpeaker.SetActive(false);

        if (currentSpeakerSide == "none") return;

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
        float constXSpeaker = 55f;
        float baseYSpeaker = 70f;
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
    /// Coroutine that simulates a typewriter effect by displaying dialogue text character by character.
    /// Ensures that rich-text tags (e.g., <b>, <i>, <link>, <u>) are appended instantly without being typed out one character at a time and preserves correct TMP formatting for links and underlines during typing.
    /// </summary>
    /// <param name="line">The raw dialogue line to display with the typing animation.</param>
    /// <returns>An enumerator for the coroutine execution.</returns>
    private IEnumerator TypeLine(string line)
    {
        // Show the force pressed "Skip" advance button and disable its interaction while typing.
        UpdateAdvanceButton(false, "Skip", true);

        // Preprocess the line to add underline tags to links before typing.
        string formattedLine = AddUnderlineToLinks(line);

        // Initialize typing state
        isTyping = true;
        isFullyTyped = false;
        dialogueText.text = "";

        int i = 0;
        while (i < formattedLine.Length)
        {
            // If a rich-text tag is encountered, append the entire tag instantly without delay
            if (formattedLine[i] == '<') // Start of tag
            {
                int tagEnd = formattedLine.IndexOf('>', i);
                if (tagEnd != -1)
                {
                    // Append the entire tag instantly
                    dialogueText.text += formattedLine.Substring(i, tagEnd - i + 1);
                    i = tagEnd + 1;
                    continue; // Skip delay for tags
                }
            }

            // Append a single visible character and wait before typing the next one.
            dialogueText.text += formattedLine[i];
            i++;
            yield return new WaitForSeconds(typingSpeed);
        }

        // Typing finished – update state
        isTyping = false;
        isFullyTyped = true;

        // Show the "Next" advance button and enable its interaction.
        UpdateAdvanceButton(true, "Next", false);
    }

    /// <summary>
    /// Adds underline tags to any clickable links in the provided dialogue text.
    /// </summary>
    /// <param name="text">The dialogue text containing link tags.</param>
    /// <returns>The modified text with underlined links.</returns>
    private string AddUnderlineToLinks(string text)
    {
        return System.Text.RegularExpressions.Regex.Replace(
            text,
            "<link=\"([^\"]+)\">(.+?)</link>",
            "<link=\"$1\"><u>$2</u></link>"
        );
    }

    /// <summary>
    /// Updates the appearance and state of the advance button, including its interactability, label, and pressed state.
    /// </summary>
    /// <param name="interactable">Whether the button should be interactable.</param>
    /// <param name="label">The text to display on the button.</param>
    /// <param name="forcePressed">Whether to force the button into the pressed visual state.</param>
    private void UpdateAdvanceButton(bool interactable, string label, bool forcePressed)
    {
        if (!advanceButton) return;

        // Set interactability
        advanceButton.interactable = interactable;

        // Update text
        if (advanceText)
            advanceText.text = label;

        // Force pressed or unpressed state
        if (advanceVisualizer) advanceVisualizer.ForcePressed(forcePressed);
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
    /// Handles advancing the dialogue when triggered by button click or input action.
    /// Ensures dialogue advancement is allowed before proceeding.
    /// </summary>
    private void OnAdvance()
    {
        if (advanceBlocked || !CanAdvanceDialogue()) return;

        AdvanceDialogue();
    }

    /// <summary>
    /// Invoked when the advance action (spacebar, button click) is performed.
    /// Calls <see cref="OnAdvance"/> to handle advancing dialogue.
    /// </summary>
    /// <param name="context">The input action context for the advance event.</param>
    private void OnAdvancePerformed(InputAction.CallbackContext context)
    {
        OnAdvance();
    }

    /// <summary>
    /// Invoked when a new scene is loaded.
    /// Updates the reference to the active scene's <see cref="SceneFlowController"/>.
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
    /// Checks if dialogue can currently be advanced.
    /// Ensures there is an active dialogue, lines remaining, and no blocking popups.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the dialogue can be advanced; 
    /// <c>false</c> otherwise.
    /// </returns>
    private bool CanAdvanceDialogue()
    {
        // Block input if any popup is currently open.
        if (UIManager.Instance != null && UIManager.Instance.IsPopupOpen()) return false;

        // Safety check: allow advancing only if there are lines remaining and the dialogue UI is active.
        return currentLines != null && currentLines.Count > 0 && dialogueCanvas.activeSelf;
    }

    #endregion
}
