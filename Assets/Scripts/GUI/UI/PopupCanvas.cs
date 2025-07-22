using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public abstract class PopupCanvas : MonoBehaviour
{
    [SerializeField] private Button closeButton;

    protected virtual void OnEnable()
    {
        // clear other listeners before adding the onClick listener, just in case.
        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(() => Close());
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
