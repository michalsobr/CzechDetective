using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shows a simple list of learned words (the player's journal).
/// Two modes:
///  • Chronological — order the player actually unlocked words
///  • Alphabetical — A→Z by the journal label
/// Paged output with Up/Down buttons and a toggle to switch modes.
/// </summary>
public class JournalPopup : PopupWindow
{
    #region Fields

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI contentText;
    [SerializeField] private Button upButton;
    [SerializeField] private Button downButton;
    [SerializeField] private Button toggleButton;
    [SerializeField] private Image toggleIcon;

    [Header("Icons")]
    [SerializeField] private Sprite alphabeticalIcon;  // “A”
    [SerializeField] private Sprite chronologicalIcon; // clock

    [Header("Settings")]
    [Min(1)][SerializeField] private int entriesPerPage = 8;

    private enum SortMode { Chronological, Alphabetical }

    private SortMode currentMode = SortMode.Chronological;
    private int currentPage = 0;

    // Cached lists for the two modes
    private readonly List<TranslationEntry> chronological = new();
    private readonly List<TranslationEntry> alphabetical = new();

    #endregion

    #region Unity Lifecycle

    protected override void OnEnable()
    {
        base.OnEnable();

        if (upButton) upButton.onClick.AddListener(OnPageUp);
        if (downButton) downButton.onClick.AddListener(OnPageDown);
        if (toggleButton) toggleButton.onClick.AddListener(OnToggleMode);
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (upButton) upButton.onClick.RemoveListener(OnPageUp);
        if (downButton) downButton.onClick.RemoveListener(OnPageDown);
        if (toggleButton) toggleButton.onClick.RemoveListener(OnToggleMode);
    }

    public override void Open()
    {
        base.Open();

        currentMode = SortMode.Chronological;
        currentPage = 0;

        RebuildLists();
        UpdateToggleIcon();
        RenderPage(0);
    }

    public override void Close()
    {
        base.Close();
    }

    #endregion

    #region Buttons

    private void OnPageUp()
    {
        if (currentPage <= 0) return;
        currentPage--;
        RenderPage(currentPage);
    }

    private void OnPageDown()
    {
        var list = ActiveList();
        int maxPage = Mathf.Max(0, (list.Count - 1) / entriesPerPage);
        if (currentPage >= maxPage) return;

        currentPage++;
        RenderPage(currentPage);
    }

    private void OnToggleMode()
    {
        currentMode = (currentMode == SortMode.Chronological)
            ? SortMode.Alphabetical
            : SortMode.Chronological;

        currentPage = 0;
        UpdateToggleIcon();
        RenderPage(0);
    }

    #endregion

    #region Build Lists

    /// <summary>
    /// Creates the two cached lists:
    /// - Chronological — based on the order of words in GameState.unlockedWords
    /// - Alphabetical  — A -> Z by the journal label
    /// </summary>
    private void RebuildLists()
    {
        chronological.Clear();
        alphabetical.Clear();

        // Build chronological list by going though unlockedWords and putting forms to their base keys.
        var seenKeys = new HashSet<string>();

        foreach (string token in GameManager.Instance.CurrentState.unlockedWords)
        {
            if (!TranslationManager.Instance.TryResolveToKey(token, out string baseKey)) continue;
            if (seenKeys.Contains(baseKey)) continue;

            if (TranslationManager.Instance.TryGetEntry(baseKey, out var entry))
            {
                seenKeys.Add(baseKey);
                chronological.Add(entry);
            }
        }

        // Build alphabetical as a sorted copy by the human-facing “journal head”.
        alphabetical.AddRange(chronological.OrderBy(entry => GetSortKey(entry)));
    }

    private static string GetSortKey(TranslationEntry entry)
    {
        // Prefer Journal label if present; else the Key (fallback default).
        if (!string.IsNullOrWhiteSpace(entry.Journal)) return entry.Journal;
        return entry.Key;
    }

    #endregion

    #region Render

    private List<TranslationEntry> ActiveList() =>
        currentMode == SortMode.Chronological ? chronological : alphabetical;

    private void RenderPage(int page)
    {
        var list = ActiveList();

        if (list.Count == 0) return;

        int start = Mathf.Clamp(page * entriesPerPage, 0, list.Count - 1);
        int end = Mathf.Min(start + entriesPerPage, list.Count);

        var stringBuilder = new StringBuilder(list.Count * 24);
        for (int i = start; i < end; i++)
            stringBuilder.AppendLine(FormatRow(list[i]));

        if (contentText) contentText.text = stringBuilder.ToString().TrimEnd();

        if (upButton) upButton.interactable = (page > 0);
        if (downButton)
        {
            int maxPage = Mathf.Max(0, (list.Count - 1) / entriesPerPage);
            downButton.interactable = (page < maxPage);
        }
    }

    private static string FormatRow(TranslationEntry entry)
    {
        return $"{entry.Journal}[{entry.Class}]  -  {entry.Translation}";
    }

    private void UpdateToggleIcon()
    {
        if (!toggleIcon) return;
        toggleIcon.sprite = (currentMode == SortMode.Chronological) ? alphabeticalIcon : chronologicalIcon;
    }

    #endregion

    #region Public

    /// <summary>
    /// Call this if new words get unlocked while the popup is open.
    /// </summary>
    public void RefreshNow()
    {
        RebuildLists();
        RenderPage(currentPage);
    }

    #endregion
}
