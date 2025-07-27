using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// Controls the visual representation of a save slot in the UI. 
/// Displays player name, slot number, save time, and scene image, and updates the image color on hover when a save exists.
/// </summary>
public class SaveSlotVisualizer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    #region Fields

    [HideInInspector] public bool hasSave = false;
    [HideInInspector] public GameState gameStateCopy = null;

    [Header("Visual Targets")]
    [SerializeField] private Image sceneImage;
    [SerializeField] private TextMeshProUGUI playerText;
    [SerializeField] private TextMeshProUGUI slotText;
    [SerializeField] private TextMeshProUGUI timeText;

    private readonly Color emptySceneImageColor = new Color32(255, 255, 255, 0);
    private readonly Color defaultSceneImageColor = new Color32(255, 255, 255, 155);
    private readonly Color hoverSceneImageColor = new Color32(255, 255, 255, 255);

    #endregion
    #region Public Methods

    /// <summary>
    /// Sets the visual data for this slot based on whether a game state exists.
    /// </summary>
    /// <param name="slotNumber">The slot number to display.</param>
    /// <param name="state">The game state to associate with this slot, or <c>null</c> if empty.</param>
    public void SetSlotData(int slotNumber, GameState state)
    {
        gameStateCopy = state;

        if (state == null)
        {
            hasSave = false;
            SetEmptySlot(slotNumber);
        }
        else
        {
            hasSave = true;
            SetFilledSlot(slotNumber);
        }
    }

    /// <summary>
    /// Updates the slot visuals to represent an empty save slot.
    /// </summary>
    /// <param name="saveSlot">The slot number to display.</param>
    public void SetEmptySlot(int saveSlot)
    {
        if (sceneImage)
        {
            sceneImage.sprite = null;
            sceneImage.color = emptySceneImageColor;
        }

        if (playerText) playerText.text = "<EMPTY>";
        if (slotText) slotText.text = saveSlot.ToString();
        if (timeText) timeText.text = "-/-/- -:-";
    }

    /// <summary>
    /// Updates the slot visuals to represent a filled save slot.
    /// Loads the appropriate scene sprite and displays player name, slot number, and save time.
    /// </summary>
    /// <param name="saveSlot">The slot number to display.</param>
    public void SetFilledSlot(int saveSlot)
    {
        if (sceneImage)
        {
            Sprite sceneSprite = Resources.Load<Sprite>($"Sprites/Enviroments/{gameStateCopy.currentScene}");
            sceneImage.sprite = sceneSprite;
            sceneImage.color = defaultSceneImageColor;
        }

        if (playerText) playerText.text = gameStateCopy.playerName;
        if (slotText) slotText.text = saveSlot.ToString();
        if (timeText) timeText.text = gameStateCopy.lastSavedTime;
    }

    #endregion
    #region Event Handlers / Callbacks

    /// <summary>
    /// Called when the pointer enters the slot area. 
    /// Changes the scene image color to the hover color if a save exists.
    /// </summary>
    /// <param name="eventData">The pointer event data.</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hasSave) sceneImage.color = hoverSceneImageColor;
    }

    /// <summary>
    /// Called when the pointer exits the slot area. 
    /// Resets the scene image color to the default color if a save exists.
    /// </summary>
    /// <param name="eventData">The pointer event data.</param>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (hasSave) sceneImage.color = defaultSceneImageColor;
    }

    #endregion
}
