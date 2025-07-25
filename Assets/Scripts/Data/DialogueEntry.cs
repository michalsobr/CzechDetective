using System;
using System.Collections.Generic;

/// <summary>
/// Represents a single dialogue entry containing an ID, speaker information, and a list of dialogue lines to be displayed in sequence.
/// </summary>
[Serializable]
public class DialogueEntry
{
    /// <summary>
    /// The unique identifier for this dialogue entry.
    /// </summary>
    public string id;

    /// <summary>
    /// The name or identifier of the speaker for this dialogue entry.
    /// </summary>
    public string speaker;

    /// <summary>
    /// The side of the dialogue UI ("left" or "right") where the speaker will appear.
    /// </summary>
    public string speakerSide;

    /// <summary>
    /// The list of dialogue lines to display for this entry.
    /// </summary>
    public List<string> lines;
}
