using System.Text;
using TMPro;
using UnityEngine;

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

    [Header("Colors")]
    [SerializeField] private string colorNotTried = "#4B4B4B";
    [SerializeField] private string colorTried = "#7F0000";
    [SerializeField] private string colorHover = "#000000";

    private string activeDialogueId;
    private string activeQuestion;      // Question part (processed)
    private string[] answers;           // Raw answer texts in order

    #endregion

    #region Unity

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
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
        this.activeQuestion = processedQuestion;
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
    private string ResolveNextDialogueId(string dialogueId, int pickedIndex)
    {
        if (dialogueId == "base.letterman.quiz")
            return pickedIndex == 1 ? "base.letterman.q_correct1" : "base.letterman.q_wrong1";

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
    }

    #endregion
}