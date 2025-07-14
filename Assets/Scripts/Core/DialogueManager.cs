using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("Dialogue Panel")]
    [SerializeField] private GameObject dialogueCanvas;
    [SerializeField] private RectTransform panelTransform;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private RectTransform textTransform;

    [Header("Dialogue Speakers")]
    [SerializeField] private RectTransform leftAvatarTransform;
    [SerializeField] private RectTransform leftSpeakerTextTransform;
    [SerializeField] private RectTransform rightAvatarTransform;
    [SerializeField] private RectTransform rightSpeakerTextTransform;

    [Header("Typewriter Settings")]
    [SerializeField] private float typingSpeed = 0.02f;

    private SceneFlowController controller;
    private DialogueEntry currentEntry;
    private List<string> currentLines;
    private int currentLineIndex = 0;

    private Coroutine typingCoroutine;
    private bool isTyping = false;
    private bool isFullyTyped = false;
    private bool clickBlocked = false;

    private InputSystem_Actions inputActions;

    private void Awake()
    {
        // safety check, if single instance already exists.
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        // persist across scenes.
        Instance = this;
        DontDestroyOnLoad(gameObject);

        inputActions = new InputSystem_Actions();

        // default and if inactive - don't show dialogue canvas.
        if (dialogueCanvas) dialogueCanvas.SetActive(false);
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name != "Init")
            controller = FindFirstObjectByType<SceneFlowController>();
    }

    // if enabled - listen for clicks and changing of scenes.
    private void OnEnable()
    {
        inputActions.UI.Enable();
        inputActions.UI.Click.performed += OnClickPerformed;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // if disabled - stop listening for clicks and changing of scenes.
    private void OnDisable()
    {
        inputActions.UI.Click.performed -= OnClickPerformed;
        inputActions.UI.Disable();

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // get the new (current) scene's controller.
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        controller = FindFirstObjectByType<SceneFlowController>();
    }

    // show dialogue by ID.
    public void ShowDialogue(string dialogueId)
    {
        DialogueEntry entry = DialogueDatabase.Instance.Get(dialogueId);
        if (entry != null)
        {
            Debug.LogWarning($"[DialogueManager] Dialogue with ID {dialogueId} will be shown.");
            ShowDialogue(entry);
        }
        else
        {
            Debug.LogWarning($"[DialogueManager] Dialogue ID not found: {dialogueId}");
        }
    }

    // gets called when we have some dialogue to show.
    private void ShowDialogue(DialogueEntry entry)
    {
        // safety check
        if (entry == null || entry.lines == null || entry.lines.Count == 0)
        {
            Debug.LogWarning("[DialogueManager] Received null or empty DialogueEntry.");
            return;
        }

        if (dialogueCanvas) dialogueCanvas.SetActive(true);

        currentEntry = entry;
        currentLines = entry.lines;
        currentLineIndex = 0;

        isTyping = false;
        isFullyTyped = false;

        DisplayNextLine();
    }

    // displays the next line.
    private void DisplayNextLine()
    {
        int lineCount = GetLineCount(currentLines[currentLineIndex]);
        AdjustDialogueUI(lineCount);

        dialogueText.text = "";
        typingCoroutine = StartCoroutine(TypeLine(currentLines[currentLineIndex]));
    }

    // get how many lines on screen will the currentline need.
    private int GetLineCount(string line)
    {
        dialogueText.text = line;
        dialogueText.ForceMeshUpdate();
        return dialogueText.textInfo.lineCount;
    }

    // adjust the dialogue UI based on the number of lines that will be on the screen. 
    private void AdjustDialogueUI(int lineCount)
    {
        if (lineCount <= 0) return;

        // base and new size for the dialogue UI panel.
        float UIBaseHeight = 72f;
        float UInewHeight = UIBaseHeight * lineCount;
        // update the size of dialogue UI panel.
        panelTransform.sizeDelta = new Vector2(panelTransform.sizeDelta.x, UInewHeight);

        // base and new padding values of the dialogue text that is showing inside the panel.
        float constPadding = 100f;
        float newPadding = 18f + ((lineCount - 1) * 12f);
        // update the size and padding of the dialogue text.
        textTransform.sizeDelta = new Vector2(textTransform.sizeDelta.x, UInewHeight);
        dialogueText.margin = new Vector4(constPadding, newPadding, constPadding, newPadding);

        // the (constant) X and Y (old and new) coordinates of the left and right avatar and speaker (text).
        float constXAvatar = 175f;
        float constXSpeaker = 375f;

        float baseYAvatarSpeaker = 85f;
        float newYAvatarSpeaker = baseYAvatarSpeaker + ((lineCount - 1) * 68f);

        // update the location of the avatars.
        leftAvatarTransform.anchoredPosition = new Vector3(constXAvatar, newYAvatarSpeaker, 0f);
        rightAvatarTransform.anchoredPosition = new Vector3(-constXAvatar, newYAvatarSpeaker, 0f);

        // update the location of the speakers (texts).
        leftSpeakerTextTransform.anchoredPosition = new Vector3(constXSpeaker, newYAvatarSpeaker, 0f);
        rightSpeakerTextTransform.anchoredPosition = new Vector3(-constXSpeaker, newYAvatarSpeaker, 0f);
    }

    // typing animation for the dialogue text.
    private IEnumerator TypeLine(string line)
    {
        isTyping = true;
        isFullyTyped = false;

        dialogueText.text = "";

        // simulate the "animation" by showing each character one after another with a "delay".
        foreach (char c in line)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        isFullyTyped = true;
    }

    // depending on the state of the dialogue, clicks perform different actions.
    private void OnClickPerformed(InputAction.CallbackContext context)
    {
        // if click still blocked - disregard the click.
        if (clickBlocked) return;

        // safety check.
        if (currentLines == null || currentLines.Count == 0 || !dialogueCanvas.activeSelf)
            return;

        // if the text is typing when we click.
        if (isTyping)
        {
            // stop the typing animation.
            StopCoroutine(typingCoroutine);
            // show the full text.
            dialogueText.text = currentLines[currentLineIndex];

            isTyping = false;
            isFullyTyped = true;

            // since we registered the click - block the possibly accidental next ones.
            StartCoroutine(ClickCooldown());
            return;
        }

        // if the text is shown fully.
        if (isFullyTyped)
        {
            currentLineIndex++;

            // if there are more lines to be shown -> show the next one.
            if (currentLineIndex < currentLines.Count)
            {
                DisplayNextLine();
            }
            // else finish this dialogue.
            else
            {
                EndDialogue();
            }

            // click registered -> block next ones.
            StartCoroutine(ClickCooldown());
        }
    }

    // prevention against accidental multi-clicks.
    private IEnumerator ClickCooldown()
    {
        clickBlocked = true;
        // 0.3 second delay.
        yield return new WaitForSeconds(0.3f);
        clickBlocked = false;
    }

    // gets called at the end of a dialogue.
    private void EndDialogue()
    {
        Debug.Log($"[DialogueManager] Ending dialogue: {currentEntry?.id}");

        // disable dialogue panel.
        dialogueCanvas.SetActive(false);

        string id = currentEntry.id;

        if (controller)
        {
            currentEntry = null;
            currentLines = null;

            Debug.Log($"[DialogueManager] Calling controller.OnDialogueComplete({id})");
            controller.OnDialogueComplete(id);
        }
        else
        {
            Debug.Log($"[DialogueManager] Controller is null.)");
            currentEntry = null;
            currentLines = null;
        }

    }
}
