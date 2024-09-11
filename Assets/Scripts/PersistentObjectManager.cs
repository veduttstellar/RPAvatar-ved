using UnityEngine;

public class PersistentObjectManager : MonoBehaviour
{
    private static PersistentObjectManager _instance;

    public static PersistentObjectManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // Find an existing instance in the scene
                _instance = Object.FindFirstObjectByType<PersistentObjectManager>();

                if (_instance == null)
                {
                    // Create a new GameObject with this script if none exists
                    GameObject singletonObject = new GameObject("PersistentObjectManager");
                    _instance = singletonObject.AddComponent<PersistentObjectManager>();
                    DontDestroyOnLoad(singletonObject);
                }
            }
            return _instance;
        }
    }

    // Example variables or methods that you want to persist
    public int exampleValue;

    private void Awake()
    {
        // Ensure only one instance exists
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
