using System;
using System.Collections.Generic;

/// <summary>
/// One dictionary entry (by base Key). 
/// Example: Key="dopis", Forms={"dopis","dopise"}.
/// "Guess" is optional. "Unlocked" is set from GameState.
/// </summary>
[Serializable]
public class TranslationEntry
{
    public string Key;             // key, e.g., "dopis"
    public string Class;           // "noun", "verb", ...
    public string Journal;         // e.g., "dopis (m • sg.)"
    public string Translation;     // final translation
    public HashSet<string> Forms;  // all forms (lowercase)
    public string Guess;           // optional pre-unlock meaning (null if not present in JSON)
    public bool Unlocked;          // runtime paremeter - set from GameState

    public bool ContainsForm(string lowercaseForm)
        => Forms != null && Forms.Contains(lowercaseForm);
}
