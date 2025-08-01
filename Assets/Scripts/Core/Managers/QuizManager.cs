using TMPro;
using UnityEngine;

/// <summary>
/// Manages quiz answers that are displayed as clickable links in TMP text.
/// Handles storing answers, checking previous attempts, and processing clicks.
/// </summary>
public class QuizManager : MonoBehaviour
{
    public static QuizManager Instance;

    [SerializeField] private TextMeshProUGUI dialogueText;

    private string currentDialogueId;
    private string questionText;
    private string[] currentAnswers;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>
    /// Prepares a quiz by storing its answers and associated dialogue ID.
    /// </summary>
    public void SetupQuiz(string dialogueId, string[] answers)
    {
        currentDialogueId = dialogueId;
        currentAnswers = answers;

        // Store the already processed line (with links)
        string processed = DialogueManager.Instance.CurrentLineText;
        processed = TranslationManager.Instance.AutoLinkText(processed);
        processed = DialogueManager.Instance.AddUnderlineToLinks(processed);

        questionText = processed;
    }

    /// <summary>
    /// Generates a formatted string with clickable links for all answers.
    /// </summary>
    public string GetFormattedAnswers()
    {
        if (currentAnswers == null || currentAnswers.Length == 0) return "";

        string result = "";
        for (int i = 0; i < currentAnswers.Length; i++)
        {
            bool alreadyTried =
                GameManager.Instance.CurrentState.puzzleAttempts.ContainsKey(currentDialogueId) &&
                GameManager.Instance.CurrentState.puzzleAttempts[currentDialogueId].Contains(currentAnswers[i]);

            string colorTag = alreadyTried ? "<color=#7F0000>" : "<color=#4B4B4B>";
            result += $"\n<link=\"Answer_{i}\">{colorTag}{currentAnswers[i]}</color></link>";
        }
        return result;
    }

    /// <summary>
    /// Highlights or unhighlights a specific quiz answer in the dialogue text.
    /// </summary>
    /// <param name="answerId">The ID of the answer (from the <link> tag).</param>
    /// <param name="highlighted">True to highlight, false to reset.</param>
    public void UpdateAnswerHighlight(string answerId, bool highlighted)
    {
        if (currentAnswers == null || currentAnswers.Length == 0) return;

        string rebuiltAnswers = "";

        for (int i = 0; i < currentAnswers.Length; i++)
        {
            string id = $"Answer_{i}";

            // Check if this answer was already tried
            bool alreadyTried =
                GameManager.Instance.CurrentState.puzzleAttempts.ContainsKey(currentDialogueId) &&
                GameManager.Instance.CurrentState.puzzleAttempts[currentDialogueId].Contains(currentAnswers[i]);

            // Base color 
            string baseColor = alreadyTried ? "#7F0000" : "#4B4B4B";

            // If this is the hovered answer
            string color = (id == answerId && highlighted) ? "#000000" : baseColor;

            rebuiltAnswers += $"\n<link=\"{id}\"><color={color}>{currentAnswers[i]}</color></link>";
        }

        // Rebuild final text: question + answers
        dialogueText.text = questionText + rebuiltAnswers;
    }

    /// <summary>
    /// Called when an answer link is clicked.
    /// </summary>
    public void OnAnswerClicked(string linkId)
    {
        if (currentAnswers == null) return;

        // Parse "Answer_X" to get index
        if (!linkId.StartsWith("Answer_")) return;
        int index = int.Parse(linkId.Replace("Answer_", ""));

        string answer = currentAnswers[index];

        // Record attempt
        GameManager.Instance.CurrentState.MarkPuzzleComplete(currentDialogueId, answer);

        // Pick next dialogue
        string nextDialogue = GetNextDialogueId(answer);
        ClearQuiz();
        DialogueManager.Instance.ShowDialogue(nextDialogue);
    }

    private string GetNextDialogueId(string answer)
    {
        if (currentDialogueId == "base.letterman.quiz")
        {
            if (answer == currentAnswers[1]) currentDialogueId = "base.letterman.q_correct1";
            else currentDialogueId = "base.letterman.q_wrong1";

            /*
            return answer switch
            {
                "Sorry. That's not your letter." => "base.letterman.q_wrong1",
                "Here. This letter is for you." => "base.letterman.q_correct1",
                "Please. This note says you need help." => "base.letterman.q_wrong1",
                "There. The post office is over there." => "base.letterman.q_wrong1",
                _ => currentDialogueId
            };
            */
        }
        return currentDialogueId;
    }

    private void ClearQuiz()
    {
        currentAnswers = null;
        currentDialogueId = questionText = null;
    }
}
