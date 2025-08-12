using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using UnityEngine;

/// <summary>
/// Loads Translations.json, keeps a fast lookup, syncs unlocks from GameState,
/// and wraps only <cz>...</cz> text into TMP <link> so hovers work.
/// Rules:
/// - Unlocked -> link + underline, popup shows final translation
/// - Locked + has a guess -> link + underline, popup shows guess
/// - Locked + no guess -> no link (no underline)
/// </summary>
public class TranslationManager : MonoBehaviour
{
    public static TranslationManager Instance { get; private set; }

    #region Fields

    // Key -> entry ("Key" is base word (e.g., "dopis"))
    private readonly Dictionary<string, TranslationEntry> entriesByKey = new();

    // Form (e.g., "dopise") -> Key ("dopis")
    private readonly Dictionary<string, string> formToKey = new();

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadTranslationsJson();

        // If a game state exists, sync the unlocked words/forms
        if (GameManager.Instance.IsGameLoaded) SyncUnlocksFrom(GameManager.Instance.CurrentState);
    }

    #endregion

    #region Load JSON
    private void LoadTranslationsJson()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "Translations.json");
        if (!File.Exists(path))
        {
            Debug.LogError($"[TranslationManager] File not found: {path}");
            return;
        }

        var root = JObject.Parse(File.ReadAllText(path)); // top-level: { "noun": {...}, "verb": ...

        foreach (var classPair in root)                         // e.g., classPair.Key = "noun"
        {
            string wordClass = classPair.Key;
            var bucket = (JObject)classPair.Value;

            foreach (var entryPair in bucket)                   // e.g., entryPair.Key = "dopis"
            {
                string key = entryPair.Key;
                var obj = (JObject)entryPair.Value;

                var words = (obj["words"] as JArray)?.Select(w => w.ToString())?.ToList() ?? new List<string>();

                var entry = new TranslationEntry
                {
                    Key = key,
                    Class = wordClass,
                    Journal = obj["journal"]?.ToString() ?? "",
                    Translation = obj["translation"]?.ToString() ?? "",
                    Guess = string.IsNullOrWhiteSpace(obj["guess"]?.ToString()) ? null : obj["guess"]!.ToString(),
                    Unlocked = false,
                    Forms = new HashSet<string>(words.Select(w => w.ToLowerInvariant()))
                };

                // Ensure the base Key is included among forms too, in case it wasn't included.
                entry.Forms.Add(key.ToLowerInvariant());

                entriesByKey[key] = entry;

                foreach (var form in entry.Forms)
                    formToKey[form] = key;
            }
        }

        Debug.Log($"[TranslationManager] Loaded {entriesByKey.Count} entries.");
    }

    #endregion

    #region Public Sync and Lookup Methods

    /// <summary>
    /// Adds one or more words to the save's unlocked list.
    /// Use base keys when you can (forms are handled automatically).
    /// </summary>
    public void UnlockWords(params string[] keysOrForms)
    {
        var state = GameManager.Instance.CurrentState;
        if (state == null || keysOrForms == null || keysOrForms.Length == 0) return;

        foreach (var token in keysOrForms)
        {
            if (string.IsNullOrWhiteSpace(token)) continue;
            state.UnlockForm(token); // Accepts either a base key or any form
        }

        // Push changes.
        SyncUnlocksFrom(state);
    }

    /// <summary>
    /// Apply unlocks from the current save. Works with either base keys or forms.
    /// </summary>
    public void SyncUnlocksFrom(GameState state)
    {
        if (state == null) return;

        // Reset
        foreach (var entry in entriesByKey.Values)
            entry.Unlocked = false;

        // Mark unlocked based on save data (case-insensitive)
        foreach (var token in state.unlockedWords)
        {
            string tokenLowerCase = token.ToLowerInvariant();

            // If it's a base Key, unlock it
            if (entriesByKey.TryGetValue(tokenLowerCase, out var byKey))
            {
                byKey.Unlocked = true;
                continue;
            }

            // If it's a form, map it to the base Key and then unlock it
            if (formToKey.TryGetValue(tokenLowerCase, out var baseKey) && entriesByKey.TryGetValue(baseKey, out var entry))
                entry.Unlocked = true;
        }
    }

    /// <summary>
    /// Build a simple popup label for a given base key. Returns false if there is nothing to show.
    /// </summary>
    public bool TryGetPopupLabel(string key, out string label)
    {
        label = null;
        if (!entriesByKey.TryGetValue(key, out var entry)) return false;

        if (entry.Unlocked && !string.IsNullOrEmpty(entry.Translation))
        {
            // Show final translation and class on a new line
            label = string.IsNullOrWhiteSpace(entry.Class)
                ? entry.Translation
                : $"{entry.Translation}\n[{entry.Class}]";
            return true;
        }

        // Show guess label if not unlocked but has a guess.
        if (!entry.Unlocked && !string.IsNullOrEmpty(entry.Guess))
        {
            label = $"{entry.Guess}?\n<b>( guess )</b>";
            return true;
        }

        return false;
    }

    #endregion

    #region Line Processing

    /// <summary>
    /// Only wrap inside <cz>...</cz>. After wrapping, we remove the <cz> tags.
    /// </summary>
    public string ProcessCzSegments(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return raw;

        string processed = Regex.Replace(raw, @"<cz>(.*?)</cz>", m =>
        {
            string inner = m.Groups[1].Value;
            return WrapCz(inner);
        }, RegexOptions.Singleline);

        // Strip tags after wrapping (since these tags are not functional - only present for code logic and functionality)
        return processed.Replace("<cz>", "").Replace("</cz>", "");
    }

    /// <summary>
    /// Add links to words we know. Underline only when locked and we have a guess.
    /// </summary>
    private string WrapCz(string czText)
    {
        var tokens = SplitKeepDelims(czText);
        var sb = new StringBuilder(czText.Length + 32);

        foreach (string surface in tokens)
        {
            string norm = Normalize(surface);

            if (string.IsNullOrEmpty(norm) || !TryFindEntry(norm, out string baseKey, out TranslationEntry entry))
            {
                sb.Append(surface);
                continue;
            }

            bool showFinal = entry.Unlocked && !string.IsNullOrEmpty(entry.Translation);
            bool showGuess = !entry.Unlocked && !string.IsNullOrEmpty(entry.Guess);

            if (!showFinal && !showGuess)
            {
                sb.Append(surface);
                continue;
            }

            bool underline = showFinal || showGuess;
            string open = underline ? $"<link=\"w:{baseKey}\"><u>" : $"<link=\"w:{baseKey}\">";
            string close = underline ? "</u></link>" : "</link>";

            sb.Append(open).Append(surface).Append(close);
        }

        return sb.ToString();
    }

    #endregion

    #region Private Helpers

    private bool TryFindEntry(string surfaceLower, out string baseKey, out TranslationEntry entry)
    {
        baseKey = null; entry = null;

        if (!formToKey.TryGetValue(surfaceLower, out var key)) return false;
        if (!entriesByKey.TryGetValue(key, out entry)) return false;

        baseKey = key;
        return true;
    }

    private static string Normalize(string token)
    {
        if (string.IsNullOrEmpty(token)) return token;
        token = token.Trim(' ', '\t', '\r', '\n', '.', ',', '!', '?', ':', ';', '„', '“', '"', '(', ')', '…', '—', '-', '\'', '’');
        return token.ToLowerInvariant();
    }

    /// <summary> Split into word vs non-word pieces and keep everything. </summary>
    private static List<string> SplitKeepDelims(string s)
    {
        var list = new List<string>(s.Length / 4 + 4);
        int i = 0;
        bool IsWord(char c) => char.IsLetter(c) || c == '\'' || c == '’';

        while (i < s.Length)
        {
            bool word = IsWord(s[i]);
            int start = i;
            i++;
            while (i < s.Length && IsWord(s[i]) == word) i++;
            list.Add(s.Substring(start, i - start));
        }
        return list;
    }

    #endregion
    #region Public Helpers

    // Resolve a word (key or any form) to the base key, e.g., "dopise" -> "dopis".
    public bool TryResolveToKey(string token, out string baseKey)
    {
        baseKey = null;
        if (string.IsNullOrEmpty(token)) return false;

        string t = token.ToLowerInvariant();
        if (entriesByKey.ContainsKey(t)) { baseKey = t; return true; }  // already a base key
        return formToKey.TryGetValue(t, out baseKey);                   // or a form
    }

    // Fetch a read-only copy of an entry by base key.
    public bool TryGetEntry(string baseKey, out TranslationEntry entry)
    {
        return entriesByKey.TryGetValue(baseKey, out entry);
    }

    #endregion
}
