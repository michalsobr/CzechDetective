using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class SaveSlotGroup
{
    public GameObject saveSlot;
    [HideInInspector] public Button slotButton;
    [HideInInspector] public SaveSlotVisualizer slotVisualizer;
    [HideInInspector] public GameObject deleteSlotButton;
    [HideInInspector] public Button deleteButton;
    [HideInInspector] public bool isButtonClicked;
}
