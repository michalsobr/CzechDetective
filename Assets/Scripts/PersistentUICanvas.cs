using UnityEngine;

public class PersistentUICanvas : MonoBehaviour
{
    void Start()
    {
        
    }

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
}
