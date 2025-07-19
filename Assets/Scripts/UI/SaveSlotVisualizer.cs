using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class SaveSlotVisualizer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [HideInInspector] public bool hasSave = false;

    [Header("Visual Targets")]
    [SerializeField] private Image slotBackground;
    [SerializeField] private Image sceneImage;
    [SerializeField] private TextMeshProUGUI playerText;
    [SerializeField] private TextMeshProUGUI slotText;
    [SerializeField] private TextMeshProUGUI timeText;

    private Color defaultColor = Color.white;
    private Color slotHoverColor = new Color32(200, 200, 200, 255);
    private Color emptySceneImageColor = new Color32(255, 255, 255, 0);
    private Color defaultSceneImageColor = new Color32(255, 255, 255, 155);
    private Color hoverSceneImageColor = new Color32(255, 255, 255, 255);

    public void OnPointerEnter(PointerEventData eventData)
    {
        slotBackground.color = slotHoverColor;

        if (hasSave) sceneImage.color = hoverSceneImageColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        slotBackground.color = defaultColor;

        if (hasSave) sceneImage.color = defaultSceneImageColor;
    }

    // This method will be called when assigning save data to this slot
    public void SetSlotData(int saveSlot, string playerName, string timestamp, Sprite sceneSprite)
    {
        hasSave = true;

        if (sceneImage)
        {
            sceneImage.sprite = sceneSprite;
            sceneImage.color = defaultSceneImageColor;
        }
        if (playerText) playerText.text = playerName;
        if (slotText) slotText.text = saveSlot.ToString();
        if (timeText) timeText.text = timestamp;
    }

    public void SetEmptySlot(int saveSlot)
    {
        hasSave = false;

        if (sceneImage)
        {
            sceneImage.sprite = null;
            sceneImage.color = emptySceneImageColor;
        }

        if (playerText) playerText.text = "<EMPTY>";
        if (slotText) slotText.text = saveSlot.ToString();
        if (timeText) timeText.text = "-/-/- -:-";
    }
}
