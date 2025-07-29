
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class EventSystemDDOL : MonoBehaviour
{
    // Singleton instance.
    public static EventSystemDDOL Instance { get; private set; }

    /// <summary>
    /// Invoked when the script instance is loaded, even if the GameObject is inactive.
    /// Ensures only one instance exists and assigns it as the singleton instance.
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
    }
}
