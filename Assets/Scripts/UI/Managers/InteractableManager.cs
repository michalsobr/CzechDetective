using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Manages the interaction button for interactable images in the UI.
/// Handles positioning, sizing, and click lifecycle.
/// </summary>
public class InteractableManager : MonoBehaviour
{
    public static InteractableManager Instance;

    private Canvas backgroundCanvas;
    private GameObject interactionButton;
    private RectTransform buttonRT;
    private Button button;
    private TextMeshProUGUI buttonText;
    private InteractableImage currentTarget;

    /// <summary>
    /// Invoked when the script instance is loaded, even if the GameObject is inactive.
    /// Ensures a single instance, and hides the interaction button by default.
    /// </summary>
    private void Awake()
    {
        // If another instance already exists, destroy this one.
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Prevent this object from being destroyed when loading new scenes.
        DontDestroyOnLoad(gameObject);

        if (SceneManager.GetActiveScene().name != "MainMenu")
        {
            StartCoroutine(DelayedAssignReferences());
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// Displays the interaction button near the mouse for the specified target (interactable image).
    /// </summary>
    /// <param name="target">The interactable image to show the button for.</param>
    public void ShowButton(InteractableImage target)
    {
        // Only one button active at any one time.
        HideButton();

        currentTarget = target;
        buttonText.text = target.buttonText;
        buttonText.ForceMeshUpdate();

        // Resize and position button
        Vector2 preferredSize = new Vector2(
            buttonText.preferredWidth + (buttonText.preferredWidth / 5),
            2 * buttonText.preferredHeight
        );

        buttonRT.sizeDelta = preferredSize;

        // Position the button at the mouse position (converted to local canvas space)
        Vector2 mousePos = Mouse.current.position.ReadValue();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            backgroundCanvas.transform as RectTransform,
            mousePos,
            null,
            out Vector2 localPos
        );

        buttonRT.localPosition = localPos;

        // Assign the interaction callback
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => target.TriggerInteraction());

        interactionButton.SetActive(true); // Show the button.
    }

    /// <summary>
    /// Hides the interaction button and clears the current interactable target.
    /// </summary>
    public void HideButton()
    {
        interactionButton.SetActive(false);
        button.onClick.RemoveAllListeners();
        currentTarget = null;
    }

    /// <summary>
    /// Checks for mouse clicks outside the button to hide it.
    /// </summary>
    private void Update()
    {
        // If an interactable image was pressed 
        if (Mouse.current.leftButton.wasPressedThisFrame && currentTarget != null)
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                // Set/Initialize position of the pointer event.
                position = Mouse.current.position.ReadValue()
            };

            // If click did not land on the current interactable, hide the button
            if (!currentTarget.WasClickedOn(pointerData))
            {
                HideButton();
            }
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "MainMenu")
        {
            Debug.Log("[InteractableManager] Scene loaded: " + scene.name);
            StartCoroutine(DelayedAssignReferences());
        }
    }

    public void SetAllInteractablesActive(bool state)
    {
        GameObject[] interactables = GameObject.FindGameObjectsWithTag("InteractableImage");

        foreach (GameObject obj in interactables)
        {
            CanvasGroup cg = obj.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                if (state)
                {
                    cg.alpha = 1f;
                    cg.interactable = true;
                    cg.blocksRaycasts = true;
                }
                else
                {
                    cg.alpha = 0f;
                    cg.interactable = false;
                    cg.blocksRaycasts = false;
                }
            }
        }
    }

    private IEnumerator DelayedAssignReferences()
    {
        yield return new WaitForEndOfFrame(); // Let UI fully initialize.

        Debug.Log("[InteractableManager] DelayedAssignReferences has been called.");

        backgroundCanvas = GameObject.Find("BackgroundCanvas").GetComponent<Canvas>();
        Debug.Log("[IM] BackgroundCanvas: " + backgroundCanvas);

        interactionButton = GameObject.Find("InteractionButton");
        Debug.Log("[IM] InteractionButton: " + interactionButton);

        if (interactionButton)
        {
            buttonRT = interactionButton.GetComponent<RectTransform>();
            button = interactionButton.GetComponent<Button>();
            buttonText = interactionButton.GetComponentInChildren<TextMeshProUGUI>();

            Debug.Log("[IM] Button: " + button + ", ButtonText: " + buttonText);

            HideButton();
        }
        else
        {
            Debug.LogWarning("InteractionButton not found in the scene.");
        }
    }

}
