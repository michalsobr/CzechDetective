using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls the VillaOutside scene flow:
/// - Shows the right entry dialogue depending on progress
/// - Responds to dialogue completions
/// - Unlocks vocabulary at the right moments
/// </summary>
public class VillaOutsideController : SceneFlowController
{
    #region Unity Lifecycle

    /// <summary>
    /// Base setup + show the correct entry dialogue for this scene.
    /// </summary>
    protected override void Start()
    {
        base.Start();
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

        Debug.Log($"[VillaOutsideController] Dialogue completed: {id}");

        string nextDialogueId = null;

        // Intro + FIB Teta
        if (id == "villaoutside.intro.one" || id == "villaoutside.teta.fib_wrong1") ShowFIBTeta();
        else if (id == "villaoutside.teta.fib_correct1" || id == "villaoutside.teta.fib_failed1")
            nextDialogueId = "villaoutside.teta.fib_correct2";

        // Second block
        else if (id == "villaoutside.teta.fib_correct2") nextDialogueId = "villaoutside.jana.one";
        else if (id == "villaoutside.jana.one") nextDialogueId = "villaoutside.jana.two";
        else if (id == "villaoutside.jana.two") nextDialogueId = "villaoutside.jana.three";

        // Jana quiz
        else if (id == "villaoutside.jana.three" || id == "villaoutside.jana.q_wrong_hotel" || id == "villaoutside.jana.q_wrong_silence") ShowJanaQuiz();
        else if (id == "villaoutside.jana.q_correct1")
        {
            nextDialogueId = "villaoutside.jana.four";
            TranslationManager.Instance.UnlockWords("pojď", "paní", "služebná");
        }

        // Come Inside quiz
        else if (id == "villaoutside.jana.four" || id == "villaoutside.comeinside.q_wrong_clean" || id == "villaoutside.comeinside.q_wrong_cook2")
            ShowComeInsideQuiz();
        else if (id == "villaoutside.comeinside.q_wrong_cook1")
        {
            nextDialogueId = "villaoutside.comeinside.q_wrong_cook2";
            TranslationManager.Instance.UnlockWords("ne", "nevaří");
        }
        else if (id == "villaoutside.comeinside.q_correct1")
            nextDialogueId = "villaoutside.transition.one";

        // "villaoutside.transition.one" is the last dialogue of the scene

        // Transition to Dining Room
        else if (id == "villaoutside.transition.one") SceneManager.LoadScene("VillaDiningRoom");

        // --- Interactable: Broom ---
        else if (id == "interactable.villaoutside.broom.one")
            nextDialogueId = "interactable.villaoutside.broom.two";
        else if (id == "interactable.villaoutside.broom.two")
        {
            nextDialogueId = "interactable.villaoutside.broom.final";
            TranslationManager.Instance.UnlockWords("omlouvám se", "slyšet", "nad", "zvuky", "pracovna");
        }

        // --- Interactable: Flowers + FIB Levender ---
        else if (id == "interactable.villaoutside.flowers.one")
            nextDialogueId = "interactable.villaoutside.flowers.two";
        else if (id == "interactable.villaoutside.flowers.two" ||
                 id == "interactable.villaoutside.flowers.fib_wrong1") ShowFIBLavender();
        else if (id == "interactable.villaoutside.flowers.fib_correct1" ||
                 id == "interactable.villaoutside.flowers.fib_failed1")
        {
            nextDialogueId = "interactable.villaoutside.flowers.final";
            TranslationManager.Instance.UnlockWords("levandule", "fialová", "vůně");
        }

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

        string nextDialogueId = null;

        // If Intro wasn't finished yet, start there
        if (!state.completedDialogues.Contains("villaoutside.intro.one"))
            nextDialogueId = "villaoutside.intro.one";

        // If the FIB Teta was not answered correctly yet, continue from there
        else if (!state.completedDialogues.Contains("villaoutside.teta.fib_correct1"))
            ShowFIBTeta();

        // If the full post-correct quiz dialogue wasn't finished yet, continue from there
        else if (!state.completedDialogues.Contains("villaoutside.teta.fib_correct2"))
            nextDialogueId = "villaoutside.teta.fib_correct1";

        // If the second block wasn't finished yet, continue from there
        else if (!state.completedDialogues.Contains("villaoutside.jana.three"))
            nextDialogueId = "villaoutside.jana.one";

        // If the Jana quiz was not answered correctly yet, continue from there
        else if (!state.completedDialogues.Contains("villaoutside.jana.q_correct1"))
            ShowJanaQuiz();

        // If the full post-correct quiz dialogue wasn't finished yet, continue from there
        else if (!state.completedDialogues.Contains("villaoutside.jana.four"))
            nextDialogueId = "villaoutside.jana.q_correct1";

        // If the Come Inside quiz was not answered correctly yet, continue from there
        else if (!state.completedDialogues.Contains("villaoutside.comeinside.q_correct1"))
            ShowComeInsideQuiz();

        // Other continue to the last dialogue
        else if (!state.completedDialogues.Contains("villaoutside.transition.one"))
            nextDialogueId = "villaoutside.transition.one";

        // Go to the next dialogue if we set one
        if (!string.IsNullOrEmpty(nextDialogueId))
            DialogueManager.Instance.ShowDialogue(nextDialogueId);
    }

    #endregion

    #region Private Helpers (Quiz)

    private void ShowFIBTeta()
    {
        string[] answers =
        {
            "Teta", // answer
            "teta" // answer lowercase
        };

        DialogueManager.Instance.ShowDialogue("villaoutside.teta.fill_in_blank", null, answers);
    }

    private void ShowJanaQuiz()
    {
        string[] answers =
        {
            "(Awkward silence.)",
            "\"Hledám hotel.\"",
            "\"Dobrý den. Ano. Nemluvím česky.\"" // correct
        };

        DialogueManager.Instance.ShowDialogue("villaoutside.jana.quiz", null, answers);
    }

    private void ShowComeInsideQuiz()
    {
        string[] answers =
        {
            "cleans",
            "waits", // correct
            "cooks"
        };

        DialogueManager.Instance.ShowDialogue("villaoutside.comeinside.quiz", null, answers);
    }

    private void ShowFIBLavender()
    {
        string[] answers =
        {
            "levender", // answer
            "Levender" // answer lowercase
        };

        DialogueManager.Instance.ShowDialogue("interactable.villaoutside.flowers.fill_in_blank", null, answers);
    }

    #endregion
}
