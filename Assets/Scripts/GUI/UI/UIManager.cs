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
    private bool isPopupOpen = false;

    // runs only once - the first time the script is enabled and active in the scene.
    private void Start() { }

    // runs immediately when the script is loaded (before the first frame) - even if the GameObject is disabled.
    private void Awake()
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

        foreach (var group in buttonGroups)
        {
            if (group.button && group.canvas)
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

    // shows a specific popup canvas base on which button was pressed.
    public void ShowPopupCanvas(UIButtonGroup target)
    {
        target.popupScript.Open();
        isPopupOpen = true;
        SetInteractable(false);
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

    public bool IsPopupOpen()
    {
        return isPopupOpen;
    }

    public void ClosePopup()
    {
        isPopupOpen = false;
    }
}
