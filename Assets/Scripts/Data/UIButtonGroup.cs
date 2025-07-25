using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Represents a group of UI elements that link a button to a popup canvas and its controlling script.
/// </summary>
[Serializable]
public class UIButtonGroup
{
    /// <summary>
    /// The UI button that will toggle the associated popup canvas.
    /// </summary>
    public Button button;

    /// <summary>
    /// The GameObject representing the popup canvas associated with the button.
    /// </summary>
    public GameObject canvas;

    /// <summary>
    /// The script component attached to the popup canvas that manages its behavior.
    /// </summary>
    public PopupCanvas popupScript;
}
