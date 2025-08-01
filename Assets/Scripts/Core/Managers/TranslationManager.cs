using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

public class TranslationManager : MonoBehaviour
{
    public static TranslationManager Instance { get; private set; }

    private Dictionary<string, string> idToTranslation = new();   // "dopis" → "<b>letter</b>"
    private Dictionary<string, string> baseWordToId = new();      // "dopis" → "dopis"
    private Dictionary<string, string> guessWordToId = new();     // "dopis" → "dopis_guess"

    private HashSet<string> unlockedWords = new(); // Tracks which words now show correct translations

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadTranslations();
        UnlockIntro();
    }

    private void LoadTranslations()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "translations.json");
        string json = File.ReadAllText(path);

        JObject root = JObject.Parse(json);

        foreach (var group in root)
        {
            foreach (var entry in (JObject)group.Value)
            {
                string id = entry.Key;
                string word = entry.Value["word"].ToString().ToLower(); // normalize to lowercase
                string translation = entry.Value["translation"].ToString();

                idToTranslation[id] = translation;

                if (id.EndsWith("_guess"))
                    guessWordToId[word] = id;
                else
                    baseWordToId[word] = id;
            }
        }
    }

    public string AutoLinkText(string line)
    {
        foreach (var pair in baseWordToId)
        {
            string word = pair.Key;
            string baseId = pair.Value;

            string id = null;

            if (unlockedWords.Contains(baseId))
                id = baseId;
            else if (guessWordToId.ContainsKey(word))
                id = guessWordToId[word];

            if (id == null)
                continue; // skip linking completely

            line = Regex.Replace(
                line,
                $@"\b{word}\b",
                match => $"<link=\"{id}\">{match.Value}</link>",  //use original matched text
                RegexOptions.IgnoreCase
            );
        }
        return line;
    }


    public string GetTranslationById(string id)
    {
        return idToTranslation.TryGetValue(id, out string translation) ? translation : "";
    }

    public void UnlockWord(string word)
    {
        if (baseWordToId.TryGetValue(word.ToLower(), out string id))
            unlockedWords.Add(id);
    }

    public void UnlockIntro()
    {
        // TODO add Mr - Pan
        UnlockWord("skip");
    }

    public void UnlockLettermanQuiz()
    {
        // TODO add Mr - Pan
        UnlockWord("tady");
        UnlockWord("dopis");
        UnlockWord("tento");
        UnlockWord("je");
        UnlockWord("pro");
        UnlockWord("vás");
    }
}
