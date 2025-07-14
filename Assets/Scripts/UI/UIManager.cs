using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Panel Button Pairs")]
    [SerializeField] private List<ButtonPanelPair> buttonPanelPairs = new();

    [Header("Highlight Button")]
    [SerializeField] private Button highlightButton;

    [Header("All Panels (for exclusivity)")]
    [SerializeField] private List<GameObject> allPanels = new();

    private bool isInteractable = false;

    void Start() { }

    void Awake()
    {
        // safety check, if single instance already exists.
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        // persist across scenes.
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // TODO disable/don't show all UI button popup panels at the start.
        foreach (var panel in allPanels)
        {
            if (panel != null)
                panel.SetActive(false);
        }

        // add click behavior to all UI buttons.
        foreach (var pair in buttonPanelPairs)
        {
            if (pair.button != null && pair.panel != null)
            {
                pair.button.onClick.AddListener(() => TogglePanel(pair.panel));
                if (!allPanels.Contains(pair.panel))
                    allPanels.Add(pair.panel);
            }
        }

        highlightButton?.onClick.AddListener(() => HighlightInteractables());

        SetInteractable(isInteractable);
    }

    // enables and disables UI button panels (exclusively).
    private void TogglePanel(GameObject panel)
    {
        if (!panel) return;
        // else
        foreach (var p in allPanels)
        {
            if (p != null)
                // only toggle target panel.
                p.SetActive(p == panel && !panel.activeSelf);
        }
    }

    // TODO
    private void HighlightInteractables()
    {
    }

    // enables or disables all UI buttons.
    public void SetInteractable(bool state)
    {
        isInteractable = state;

        foreach (var pair in buttonPanelPairs)
        {
            if (pair.button != null) pair.button.interactable = state;
        }
        if (highlightButton) highlightButton.interactable = state;
    }

    // returns true if UI is currently interactable.
    public bool IsInteractable()
    {
        return isInteractable;
    }
}
