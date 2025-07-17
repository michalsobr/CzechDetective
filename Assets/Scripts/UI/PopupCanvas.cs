using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public abstract class PopupCanvas : MonoBehaviour
{
    protected Button closeButton;
    [SerializeField] private Button closeButtonman;

    protected virtual void OnEnable()
    {
        closeButtonman.onClick.RemoveAllListeners(); // clear just in case
        closeButtonman.onClick.AddListener(() =>
        {
            Debug.Log($"[PopupCanvas] CloseButton clicked on {gameObject.name}");
            Close();
        });

        Debug.Log($"[PopupCanvas] Listener added to {closeButtonman.name}");
    



        /*
        Debug.Log($"[PopupCanvas] OnEnable called {gameObject.name}");
        // find the close button by name among children (inactive too).
        closeButton = GetComponentsInChildren<Button>(true).FirstOrDefault(b => b.name == "CloseButton");

        if (closeButton != null)
        {
            Debug.Log($"[PopupCanvas] OnEnable, CloseButton is not null {gameObject.name}");
            //closeButton.onClick.AddListener(() => Close());
            closeButton.onClick.AddListener(() =>
    {
        Debug.Log($"[PopupCanvas] CloseButton clicked on {gameObject.name}");
        Close();
    });
        }
        else
        {
            Debug.LogWarning($"No 'Close button' found on {gameObject.name}");
        }
        */
    }

    public virtual void Open()
    {
        gameObject.SetActive(true);
    }

    public virtual void Close()
    {
        // UIManager.Instance.SetInteractable(true);
        gameObject.SetActive(false);
    }
}
