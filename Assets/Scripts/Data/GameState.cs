using System;
using System.Collections.Generic;


/// <summary>
/// Represents the player's current game state, including progress, scene data, completed interactions, dialogue history, and puzzle attempts.
/// </summary>
public class GameState
{
    #region Fields
    /// <summary>
    /// The name of the player associated with this game state.
    /// </summary>
    public string playerName;

    /// <summary>
    /// The name of the currently active scene.
    /// </summary>
    public string currentScene;

    /// <summary>
    /// The timestamp of when this game state was last saved.
    /// </summary>
    public string lastSavedTime;

    /// <summary>
    /// The slot number in which this game state is stored.
    /// </summary>
    public int currentSaveSlot;

    /// <summary>
    /// A list of IDs representing interactable objects the player has fully interacted with.
    /// </summary>
    public List<string> completedInteractables = new();

    /// <summary>
    /// A list of IDs representing dialogue entries the player has completed.
    /// </summary>
    public List<string> completedDialogues = new();

    /// <summary>
    /// A dictionary storing the player's attempts for each puzzle or quiz.
    /// The key is the puzzle/quiz ID, and the value is a list of unique attempts.
    /// </summary>
    public Dictionary<string, List<string>> puzzleAttempts = new();

    #endregion
    #region Public Methods

    /// <summary>
    /// Creates a new <see cref="GameState"/> instance for a new game, initializing it with the player name, starting scene, and current timestamp.
    /// </summary>
    /// <param name="playerName">The name of the player starting a new game.</param>
    /// <returns>A new initialized <see cref="GameState"/>.</returns>
    public static GameState NewGame(string playerName)
    {
        return new GameState
        {
            playerName = playerName,
            currentScene = "Base",
            lastSavedTime = DateTime.Now.ToString("d/M/yy HH:mm")
        };
    }

    /// <summary>
    /// Marks a dialogue entry as completed by adding its ID to the list, if it has not already been added.
    /// </summary>
    /// <param name="id">The unique ID of the dialogue entry.</param>
    public void MarkDialogueComplete(string id)
    {
        if (!completedDialogues.Contains(id)) completedDialogues.Add(id);
    }

    /// <summary>
    /// Marks an interactable object as completed by adding its ID to the list, if it has not already been added.
    /// </summary>
    /// <param name="id">The unique ID of the interactable object.</param>
    public void MarkInteractableComplete(string id)
    {
        if (!completedInteractables.Contains(id)) completedInteractables.Add(id);
    }

    /// <summary>
    /// Records a puzzle attempt for the specified puzzle ID, ensuring each attempt is stored only once per puzzle.
    /// </summary>
    /// <param name="id">The unique ID of the puzzle or quiz.</param>
    /// <param name="attempt">The attempt to record.</param>
    public void MarkPuzzleComplete(string id, string attempt)
    {
        if (!puzzleAttempts.ContainsKey(id)) puzzleAttempts[id] = new List<string>();

        if (!puzzleAttempts[id].Contains(attempt)) puzzleAttempts[id].Add(attempt);
    }

    #endregion
}
