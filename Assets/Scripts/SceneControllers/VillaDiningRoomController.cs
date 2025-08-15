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

        Debug.Log($"[VillaDiningRoomController] Dialogue completed: {id}");

        string nextDialogueId = null;

        // TODO

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
        if (!state.completedDialogues.Contains("villadiningroom.intro.one"))
            nextDialogueId = "villadiningroom.intro.one";

        // Go to the next dialogue if we set one
        if (!string.IsNullOrEmpty(nextDialogueId))
            DialogueManager.Instance.ShowDialogue(nextDialogueId);
    }

    #endregion

    #region Private Helpers (Quiz)

    // TODO

    #endregion
}
