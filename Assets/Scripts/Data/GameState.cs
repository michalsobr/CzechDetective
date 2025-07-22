using System;
using System.Collections.Generic;

public class GameState
{
    public string playerName;
    public string currentScene;
    public string lastSavedTime;
    public int currentSaveSlot;

    // fully interacted object IDs.
    public List<string> completedInteractables = new();
    // fully interacted dialogue IDs.
    public List<string> completedDialogues = new();
    // player's asnwers/attempts per quiz/puzzle ID.
    public Dictionary<string, List<string>> puzzleAttempts = new();

    // new game save file (used after choosing a player name).
    public static GameState NewGame(string playerName, string startScene)
    {
        return new GameState
        {
            playerName = playerName,
            currentScene = startScene,
            lastSavedTime = DateTime.Now.ToString("d/M/yy HH:mm")
        };
    }

    // adds every unique completed dialogue's id if it isn't on the list yet.
    public void MarkDialogueComplete(string id)
    {
        if (!completedDialogues.Contains(id)) completedDialogues.Add(id);
    }

    // adds every unique completed interaction on an object id if it isn't on the list yet.
    public void MarkInteractableComplete(string id)
    {
        if (!completedInteractables.Contains(id)) completedInteractables.Add(id);
    }

    // adds every unique puzzle attempt.
    public void MarkPuzzleComplete(string id, string attempt)
    {
        if (!puzzleAttempts.ContainsKey(id)) puzzleAttempts[id] = new List<string>();

        if (!puzzleAttempts[id].Contains(attempt)) puzzleAttempts[id].Add(attempt);
    }

}
