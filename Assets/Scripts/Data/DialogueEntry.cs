using System;
using System.Collections.Generic;

[Serializable]
public class DialogueEntry
{
    public string id;
    public string speaker;
    public string speakerSide;
    public List<string> lines;
}
