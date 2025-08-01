using System;
using System.Collections.Generic;

/// <summary>
/// Represents a single dialogue entry containing an ID, speaker information, and a list of dialogue lines to be displayed in sequence.
/// </summary>
[Serializable]
public class DialogueEntry
{
    /// <summary> The unique identifier for this dialogue entry. </summary>
    public string id;

    /// <summary> The name or identifier of the speaker on the left side for this dialogue entry. </summary>
    public string speakerLeft;

    /// <summary> The name or identifier of the speaker on the right side for this dialogue entry. </summary>
    public string speakerRight;

    /// <summary> The list of dialogue lines to display for this entry. </summary>
    public List<string> lines;
}
