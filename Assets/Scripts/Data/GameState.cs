using System;
using System.Collections.Generic;

public class GameState
{
    public string playerName;
    public string currentScene;
    public string lastSavedTime;

    // fully interacted object IDs.
    public List<string> completedInteractables = new();
    // fully interacted dialogue IDs.
    public List<string> completedDialogues = new();
    // player's asnwers/attempts per quiz/puzzle ID.
    public Dictionary<string, List<string>> puzzleAttempts = new();

    // init save file after choosing a player name.
    public static GameState NewGame(string playerName, string startScene)
    {
        return new GameState
        {
            playerName = playerName,
            currentScene = startScene,
            lastSavedTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };
    }

    public void MarkDialogueComplete(string id)
    {
        if (!completedDialogues.Contains(id)) completedDialogues.Add(id);
    }

    public void MarkInteractableComplete(string id)
    {
        if (!completedInteractables.Contains(id)) completedInteractables.Add(id);
    }

}
