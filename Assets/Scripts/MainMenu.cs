using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEditor;
using Unity.VisualScripting;
using System;

public class MainMenu : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public TextMeshProUGUI text;
    private Color normalColor = Color.white;
    // Gradient hover colors
    private Color32 topLeft = new Color32(0x00, 0x5B, 0xBF, 255);
    private Color32 topRight = new Color32(0x00, 0x45, 0xA5, 255);
    private Color32 bottomLeft = new Color32(0xFF, 0xFF, 0xFF, 255);
    private Color32 bottomRight = new Color32(0xEF, 0x33, 0x40, 255);

    public enum ActionType { Start, Load, Exit }
    public ActionType action;

    void Start()
    {
        if (text == null)
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

                SceneManager.LoadScene("Base(ArcOne)");

                break;
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
