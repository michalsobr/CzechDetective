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

        // TODO testing
        if (id == "villaoutside.intro.one")
        {
            nextDialogueId = "villaoutside.arrival.one";
            // TranslationManager.Instance.UnlockWords("pan"); // Mr.
        }
        // else if (id == "base.letterman.two") nextDialogueId = "base.letterman.three";

        // "" is the last dialogue of the scene.

        // Interactable dialogues
        else if (id == "interactable.villaoutside.fountain.one")
            nextDialogueId = "interactable.villaoutside.fountain.final";

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

        // TODO testing
        if (!state.completedDialogues.Contains("villaoutside.intro.one"))
            nextDialogueId = "villaoutside.intro.one";

        // Go to the next dialogue if we set one
        if (!string.IsNullOrEmpty(nextDialogueId))
            DialogueManager.Instance.ShowDialogue(nextDialogueId);
    }

    #endregion

    #region Private Helpers (Quiz)


    #endregion
}
