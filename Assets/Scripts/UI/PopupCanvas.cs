using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public abstract class PopupCanvas : MonoBehaviour
{
    protected Button closeButton;
    [SerializeField] private Button closeButtonman;

    protected virtual void OnEnable()
    {
        // clear other listeners before adding the onClick listener, just in case.
        closeButtonman.onClick.RemoveAllListeners();
        closeButtonman.onClick.AddListener(() => Close());
    }

    public virtual void Open()
    {
        gameObject.SetActive(true);
    }

    public virtual void Close()
    {
        UIManager.Instance.SetInteractable(true);
        UIManager.Instance.ClosePopup();

        gameObject.SetActive(false);
    }
}
