using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls the VillaDiningRoom scene flow:
/// - Shows the right entry dialogue depending on progress
/// - Responds to dialogue completions
/// - Unlocks vocabulary at the right moments
/// </summary>
public class VillaDiningRoomController : SceneFlowController
{
    #region Unity Lifecycle

    /// <summary>
    /// Base setup
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

        Debug.Log($"[VillaDiningRoomController] Dialogue completed: {id}");

        string nextDialogueId = null;

        // Intro dialogue
        if (id == "villadiningroom.intro.one") nextDialogueId = "villadiningroom.aunt.one";
        else if (id == "villadiningroom.aunt.one")
        {
            nextDialogueId = "villadiningroom.aunt.two";
            TranslationManager.Instance.UnlockWords("ráda tě poznávám", "třemi", "doma");
        }

        // "villadiningroom.aunt.two" is the last dialogue of the scene

        // --- Interactable: Tea + FIB ---
        else if (id == "interactable.villadiningroom.tea.one")
        {
            nextDialogueId = "interactable.villadiningroom.tea.two";
            TranslationManager.Instance.UnlockWords("čaj");
        }
        else if (id == "interactable.villadiningroom.tea.two")
            nextDialogueId = "interactable.villadiningroom.tea.three";
        else if (id == "interactable.villadiningroom.tea.three")
        {
            nextDialogueId = "interactable.villadiningroom.tea.four";
            TranslationManager.Instance.UnlockWords("ne", "limonádu", "kávu", "džus");
        }
        // FIB
        else if (id == "interactable.villadiningroom.tea.four" ||
                 id == "interactable.villadiningroom.tea.fib_wrong1") ShowFIBTea();
        else if (id == "interactable.villadiningroom.tea.fib_correct1" ||
                 id == "interactable.villadiningroom.tea.fib_failed1")
            nextDialogueId = "interactable.villadiningroom.tea.final";


        // --- Interactable: Pictures + Quiz ---
        else if (id == "interactable.villadiningroom.pictures.one")
        {
            nextDialogueId = "interactable.villadiningroom.pictures.two";
            TranslationManager.Instance.UnlockWords("sestra", "táta", "děda", "fotografie");
        }
        else if (id == "interactable.villadiningroom.pictures.two")
            nextDialogueId = "interactable.villadiningroom.pictures.three";
        // Quiz
        else if (id == "interactable.villadiningroom.pictures.three" ||
         id == "interactable.villadiningroom.pictures.q_wrong1") ShowPicturesQuiz();
        else if (id == "interactable.villadiningroom.pictures.q_correct1")
        {
            nextDialogueId = "interactable.villadiningroom.pictures.final";
            TranslationManager.Instance.UnlockWords("bratr", "strýc");
        }

        // --- Interactable: Table ---
        else if (id == "interactable.villadiningroom.table.one")
        {
            nextDialogueId = "interactable.villadiningroom.table.two";
            TranslationManager.Instance.UnlockWords("pojď", "židle", "městečka", "město", "posaď");
        }
        else if (id == "interactable.villadiningroom.table.two")
            nextDialogueId = "interactable.villadiningroom.table.three";
        else if (id == "interactable.villadiningroom.table.three")
        {
            nextDialogueId = "interactable.villadiningroom.table.four";
            TranslationManager.Instance.UnlockWords("vaří");
        }
        else if (id == "interactable.villadiningroom.table.four")
            nextDialogueId = "interactable.villadiningroom.table.five";
        else if (id == "interactable.villadiningroom.table.five")
            nextDialogueId = "interactable.villadiningroom.table.six";
        else if (id == "interactable.villadiningroom.table.six")
        {
            nextDialogueId = "interactable.villadiningroom.table.seven";
            TranslationManager.Instance.UnlockWords("děkuji", "ne");
        }
        else if (id == "interactable.villadiningroom.table.seven")
            nextDialogueId = "interactable.villadiningroom.table.final";

        // --- Interactable: Drawer ---
        else if (id == "interactable.villadiningroom.drawer.one")
        {
            nextDialogueId = "interactable.villadiningroom.drawer.final";
            TranslationManager.Instance.UnlockWords("fotografie", "přítel", "babička");
        }

        // --- Interactable: Glass Display + Quiz ---
        else if (id == "interactable.villadiningroom.glassdisplay.one")
        {
            TranslationManager.Instance.UnlockWords("ano");
            nextDialogueId = "interactable.villadiningroom.glassdisplay.two";
        }
        // Quiz
        else if (id == "interactable.villadiningroom.glassdisplay.two" ||
         id == "interactable.villadiningroom.glassdisplay.q_wrong1") ShowGlassDisplayQuiz();
        else if (id == "interactable.villadiningroom.glassdisplay.q_correct1")
            nextDialogueId = "interactable.villadiningroom.glassdisplay.final";

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

        // If the full Intro wasn't finished yet, start there
        if (!state.completedDialogues.Contains("villadiningroom.aunt.two"))
        {
            nextDialogueId = "villadiningroom.intro.one";
            TranslationManager.Instance.UnlockWords("jídelna", "pojď");
        }

        // Go to the next dialogue if we set one
        if (!string.IsNullOrEmpty(nextDialogueId))
            DialogueManager.Instance.ShowDialogue(nextDialogueId);
    }

    #endregion

    #region Private Helpers (Quiz)

    private void ShowFIBTea()
    {
        string[] answers =
        {
            "Water", // answer
            "water" // answer lowercase
        };

        DialogueManager.Instance.ShowDialogue("interactable.villadiningroom.tea.fill_in_blank", null, answers);
    }

    private void ShowPicturesQuiz()
    {
        string[] answers =
        {
            "brácha & strejc",
            "broha & scrýc",
            "bratr & strýc" // correct
        };

        DialogueManager.Instance.ShowDialogue("interactable.villadiningroom.pictures.quiz", null, answers);
    }

    private void ShowGlassDisplayQuiz()
    {
        string[] answers =
        {
            "necklace", // correct
            "tie",
            "lanyard"
        };

        DialogueManager.Instance.ShowDialogue("interactable.villadiningroom.glassdisplay.quiz", null, answers);
    }

    #endregion
}
