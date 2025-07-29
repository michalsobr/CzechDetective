using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// Handles the visual representation of a save slot in the Load Game UI, showing player name, slot number, save time, and scene image.
/// Changes the scene image color on hover when a save exists.
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
    /// Updates the slot visuals based on whether a game state exists.
    /// </summary>
    /// <param name="saveSlotNumber">The index of the save slot to display.</param>
    /// <param name="state">The <see cref="GameState"/> to associate with this slot, or <c>null</c> if the slot is empty.</param>
    public void SetSlotData(int saveSlotNumber, GameState state)
    {
        gameStateCopy = state;

        if (state == null) SetEmptySlot(saveSlotNumber);
        else SetFilledSlot(saveSlotNumber);
    }

    /// <summary>
    /// Configures the slot to display as empty, clearing the image and setting placeholder text.
    /// </summary>
    /// <param name="saveSlotNumber">The save slot number to display.</param>
    public void SetEmptySlot(int saveSlotNumber)
    {
        hasSave = false;

        if (sceneImage)
        {
            sceneImage.sprite = null;
            sceneImage.color = emptySceneImageColor;
        }

        if (playerText) playerText.text = "<EMPTY>";
        if (slotText) slotText.text = saveSlotNumber.ToString();
        if (timeText) timeText.text = "-/-/- -:-";
    }

    /// <summary>
    /// Configures the slot to display a saved game, loading the scene image and showing the player name, save slot number, and save time.
    /// </summary>
    /// <param name="saveSlotNumber">The save slot number to display.</param>
    public void SetFilledSlot(int saveSlotNumber)
    {
        hasSave = true;

        if (sceneImage)
        {
            Sprite sceneSprite = Resources.Load<Sprite>($"Sprites/Enviroments/{gameStateCopy.currentScene}");
            sceneImage.sprite = sceneSprite;
            sceneImage.color = defaultSceneImageColor;
        }

        if (playerText) playerText.text = gameStateCopy.playerName;
        if (slotText) slotText.text = saveSlotNumber.ToString();
        if (timeText) timeText.text = gameStateCopy.lastSavedTime;
    }

    #endregion
    #region Event Handlers / Callbacks

    /// <summary>
    /// Invoked when the pointer enters the slot area.
    /// Changes the scene image color to the hover color if a save exists.
    /// </summary>
    /// <param name="eventData">The pointer event data.</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hasSave) sceneImage.color = hoverSceneImageColor;
    }

    /// <summary>
    /// Invoked when the pointer exits the slot area.
    /// Resets the scene image color to the default color if a save exists.
    /// </summary>
    /// <param name="eventData">The pointer event data.</param>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (hasSave) sceneImage.color = defaultSceneImageColor;
    }

    #endregion
}
