using UnityEngine;
using UnityEngine.UI;

public abstract class PopupWindow : MonoBehaviour
{
    [SerializeField] protected Button closeButton;

    protected virtual void OnEnable()
    {
        if(closeButton) closeButton.onClick.AddListener(() => Close());
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
