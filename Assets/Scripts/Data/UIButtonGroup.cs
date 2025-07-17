using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class UIButtonGroup
{
    public Button button;
    public GameObject canvas;
    [HideInInspector] public PopupCanvas popupScript;
}
