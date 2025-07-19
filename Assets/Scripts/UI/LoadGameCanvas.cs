using System;
using System.IO;
using TMPro;
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
    // [SerializeField] private GameObject confirmLoadCanvas;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button navigationButton;
    [SerializeField] private Image navigationButtonImage;
    [SerializeField] private Sprite upIcon;
    [SerializeField] private Sprite downIcon;

    [Header("Save Slot Visualizers")]
    [SerializeField] private SaveSlotVisualizer[] slots;

    private int currentPage = 0;

    private void Start()
    {
        if (closeButton) closeButton.onClick.AddListener(HideLoadGamePopup);
        if (navigationButton) navigationButton.onClick.AddListener(TogglePage);
    }

    private void TogglePage()
    {
        currentPage = 1 - currentPage;

        // swap icon
        if (navigationButtonImage) navigationButtonImage.sprite = currentPage == 0 ? downIcon : upIcon;

        RefreshAllSlots();
    }

    private void RefreshAllSlots()
    {
        int baseIndex = currentPage * 4;

        for (int i = 0; i < slots.Length; i++)
        {
            int slotNumber = baseIndex + i + 1;

            GameState state = SaveManager.Instance.Load(slotNumber);
            if (state == null)
            {
                slots[i].SetEmptySlot(slotNumber);
                continue;
            }
            else
            {
                string formattedTime = FormatSaveTime(state.lastSavedTime);
                Sprite sceneSprite = Resources.Load<Sprite>($"Sprites/Enviroments/{state.currentScene}");

                if (sceneSprite == null) Debug.LogWarning($"[LoadGameCanvas] Scene sprite not found for scene '{state.currentScene}'");

                slots[i].SetSlotData(slotNumber, state.playerName, formattedTime, sceneSprite);
            }
        }
    }



    /*
    int baseIndex = currentPage * 4;

    for (int i = 0; i < slots.Length; i++)
    {
        int slotNumber = baseIndex + i + 1;

        string path = Path.Combine(Application.persistentDataPath, $"save_slot{slotNumber}.json");

        if (File.Exists(path))
        {
            GameState state = SaveManager.Instance.Load(slotNumber);

            if (state == null)
            {
                Debug.LogWarning($"[LoadGameCanvas] Could not deserialize GameState from file: {path}");
                slots[i].SetEmptySlot(slotNumber);
                continue;
            }

            string formattedTime = FormatSaveTime(state.lastSavedTime);
            Sprite sceneSprite = Resources.Load<Sprite>($"Sprites/Enviroments/{state.currentScene}");
            if (sceneSprite == null)
            {
                Debug.LogWarning($"[LoadGameCanvas] Scene sprite not found for scene '{state.currentScene}'.");
            }
        }
        else slots[i].SetEmptySlot(slotNumber);
    }
}
*/


    /*
    string json = File.ReadAllText(path);
    GameState state = JsonUtility.FromJson<GameState>(json);

    string formattedTime = FormatSaveTime(state.lastSavedTime);
    Sprite sceneSprite = Resources.Load<Sprite>($"Sprites/Enviroments/{state.currentScene}");

    slots[i].SetSlotData(slotNumber, state.playerName, formattedTime, sceneSprite);
}
else slots[i].SetEmptySlot(slotNumber);
}
}
*/

    private string FormatSaveTime(string rawTime)
    {
        if (DateTime.TryParse(rawTime, out DateTime time)) return time.ToString("d/M/yy HH:mm");
        // else
        return "-/-/- -:-";
    }

    public void ShowLoadGamePopup()
    {
        gameObject.SetActive(true);

        SetMainMenuInteractable(false);

        currentPage = 0;
        RefreshAllSlots();
    }

    public void HideLoadGamePopup()
    {
        SetMainMenuInteractable(true);

        gameObject.SetActive(false);
    }

    private void SetMainMenuInteractable(bool state)
    {
        if (startText) startText.interactable = state;
        if (loadText) loadText.interactable = state;
        if (exitText) exitText.interactable = state;
    }
}
