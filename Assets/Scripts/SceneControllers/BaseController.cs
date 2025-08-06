using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    protected override void Start()
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

        // Intro.
        if (id == "base.intro.one")
        {
            backgroundImage.SetActive(true);
            dialogueIDToLoad = "base.arrival.one";
        }

        // Second block.
        else if (id == "base.arrival.one") dialogueIDToLoad = "base.letterman.one";
        else if (id == "base.letterman.one") dialogueIDToLoad = "base.letterman.two";
        else if (id == "base.letterman.two") dialogueIDToLoad = "base.letterman.three";
        else if (id == "base.letterman.three") dialogueIDToLoad = "base.letterman.four";
        else if (id == "base.letterman.four") dialogueIDToLoad = "base.letterman.five";

        // Quiz.
        else if (id == "base.letterman.five") ShowLettermanQuiz();
        // Quiz loop.
        else if (id == "base.letterman.q_wrong1") ShowLettermanQuiz();

        // Quiz correct.
        else if (id == "base.letterman.q_correct1") dialogueIDToLoad = "base.letterman.q_correct2";
        else if (id == "base.letterman.q_correct2") dialogueIDToLoad = "base.journal.one";

        // Third block.
        else if (id == "base.journal.one") dialogueIDToLoad = "base.letter.one";
        else if (id == "base.letter.one") dialogueIDToLoad = "base.letter.two";
        else if (id == "base.letter.two") dialogueIDToLoad = "base.letter.three";
        else if (id == "base.letter.three") dialogueIDToLoad = "base.letter.four";
        else if (id == "base.letter.four") dialogueIDToLoad = "base.letter.five";
        else if (id == "base.letter.five") dialogueIDToLoad = "base.letter.six";
        else if (id == "base.letter.six") dialogueIDToLoad = "base.letter.seven";
        else if (id == "base.letter.seven") dialogueIDToLoad = "base.location_change.one";

        // "base.location_change.one" is the last dialogue of the scene.

        // Chained interactable dialogues.
        else if (id == "interactable.base.fountain.one") dialogueIDToLoad = "interactable.base.fountain.final";

        // Show the next dialogue, if there is one that needs to be shown.
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

        // New game/save - show intro dialogue with the background image hidden.
        if (!state.completedDialogues.Contains("base.intro.one"))
        {
            if (backgroundImage) backgroundImage.SetActive(false);
            DialogueManager.Instance.ShowDialogue("base.intro.one");
            return;
        }
        // Background image is visible.
        else if (backgroundImage) backgroundImage.SetActive(true);

        // Trigger the "checkpointed" dialogue based on which dialogue was last completed.
        // Second block wasn't finished.
        if (!state.completedDialogues.Contains("base.letterman.q_correct1")) dialogueIDToLoad = "base.arrival.one";

        // Correct quiz dialogue wasn't finished.
        else if (!state.completedDialogues.Contains("base.letter.one")) dialogueIDToLoad = "base.letterman.q_correct1";

        // Third block wasn't finished.
        else if (!state.completedDialogues.Contains("base.location_change.one")) dialogueIDToLoad = "base.letter.one";

        // Show an entry dialogue, if there is one that needs to be shown.
        if (dialogueIDToLoad != null) DialogueManager.Instance.ShowDialogue(dialogueIDToLoad);
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
        DialogueManager.Instance.ShowDialogue("base.letterman.quiz", null, answers);
    }

    #endregion

}
