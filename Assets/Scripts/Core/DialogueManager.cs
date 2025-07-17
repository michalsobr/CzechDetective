using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using UnityEngine.UI;
using System.IO;

public class DialogueManager : MonoBehaviour
{
    // singleton.
    public static DialogueManager Instance { get; private set; }

    [Header("Dialogue Panel")]
    [SerializeField] private GameObject dialogueCanvas;
    [SerializeField] private RectTransform panelTransform;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private RectTransform textTransform;

    [Header("Dialogue Speakers")]
    [SerializeField] private GameObject leftSpeaker;
    [SerializeField] private GameObject rightSpeaker;
    private RectTransform leftSpeakerTransform;
    private RectTransform rightSpeakerTransform;
    private Image leftSpeakerImage;
    private Image rightSpeakerImage;
    private TextMeshProUGUI leftSpeakerText;
    private TextMeshProUGUI rightSpeakerText;

    [Header("Typewriter Settings")]
    [SerializeField] private float typingSpeed;

    private SceneFlowController controller;
    private DialogueEntry currentEntry;
    private List<string> currentLines;
    private int currentLineCount;
    private string currentSpeaker;
    private string currentSpeakerSide;
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

        leftSpeakerTransform = leftSpeaker.GetComponent<RectTransform>();
        rightSpeakerTransform = rightSpeaker.GetComponent<RectTransform>();

        leftSpeakerImage = leftSpeaker.GetComponent<Image>();
        rightSpeakerImage = rightSpeaker.GetComponent<Image>();

        leftSpeakerText = leftSpeaker.GetComponentInChildren<TextMeshProUGUI>();
        rightSpeakerText = rightSpeaker.GetComponentInChildren<TextMeshProUGUI>();

        // default and if inactive - don't show dialogue canvas.
        if (dialogueCanvas) dialogueCanvas.SetActive(false);
    }

    private void Start()
    {
        // get the scene-specific controller.
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

    // update the scene-specific controller if a scene was changed.
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        controller = FindFirstObjectByType<SceneFlowController>();
    }

    // show dialogue by ID.
    public void ShowDialogue(string dialogueId)
    {
        DialogueEntry entry = DialogueDatabase.Instance.Get(dialogueId);

        // if dialogue with that id exist.
        if (entry != null) ShowDialogue(entry);
        else Debug.LogWarning($"[DialogueManager] Dialogue ID not found: {dialogueId}");
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
        currentSpeaker = entry.speaker;
        currentSpeakerSide = entry.speakerSide;
        currentLineIndex = 0;
        isTyping = false;
        isFullyTyped = false;

        DisplayNextLine();
    }

    // displays the current dialogue line.
    private void DisplayNextLine()
    {
        currentLineCount = GetLineCount(currentLines[currentLineIndex]);
        AdjustDialogueUI();

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
    private void AdjustDialogueUI()
    {
        // safety check.
        if (currentLineCount <= 0) return;

        UpdateDialoguePanel();
        UpdateDialogueSpeakers();
    }

    private void UpdateDialoguePanel()
    {
        // base and new size for the dialogue UI panel.
        float UIBaseHeight = 72f;
        float UInewHeight = UIBaseHeight * currentLineCount;
        // update the size of dialogue UI panel.
        panelTransform.sizeDelta = new Vector2(panelTransform.sizeDelta.x, UInewHeight);

        // base and new padding values of the dialogue text that is showing inside the panel.
        float constPadding = 100f;
        float newPadding = 18f + ((currentLineCount - 1) * 12f);
        // update the size and padding of the dialogue text.
        textTransform.sizeDelta = new Vector2(textTransform.sizeDelta.x, UInewHeight);
        dialogueText.margin = new Vector4(constPadding, newPadding, constPadding, newPadding);
    }

    private void UpdateDialogueSpeakers()
    {
        // hide both by default.
        leftSpeaker.SetActive(false);
        rightSpeaker.SetActive(false);

        if (currentSpeakerSide == "left")
        {
            leftSpeaker.SetActive(true);
            leftSpeakerImage.sprite = LoadSpeakerSprite();
            leftSpeakerText.text = UpdateSpeakerText();
        }
        if (currentSpeakerSide == "right")
        {
            rightSpeaker.SetActive(true);
            rightSpeakerImage.sprite = LoadSpeakerSprite();
            rightSpeakerText.text = UpdateSpeakerText();
        }
        UpdateSpeakerPosition();
    }

    private Sprite LoadSpeakerSprite()
    {
        Sprite sprite = Resources.Load<Sprite>($"Sprites/Characters/{currentSpeaker}");

        if (sprite == null) Debug.LogWarning($"[DialogueManager] Portrait not found for: {currentSpeaker}");

        return sprite;
    }

    private string UpdateSpeakerText()
    {
        string speakerName = "";

        // currentSpeaker could be detective_init, detective_happy, detective_angry, etc.
        if (currentSpeaker.Contains("detective")) speakerName = "Tobiáš";
        else if (currentSpeaker == "letterman") speakerName = "???";
        else if (currentSpeaker == "aunt_unknown") speakerName = "Růžena Nováková";
        else if (currentSpeaker == "aunt_known") speakerName = "teta Růžena";

        return string.IsNullOrEmpty(speakerName) ? currentSpeaker : speakerName;
    }

    private void UpdateSpeakerPosition()
    {
        // the (constant) X and (old and new) Y coordinates of the speaker.
        float constXSpeaker = 175f;
        float baseYSpeaker = 85f;
        float newYSpeaker = baseYSpeaker + ((currentLineCount - 1) * 68f);

        // update the location of the speaker.
        if (currentSpeakerSide == "left") leftSpeakerTransform.anchoredPosition = new Vector3(constXSpeaker, newYSpeaker, 0f);
        if (currentSpeakerSide == "right") rightSpeakerTransform.anchoredPosition = new Vector3(-constXSpeaker, newYSpeaker, 0f);
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
        if (currentLines == null || currentLines.Count == 0 || !dialogueCanvas.activeSelf) return;

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

    // gets called at the end of a dialogue to reset values.
    private void EndDialogue()
    {
        dialogueCanvas.SetActive(false);

        string id = currentEntry.id;

        currentEntry = null;
        currentLines = null;

        if (controller)
        {
            Debug.Log($"[DialogueManager] Calling {controller.name}.OnDialogueComplete({id})");
            controller.OnDialogueComplete(id);
        }
        else Debug.Log($"[DialogueManager] Controller is null.)");
    }

}
