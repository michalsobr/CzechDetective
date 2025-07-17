using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // singleton.
    public static UIManager Instance { get; private set; }

    [Header("Canvas Button Pairs")]
    [SerializeField] private List<UIButtonGroup> buttonGroups = new();

    [Header("Highlight Button")]
    [SerializeField] private Button highlightButton;

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

        /*
        // TODO disable/don't show all UI button popup canvases at the start.
        foreach (var canvas in allCanvases)
        {
            if (canvas != null)
                canvas.SetActive(false);
        }

        // add click behavior to all UI buttons.
        foreach (var pair in buttonCanvasPairs)
        {
            if (pair.button != null && pair.canvas != null)
            {
                pair.button.onClick.AddListener(() => ShowPopupCanvas(pair.canvas));
                if (!allCanvases.Contains(pair.canvas))
                    allCanvases.Add(pair.canvas);
            }
        }
        */

        foreach (var group in buttonGroups)
        {
            if (group.button != null && group.canvas != null)
            {
                group.canvas.SetActive(false);

                group.button.onClick.AddListener(() => ShowPopupCanvas(group));

                group.popupScript = group.canvas.GetComponent<PopupCanvas>();
                if (group.popupScript == null)
                {
                    Debug.LogWarning($"{group.canvas.name} is missing a PopupCanvas script.");
                }
            }
        }

        highlightButton?.onClick.AddListener(() => HighlightInteractables());

        SetInteractable(isInteractable);
    }

    // enables and disables UI button canvas.
    public void ShowPopupCanvas(UIButtonGroup target)
    {
        target.popupScript.Open();
        // SetInteractable(false);
    }

    private void HighlightInteractables()
    {
    }

    // enables or disables all UI buttons.
    public void SetInteractable(bool state)
    {
        isInteractable = state;

        foreach (var group in buttonGroups)
        {
            if (group.button != null) group.button.interactable = state;
        }
        if (highlightButton) highlightButton.interactable = state;
    }

    // returns true if UI is currently interactable.
    public bool IsInteractable()
    {
        return isInteractable;
    }
}
