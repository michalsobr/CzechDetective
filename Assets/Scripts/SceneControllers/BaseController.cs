using System;
using UnityEngine;

/// <summary>
/// Controls the logic for the Base scene, including entry behavior, dialogue progression, and new game initialization.
/// Inherits from <see cref="SceneFlowController"/> to reuse common scene flow functionality.
/// </summary>
public class BaseController : SceneFlowController
{
    #region Unity Lifecycle Methods

    /// <summary>
    /// Invoked on the first frame when the script is enabled and active.
    /// Calls the base setup and triggers the entry dialogue for the current scene state.
    /// </summary>
    public override void Start()
    {
        base.Start();
        ShowSceneEntryDialogue(GameManager.Instance.CurrentState);
    }

    #endregion
    #region Overrides

    /// <summary>
    /// Invoked by the <see cref="DialogueManager"/> when a dialogue is completed.
    /// Updates the game state and triggers the next relevant dialogue in the story flow.
    /// </summary>
    /// <param name="id">The ID of the completed dialogue.</param>
    public override void OnDialogueComplete(string id)
    {
        base.OnDialogueComplete(id);
        Debug.Log($"[BaseController] Dialogue completed: {id}");

        string dialogueIDToLoad = null;

        if (id == "base.intro.one") dialogueIDToLoad = "base.arrival.one";

        else if (id == "base.arrival.one") dialogueIDToLoad = "base.letterman.one";
        else if (id == "base.letterman.one") dialogueIDToLoad = "base.letterman.two";
        else if (id == "base.letterman.two") dialogueIDToLoad = "base.letterman.three";
        else if (id == "base.letterman.three") dialogueIDToLoad = "base.letterman.four";
        else if (id == "base.letterman.four") dialogueIDToLoad = "base.letterman.five";

        else if (id == "base.letterman.five") ShowLettermanQuiz();
        else if (id == "base.letterman.q_wrong1") ShowLettermanQuiz();

        else if (id == "base.letterman.q_correct1") dialogueIDToLoad = "base.letterman.q_correct2";

        else if (id == "base.letterman.q_correct2") dialogueIDToLoad = "base.journal.one";

        else if (id == "base.journal.one") dialogueIDToLoad = "base.letter.one";
        else if (id == "base.letter.one") dialogueIDToLoad = "base.letter.two";
        else if (id == "base.letter.two") dialogueIDToLoad = "base.letter.three";
        else if (id == "base.letter.three") dialogueIDToLoad = "base.letter.four";
        else if (id == "base.letter.four") dialogueIDToLoad = "base.letter.five";
        else if (id == "base.letter.five") dialogueIDToLoad = "base.letter.six";
        else if (id == "base.letter.six") dialogueIDToLoad = "base.letter.seven";

        else if (id == "base.letter.seven") dialogueIDToLoad = "base.location_change.one";

        if (dialogueIDToLoad != null) DialogueManager.Instance.ShowDialogue(dialogueIDToLoad);
    }

    /// <summary>
    /// Displays the correct entry dialogue for the current scene based on which dialogues have already been completed.
    /// </summary>
    /// <param name="state">The current <see cref="GameState"/>.</param>
    public override void ShowSceneEntryDialogue(GameState state)
    {
        base.ShowSceneEntryDialogue(state);

        string dialogueIDToLoad = null;

        if (!state.completedDialogues.Contains("base.intro.one")) dialogueIDToLoad = "base.intro.one";
        else if (!state.completedDialogues.Contains("base.location_change.one")) dialogueIDToLoad = "base.letterman.q_correct1";
        
        // TODO rewrite!
        /*
        else if (!state.completedDialogues.Contains("base.arrival.one")) ShowArrivalDialogue();
        else if (!state.completedDialogues.Contains("base.letterman.five")) ShowLettermanDialogue("one");
        else if (!state.completedDialogues.Contains("base.letterman.q_correct")) ShowLettermanQuiz();
        */

        if (dialogueIDToLoad != null) DialogueManager.Instance.ShowDialogue(dialogueIDToLoad);
    }

    #endregion
    #region Public Methods (Story Progression)

    /// <summary>
    /// Triggers the first intro dialogue.
    /// </summary>
    public void ShowIntroDialogue()
    {
        DialogueManager.Instance.ShowDialogue("base.intro.one");
    }

    #endregion
    #region Private Methods (Quizes)

    private void ShowLettermanQuiz()
    {
        string[] answers =
        {
        "Sorry. That's not your letter.",
        "Here. This letter is for you.",
        "Please. This note says you need help.",
        "There. The post office is right over there."
        };

        // Show base.letterman.quiz with answers
        DialogueManager.Instance.ShowDialogue("base.letterman.quiz", answers);
    }

    #endregion
}
