using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public enum ActionType { Start, Load, Exit }
    public ActionType action;

    private TextMeshProUGUI text;

    private Color normalColor = Color.white;
    // Czech gradient hover colors.
    private Color32 topLeft = new Color32(0x00, 0x5B, 0xBF, 255);
    private Color32 topRight = new Color32(0x00, 0x45, 0xA5, 255);
    private Color32 bottomLeft = new Color32(0xFF, 0xFF, 0xFF, 255);
    private Color32 bottomRight = new Color32(0xEF, 0x33, 0x40, 255);

    void Start() { }
    void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        text.color = normalColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        text.enableVertexGradient = true;
        text.colorGradient = new VertexGradient(topLeft, topRight, bottomLeft, bottomRight);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        text.enableVertexGradient = false;
        text.color = normalColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        switch (action)
        {
            case ActionType.Start:
                Debug.Log("Start Game clicked");
                SceneManager.LoadScene("Init");
                break;
            // TODO Load popup.
            case ActionType.Load:
                Debug.Log("Load Game clicked");
                break;
            case ActionType.Exit:
                Debug.Log("Exit clicked");
                Application.Quit();
                break;
        }
    }
}
