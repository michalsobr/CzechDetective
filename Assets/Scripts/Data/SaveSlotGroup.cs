using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Represents a group of UI elements that make up a single save slot, including the slot button, visualizer, and delete button.
/// </summary>
[Serializable]
public class SaveSlotGroup
{
    /// <summary>
    /// The root GameObject representing the save slot in the UI.
    /// </summary>
    public GameObject saveSlot;

    /// <summary>
    /// The main button component for the save slot.
    /// </summary>
    public Button slotButton;

    /// <summary>
    /// The visualizer component that displays save slot data (player name, scene, timestamp).
    /// </summary>
    public SaveSlotVisualizer slotVisualizer;

    /// <summary>
    /// The GameObject representing the delete button for the save slot.
    /// </summary>
    public GameObject deleteSlotButton;

    /// <summary>
    /// The button component for the delete button.
    /// </summary>
    public Button deleteButton;

    /// <summary>
    /// Indicates whether the slot button has been clicked.
    /// Used to track user interaction state at runtime.
    /// </summary>
    [HideInInspector] public bool isButtonClicked;
}
