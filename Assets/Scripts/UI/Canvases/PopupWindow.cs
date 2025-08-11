using UnityEngine;
using UnityEngine.UI;

public abstract class PopupWindow : MonoBehaviour
{
    [SerializeField] protected Button closeButton;

    protected virtual void OnEnable()
    {
        if (closeButton) closeButton.onClick.AddListener(() => Close());
    }

    protected virtual void OnDisable()
    {
        if (closeButton) closeButton.onClick.RemoveListener(() => Close());
    }

    public virtual void Open()
    {
        gameObject.SetActive(true);
    }

    public virtual void Close()
    {
        UIManager.Instance.ClosePopup();

        gameObject.SetActive(false);
    }
}
