using UnityEngine;

/// <summary>
/// Controls the logic for the Base scene, including scene entry behavior, dialogue progression, and new game initialization.
/// Inherits from <see cref="SceneFlowController"/> to use common scene flow functionality.
/// </summary>
public class BaseController : SceneFlowController
{
    #region Unity Lifecycle Methods

    /// <summary>
    /// Called only once, on the first frame when the script is enabled and active.
    /// Triggers the appropriate entry dialogue for the current scene state.
    /// </summary>
    public override void Start()
    {
        base.Start();
        ShowSceneEntryDialogue(GameManager.Instance.CurrentState);
    }

    #endregion
    #region Overrides

    /// <summary>
    /// Called by the <see cref="DialogueManager"/> when a dialogue is completed.
    /// Advances the story by triggering the next relevant dialogue.
    /// </summary>
    /// <param name="id">The ID of the completed dialogue.</param>
    public override void OnDialogueComplete(string id)
    {
        base.OnDialogueComplete(id);
        Debug.Log($"[BaseController] Dialogue completed: {id}");

        if (id == "base.intro.one") ShowArrivalDialogue();
        else if (id == "base.arrival.one") ShowLettermanDialogue("one");
        else if (id == "base.letterman.one") ShowLettermanDialogue("two");
        else if (id == "base.letterman.two") ShowLettermanDialogue("three");
        else if (id == "base.letterman.three") ShowLettermanDialogue("four");
        else if (id == "base.letterman.four") ShowLettermanDialogue("five");
    }

    /// <summary>
    /// Displays the appropriate entry dialogue for the current scene state based on which dialogues have already been completed.
    /// </summary>
    /// <param name="state">The current <see cref="GameState"/>.</param>
    public override void ShowSceneEntryDialogue(GameState state)
    {
        base.ShowSceneEntryDialogue(state);

        if (!state.completedDialogues.Contains("base.intro.one")) ShowIntroDialogue();
        else if (!state.completedDialogues.Contains("base.arrival.one")) ShowArrivalDialogue();
        else if (!state.completedDialogues.Contains("base.letterman.five")) ShowLettermanDialogue("one");
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

    /// <summary>
    /// Triggers the arrival dialogue and ensures the background image is visible.
    /// </summary>
    public void ShowArrivalDialogue()
    {
        backgroundImage.SetActive(true);
        DialogueManager.Instance.ShowDialogue("base.arrival.one");
    }

    /// <summary>
    /// Triggers one of the letterman dialogues based on the provided sequence number.
    /// </summary>
    /// <param name="sequenceNum">The sequence number for the letterman dialogue.</param>
    public void ShowLettermanDialogue(string sequenceNum)
    {
        DialogueManager.Instance.ShowDialogue($"base.letterman.{sequenceNum}");
    }

    #endregion
}
