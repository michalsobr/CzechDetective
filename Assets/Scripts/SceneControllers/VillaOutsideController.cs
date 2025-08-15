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

        // TODO
        if (id == "villaoutside.intro.one" || id == "villaoutside.teta.fib_wrong1") ShowFIBTeta();
        else if (id == "villaoutside.teta.fib_correct1" || id == "villaoutside.teta.fib_failed1")
            nextDialogueId = "villaoutside.teta.fib_correct2";

        // "" is the last dialogue of the scene.

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

        // TODO
        if (!state.completedDialogues.Contains("villaoutside.intro.one"))
            nextDialogueId = "villaoutside.intro.one";

        else if (!state.completedDialogues.Contains("villaoutside.teta.fib_correct1"))
            ShowFIBTeta();
        else if (!state.completedDialogues.Contains("villaoutside.teta.fib_correct2"))
            nextDialogueId = "villaoutside.teta.fib_correct1";

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

    #endregion
}
