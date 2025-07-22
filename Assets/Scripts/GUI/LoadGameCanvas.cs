using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UI;

public class LoadGameCanvas : MonoBehaviour
{
    [Header("Main Menu Buttons")]
    [SerializeField] private MainMenuController startText;
    [SerializeField] private MainMenuController loadText;
    [SerializeField] private MainMenuController exitText;

    [Header("Popup Elements")]
    [SerializeField] private NoticePanel noticePanel;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button navigationButton;
    [SerializeField] private Image navigationButtonImage;
    [SerializeField] private Sprite upIcon;
    [SerializeField] private Sprite downIcon;

    [Header("Save Slot Groups")]
    [SerializeField] private List<SaveSlotGroup> saveSlotGroups = new();

    private GameObject loadGamePanel;
    private int currentPage = 0;

    // runs immediately when the script is loaded (before the first frame) - even if the GameObject is disabled.
        /// <summary>
    /// runs immediately when the script is loaded (before the first frame) - even if the GameObject is disabled - makes sure only a single instance of this object exists.
    /// </summary>
    private void Awake()
    {
        Transform loadGamePanelTransform = gameObject.transform.Find("LoadGamePanel");
        if (loadGamePanelTransform) loadGamePanel = loadGamePanelTransform.gameObject;

        if (closeButton) closeButton.onClick.AddListener(HideLoadGamePopup);
        if (navigationButton) navigationButton.onClick.AddListener(TogglePage);

        int slotNumber = 1;

        foreach (var group in saveSlotGroups)
        {
            int saveSlotNumber = slotNumber;
            if (group.saveSlot)
            {
                group.slotButton = group.saveSlot.GetComponent<Button>();
                group.slotVisualizer = group.saveSlot.GetComponent<SaveSlotVisualizer>();

                Transform deleteButtonTransform = group.saveSlot.transform.Find("DeleteButton");
                if (deleteButtonTransform)
                {
                    group.deleteSlotButton = deleteButtonTransform.gameObject;
                    group.deleteButton = group.deleteSlotButton.GetComponent<Button>();
                }

                Debug.Log($"Save slot number is {saveSlotNumber}.");
                group.slotButton.onClick.AddListener(() => OnNoticePanelOpen(true, saveSlotNumber, group.slotVisualizer.gameStateCopy));
                group.deleteButton.onClick.AddListener(() => OnNoticePanelOpen(false, saveSlotNumber, group.slotVisualizer.gameStateCopy));

                group.deleteSlotButton.SetActive(false);
            }
            slotNumber++;
        }
        gameObject.SetActive(false);
    }

    /*
                    GameState state = SaveManager.Instance.Load(saveSlotNumber);

                group.slotButton.onClick.AddListener(() =>
                noticePanel.ShowNoticePopup(group.slotVisualizer.stat

                group.deleteButton.onClick.AddListener(() => noticePanel.ShowNoticePopup(false, slotNumber, ));
                */

    // runs only once - the first time the script is enabled and active in the scene.
    private void Start() { }

    private void TogglePage()
    {
        currentPage = 1 - currentPage;

        UpdateNavigation();

        RefreshAllSlots();
    }

    /// <summary>
    /// swap navigation icon based on which page we're at.
    /// </summary>
    private void UpdateNavigation()
    {
        if (navigationButtonImage) navigationButtonImage.sprite = currentPage == 0 ? downIcon : upIcon;
    }

    public void RefreshAllSlots()
    {

        int slotNumber = (currentPage * 4) + 1;

        foreach (var group in saveSlotGroups)
        {
            GameState state = SaveManager.Instance.Load(slotNumber);
            group.slotVisualizer.SetSlotData(slotNumber, state);

            if (state == null)
            {
                group.slotButton.interactable = false;
                group.deleteSlotButton.SetActive(false);
            }
            else
            {
                group.slotButton.interactable = true;
                group.deleteSlotButton.SetActive(true);
            }

            slotNumber++;
        }
    }

    /*
    int slotNumber = (currentPage * 4) + 1;

    foreach (var group in saveSlotGroups)
    {
        GameState state = SaveManager.Instance.Load(slotNumber);
        if (state == null)
        {
            group.slotVisualizer.SetEmptySlot(slotNumber);
            group.slotButton.interactable = false;
            group.deleteSlotButton.SetActive(false);
        }
        else
        {
            string formattedTime = FormatSaveTime(state.lastSavedTime);
            Sprite sceneSprite = Resources.Load<Sprite>($"Sprites/Enviroments/{state.currentScene}");

            if (sceneSprite == null) Debug.LogWarning($"[LoadGameCanvas] Scene sprite not found for scene '{state.currentScene}'");

            group.slotVisualizer.SetSlotData(slotNumber, state.playerName, formattedTime, sceneSprite);
            group.slotButton.interactable = true;
            group.deleteSlotButton.SetActive(true);
        }
        slotNumber++;
    }
    */

    /*
    private string FormatSaveTime(string rawTime)
    {
        if (DateTime.TryParse(rawTime, out DateTime time)) return time.ToString("d/M/yy HH:mm");
        // else
        return "-/-/- -:-";
    }
    */

    public void ShowLoadGamePopup()
    {
        // show the canvas.
        gameObject.SetActive(true);

        // disable Main Menu "buttons".
        SetMainMenuInteractable(false);

        ShowLoadGamePanel();
    }

    public void ShowLoadGamePanel()
    {
        // show the panel.
        loadGamePanel.SetActive(true);

        // set the Load Game page to be the first and refresh/update the save slots.
        currentPage = 0;
        UpdateNavigation();
        RefreshAllSlots();
    }

    public void HideLoadGamePopup()
    {
        // make Main Menu "buttons" interactable again.
        SetMainMenuInteractable(true);

        // hide the canvas.
        gameObject.SetActive(false);
    }

    public void HideLoadGamePanel()
    {
        // hide the panel.
        loadGamePanel.SetActive(false);
    }

    private void SetMainMenuInteractable(bool state)
    {
        if (startText) startText.interactable = state;
        if (loadText) loadText.interactable = state;
        if (exitText) exitText.interactable = state;
    }

    private void OnNoticePanelOpen(bool isLoadAttempted, int saveSlot, GameState state)
    {
        HideLoadGamePanel();
        //SetSlotsInteractable(false);

        noticePanel.ShowNoticePopup(isLoadAttempted, (currentPage * 4) + saveSlot, state);
    }

    public void OnNoticePanelClosed()
    {
        ShowLoadGamePanel();
        //SetSlotsInteractable(true);
    }
    private void SetSlotsInteractable(bool interactable)
    {
        foreach (var group in saveSlotGroups)
        {
            if (group.slotButton) group.slotButton.interactable = interactable;
            if (group.deleteButton) group.deleteButton.interactable = interactable;
        }
    }
}
