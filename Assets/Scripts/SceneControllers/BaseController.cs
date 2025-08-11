using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls the Base scene flow:
/// - Shows the right entry dialogue depending on progress
/// - Responds to dialogue completions
/// - Unlocks vocabulary at the right moments
/// </summary>
public class BaseController : SceneFlowController
{
    #region Unity Lifecycle

    /// <summary>
    /// Base setup + show the correct entry dialogue for this scene.
    /// </summary>
    protected override void Start()
    {
        base.Start();

        ShowSceneEntryDialogue(GameManager.Instance.CurrentState);
    }

    #endregion

    #region Dialogue Callbacks

    /// <summary>
    /// Called by DialogueManager when a dialogue finishes.
    /// Decides the next dialogue and unlocks words where needed.
    /// </summary>
    public override void OnDialogueComplete(string id)
    {
        base.OnDialogueComplete(id);

        Debug.Log($"[BaseController] Dialogue completed: {id}");

        string nextDialogueId = null;

        // Intro
        if (id == "base.intro.one")
        {
            if (backgroundImage) backgroundImage.SetActive(true);
            nextDialogueId = "base.arrival.one";
        }

        // Second block
        else if (id == "base.arrival.one") nextDialogueId = "base.letterman.one";
        else if (id == "base.letterman.one")
        {
            nextDialogueId = "base.letterman.two";
            UnlockWords("pan"); // Mr.
        }
        else if (id == "base.letterman.two") nextDialogueId = "base.letterman.three";
        else if (id == "base.letterman.three")
        {
            nextDialogueId = "base.letterman.four";
            UnlockWords("tady"); // here
        }
        else if (id == "base.letterman.four") nextDialogueId = "base.letterman.five";

        // Quiz entry / retry loop
        else if (id == "base.letterman.five" || id == "base.letterman.q_wrong1") ShowLettermanQuiz();

        // Quiz correct branch
        else if (id == "base.letterman.q_correct1")
        {
            nextDialogueId = "base.letterman.q_correct2";
            // Unlock base keys (forms are covered as well automatically)
            UnlockWords("dopis", "tento", "je", "pro", "vás");
        }
        else if (id == "base.letterman.q_correct2")
        {
            nextDialogueId = "base.journal.one";
            UIManager.Instance.UnlockJournal();
        }

        // Third block
        else if (id == "base.journal.one") nextDialogueId = "base.letter.one";
        else if (id == "base.letter.one") nextDialogueId = "base.letter.two";
        else if (id == "base.letter.two") nextDialogueId = "base.letter.three";
        else if (id == "base.letter.three") nextDialogueId = "base.letter.four";
        else if (id == "base.letter.four") nextDialogueId = "base.letter.five";
        else if (id == "base.letter.five")
        {
            nextDialogueId = "base.letter.six";
            UnlockWords("milý");
        }
        else if (id == "base.letter.six") nextDialogueId = "base.letter.seven"; // "čteš" guess
        else if (id == "base.letter.seven")
        {
            nextDialogueId = "base.letter.eight";
            UnlockWords("já", "a", "tvoje", "máma", "jsme", "let", "jedna", "rodina");
        }
        else if (id == "base.letter.eight")
        {
            nextDialogueId = "base.letter.nine";
            UnlockWords("tobě", "rodinný"); // "tajemnství", "babička" and "říct" guess
        }
        else if (id == "base.letter.nine")
        {
            nextDialogueId = "base.letter.ten";
            UnlockWords("ty", "pravda"); // "člověk" guess
        }
        else if (id == "base.letter.ten")
        {
            nextDialogueId = "base.letter.eleven";
            UnlockWords("o", "vila"); // "mě" guess
        }
        else if (id == "base.letter.eleven")
        {
            nextDialogueId = "base.letter.twelve";
            UnlockWords("s", "láska", "teta");
        }
        else if (id == "base.letter.twelve") nextDialogueId = "base.letter.thirteen";
        else if (id == "base.letter.thirteen")
        {
            nextDialogueId = "base.location.one";
            UIManager.Instance.UnlockMapAndHighlight();
        }

        // "base.location.one" is the last dialogue of the scene.

        // Interactable dialogues
        else if (id == "interactable.base.fountain.one")
            nextDialogueId = "interactable.base.fountain.final";

        // Go to the next dialogue if we set one
        if (!string.IsNullOrEmpty(nextDialogueId))
            DialogueManager.Instance.ShowDialogue(nextDialogueId);
    }

    #endregion

    #region Entry / Resume

    /// <summary>
    /// Chooses which entry dialogue to show when entering the scene, based on what the player already finished.
    /// </summary>
    public override void ShowSceneEntryDialogue(GameState state)
    {
        base.ShowSceneEntryDialogue(state);

        // New game: show intro with hidden background
        if (!state.completedDialogues.Contains("base.intro.one"))
        {
            if (backgroundImage) backgroundImage.SetActive(false);
            DialogueManager.Instance.ShowDialogue("base.intro.one");
            return;
        }

        // If intro was done before, background can be visible now
        if (backgroundImage) backgroundImage.SetActive(true);

        string nextDialogueId = null;

        // If the second block wasn't finished yet, continue there
        if (!state.completedDialogues.Contains("base.letterman.q_correct1"))
            nextDialogueId = "base.arrival.one";

        // If the quiz correct branch wasn't finished yet, jump to it
        else if (!state.completedDialogues.Contains("base.letter.one"))
            nextDialogueId = "base.letterman.q_correct1";

        // Otherwise continue the third block
        else if (!state.completedDialogues.Contains("base.location.one"))
            nextDialogueId = "base.letter.one";

        // Go to the next dialogue if we set one
        if (!string.IsNullOrEmpty(nextDialogueId))
            DialogueManager.Instance.ShowDialogue(nextDialogueId);
    }

    #endregion

    #region Private Helpers (Quiz + Unlocks)

    /// <summary>
    /// Shows the Letterman multiple-choice quiz with 4 answers. Correct answer 2 (index 1).
    /// </summary>
    private void ShowLettermanQuiz()
    {
        string[] answers =
        {
            "Sorry. That's not your letter.",
            "Here. This letter is for you.",
            "Please. This note says you need help.",
            "There. The post office is right over there."
        };

        DialogueManager.Instance.ShowDialogue("base.letterman.quiz", null, answers);
    }

    /// <summary>
    /// Adds one or more words to the save's unlocked list and refreshes TranslationManager so links/underlines and popups are up-to-date immediately.
    /// Use base keys when you can (forms are handled automatically).
    /// </summary>
    private void UnlockWords(params string[] keysOrForms)
    {
        var state = GameManager.Instance?.CurrentState;
        if (state == null || keysOrForms == null || keysOrForms.Length == 0) return;

        foreach (var token in keysOrForms)
        {
            if (string.IsNullOrWhiteSpace(token)) continue;
            state.UnlockForm(token); // Accepts either a base key or any form
        }

        // Push changes into TranslationManager so the current line reflects unlocks
        TranslationManager.Instance.SyncUnlocksFrom(state);
    }

    #endregion
}
