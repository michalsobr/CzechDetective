using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public TextMeshProUGUI text;
    public Color normalColor = Color.white;
    public Color hoverColor = Color.yellow;

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
        text.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        text.color = normalColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        switch (action)
        {
            case ActionType.Start:
                Debug.Log("Start Game clicked");

                SceneManager.LoadScene("Pre-Arc1");

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
