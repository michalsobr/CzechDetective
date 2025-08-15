using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Builds and manages quiz answers that appear as clickable TMP <link> items
/// under the current dialogue line. Also handles hover highlight and clicks.
/// </summary>
public class QuizManager : MonoBehaviour
{
    #region Fields

    public static QuizManager Instance;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Header("Fill-in-the-blank Quiz UI")]
    [SerializeField] private GameObject input;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private GameObject submit;
    [SerializeField] private Button submitButton;

    [Header("Colors")]
    [SerializeField] private string colorNotTried = "#4B4B4B";
    [SerializeField] private string colorTried = "#7F0000";
    [SerializeField] private string colorHover = "#000000";

    private string activeDialogueId;
    private string activeQuestion;      // Question part (processed)
    private string[] answers;           // Raw answer texts in order

    private List<string> correctAnswers;
    private int wrongAttempts;

    #endregion

    #region Unity

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        submitButton.onClick.AddListener(OnSubmit);

        submit.SetActive(false);
        input.SetActive(false);
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Remember which quiz is active and prepare the processed question text.
    /// Call this before appending answers to the dialogue.
    /// </summary>
    public void SetupQuiz(string dialogueId, string[] answerOptions, string processedQuestion)
    {
        activeDialogueId = dialogueId;
        answers = answerOptions;
        activeQuestion = processedQuestion;
    }

    public void SetupFillInBlankQuiz(string dialogueId, string[] acceptedAnswers)
    {
        if (activeDialogueId != dialogueId) wrongAttempts = 0;
        
        activeDialogueId = dialogueId;
        correctAnswers = acceptedAnswers.Select(a => a.ToLower().Trim()).ToList();

        submit.SetActive(true);
        submitButton.interactable = true;

        input.SetActive(true);
        inputField.text = string.Empty;
        inputField.Select();
        inputField.ActivateInputField();
    }

    /// <summary>
    /// Build the markup for all answers. DialogueManager will append this to the question.
    /// </summary>
    public string BuildAnswersMarkup()
    {
        if (answers == null || answers.Length == 0) return "";

        var stringBuilder = new StringBuilder(answers.Length * 32);

        for (int i = 0; i < answers.Length; i++)
        {
            string id = $"a:{i}";
            string color = IsAlreadyTried(i) ? colorTried : colorNotTried;

            stringBuilder.Append('\n');
            stringBuilder.Append($"<link=\"{id}\"><color={color}>{answers[i]}</color></link>");
        }

        return stringBuilder.ToString();
    }

    /// <summary>
    /// Rebuilds the answers with a hover highlight for the given answer ID.
    /// Call with highlighted=true on hover enter, and highlighted=false on hover exit.
    /// </summary>
    public void UpdateAnswerHighlight(string answerId, bool highlighted)
    {
        if (answers == null || answers.Length == 0 || string.IsNullOrEmpty(activeQuestion) || dialogueText == null)
            return;

        var stringBuilder = new StringBuilder(answers.Length * 32);

        for (int i = 0; i < answers.Length; i++)
        {
            string id = $"a:{i}";
            bool isThis = id == answerId;

            string baseColor = IsAlreadyTried(i) ? colorTried : colorNotTried;
            string color = (highlighted && isThis) ? colorHover : baseColor;

            stringBuilder.Append('\n');
            stringBuilder.Append($"<link=\"{id}\"><color={color}>{answers[i]}</color></link>");
        }

        // Question + rebuilt answers
        dialogueText.text = activeQuestion + stringBuilder.ToString();
        dialogueText.ForceMeshUpdate(); // ensure links are hoverable
    }

    /// <summary>
    /// Handle a click on an answer link ("a:i").
    /// Records the attempt and tells DialogueManager which dialogue to show next.
    /// </summary>
    public void OnAnswerClicked(string linkId)
    {
        if (answers == null || answers.Length == 0 || string.IsNullOrEmpty(activeDialogueId))
            return;

        if (!linkId.StartsWith("a:")) return;

        if (!int.TryParse(linkId.Substring("a:".Length), out int index)) return;
        if (index < 0 || index >= answers.Length) return;

        // Save the attempt in GameState
        GameManager.Instance.CurrentState.MarkPuzzleComplete(activeDialogueId, index.ToString());

        // Pick next dialogue based on the picked answer
        string nextDialogueId = ResolveNextDialogueId(activeDialogueId, index);

        // Clear quiz state and continue
        Clear();
        DialogueManager.Instance.ShowDialogue(nextDialogueId);
    }

    public void OnSubmit()
    {
        if (correctAnswers == null || correctAnswers.Count == 0 || string.IsNullOrEmpty(activeDialogueId)) return;

        submitButton.interactable = false;
        submit.SetActive(false);

        string playerInput = inputField.text.ToLower().Trim();

        input.SetActive(false);
        inputField.text = string.Empty;

        // Save the attempt in GameState
        GameManager.Instance.CurrentState.MarkPuzzleComplete(activeDialogueId, playerInput);

        // Pick next dialogue based on the picked answer
        string nextDialogueId = ResolveNextDialogueId(activeDialogueId, -1, playerInput);

        if (wrongAttempts == 3) Clear();

        DialogueManager.Instance.ShowDialogue(nextDialogueId);
    }

    #endregion

    #region Private Helpers

    /// <summary>
    /// Returns true if the player already tried this answer for this dialogue.
    /// </summary>
    private bool IsAlreadyTried(int answerIndex)
    {
        // Safety check.
        if (GameManager.Instance?.CurrentState == null) return false;
        if (string.IsNullOrEmpty(activeDialogueId)) return false;
        if (answerIndex < 0 || answerIndex >= (answers?.Length ?? 0)) return false;

        var state = GameManager.Instance.CurrentState;
        if (!state.puzzleAttempts.TryGetValue(activeDialogueId, out var list)) return false;

        return list.Contains(answerIndex.ToString());
    }

    /// <summary>
    /// Decide which dialogue to show next based on the active quiz, the picked answer text, or its index.
    /// </summary>
    private string ResolveNextDialogueId(string dialogueId, int pickedIndex, string playerInput = null)
    {
        // Quiz.
        if (dialogueId == "base.letterman.quiz")
            return pickedIndex == 1 ? "base.letterman.q_correct1" : "base.letterman.q_wrong1";

        // Fill-in-the-blank quiz.
        else if (dialogueId == "villaoutside.teta.fill_in_blank")
        {
            if (correctAnswers.Contains(playerInput))
            {
                wrongAttempts = 3;
                return "villaoutside.teta.fib_correct1";
            }
            else if (wrongAttempts < 2)
            {
                wrongAttempts++;
                return "villaoutside.teta.fib_wrong1";
            }
            else
            {
                wrongAttempts = 3;
                return "villaoutside.teta.fib_failed1";
            }
        }

        // Fallback default: stay on the same id.
        return dialogueId;
    }

    /// <summary>
    /// Remove quiz state so the next line is clean.
    /// </summary>
    private void Clear()
    {
        activeDialogueId = null;
        activeQuestion = null;
        answers = null;

        wrongAttempts = 0;
    }

    #endregion
}